using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MatCapMaker
{
    public class MatCapMaker : EditorWindow
    {
        #region Constants
        private const string PREFS_KEY_SAVE_PATH = "MatCapMaker_SavePath";
        private const string DEFAULT_SAVE_PATH = "Assets/MatCaps";

        private static readonly string[] ResolutionOptions = { "128", "256", "512", "1024", "2048", "4096" };
        private static readonly int[] ResolutionValues = { 128, 256, 512, 1024, 2048, 4096 };
        private static readonly string[] ExportFormatOptions = { "PNG (8-bit)", "EXR (16-bit HDR)" };

        private static readonly Color AccentColor = new Color(0.3f, 0.7f, 1f);
        #endregion

        #region Private Variables
        private MatCapMakerSettingsWrapper settingsWrapper;
        private MatCapSettings settings => settingsWrapper?.settings;
        private string savePath;
        private int resolutionIndex = 2; 
        private int selectedTab = 0;
        private Texture2D[] presetThumbnails;
        private Vector2 previewScroll;
        private int textureWorkspaceTab = 1;
        private MatCapTextureGeneratorPanel textureGenerator;
        private int selectedBuiltInPreset = 0;
        private int exportFormatIndex = 0; 

        private RenderTexture previewRT;
        private RenderTexture[] layerRTs;
        private RenderTexture[] layerThumbnails; 
        private int previewSize = 512;
        private const int ThumbnailSize = 48;

        private Material baseMat;
        private Material spotLightMat;
        private Material blendMat;
        private Material fresnelMat;
        private Material rimMat;
        private Material reflectionMat;
        private Material envMat;
        private Material envCubemapMat;
        private Material noiseMat;
        private Material postProcessMat;

        private Vector2 scrollPosition;
        private bool[] lightFoldouts = { true, false, false, false };

        [SerializeField] private int[] layerOrder = { 1, 2, 3, 4, 5, 6, 7 };
        private static readonly string[] LayerNames = { "Base", "Lighting", "Fresnel", "Rim Effects", "Reflections", "Environment", "Noise", "Scene Capture" };

        private static readonly string[] CaptureResolutionOptions = { "128", "256", "512", "1024", "2048" };
        private static readonly int[] CaptureResolutionValues = { 128, 256, 512, 1024, 2048 };
        private int captureResolutionIndex = 2; 
        private ReflectionProbe selectedReflectionProbe;
        private ReflectionProbe pendingProbe;
        private int pendingProbeRenderId = -1;
        private double probeDeadline;
        private readonly System.Collections.Generic.List<Texture> ownedTextures = new System.Collections.Generic.List<Texture>();
        private int draggingLayerIndex = -1;
        private int dragTargetIndex = -1;

        private GUIStyle sectionHeaderStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private bool stylesInitialized = false;
        #endregion

        #region Window Setup
        [MenuItem("Tools/Fynn's Tools/MatCap Maker", false, 50)]
        public static void ShowWindow()
        {
            var window = GetWindow<MatCapMaker>(false, "MatCap Maker", true);
            window.minSize = new Vector2(860, 640);
            window.Show();
        }
        #endregion

        #region Unity Messages
        private void OnEnable()
        {
            presetThumbnails = null;
            settingsWrapper = CreateInstance<MatCapMakerSettingsWrapper>();
            settingsWrapper.hideFlags = HideFlags.HideAndDontSave;

            LoadMaterials();
            LoadPreferences();
            InitializeRenderTextures();
            settingsWrapper.settings = BuiltInPresets.GetDefault();
            selectedBuiltInPreset = Mathf.Clamp(selectedBuiltInPreset, 0, BuiltInPresets.GetPresetNames().Length - 1);
            ApplyPresetWithTextures(selectedBuiltInPreset);
            UpdatePreview();

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.update -= PollProbeCapture;
            if (textureGenerator != null) { textureGenerator.Dispose(); textureGenerator = null; }
            foreach (var texture in ownedTextures)
                if (texture != null && !AssetDatabase.Contains(texture)) DestroyImmediate(texture);
            ownedTextures.Clear();

            ReleaseRenderTextures();
            ReleaseMaterials();

            if (settingsWrapper != null)
            {
                DestroyImmediate(settingsWrapper);
                settingsWrapper = null;
            }
        }

        private void OnUndoRedo()
        {
            UpdatePreview();
            Repaint();
        }

        private void OnGUI()
        {
            InitStyles();
            if (settings == null) return;
            GUILayout.Space(10);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(14);
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("MatCap Maker", titleStyle);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Texture Generator", EditorStyles.miniButton, GUILayout.Width(130)))
                            { selectedTab = 6; textureWorkspaceTab = 1; }
                    }
                    GUILayout.Space(10);
                    int top = selectedTab < 5 ? selectedTab : -1;
                    int next = GUILayout.Toolbar(top, new[] { "Presets", "Surface", "Reflections", "Lighting", "Layers" }, GUILayout.Height(26));
                    if (next >= 0 && next != top) selectedTab = next;
                    int bottom = selectedTab >= 5 ? selectedTab - 5 : -1;
                    next = GUILayout.Toolbar(bottom, new[] { "Environment", "Texture", "Finish", "Capture" }, GUILayout.Height(24));
                    if (next >= 0 && next != bottom) selectedTab = next + 5;
                    GUILayout.Space(10);
                    using (new GUILayout.HorizontalScope(GUILayout.Height(Mathf.Max(300, position.height - 164))))
                    {
                        using (new GUILayout.VerticalScope(GUILayout.Width(Mathf.Clamp(position.width * .39f, 290, 480))))
                        {
                            previewScroll = EditorGUILayout.BeginScrollView(previewScroll);
                            DrawPreview();
                            EditorGUILayout.EndScrollView();
                            DrawExportSection();
                        }
                        GUILayout.Space(10);
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                            switch (selectedTab)
                            {
                                case 0: DrawPresetGallery(); break;
                                case 1: DrawMaterialTab(); break;
                                case 2: DrawReflectionsTab(); break;
                                case 3: DrawLightingTab(); break;
                                case 4: DrawLayersTab(); break;
                                case 5: DrawEnvironmentTab(); break;
                                case 6: DrawTextureWorkspace(); break;
                                case 7: DrawPostProcessTab(); break;
                                case 8: DrawSceneCaptureTab(); break;
                            }
                            EditorGUILayout.EndScrollView();
                        }
                    }
                    DrawFooter();
                }
                GUILayout.Space(14);
            }
        }
        #endregion

        #region Initialization
        private void InitStyles()
        {
            if (stylesInitialized) return;

            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };

            subtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Italic,
                normal = { textColor = Color.gray }
            };

            sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                richText = true
            };

            stylesInitialized = true;
        }

        private void LoadMaterials()
        {
            baseMat = CreateMaterial("MCM_MatCapBase");
            spotLightMat = CreateMaterial("MCM_SpotLight");
            blendMat = CreateMaterial("MCM_Blend");
            fresnelMat = CreateMaterial("MCM_Fresnel");
            rimMat = CreateMaterial("MCM_RimEffect");
            reflectionMat = CreateMaterial("MCM_Specular"); 
            envMat = CreateMaterial("MCM_Environment");
            envCubemapMat = CreateMaterial("MCM_EnvironmentCubemap");
            noiseMat = CreateMaterial("MCM_Noise");
            postProcessMat = CreateMaterial("MCM_PostProcess");
        }

        private Material CreateMaterial(string shaderName)
        {
            var shader = Resources.Load<Shader>(shaderName);
            if (shader == null)
            {
                shader = Shader.Find("Hidden/" + shaderName);
            }
            if (shader == null)
            {
                Debug.LogError($"[MatCap Maker] Could not find shader: {shaderName}");
                return null;
            }
            return new Material(shader);
        }

        private void ReleaseMaterials()
        {
            if (baseMat != null) DestroyImmediate(baseMat);
            if (spotLightMat != null) DestroyImmediate(spotLightMat);
            if (blendMat != null) DestroyImmediate(blendMat);
            if (fresnelMat != null) DestroyImmediate(fresnelMat);
            if (rimMat != null) DestroyImmediate(rimMat);
            if (reflectionMat != null) DestroyImmediate(reflectionMat);
            if (envMat != null) DestroyImmediate(envMat);
            if (envCubemapMat != null) DestroyImmediate(envCubemapMat);
            if (noiseMat != null) DestroyImmediate(noiseMat);
            if (postProcessMat != null) DestroyImmediate(postProcessMat);
        }

        private void LoadPreferences()
        {
            savePath = EditorPrefs.GetString(PREFS_KEY_SAVE_PATH, DEFAULT_SAVE_PATH);
        }

        private void InitializeRenderTextures()
        {
            ReleaseRenderTextures();

            previewRT = new RenderTexture(previewSize, previewSize, 0, RenderTextureFormat.ARGB32);
            previewRT.Create();

            layerRTs = new RenderTexture[8];
            for (int i = 0; i < 8; i++)
            {
                layerRTs[i] = new RenderTexture(previewSize, previewSize, 0, RenderTextureFormat.ARGB32);
                layerRTs[i].Create();
            }

            layerThumbnails = new RenderTexture[8];
            for (int i = 0; i < 8; i++)
            {
                layerThumbnails[i] = new RenderTexture(ThumbnailSize, ThumbnailSize, 0, RenderTextureFormat.ARGB32);
                layerThumbnails[i].Create();
            }
        }

        private void ReleaseRenderTextures()
        {
            if (previewRT != null)
            {
                if (RenderTexture.active == previewRT) RenderTexture.active = null;
                previewRT.Release();
                DestroyImmediate(previewRT);
                previewRT = null;
            }

            if (layerRTs != null)
            {
                for (int i = 0; i < layerRTs.Length; i++)
                {
                    if (layerRTs[i] != null)
                    {
                        if (RenderTexture.active == layerRTs[i]) RenderTexture.active = null;
                    layerRTs[i].Release();
                        DestroyImmediate(layerRTs[i]);
                    }
                }
                layerRTs = null;
            }

            if (layerThumbnails != null)
            {
                for (int i = 0; i < layerThumbnails.Length; i++)
                {
                    if (layerThumbnails[i] != null)
                    {
                        layerThumbnails[i].Release();
                        DestroyImmediate(layerThumbnails[i]);
                    }
                }
                layerThumbnails = null;
            }
        }
        #endregion

        #region Header and Footer
        private void DrawFooter()
        {
            EditorGUILayout.Space(5);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<b>Created by Fynn9563</b>", new GUIStyle(EditorStyles.label) { richText = true }))
                    Application.OpenURL("https://github.com/Fynn9563");
            }
        }
        #endregion

        #region Material Tab
        private void DrawMaterialTab()
        {
            DrawSection("Base Material", () =>
            {
                bool changed = false;

                var newColor = EditorGUILayout.ColorField("Base Color", settings.baseSettings.baseColor);
                if (newColor != settings.baseSettings.baseColor) { RecordUndo("Base Color"); settings.baseSettings.baseColor = newColor; changed = true; }

                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("Base Texture (Optional)", EditorStyles.miniBoldLabel);
                var newTex = (Texture2D)EditorGUILayout.ObjectField("Texture", settings.baseSettings.baseTexture, typeof(Texture2D), false);
                if (newTex != settings.baseSettings.baseTexture) { RecordUndo("Base Texture"); settings.baseSettings.baseTexture = newTex; changed = true; }

                if (settings.baseSettings.baseTexture != null)
                {
                    EditorGUI.indentLevel++;
                    float newStrength = EditorGUILayout.Slider("Strength", settings.baseSettings.textureStrength, 0f, 1f);
                    if (newStrength != settings.baseSettings.textureStrength) { RecordUndo("Texture Strength"); settings.baseSettings.textureStrength = newStrength; changed = true; }

                    var newBlend = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", settings.baseSettings.textureBlendMode);
                    if (newBlend != settings.baseSettings.textureBlendMode) { RecordUndo("Texture Blend"); settings.baseSettings.textureBlendMode = newBlend; changed = true; }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(5);

                float newMetallic = EditorGUILayout.Slider("Metallic", settings.baseSettings.metallic, 0f, 1f);
                if (newMetallic != settings.baseSettings.metallic) { RecordUndo("Metallic"); settings.baseSettings.metallic = newMetallic; changed = true; }

                float newRoughness = EditorGUILayout.Slider("Roughness", settings.baseSettings.roughness, 0f, 1f);
                if (newRoughness != settings.baseSettings.roughness) { RecordUndo("Roughness"); settings.baseSettings.roughness = newRoughness; changed = true; }

                if (changed)
                    UpdatePreview();
            });

            DrawSection("Quick Generate Base Textures", () =>
            {
                EditorGUILayout.LabelField("Generate procedural base textures:", EditorStyles.miniLabel);
                EditorGUILayout.Space(3);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Marble"))
                    {
                        RecordUndo("Generate Marble Texture");
                        settings.baseSettings.baseTexture = OwnTexture(MatCapTextureGenerator.GenerateMarbleNoise(256, 4f, 5f));
                        settings.baseSettings.textureStrength = 0.35f;
                        settings.baseSettings.textureBlendMode = BlendMode.Multiply;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Brushed"))
                    {
                        RecordUndo("Generate Brushed Texture");
                        settings.baseSettings.baseTexture = OwnTexture(MatCapTextureGenerator.GenerateBrushedMetal(256, 0.8f));
                        settings.baseSettings.textureStrength = 0.3f;
                        settings.baseSettings.textureBlendMode = BlendMode.Overlay;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Cellular"))
                    {
                        RecordUndo("Generate Cellular Texture");
                        settings.baseSettings.baseTexture = OwnTexture(MatCapTextureGenerator.GenerateCellularNoise(256, 6));
                        settings.baseSettings.textureStrength = 0.4f;
                        settings.baseSettings.textureBlendMode = BlendMode.Multiply;
                        UpdatePreview();
                    }
                }
            });

            DrawSection("Fresnel (Edge Reflection)", () =>
            {
                bool changed = false;

                bool newEnabled = EditorGUILayout.Toggle("Enabled", settings.fresnel.enabled);
                if (newEnabled != settings.fresnel.enabled) { RecordUndo("Fresnel Enabled"); settings.fresnel.enabled = newEnabled; changed = true; }

                if (settings.fresnel.enabled)
                {
                    EditorGUI.indentLevel++;
                    float newPower = EditorGUILayout.Slider("Power", settings.fresnel.power, 0.5f, 10f);
                    if (newPower != settings.fresnel.power) { RecordUndo("Fresnel Power"); settings.fresnel.power = newPower; changed = true; }

                    float newIntensity = EditorGUILayout.Slider("Intensity", settings.fresnel.intensity, 0f, 2f);
                    if (newIntensity != settings.fresnel.intensity) { RecordUndo("Fresnel Intensity"); settings.fresnel.intensity = newIntensity; changed = true; }

                    var newColor = EditorGUILayout.ColorField("Color", settings.fresnel.color);
                    if (newColor != settings.fresnel.color) { RecordUndo("Fresnel Color"); settings.fresnel.color = newColor; changed = true; }
                    EditorGUI.indentLevel--;
                }

                if (changed)
                    UpdatePreview();
            });

            DrawSection("Rim Effects", () =>
            {
                bool changed = false;

                bool newEnabled = EditorGUILayout.Toggle("Enabled", settings.rim.enabled);
                if (newEnabled != settings.rim.enabled) { RecordUndo("Rim Enabled"); settings.rim.enabled = newEnabled; changed = true; }

                if (settings.rim.enabled)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("Rim Light", EditorStyles.miniBoldLabel);
                    float newLightInt = EditorGUILayout.Slider("Intensity", settings.rim.rimLightIntensity, 0f, 1f);
                    if (newLightInt != settings.rim.rimLightIntensity) { RecordUndo("Rim Light Intensity"); settings.rim.rimLightIntensity = newLightInt; changed = true; }

                    float newLightBlur = EditorGUILayout.Slider("Blur", settings.rim.rimLightBlur, 0.1f, 2f);
                    if (newLightBlur != settings.rim.rimLightBlur) { RecordUndo("Rim Light Blur"); settings.rim.rimLightBlur = newLightBlur; changed = true; }

                    var newLightColor = EditorGUILayout.ColorField("Color", settings.rim.rimLightColor);
                    if (newLightColor != settings.rim.rimLightColor) { RecordUndo("Rim Light Color"); settings.rim.rimLightColor = newLightColor; changed = true; }

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("Rim Shade", EditorStyles.miniBoldLabel);
                    float newShadeInt = EditorGUILayout.Slider("Intensity", settings.rim.rimShadeIntensity, 0f, 1f);
                    if (newShadeInt != settings.rim.rimShadeIntensity) { RecordUndo("Rim Shade Intensity"); settings.rim.rimShadeIntensity = newShadeInt; changed = true; }

                    float newShadeBlur = EditorGUILayout.Slider("Blur", settings.rim.rimShadeBlur, 0.1f, 2f);
                    if (newShadeBlur != settings.rim.rimShadeBlur) { RecordUndo("Rim Shade Blur"); settings.rim.rimShadeBlur = newShadeBlur; changed = true; }

                    var newShadeColor = EditorGUILayout.ColorField("Color", settings.rim.rimShadeColor);
                    if (newShadeColor != settings.rim.rimShadeColor) { RecordUndo("Rim Shade Color"); settings.rim.rimShadeColor = newShadeColor; changed = true; }

                    EditorGUILayout.Space(5);

                    float newEmissive = EditorGUILayout.Slider("Emissive", settings.rim.emissive, 0f, 1f);
                    if (newEmissive != settings.rim.emissive) { RecordUndo("Rim Emissive"); settings.rim.emissive = newEmissive; changed = true; }

                    EditorGUI.indentLevel--;
                }

                if (changed)
                    UpdatePreview();
            });

        }

        private void DrawReflectionsTab()
        {
            DrawSection("Reflections (Procedural Environment)", () =>
            {
                bool changed = false;

                bool newEnabled = EditorGUILayout.Toggle("Enabled", settings.reflections.enabled);
                if (newEnabled != settings.reflections.enabled) { RecordUndo("Reflections Enabled"); settings.reflections.enabled = newEnabled; changed = true; }

                if (settings.reflections.enabled)
                {
                    EditorGUI.indentLevel++;

                    var studio = settings.reflections.studio.Copy();
                    EditorGUI.BeginChangeCheck();
                    studio.enabled = EditorGUILayout.Toggle("Studio Reflections", studio.enabled);
                    if (studio.enabled)
                    {
                        studio.metalColor = EditorGUILayout.ColorField("Metal Color", studio.metalColor);
                        studio.shadowColor = EditorGUILayout.ColorField("Shadow Color", studio.shadowColor);
                        studio.rotation = EditorGUILayout.Slider("Studio Rotation", studio.rotation, -180, 180);
                        studio.softness = EditorGUILayout.Slider("Reflection Softness", studio.softness, .01f, .8f);
                        studio.distortion = EditorGUILayout.Slider("Reflection Distortion", studio.distortion, 0, .6f);
                        studio.grain = EditorGUILayout.Slider("Surface Grain", studio.grain, 0, .3f);
                        studio.iridescence = EditorGUILayout.Slider("Iridescence", studio.iridescence, 0, 1);
                        studio.satin = EditorGUILayout.Slider("Satin Finish", studio.satin, 0, 1);
                        studio.pattern = (SurfacePattern)EditorGUILayout.EnumPopup("Surface Pattern", studio.pattern);
                        if (studio.pattern != SurfacePattern.Smooth)
                        {
                            studio.patternScale = EditorGUILayout.Slider("Pattern Scale", studio.patternScale, 1, 30);
                            studio.patternStrength = EditorGUILayout.Slider("Pattern Strength", studio.patternStrength, 0, 1);
                            studio.patternDetail = EditorGUILayout.IntSlider("Pattern Detail", studio.patternDetail, 2, 7);
                            studio.patternSeed = EditorGUILayout.FloatField("Pattern Seed", studio.patternSeed);
                            studio.patternColor = EditorGUILayout.ColorField("Pattern Color", studio.patternColor);
                            studio.wrapPattern = EditorGUILayout.Toggle("Wrap Around Sphere", studio.wrapPattern);
                            studio.patternWarp = EditorGUILayout.Slider("Pattern Warp", studio.patternWarp, 0, 1);
                            studio.patternGlow = EditorGUILayout.Slider("Pattern Glow", studio.patternGlow, 0, 3);
                            studio.patternRotation = EditorGUILayout.Slider("Pattern Rotation", studio.patternRotation, -180, 180);
                            studio.patternContrast = EditorGUILayout.Slider("Pattern Contrast", studio.patternContrast, .2f, 3);
                        }
                        for (int panelIndex = 0; panelIndex < studio.panels.Length; panelIndex++)
                        {
                            var panel = studio.panels[panelIndex];
                            EditorGUILayout.Space(4);
                            panel.enabled = EditorGUILayout.Toggle("Reflection Panel " + (panelIndex + 1), panel.enabled);
                            if (!panel.enabled) continue;
                            panel.shape = (ReflectionShape)EditorGUILayout.EnumPopup("Shape", panel.shape);
                            panel.direction = EditorGUILayout.Vector3Field("Direction", panel.direction);
                            panel.size = EditorGUILayout.Vector2Field("Width / Height", panel.size);
                            panel.size.x = Mathf.Clamp(panel.size.x, .03f, 3f);
                            panel.size.y = Mathf.Clamp(panel.size.y, .03f, 3f);
                            panel.rotation = EditorGUILayout.Slider("Panel Rotation", panel.rotation, -180, 180);
                            panel.color = EditorGUILayout.ColorField("Panel Color", panel.color);
                            panel.intensity = EditorGUILayout.Slider("Panel Brightness", panel.intensity, 0, 5);
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        RecordUndo("Studio Reflections");
                        settings.reflections.studio = studio;
                        changed = true;
                    }
                    if (!studio.enabled)
                    {
                    float newSmooth = EditorGUILayout.Slider("Smoothness", settings.reflections.smoothness, 0f, 1f);
                    if (newSmooth != settings.reflections.smoothness) { RecordUndo("Reflections Smoothness"); settings.reflections.smoothness = newSmooth; changed = true; }

                    float newIntensity = EditorGUILayout.Slider("Intensity", settings.reflections.intensity, 0f, 3f);
                    if (newIntensity != settings.reflections.intensity) { RecordUndo("Reflections Intensity"); settings.reflections.intensity = newIntensity; changed = true; }

                    var newTint = EditorGUILayout.ColorField("Tint", settings.reflections.reflectionColor);
                    if (newTint != settings.reflections.reflectionColor) { RecordUndo("Reflections Tint"); settings.reflections.reflectionColor = newTint; changed = true; }

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Fresnel", EditorStyles.miniBoldLabel);
                    float newBias = EditorGUILayout.Slider("Bias (Center)", settings.reflections.fresnelBias, 0f, 1f);
                    if (newBias != settings.reflections.fresnelBias) { RecordUndo("Reflections Fresnel Bias"); settings.reflections.fresnelBias = newBias; changed = true; }

                    float newPower = EditorGUILayout.Slider("Power (Edges)", settings.reflections.fresnelPower, 0.5f, 10f);
                    if (newPower != settings.reflections.fresnelPower) { RecordUndo("Reflections Fresnel Power"); settings.reflections.fresnelPower = newPower; changed = true; }

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Environment Colors", EditorStyles.miniBoldLabel);
                    var newHorizon = EditorGUILayout.ColorField("Horizon/Sky", settings.reflections.horizonColor);
                    if (newHorizon != settings.reflections.horizonColor) { RecordUndo("Reflections Horizon"); settings.reflections.horizonColor = newHorizon; changed = true; }

                    var newGround = EditorGUILayout.ColorField("Ground", settings.reflections.groundColor);
                    if (newGround != settings.reflections.groundColor) { RecordUndo("Reflections Ground"); settings.reflections.groundColor = newGround; changed = true; }

                    }
                    EditorGUI.indentLevel--;
                }

                if (changed)
                    UpdatePreview();
            });
        }
        #endregion

        #region Lighting Tab
        private bool lightingChanged = false;

        private void DrawLightingTab()
        {
            lightingChanged = false;

            bool newEnabled = EditorGUILayout.Toggle("Enable Lighting", settings.lighting.enabled);
            if (newEnabled != settings.lighting.enabled) { RecordUndo("Lighting Enabled"); settings.lighting.enabled = newEnabled; lightingChanged = true; }

            if (settings.lighting.enabled)
            {
                EditorGUILayout.Space(5);

                for (int i = 0; i < 4; i++)
                {
                    DrawLightPanel(i);
                    EditorGUILayout.Space(3);
                }
            }

            if (lightingChanged)
                UpdatePreview();
        }

        private void DrawLightPanel(int index)
        {
            var light = settings.lighting.lights[index];

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    bool newLightEnabled = EditorGUILayout.Toggle(light.enabled, GUILayout.Width(20));
                    if (newLightEnabled != light.enabled) { RecordUndo($"Light {index+1} Enabled"); light.enabled = newLightEnabled; lightingChanged = true; }

                    lightFoldouts[index] = EditorGUILayout.Foldout(lightFoldouts[index], $"Light {index + 1}", true,
                        new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
                }

                if (lightFoldouts[index] && light.enabled)
                {
                    EditorGUI.indentLevel++;

                    var newPos = EditorGUILayout.Vector3Field("Position", light.position);
                    newPos.x = Mathf.Clamp(newPos.x, -2f, 2f);
                    newPos.y = Mathf.Clamp(newPos.y, -2f, 2f);
                    newPos.z = Mathf.Clamp(newPos.z, -2f, 2f);
                    if (newPos != light.position) { RecordUndo($"Light {index+1} Position"); light.position = newPos; lightingChanged = true; }

                    float newIntensity = EditorGUILayout.Slider("Intensity", light.intensity, 0f, 5f);
                    if (newIntensity != light.intensity) { RecordUndo($"Light {index+1} Intensity"); light.intensity = newIntensity; lightingChanged = true; }

                    var newColor = EditorGUILayout.ColorField("Color", light.color);
                    if (newColor != light.color) { RecordUndo($"Light {index+1} Color"); light.color = newColor; lightingChanged = true; }

                    float newRange = EditorGUILayout.Slider("Range", light.range, 0.1f, 5f);
                    if (newRange != light.range) { RecordUndo($"Light {index+1} Range"); light.range = newRange; lightingChanged = true; }

                    float newBlur = EditorGUILayout.Slider("Blur", light.blur, 0.01f, 3f);
                    if (newBlur != light.blur) { RecordUndo($"Light {index+1} Blur"); light.blur = newBlur; lightingChanged = true; }

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Specular", EditorStyles.miniBoldLabel);

                    bool newSpecEnabled = EditorGUILayout.Toggle("Enabled", light.specularEnabled);
                    if (newSpecEnabled != light.specularEnabled) { RecordUndo($"Light {index+1} Specular"); light.specularEnabled = newSpecEnabled; lightingChanged = true; }

                    if (light.specularEnabled)
                    {
                        float newSpecInt = EditorGUILayout.Slider("Intensity", light.specularIntensity, 0f, 2f);
                        if (newSpecInt != light.specularIntensity) { RecordUndo($"Light {index+1} Spec Intensity"); light.specularIntensity = newSpecInt; lightingChanged = true; }

                        float newSpecSize = EditorGUILayout.Slider("Size", light.specularSize, 0.1f, 3f);
                        if (newSpecSize != light.specularSize) { RecordUndo($"Light {index+1} Spec Size"); light.specularSize = newSpecSize; lightingChanged = true; }
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion

        #region Layers Tab
        private void DrawLayersTab()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.HelpBox("Drag layers to reorder. Layers are composited from bottom to top.", MessageType.Info);
                if (GUILayout.Button("Reset Order", GUILayout.Width(80), GUILayout.Height(38)))
                {
                    RecordUndo("Reset Layer Order");
                    ResetLayerOrder();
                    UpdatePreview();
                }
            }

            EditorGUI.BeginChangeCheck();

            DrawLockedLayerControl(0);

            for (int i = 0; i < layerOrder.Length; i++)
            {
                int layerIndex = layerOrder[i];
                DrawDraggableLayerControl(i, layerIndex);
            }

            HandleLayerDragAndDrop();

            if (EditorGUI.EndChangeCheck())
                UpdatePreview();
        }

        private void ResetLayerOrder()
        {
            for (int i = 0; i < layerOrder.Length; i++)
                layerOrder[i] = i + 1;
        }

        private void DrawLockedLayerControl(int layerIndex)
        {
            string name = LayerNames[layerIndex];
            int thumbnailIndex = layerIndex;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    Rect lockRect = GUILayoutUtility.GetRect(20, ThumbnailSize, GUILayout.Width(20));
                    GUI.Label(lockRect, EditorGUIUtility.IconContent("LockIcon"));

                    if (layerThumbnails != null && thumbnailIndex < layerThumbnails.Length && layerThumbnails[thumbnailIndex] != null)
                    {
                        Rect thumbRect = GUILayoutUtility.GetRect(ThumbnailSize, ThumbnailSize, GUILayout.Width(ThumbnailSize), GUILayout.Height(ThumbnailSize));
                        EditorGUI.DrawPreviewTexture(thumbRect, layerThumbnails[thumbnailIndex]);
                        GUILayout.Space(5);
                    }

                    using (new GUILayout.VerticalScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(name, EditorStyles.boldLabel, GUILayout.Width(100));
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField("Always On",
                                new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight },
                                GUILayout.Width(60));
                        }
                    }
                }
            }
        }

        private void DrawDraggableLayerControl(int displayIndex, int layerIndex)
        {
            string name = LayerNames[layerIndex];
            bool enabled = GetLayerEnabled(layerIndex);
            bool showControls = true;
            int thumbnailIndex = layerIndex;

            Rect dragRect = Rect.zero;
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    dragRect = GUILayoutUtility.GetRect(20, ThumbnailSize, GUILayout.Width(20));
                    if (Event.current.type == EventType.Repaint)
                        for (int line = -1; line <= 1; line++)
                            EditorGUI.DrawRect(new Rect(dragRect.center.x - 5, dragRect.center.y + line * 4, 10, 1), new Color(.6f,.6f,.6f));
                    EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.Pan);

                    if (layerThumbnails != null && thumbnailIndex < layerThumbnails.Length && layerThumbnails[thumbnailIndex] != null)
                    {
                        Rect thumbRect = GUILayoutUtility.GetRect(ThumbnailSize, ThumbnailSize, GUILayout.Width(ThumbnailSize), GUILayout.Height(ThumbnailSize));
                        EditorGUI.DrawPreviewTexture(thumbRect, layerThumbnails[thumbnailIndex]);
                        GUILayout.Space(5);
                    }

                    using (new GUILayout.VerticalScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            Color origColor = GUI.color;
                            GUI.color = enabled ? Color.white : Color.gray;
                            EditorGUILayout.LabelField(name, EditorStyles.boldLabel, GUILayout.Width(100));
                            GUI.color = origColor;

                            GUILayout.FlexibleSpace();

                            EditorGUILayout.LabelField(enabled ? "Enabled" : "Disabled",
                                new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight },
                                GUILayout.Width(60));
                        }

                        if (showControls && enabled)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField("Blend", GUILayout.Width(40));
                                BlendMode blendMode = GetLayerBlendMode(layerIndex);
                                BlendMode newBlendMode = (BlendMode)EditorGUILayout.EnumPopup(blendMode, GUILayout.Width(100));
                                if (newBlendMode != blendMode)
                                {
                                    RecordUndo($"{LayerNames[layerIndex]} Blend Mode");
                                    SetLayerBlendMode(layerIndex, newBlendMode);
                                    UpdatePreview();
                                }

                                GUILayout.Space(10);

                                EditorGUILayout.LabelField("Opacity", GUILayout.Width(50));
                                float opacity = GetLayerOpacity(layerIndex);
                                float newOpacity = EditorGUILayout.Slider(opacity, 0f, 1f);
                                if (newOpacity != opacity)
                                {
                                    RecordUndo($"{LayerNames[layerIndex]} Opacity");
                                    SetLayerOpacity(layerIndex, newOpacity);
                                    UpdatePreview();
                                }
                            }
                        }
                    }
                }

            }

            Rect fullRect = GUILayoutUtility.GetLastRect();

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && dragRect.Contains(Event.current.mousePosition))
            {
                draggingLayerIndex = displayIndex;
                Event.current.Use();
            }

            if (draggingLayerIndex >= 0 && draggingLayerIndex != displayIndex)
            {
                if (fullRect.Contains(Event.current.mousePosition))
                {
                    dragTargetIndex = displayIndex;
                    Rect indicatorRect = new Rect(fullRect.x, fullRect.y - 2, fullRect.width, 4);
                    EditorGUI.DrawRect(indicatorRect, AccentColor);
                    Repaint();
                }
            }
        }

        private void HandleLayerDragAndDrop()
        {
            if (draggingLayerIndex < 0) return;

            if (Event.current.type == EventType.MouseUp)
            {
                if (dragTargetIndex >= 0 && dragTargetIndex != draggingLayerIndex)
                {
                    Undo.RecordObject(this, "Reorder Layers");
                    int movingLayer = layerOrder[draggingLayerIndex];

                    for (int i = draggingLayerIndex; i < layerOrder.Length - 1; i++)
                        layerOrder[i] = layerOrder[i + 1];

                    int insertAt = dragTargetIndex;

                    for (int i = layerOrder.Length - 1; i > insertAt; i--)
                        layerOrder[i] = layerOrder[i - 1];
                    layerOrder[insertAt] = movingLayer;

                    UpdatePreview();
                }

                draggingLayerIndex = -1;
                dragTargetIndex = -1;
                Event.current.Use();
                Repaint();
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                Repaint();
            }
        }

        private bool GetLayerEnabled(int layerIndex)
        {
            switch (layerIndex)
            {
                case 0: return true; 
                case 1: return settings.lighting.enabled;
                case 2: return settings.fresnel.enabled;
                case 3: return settings.rim.enabled;
                case 4: return settings.reflections.enabled;
                case 5: return settings.environment.enabled;
                case 6: return settings.noise.enabled;
                case 7: return settings.sceneCapture.enabled;
                default: return false;
            }
        }

        private BlendMode GetLayerBlendMode(int layerIndex)
        {
            switch (layerIndex)
            {
                case 1: return settings.lighting.blendMode;
                case 2: return settings.fresnel.blendMode;
                case 3: return settings.rim.blendMode;
                case 4: return settings.reflections.blendMode;
                case 5: return settings.environment.blendMode;
                case 6: return settings.noise.blendMode;
                case 7: return settings.sceneCapture.blendMode;
                default: return BlendMode.Normal;
            }
        }

        private void SetLayerBlendMode(int layerIndex, BlendMode mode)
        {
            switch (layerIndex)
            {
                case 1: settings.lighting.blendMode = mode; break;
                case 2: settings.fresnel.blendMode = mode; break;
                case 3: settings.rim.blendMode = mode; break;
                case 4: settings.reflections.blendMode = mode; break;
                case 5: settings.environment.blendMode = mode; break;
                case 6: settings.noise.blendMode = mode; break;
                case 7: settings.sceneCapture.blendMode = mode; break;
            }
        }

        private float GetLayerOpacity(int layerIndex)
        {
            switch (layerIndex)
            {
                case 1: return settings.lighting.opacity;
                case 2: return settings.fresnel.opacity;
                case 3: return settings.rim.opacity;
                case 4: return settings.reflections.opacity;
                case 5: return settings.environment.opacity;
                case 6: return settings.noise.opacity;
                case 7: return settings.sceneCapture.opacity;
                default: return 1f;
            }
        }

        private void SetLayerOpacity(int layerIndex, float opacity)
        {
            switch (layerIndex)
            {
                case 1: settings.lighting.opacity = opacity; break;
                case 2: settings.fresnel.opacity = opacity; break;
                case 3: settings.rim.opacity = opacity; break;
                case 4: settings.reflections.opacity = opacity; break;
                case 5: settings.environment.opacity = opacity; break;
                case 6: settings.noise.opacity = opacity; break;
                case 7: settings.sceneCapture.opacity = opacity; break;
            }
        }

        #endregion

        #region Environment Tab
        private void DrawEnvironmentTab()
        {
            DrawSection("Environment Map", () =>
            {
                bool changed = false;

                bool newEnabled = EditorGUILayout.Toggle("Enabled", settings.environment.enabled);
                if (newEnabled != settings.environment.enabled) { RecordUndo("Environment Enabled"); settings.environment.enabled = newEnabled; changed = true; }

                if (settings.environment.enabled)
                {
                    EditorGUI.indentLevel++;

                    var newUVMode = (EnvironmentUVMode)EditorGUILayout.EnumPopup("UV Mode", settings.environment.uvMode);
                    if (newUVMode != settings.environment.uvMode) { RecordUndo("Environment UV Mode"); settings.environment.uvMode = newUVMode; changed = true; }

                    EditorGUILayout.Space(5);

                    if (settings.environment.uvMode == EnvironmentUVMode.Cubemap)
                    {
                        var newCubemap = (Cubemap)EditorGUILayout.ObjectField("Cubemap", settings.environment.cubemap, typeof(Cubemap), false);
                        if (newCubemap != settings.environment.cubemap) { RecordUndo("Environment Cubemap"); settings.environment.cubemap = newCubemap; changed = true; }

                        float newBlur = EditorGUILayout.Slider("Smoothness", settings.environment.blur, 0f, 1f);
                        if (newBlur != settings.environment.blur) { RecordUndo("Environment Smoothness"); settings.environment.blur = newBlur; changed = true; }

                        float newRot = EditorGUILayout.Slider("Y Rotation", settings.environment.rotation, 0f, 360f);
                        if (newRot != settings.environment.rotation) { RecordUndo("Environment Rotation"); settings.environment.rotation = newRot; changed = true; }
                    }
                    else
                    {
                        var newEnvMap = (Texture2D)EditorGUILayout.ObjectField("Environment Map", settings.environment.envMap, typeof(Texture2D), false);
                        if (newEnvMap != settings.environment.envMap) { RecordUndo("Environment Map"); settings.environment.envMap = newEnvMap; changed = true; }

                        var newNormalMap = (Texture2D)EditorGUILayout.ObjectField("Normal Map", settings.environment.normalMap, typeof(Texture2D), false);
                        if (newNormalMap != settings.environment.normalMap) { RecordUndo("Environment Normal Map"); settings.environment.normalMap = newNormalMap; changed = true; }

                        if (settings.environment.normalMap != null)
                        {
                            EditorGUI.indentLevel++;
                            float newNormalStrength = EditorGUILayout.Slider("Normal Strength", settings.environment.normalStrength, 0f, 2f);
                            if (newNormalStrength != settings.environment.normalStrength) { RecordUndo("Normal Strength"); settings.environment.normalStrength = newNormalStrength; changed = true; }
                            EditorGUI.indentLevel--;
                        }

                        float newBorder = EditorGUILayout.Slider("Border (Scale)", settings.environment.border, 0.1f, 2f);
                        if (newBorder != settings.environment.border) { RecordUndo("Environment Border"); settings.environment.border = newBorder; changed = true; }

                        var newOffset = EditorGUILayout.Vector2Field("Offset (X/Y)", settings.environment.offset);
                        if (newOffset != settings.environment.offset) { RecordUndo("Environment Offset"); settings.environment.offset = newOffset; changed = true; }

                        var newTiling = EditorGUILayout.Vector2Field("Tiling", settings.environment.tiling);
                        if (newTiling != settings.environment.tiling) { RecordUndo("Environment Tiling"); settings.environment.tiling = newTiling; changed = true; }

                        float newBlur = EditorGUILayout.Slider("Blur", settings.environment.blur, 0f, 5f);
                        if (newBlur != settings.environment.blur) { RecordUndo("Environment Blur"); settings.environment.blur = newBlur; changed = true; }

                        float newRot = EditorGUILayout.Slider("Rotation", settings.environment.rotation, 0f, 360f);
                        if (newRot != settings.environment.rotation) { RecordUndo("Environment Rotation"); settings.environment.rotation = newRot; changed = true; }
                    }

                    float newIntensity = EditorGUILayout.Slider("Intensity", settings.environment.intensity, 0f, 3f);
                    if (newIntensity != settings.environment.intensity) { RecordUndo("Environment Intensity"); settings.environment.intensity = newIntensity; changed = true; }

                    EditorGUI.indentLevel--;
                }

                if (changed)
                    UpdatePreview();
            });

            DrawSection("Quick Generate", () =>
            {
                EditorGUILayout.LabelField("Generate procedural environment maps:", EditorStyles.miniLabel);
                EditorGUILayout.Space(3);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Studio"))
                    {
                        RecordUndo("Generate Studio Environment");
                        settings.environment.envMap = OwnTexture(MatCapTextureGenerator.GenerateStudioEnvironment(256));
                        settings.environment.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Iridescent"))
                    {
                        RecordUndo("Generate Iridescent Environment");
                        settings.environment.envMap = OwnTexture(MatCapTextureGenerator.GenerateIridescentEnvironment(256));
                        settings.environment.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Sunset"))
                    {
                        RecordUndo("Generate Sunset Environment");
                        settings.environment.envMap = OwnTexture(MatCapTextureGenerator.GenerateSunsetEnvironment(256));
                        settings.environment.enabled = true;
                        UpdatePreview();
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Gradient"))
                    {
                        RecordUndo("Generate Gradient Environment");
                        settings.environment.envMap = OwnTexture(MatCapTextureGenerator.GenerateGradientEnvironment(256,
                            new Color(0.9f, 0.9f, 0.95f), new Color(0.2f, 0.2f, 0.25f)));
                        settings.environment.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Radial"))
                    {
                        RecordUndo("Generate Radial Environment");
                        settings.environment.envMap = OwnTexture(MatCapTextureGenerator.GenerateRadialEnvironment(256,
                            new Color(0.95f, 0.95f, 1f), new Color(0.1f, 0.1f, 0.15f)));
                        settings.environment.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Generator..."))
                    {
                        { selectedTab = 6; textureWorkspaceTab = 1; }
                    }
                }
            });

            EditorGUILayout.HelpBox("Environment maps are spherically mapped onto the surface. Use for reflections or patterns.", MessageType.Info);
        }
        #endregion

        #region Noise Tab
        private void DrawTextureWorkspace()
        {
            textureWorkspaceTab = GUILayout.Toolbar(textureWorkspaceTab, new[] { "Noise Layer", "Texture Generator" }, GUILayout.Height(25));
            GUILayout.Space(10);
            if (textureWorkspaceTab == 0) DrawNoiseTab();
            else
            {
                if (textureGenerator == null) textureGenerator = new MatCapTextureGeneratorPanel(Repaint, ApplyGeneratedTexture);
                textureGenerator.DrawGUI();
            }
        }

        private void ApplyGeneratedTexture(Texture2D source, int role)
        {
            if (source == null) return;
            var texture = OwnTexture(Instantiate(source));
            texture.name = source.name;
            RecordUndo("Apply Generated Texture");
            if (role == 0)
            {
                settings.environment.envMap = texture;
                settings.environment.enabled = true;
                settings.environment.uvMode = EnvironmentUVMode.DirectNormal;
                settings.environment.blendMode = BlendMode.Normal;
            }
            else if (role == 1)
            {
                settings.baseSettings.baseTexture = texture;
                settings.baseSettings.textureStrength = .4f;
            }
            else
            {
                settings.noise.noiseTex = texture;
                settings.noise.enabled = true;
                settings.noise.opacity = .35f;
            }
            UpdatePreview();
        }

        private void DrawNoiseTab()
        {
            DrawSection("Noise Texture", () =>
            {
                bool changed = false;

                bool newEnabled = EditorGUILayout.Toggle("Enabled", settings.noise.enabled);
                if (newEnabled != settings.noise.enabled) { RecordUndo("Noise Enabled"); settings.noise.enabled = newEnabled; changed = true; }

                if (settings.noise.enabled)
                {
                    EditorGUI.indentLevel++;

                    var newTex = (Texture2D)EditorGUILayout.ObjectField("Noise Texture", settings.noise.noiseTex, typeof(Texture2D), false);
                    if (newTex != settings.noise.noiseTex) { RecordUndo("Noise Texture"); settings.noise.noiseTex = newTex; changed = true; }

                    EditorGUILayout.Space(5);

                    float newStrength = EditorGUILayout.Slider("Strength", settings.noise.strength, 0f, 2f);
                    if (newStrength != settings.noise.strength) { RecordUndo("Noise Strength"); settings.noise.strength = newStrength; changed = true; }

                    float newBlur = EditorGUILayout.Slider("Blur", settings.noise.blur, 0f, 3f);
                    if (newBlur != settings.noise.blur) { RecordUndo("Noise Blur"); settings.noise.blur = newBlur; changed = true; }

                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Tiling", EditorStyles.miniBoldLabel);
                    float newTilingX = EditorGUILayout.Slider("X", settings.noise.tiling.x, 0.1f, 10f);
                    if (newTilingX != settings.noise.tiling.x) { RecordUndo("Noise Tiling X"); settings.noise.tiling.x = newTilingX; changed = true; }

                    float newTilingY = EditorGUILayout.Slider("Y", settings.noise.tiling.y, 0.1f, 10f);
                    if (newTilingY != settings.noise.tiling.y) { RecordUndo("Noise Tiling Y"); settings.noise.tiling.y = newTilingY; changed = true; }

                    EditorGUI.indentLevel--;
                }

                if (changed)
                    UpdatePreview();
            });

            DrawSection("Quick Generate", () =>
            {
                EditorGUILayout.LabelField("Generate procedural noise textures:", EditorStyles.miniLabel);
                EditorGUILayout.Space(3);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Perlin"))
                    {
                        RecordUndo("Generate Perlin Noise");
                        settings.noise.noiseTex = OwnTexture(MatCapTextureGenerator.GeneratePerlinNoise(256, 5f));
                        settings.noise.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Fractal"))
                    {
                        RecordUndo("Generate Fractal Noise");
                        settings.noise.noiseTex = OwnTexture(MatCapTextureGenerator.GenerateFractalNoise(256, 4f, 4, 0.5f));
                        settings.noise.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Cellular"))
                    {
                        RecordUndo("Generate Cellular Noise");
                        settings.noise.noiseTex = OwnTexture(MatCapTextureGenerator.GenerateCellularNoise(256, 8));
                        settings.noise.enabled = true;
                        UpdatePreview();
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Marble"))
                    {
                        RecordUndo("Generate Marble Noise");
                        settings.noise.noiseTex = OwnTexture(MatCapTextureGenerator.GenerateMarbleNoise(256, 4f, 5f));
                        settings.noise.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Brushed"))
                    {
                        RecordUndo("Generate Brushed Metal");
                        settings.noise.noiseTex = OwnTexture(MatCapTextureGenerator.GenerateBrushedMetal(256, 1f));
                        settings.noise.enabled = true;
                        UpdatePreview();
                    }
                    if (GUILayout.Button("Generator..."))
                    {
                        { selectedTab = 6; textureWorkspaceTab = 1; }
                    }
                }
            });

            EditorGUILayout.HelpBox("Noise textures are overlaid flat (not spherically mapped). You can use any image, not just noise.", MessageType.Info);
        }
        #endregion

        #region Post-Process Tab
        private void DrawPostProcessTab()
        {
            DrawSection("Post-Processing Effects", () =>
            {
                bool changed = false;

                bool newEnabled = EditorGUILayout.Toggle("Enable Post-Processing", settings.postProcess.enabled);
                if (newEnabled != settings.postProcess.enabled) { RecordUndo("Post-Process Enabled"); settings.postProcess.enabled = newEnabled; changed = true; }

                if (settings.postProcess.enabled)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var bloom = new PostProcessSettings(settings.postProcess);
                        EditorGUI.BeginChangeCheck();
                        bloom.bloomEnabled = EditorGUILayout.Toggle("Bloom", bloom.bloomEnabled);
                        if (bloom.bloomEnabled)
                        {
                            bloom.bloomStrength = EditorGUILayout.Slider("Strength", bloom.bloomStrength, 0, 3);
                            bloom.bloomThreshold = EditorGUILayout.Slider("Threshold", bloom.bloomThreshold, 0, 1.5f);
                            bloom.bloomSpread = EditorGUILayout.Slider("Spread", bloom.bloomSpread, .5f, 20);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            RecordUndo("Bloom");
                            settings.postProcess = bloom;
                            changed = true;
                        }
                    }

                    EditorGUILayout.Space(5);

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        bool newBlurEnabled = EditorGUILayout.Toggle("Blur", settings.postProcess.blurEnabled);
                        if (newBlurEnabled != settings.postProcess.blurEnabled) { RecordUndo("PP Blur Enabled"); settings.postProcess.blurEnabled = newBlurEnabled; changed = true; }

                        if (settings.postProcess.blurEnabled)
                        {
                            EditorGUI.indentLevel++;
                            float newBlurRadius = EditorGUILayout.Slider("Radius", settings.postProcess.blurRadius, 0.1f, 10f);
                            if (newBlurRadius != settings.postProcess.blurRadius) { RecordUndo("PP Blur Radius"); settings.postProcess.blurRadius = newBlurRadius; changed = true; }
                            EditorGUI.indentLevel--;
                        }
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        bool newSharpenEnabled = EditorGUILayout.Toggle("Sharpen", settings.postProcess.sharpenEnabled);
                        if (newSharpenEnabled != settings.postProcess.sharpenEnabled) { RecordUndo("PP Sharpen Enabled"); settings.postProcess.sharpenEnabled = newSharpenEnabled; changed = true; }

                        if (settings.postProcess.sharpenEnabled)
                        {
                            EditorGUI.indentLevel++;
                            float newSharpenStrength = EditorGUILayout.Slider("Strength", settings.postProcess.sharpenStrength, 0f, 2f);
                            if (newSharpenStrength != settings.postProcess.sharpenStrength) { RecordUndo("PP Sharpen Strength"); settings.postProcess.sharpenStrength = newSharpenStrength; changed = true; }
                            EditorGUI.indentLevel--;
                        }
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        bool newCAEnabled = EditorGUILayout.Toggle("Chromatic Aberration", settings.postProcess.chromaticAberrationEnabled);
                        if (newCAEnabled != settings.postProcess.chromaticAberrationEnabled) { RecordUndo("PP Chromatic Aberration"); settings.postProcess.chromaticAberrationEnabled = newCAEnabled; changed = true; }

                        if (settings.postProcess.chromaticAberrationEnabled)
                        {
                            EditorGUI.indentLevel++;
                            float newCAIntensity = EditorGUILayout.Slider("Intensity", settings.postProcess.chromaticAberrationIntensity, 0f, 1f);
                            if (newCAIntensity != settings.postProcess.chromaticAberrationIntensity) { RecordUndo("PP CA Intensity"); settings.postProcess.chromaticAberrationIntensity = newCAIntensity; changed = true; }
                            EditorGUI.indentLevel--;
                        }
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        bool newGrainEnabled = EditorGUILayout.Toggle("Film Grain", settings.postProcess.filmGrainEnabled);
                        if (newGrainEnabled != settings.postProcess.filmGrainEnabled) { RecordUndo("PP Film Grain"); settings.postProcess.filmGrainEnabled = newGrainEnabled; changed = true; }

                        if (settings.postProcess.filmGrainEnabled)
                        {
                            EditorGUI.indentLevel++;
                            float newGrainStrength = EditorGUILayout.Slider("Strength", settings.postProcess.filmGrainStrength, 0f, 1f);
                            if (newGrainStrength != settings.postProcess.filmGrainStrength) { RecordUndo("PP Grain Strength"); settings.postProcess.filmGrainStrength = newGrainStrength; changed = true; }

                            float newGrainScale = EditorGUILayout.Slider("Scale", settings.postProcess.filmGrainScale, 0.5f, 3f);
                            if (newGrainScale != settings.postProcess.filmGrainScale) { RecordUndo("PP Grain Scale"); settings.postProcess.filmGrainScale = newGrainScale; changed = true; }
                            EditorGUI.indentLevel--;
                        }
                    }
                }

                if (changed)
                    UpdatePreview();
            });

            EditorGUILayout.HelpBox("Post-processing effects are applied after all layers are composited.", MessageType.Info);
        }
        #endregion

        #region Scene Capture Tab
        private void DrawSceneCaptureTab()
        {
            DrawSection("Scene Capture", () =>
            {
                bool changed = false;

                bool newEnabled = EditorGUILayout.Toggle("Enable Scene Capture", settings.sceneCapture.enabled);
                if (newEnabled != settings.sceneCapture.enabled) { RecordUndo("Scene Capture Enabled"); settings.sceneCapture.enabled = newEnabled; changed = true; }

                if (settings.sceneCapture.enabled)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.Space(5);

                    var newCaptureMode = (SceneCaptureMode)EditorGUILayout.EnumPopup("Capture Mode", settings.sceneCapture.captureMode);
                    if (newCaptureMode != settings.sceneCapture.captureMode) { RecordUndo("Scene Capture Mode"); settings.sceneCapture.captureMode = newCaptureMode; changed = true; }

                    EditorGUILayout.Space(5);

                    if (settings.sceneCapture.captureMode == SceneCaptureMode.ReflectionProbe)
                    {
                        EditorGUILayout.LabelField("Reflection Probe from Scene", EditorStyles.miniBoldLabel);

                        var newProbe = (ReflectionProbe)EditorGUILayout.ObjectField("Reflection Probe", selectedReflectionProbe, typeof(ReflectionProbe), true);
                        if (newProbe != selectedReflectionProbe) { RecordUndo("Scene Capture Probe"); selectedReflectionProbe = newProbe; changed = true; }

                        var probesInScene = FindObjectsOfType<ReflectionProbe>();
                        if (probesInScene.Length > 0)
                        {
                            EditorGUILayout.Space(3);
                            EditorGUILayout.LabelField($"Found {probesInScene.Length} probe(s) in scene:", EditorStyles.miniLabel);

                            using (new GUILayout.HorizontalScope())
                            {
                                foreach (var probe in probesInScene)
                                {
                                    if (GUILayout.Button(probe.name, GUILayout.MaxWidth(120)))
                                    {
                                        RecordUndo("Select Reflection Probe");
                                        selectedReflectionProbe = probe;
                                        changed = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("No Reflection Probes found in scene. Add a Reflection Probe to your scene first.", MessageType.Warning);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Material to Capture", EditorStyles.miniBoldLabel);

                        var newMaterial = (Material)EditorGUILayout.ObjectField("Material", settings.sceneCapture.captureMaterial, typeof(Material), false);
                        if (newMaterial != settings.sceneCapture.captureMaterial) { RecordUndo("Scene Capture Material"); settings.sceneCapture.captureMaterial = newMaterial; changed = true; }

                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("Lighting Options", EditorStyles.miniBoldLabel);

                        bool newUseSceneLighting = EditorGUILayout.Toggle("Use Scene Lighting", settings.sceneCapture.useSceneLighting);
                        if (newUseSceneLighting != settings.sceneCapture.useSceneLighting) { RecordUndo("Scene Capture Lighting"); settings.sceneCapture.useSceneLighting = newUseSceneLighting; changed = true; }

                        bool newUseReflectionProbe = EditorGUILayout.Toggle("Use Reflection Probes", settings.sceneCapture.useReflectionProbe);
                        if (newUseReflectionProbe != settings.sceneCapture.useReflectionProbe) { RecordUndo("Scene Capture Reflections"); settings.sceneCapture.useReflectionProbe = newUseReflectionProbe; changed = true; }
                    }

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Output Settings", EditorStyles.miniBoldLabel);

                    int newResIndex = EditorGUILayout.Popup("Resolution", captureResolutionIndex, CaptureResolutionOptions);
                    if (newResIndex != captureResolutionIndex)
                    {
                        RecordUndo("Scene Capture Resolution");
                        captureResolutionIndex = newResIndex;
                        settings.sceneCapture.captureResolution = CaptureResolutionValues[newResIndex];
                        changed = true;
                    }

                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space(10);

                    Color origBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);

                    bool canCapture = settings.sceneCapture.captureMode == SceneCaptureMode.ReflectionProbe
                        ? selectedReflectionProbe != null
                        : settings.sceneCapture.captureMaterial != null;

                    string buttonText = settings.sceneCapture.captureMode == SceneCaptureMode.ReflectionProbe
                        ? "Capture from Reflection Probe"
                        : "Capture from Material";

                    using (new EditorGUI.DisabledScope(!canCapture || pendingProbe != null))
                    {
                        if (GUILayout.Button(pendingProbe != null ? "Capturing probe..." : buttonText, GUILayout.Height(30)))
                        {
                            if (settings.sceneCapture.captureMode == SceneCaptureMode.ReflectionProbe)
                                CaptureFromReflectionProbe();
                            else
                                CaptureSceneToMatCap();
                        }
                    }

                    GUI.backgroundColor = origBg;
                }

                if (changed)
                    UpdatePreview();
            });

            if (settings.sceneCapture.captureMode == SceneCaptureMode.ReflectionProbe)
            {
                EditorGUILayout.HelpBox(
                    "Reflection Probe mode captures the cubemap from a Reflection Probe in your scene " +
                    "and converts it to a MatCap texture.\n\n" +
                    "This captures the actual reflections baked or rendered by Unity, including:\n" +
                    "- Skybox and environment\n" +
                    "- Scene geometry reflections\n" +
                    "- Real-time or baked probe data",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Material mode renders a sphere with your chosen material using Unity's actual lighting " +
                    "and reflection probes.\n\n" +
                    "For best results:\n" +
                    "- Use materials with reflections/metallic properties\n" +
                    "- Ensure reflection probes are baked in your scene",
                    MessageType.Info);
            }
        }

        private void CaptureFromReflectionProbe()
        {
            if (selectedReflectionProbe == null || pendingProbe != null) return;
            if (selectedReflectionProbe.mode != UnityEngine.Rendering.ReflectionProbeMode.Realtime)
            {
                ApplyProbeTexture(selectedReflectionProbe.texture);
                return;
            }
            pendingProbe = selectedReflectionProbe;
            pendingProbeRenderId = pendingProbe.RenderProbe();
            probeDeadline = EditorApplication.timeSinceStartup + 30;
            EditorApplication.update += PollProbeCapture;
            Repaint();
        }

        private void PollProbeCapture()
        {
            if (pendingProbe != null && !pendingProbe.IsFinishedRendering(pendingProbeRenderId)
                && EditorApplication.timeSinceStartup < probeDeadline) return;
            EditorApplication.update -= PollProbeCapture;
            var probe = pendingProbe;
            pendingProbe = null;
            try
            {
                if (probe == null || !probe.IsFinishedRendering(pendingProbeRenderId))
                    Debug.LogWarning("[MatCap Maker] Probe capture cancelled or timed out. Try capturing again.");
                else ApplyProbeTexture(probe.texture);
            }
            catch (Exception error) { Debug.LogException(error); }
            finally { Repaint(); }
        }

        private void ApplyProbeTexture(Texture texture)
        {
            var cube = texture as Cubemap;
            var rt = texture as RenderTexture;
            if (cube == null && rt != null && rt.dimension == UnityEngine.Rendering.TextureDimension.Cube)
            {
                var previous = RenderTexture.active;
                Texture2D faceTexture = null;
                Cubemap copy = null;
                try
                {
                    copy = new Cubemap(rt.width, TextureFormat.RGBAHalf, false) { name = "Captured Probe" };
                    faceTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false, true);
                    for (int face = 0; face < 6; face++)
                    {
                        Graphics.SetRenderTarget(rt, 0, (CubemapFace)face);
                        faceTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                        faceTexture.Apply();
                        copy.SetPixels(faceTexture.GetPixels(), (CubemapFace)face);
                    }
                    copy.Apply();
                    cube = OwnTexture(copy);
                    copy = null;
                }
                finally
                {
                    RenderTexture.active = previous;
                    if (faceTexture != null) DestroyImmediate(faceTexture);
                    if (copy != null) DestroyImmediate(copy);
                }
            }
            if (cube == null)
            {
                Debug.LogWarning("[MatCap Maker] This probe has no cubemap. Bake it or use a realtime probe.");
                return;
            }
            RecordUndo("Capture Probe");
            settings.environment.cubemap = cube;
            settings.environment.uvMode = EnvironmentUVMode.Cubemap;
            settings.environment.enabled = true;
            settings.environment.intensity = 1f;
            settings.environment.blur = 0f;
            UpdatePreview();
        }

        private void CaptureSceneToMatCap()
        {
            if (settings.sceneCapture.captureMaterial == null) return;
            var previous = RenderTexture.active;
            PreviewRenderUtility preview = null;
            RenderTexture target = null;
            Texture2D captured = null;
            bool previewOpened = false;
            try
            {
                int size = settings.sceneCapture.captureResolution;
                if (size < 16 || size > 4096) throw new ArgumentOutOfRangeException("captureResolution");
                preview = new PreviewRenderUtility();
                preview.ambientColor = settings.sceneCapture.useSceneLighting ? RenderSettings.ambientLight : new Color(.12f,.12f,.12f);
                previewOpened = true;
                preview.BeginPreview(new Rect(0, 0, size, size), GUIStyle.none);
                var camera = preview.camera;
                camera.transform.position = new Vector3(0, 0, -2f);
                camera.transform.rotation = Quaternion.identity;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = settings.sceneCapture.backgroundColor;
                camera.orthographic = true;
                camera.orthographicSize = 1.1f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 10f;
                camera.cullingMask = 1 << 31;
                foreach (var light in preview.lights) light.enabled = false;
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                preview.AddSingleGO(sphere);
                sphere.transform.position = Vector3.zero;
                sphere.transform.localScale = Vector3.one * 2f;
                sphere.layer = 31;
                DestroyImmediate(sphere.GetComponent<Collider>());
                var renderer = sphere.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = settings.sceneCapture.captureMaterial;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                if (settings.sceneCapture.useReflectionProbe)
                {
                    foreach (var source in FindObjectsOfType<ReflectionProbe>())
                    {
                        if (source.texture == null) continue;
                        var go = new GameObject("Capture Reflection");
                        preview.AddSingleGO(go);
                        go.transform.position = source.transform.position;
                        var probe = go.AddComponent<ReflectionProbe>();
                        probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
                        probe.customBakedTexture = source.texture;
                        probe.size = Vector3.one * 10000;
                        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple;
                        break;
                    }
                }
                int lightCount = 0;
                if (settings.sceneCapture.useSceneLighting)
                {
                    foreach (var source in FindObjectsOfType<Light>())
                    {
                        if (!source.isActiveAndEnabled || !source.gameObject.scene.IsValid()
                            || UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(source.gameObject.scene)) continue;
                        var go = new GameObject("Capture Light");
                        preview.AddSingleGO(go);
                        go.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                        var light = go.AddComponent<Light>();
                        light.type = source.type;
                        light.color = source.color;
                        light.intensity = source.intensity;
                        light.range = source.range;
                        light.spotAngle = source.spotAngle;
                        light.innerSpotAngle = source.innerSpotAngle;
                        light.cookie = source.cookie;
                        light.cookieSize = source.cookieSize;
                        light.useColorTemperature = source.useColorTemperature;
                        light.colorTemperature = source.colorTemperature;
                        light.cullingMask = 1 << 31;
                        lightCount++;
                    }
                }
                if (lightCount == 0)
                {
                    preview.lights[0].enabled = true;
                    preview.lights[0].intensity = 1f;
                    preview.lights[0].transform.rotation = Quaternion.Euler(35, -35, 0);
                    preview.lights[0].cullingMask = 1 << 31;
                }
                preview.Render();
                target = preview.EndPreview() as RenderTexture;
                previewOpened = false;
                if (target == null) throw new InvalidOperationException("Capture returned no texture.");
                size = target.width;
                RenderTexture.active = target;
                captured = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Captured Material", wrapMode = TextureWrapMode.Clamp };
                captured.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                captured.Apply();
                RecordUndo("Capture Material");
                settings.sceneCapture.capturedTexture = OwnTexture(captured);
                captured = null;
                UpdatePreview();
            }
            finally
            {
                try
                {
                    if (preview != null)
                    {
                        try { if (previewOpened) preview.EndPreview(); }
                        finally { RenderTexture.active = previous; preview.Cleanup(); }
                    }
                }
                finally
                {
                    RenderTexture.active = previous;
                    if (captured != null) DestroyImmediate(captured);
                }

            }
        }
        #endregion

        #region Preview Section
        private void DrawPreview()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("LIVE PREVIEW", EditorStyles.miniBoldLabel);
                Rect rect = GUILayoutUtility.GetAspectRect(1f);
                EditorGUI.DrawRect(rect, new Color(.055f,.06f,.075f));
                if (previewRT != null) EditorGUI.DrawPreviewTexture(rect, previewRT, null, ScaleMode.ScaleToFit);
            }
        }
        #endregion

        #region Preset Section
        private void DrawPresetGallery()
        {
            GUILayout.Label("Presets", sectionHeaderStyle);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Preset")) SavePreset();
                if (GUILayout.Button("Load Preset")) LoadPreset();
                if (GUILayout.Button("Reset"))
                {
                    RecordUndo("Reset Settings");
                    settingsWrapper.settings = BuiltInPresets.GetDefault();
                    selectedBuiltInPreset = 0;
                    ResetLayerOrder();
                    UpdatePreview();
                }
            }
            GUILayout.Space(8);
            if (presetThumbnails == null || Array.Exists(presetThumbnails, texture => texture == null)) BuildPresetThumbnails();
            var names = BuiltInPresets.GetPresetNames();
            DrawPresetGrid(names, 0, names.Length);
            GUILayout.Space(12);
        }

        private void DrawPresetGrid(string[] names, int start, int end)
        {
            int columns = position.width >= 1100 ? 3 : 2;
            float tileWidth = (position.width - Mathf.Clamp(position.width * .39f, 290, 480) - 90) / columns;
            var style = new GUIStyle(GUI.skin.button) { imagePosition = ImagePosition.ImageAbove, wordWrap = true, fontSize = 11 };
            for (int row = start; row < end; row += columns)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int column = 0; column < columns; column++)
                    {
                        int index = row + column;
                        if (index >= end) { GUILayout.Space(tileWidth); continue; }
                        var color = GUI.backgroundColor;
                        if (selectedBuiltInPreset == index) GUI.backgroundColor = AccentColor;
                        bool clicked = GUILayout.Button(new GUIContent(names[index], presetThumbnails[index]), style, GUILayout.Width(tileWidth), GUILayout.Height(122));
                        GUI.backgroundColor = color;
                        if (clicked)
                        {
                            RecordUndo("Apply Preset");
                            selectedBuiltInPreset = index;
                            ApplyPresetWithTextures(index);
                            ResetLayerOrder();
                            UpdatePreview();
                        }
                    }
                }
            }
        }

        private void BuildPresetThumbnails()
        {
            var randomState = UnityEngine.Random.state;
            var saved = settingsWrapper.settings;
            var savedOrder = layerOrder;
            var previous = RenderTexture.active;
            var layers = new RenderTexture[8];
            RenderTexture final = null;
            try
            {
                presetThumbnails = new Texture2D[BuiltInPresets.GetPresetNames().Length];
                for (int i = 0; i < layers.Length; i++) { layers[i] = new RenderTexture(96,96,0); layers[i].Create(); }
                final = new RenderTexture(96,96,0); final.Create();
                layerOrder = new[] { 1,2,3,4,5,6,7 };
                for (int i = 0; i < presetThumbnails.Length; i++)
                {
                    settingsWrapper.settings = BuiltInPresets.GetDefault();
                    ApplyPresetWithTextures(i);
                    RenderAllLayers(layers,96);
                    CompositeToFinal(layers,final);
                    if (settings.postProcess.enabled) ApplyPostProcessing(final,96);
                    RenderTexture.active = final;
                    var texture = OwnTexture(new Texture2D(96,96,TextureFormat.RGBA32,false));
                    texture.ReadPixels(new Rect(0,0,96,96),0,0); texture.Apply();
                    presetThumbnails[i] = texture;
                }
            }
            finally
            {
                settingsWrapper.settings = saved;
                layerOrder = savedOrder;
                UnityEngine.Random.state = randomState;
                RenderTexture.active = previous;
                foreach (var layer in layers) if (layer != null) { layer.Release(); DestroyImmediate(layer); }
                if (final != null) { final.Release(); DestroyImmediate(final); }
            }
        }

        private void ApplyPresetWithTextures(int presetIndex)
        {
            settings.CopyFrom(BuiltInPresets.GetPresetByIndex(presetIndex));

            BuiltInPresets.GetPresetTextureRequirements(presetIndex, out bool needsEnvMap, out bool needsNoise, out bool needsBaseTexture);

            if (needsEnvMap)
            {
                settings.environment.envMap = OwnTexture(MatCapTextureGenerator.GenerateIridescentEnvironment(256));
            }

            if (needsNoise)
            {
                settings.noise.noiseTex = OwnTexture(MatCapTextureGenerator.GenerateGrainNoise(256));
            }

            if (needsBaseTexture)
            {
                settings.baseSettings.baseTexture = OwnTexture(MatCapTextureGenerator.GenerateMarbleNoise(256, 4f, 5f));
            }
        }

        private void SavePreset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save MatCap Preset", "MatCapPreset", "asset",
                "Specify a filename for the preset.");

            if (string.IsNullOrEmpty(path)) return;

            SavePresetAsset(path);

            Debug.Log($"<color=#5ef02f>[MatCap Maker]</color> Preset saved: {path}");
        }

        private void LoadPreset()
        {
            string path = EditorUtility.OpenFilePanel("Load MatCap Preset", "Assets", "asset");

            if (string.IsNullOrEmpty(path)) return;

            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var preset = AssetDatabase.LoadAssetAtPath<MatCapMakerPreset>(path);

            if (preset == null)
            {
                Debug.LogWarning("[MatCap Maker] Could not load preset from: " + path);
                return;
            }

            RecordUndo("Load Preset");
            preset.ApplyToSettings(settings);
            selectedBuiltInPreset = -1;
            layerOrder = preset.layerOrder != null && preset.layerOrder.Length == 7 ? (int[])preset.layerOrder.Clone() : new[] { 1, 2, 3, 4, 5, 6, 7 };
            UpdatePreview();

            Debug.Log($"<color=#5ef02f>[MatCap Maker]</color> Preset loaded: {path}");
        }
        #endregion

        #region Export Section
        private void DrawExportSection()
        {
            EditorGUILayout.Space(5);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("<b>Export</b>", sectionHeaderStyle);

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Resolution", GUILayout.Width(70));
                    resolutionIndex = EditorGUILayout.Popup(resolutionIndex, ResolutionOptions);
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Format", GUILayout.Width(70));
                    exportFormatIndex = EditorGUILayout.Popup(exportFormatIndex, ExportFormatOptions);
                }

                using (new GUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.TextField("Save To", savePath);

                    if (GUILayout.Button("...", GUILayout.Width(30)))
                    {
                        string newPath = EditorUtility.OpenFolderPanel("Select Save Location", savePath, "");
                        if (!string.IsNullOrEmpty(newPath) && newPath.Contains("Assets"))
                        {
                            savePath = FileUtil.GetProjectRelativePath(newPath);
                            EditorPrefs.SetString(PREFS_KEY_SAVE_PATH, savePath);
                        }
                    }
                }

                EditorGUILayout.Space(5);

                Color origBg = GUI.backgroundColor;
                GUI.backgroundColor = AccentColor;

                if (GUILayout.Button("Export MatCap", GUILayout.Height(35)))
                {
                    ExportMatCap();
                }

                GUI.backgroundColor = origBg;
            }
        }

        private void ExportMatCap()
        {
            var previousTarget = RenderTexture.active;
            RenderTexture exportRT = null;
            var exportLayerRTs = new RenderTexture[8];
            Texture2D exportTex = null;
            try
            {
                int exportRes = ResolutionValues[resolutionIndex];
                bool isHDR = exportFormatIndex == 1;

                RenderTextureFormat rtFormat = isHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
                exportRT = new RenderTexture(exportRes, exportRes, 0, rtFormat);
                exportRT.Create();

                for (int i = 0; i < 8; i++)
                {
                    exportLayerRTs[i] = new RenderTexture(exportRes, exportRes, 0, rtFormat);
                    exportLayerRTs[i].Create();
                }

                RenderAllLayers(exportLayerRTs, exportRes);
                CompositeToFinal(exportLayerRTs, exportRT);

                if (settings.postProcess.enabled)
                {
                    ApplyPostProcessing(exportRT, exportRes);
                }

                RenderTexture.active = exportRT;
                TextureFormat texFormat = isHDR ? TextureFormat.RGBAHalf : TextureFormat.RGBA32;
                exportTex = new Texture2D(exportRes, exportRes, texFormat, false);
                exportTex.ReadPixels(new Rect(0, 0, exportRes, exportRes), 0, 0);
                exportTex.Apply();
                RenderTexture.active = null;

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                string fileName = $"MatCap_{DateTime.Now:yyyyMMdd_HHmmss}";
                string extension = isHDR ? "exr" : "png";
                string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/{fileName}.{extension}");

                byte[] data;
                if (isHDR)
                {
                    data = exportTex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                }
                else
                {
                    data = exportTex.EncodeToPNG();
                }
                File.WriteAllBytes(fullPath, data);

                AssetDatabase.ImportAsset(fullPath);

                TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.sRGBTexture = !isHDR;
                    importer.maxTextureSize = exportRes;
                    importer.mipmapEnabled = true;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                }

                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath));

                Debug.Log($"<color=#5ef02f>[MatCap Maker]</color> MatCap exported to: {fullPath}");

            }
            finally
            {
                RenderTexture.active = previousTarget;
                if (exportTex != null) DestroyImmediate(exportTex);
                if (exportRT != null) { exportRT.Release(); DestroyImmediate(exportRT); }
                foreach (var layer in exportLayerRTs)
                    if (layer != null) { layer.Release(); DestroyImmediate(layer); }
            }
        }
        #endregion

        #region Rendering
        private void UpdatePreview()
        {
            var previousTarget = RenderTexture.active;
            try
            {
                if (previewRT == null || layerRTs == null) return;

                RenderAllLayers(layerRTs, previewSize);
                CompositeToFinal(layerRTs, previewRT);

                if (settings.postProcess.enabled)
                {
                    ApplyPostProcessing(previewRT, previewSize);
                }

                UpdateLayerThumbnails();

                Repaint();

            }
            finally
            {
                RenderTexture.active = previousTarget;
            }
        }

        private void UpdateLayerThumbnails()
        {
            if (layerThumbnails == null || layerRTs == null) return;

            for (int i = 0; i < 8; i++)
            {
                if (layerThumbnails[i] != null && layerRTs[i] != null)
                {
                    Graphics.Blit(layerRTs[i], layerThumbnails[i]);
                }
            }
        }

        private void ApplyPostProcessing(RenderTexture target, int size)
        {
            var previousTarget = RenderTexture.active;
            RenderTexture temp1 = null, temp2 = null, bloomA = null, bloomB = null;
            try
            {
                if (postProcessMat == null) return;

                temp1 = RenderTexture.GetTemporary(size, size, 0, target.format);
                temp2 = RenderTexture.GetTemporary(size, size, 0, target.format);

                Graphics.Blit(target, temp1);

                if (settings.postProcess.blurEnabled && settings.postProcess.blurRadius > 0.1f)
                {
                    postProcessMat.SetFloat("_BlurRadius", settings.postProcess.blurRadius);

                    Graphics.Blit(temp1, temp2, postProcessMat, 0);
                    Graphics.Blit(temp2, temp1, postProcessMat, 1);
                }

                if (settings.postProcess.sharpenEnabled && settings.postProcess.sharpenStrength > 0.01f)
                {
                    postProcessMat.SetFloat("_SharpenStrength", settings.postProcess.sharpenStrength);
                    Graphics.Blit(temp1, temp2, postProcessMat, 2);
                    Graphics.Blit(temp2, temp1);
                }

                if (settings.postProcess.chromaticAberrationEnabled && settings.postProcess.chromaticAberrationIntensity > 0.01f)
                {
                    postProcessMat.SetFloat("_ChromaticIntensity", settings.postProcess.chromaticAberrationIntensity);
                    Graphics.Blit(temp1, temp2, postProcessMat, 3);
                    Graphics.Blit(temp2, temp1);
                }

                if (settings.postProcess.bloomEnabled && settings.postProcess.bloomStrength > 0)
                {
                    int bloomSize = Mathf.Max(32, size / 2);
                    bloomA = RenderTexture.GetTemporary(bloomSize, bloomSize, 0, RenderTextureFormat.ARGBHalf);
                    bloomB = RenderTexture.GetTemporary(bloomSize, bloomSize, 0, RenderTextureFormat.ARGBHalf);
                    bloomA.filterMode = bloomB.filterMode = FilterMode.Bilinear;
                    bloomA.wrapMode = bloomB.wrapMode = TextureWrapMode.Clamp;
                    postProcessMat.SetFloat("_BloomThreshold", settings.postProcess.bloomThreshold);
                    postProcessMat.SetFloat("_BloomStrength", settings.postProcess.bloomStrength);
                    Graphics.Blit(temp1, bloomA, postProcessMat, 5);
                    for (int iteration = 0; iteration < 3; iteration++)
                    {
                        postProcessMat.SetFloat("_BlurRadius", settings.postProcess.bloomSpread * bloomSize / 512f * (1 + iteration * .5f));
                        Graphics.Blit(bloomA, bloomB, postProcessMat, 0);
                        Graphics.Blit(bloomB, bloomA, postProcessMat, 1);
                    }
                    postProcessMat.SetTexture("_BloomTex", bloomA);
                    Graphics.Blit(temp1, temp2, postProcessMat, 6);
                    Graphics.Blit(temp2, temp1);
                }

                if (settings.postProcess.filmGrainEnabled && settings.postProcess.filmGrainStrength > 0.01f)
                {
                    postProcessMat.SetFloat("_GrainStrength", settings.postProcess.filmGrainStrength);
                    postProcessMat.SetFloat("_GrainScale", settings.postProcess.filmGrainScale);
                    postProcessMat.SetFloat("_GrainTime", (float)EditorApplication.timeSinceStartup);
                    Graphics.Blit(temp1, temp2, postProcessMat, 4);
                    Graphics.Blit(temp2, temp1);
                }

                Graphics.Blit(temp1, target);

            }
            finally
            {
                RenderTexture.active = previousTarget;
                if (temp1 != null) RenderTexture.ReleaseTemporary(temp1);
                if (temp2 != null) RenderTexture.ReleaseTemporary(temp2);
                if (postProcessMat != null) postProcessMat.SetTexture("_BloomTex", null);
                if (bloomA != null) RenderTexture.ReleaseTemporary(bloomA);
                if (bloomB != null) RenderTexture.ReleaseTemporary(bloomB);
            }
        }

        private void RenderAllLayers(RenderTexture[] targetLayers, int size)
        {
            var previousTarget = RenderTexture.active;
            try
            {
                for (int i = 0; i < targetLayers.Length; i++)
                {
                    RenderTexture.active = targetLayers[i];
                    GL.Clear(true, true, Color.clear);
                }

                RenderBaseLayer(targetLayers[0]);

                if (settings.lighting.enabled)
                    RenderLightingLayer(targetLayers[1], size);

                if (settings.fresnel.enabled)
                    RenderFresnelLayer(targetLayers[2]);

                if (settings.rim.enabled)
                    RenderRimLayer(targetLayers[3]);

                if (settings.reflections.enabled)
                    RenderReflectionsLayer(targetLayers[4]);

                bool hasEnvTexture = settings.environment.uvMode == EnvironmentUVMode.Cubemap
                    ? settings.environment.cubemap != null
                    : settings.environment.envMap != null;
                if (settings.environment.enabled && hasEnvTexture)
                    RenderEnvironmentLayer(targetLayers[5]);

                if (settings.noise.enabled && settings.noise.noiseTex != null)
                    RenderNoiseLayer(targetLayers[6]);

                if (settings.sceneCapture.enabled && settings.sceneCapture.captureMode == SceneCaptureMode.Material
                    && settings.sceneCapture.capturedTexture != null)
                    Graphics.Blit(settings.sceneCapture.capturedTexture, targetLayers[7]);
                RenderTexture.active = null;

            }
            finally
            {
                RenderTexture.active = previousTarget;
            }
        }

        private void RenderBaseLayer(RenderTexture target)
        {
            if (baseMat == null) return;

            baseMat.SetColor("_BaseColor", settings.baseSettings.baseColor);
            baseMat.SetFloat("_Metallic", settings.baseSettings.metallic);
            baseMat.SetFloat("_Roughness", settings.baseSettings.roughness);

            if (settings.baseSettings.baseTexture != null)
            {
                baseMat.SetTexture("_BaseTex", settings.baseSettings.baseTexture);
                baseMat.SetFloat("_UseTexture", 1f);
                baseMat.SetFloat("_TextureStrength", settings.baseSettings.textureStrength);
                baseMat.SetInt("_TextureBlendMode", (int)settings.baseSettings.textureBlendMode);
            }
            else
            {
                baseMat.SetFloat("_UseTexture", 0f);
            }

            Graphics.Blit(null, target, baseMat);
        }

        private void RenderLightingLayer(RenderTexture target, int size)
        {
            var previous = RenderTexture.active;
            RenderTexture accumulator = null, singleLight = null, next = null;
            try
            {
                accumulator = RenderTexture.GetTemporary(size,size,0,target.format);
                RenderTexture.active = accumulator;
                GL.Clear(true,true,Color.clear);
                foreach (var light in settings.lighting.lights)
                {
                    if (!light.enabled) continue;
                    singleLight = RenderTexture.GetTemporary(size,size,0,target.format);
                    spotLightMat.SetVector("_LightPos",new Vector4(light.position.x,light.position.y,light.position.z,light.intensity));
                    spotLightMat.SetColor("_LightColor",light.color);
                    spotLightMat.SetFloat("_Range",light.range);
                    spotLightMat.SetFloat("_Blur",light.blur);
                    spotLightMat.SetFloat("_SpecularEnabled",light.specularEnabled ? 1 : 0);
                    spotLightMat.SetFloat("_SpecularIntensity",light.specularIntensity);
                    spotLightMat.SetFloat("_SpecularSize",light.specularSize);
                    Graphics.Blit(null,singleLight,spotLightMat);
                    blendMat.SetTexture("_BaseTex",accumulator);
                    blendMat.SetTexture("_BlendTex",singleLight);
                    blendMat.SetInt("_BlendMode",1);
                    blendMat.SetFloat("_Opacity",1);
                    next = RenderTexture.GetTemporary(size,size,0,target.format);
                    Graphics.Blit(accumulator,next,blendMat);
                    RenderTexture.ReleaseTemporary(singleLight); singleLight = null;
                    RenderTexture.ReleaseTemporary(accumulator);
                    accumulator = next; next = null;
                }
                Graphics.Blit(accumulator,target);
            }
            finally
            {
                RenderTexture.active = previous;
                if (next != null) RenderTexture.ReleaseTemporary(next);
                if (singleLight != null) RenderTexture.ReleaseTemporary(singleLight);
                if (accumulator != null) RenderTexture.ReleaseTemporary(accumulator);
            }
        }

        private void RenderFresnelLayer(RenderTexture target)
        {
            if (fresnelMat == null) return;

            fresnelMat.SetFloat("_FresnelPower", settings.fresnel.power);
            fresnelMat.SetColor("_FresnelColor", settings.fresnel.color);
            fresnelMat.SetFloat("_Intensity", settings.fresnel.intensity);

            Graphics.Blit(null, target, fresnelMat);
        }

        private void RenderRimLayer(RenderTexture target)
        {
            if (rimMat == null) return;

            rimMat.SetFloat("_RimLightIntensity", settings.rim.rimLightIntensity);
            rimMat.SetFloat("_RimLightBlur", settings.rim.rimLightBlur);
            rimMat.SetColor("_RimLightColor", settings.rim.rimLightColor);
            rimMat.SetFloat("_RimShadeIntensity", settings.rim.rimShadeIntensity);
            rimMat.SetFloat("_RimShadeBlur", settings.rim.rimShadeBlur);
            rimMat.SetColor("_RimShadeColor", settings.rim.rimShadeColor);
            rimMat.SetFloat("_Emissive", settings.rim.emissive);

            Graphics.Blit(null, target, rimMat);
        }

        private void RenderReflectionsLayer(RenderTexture target)
        {
            if (reflectionMat == null) return;

            reflectionMat.SetTexture("_StudioBaseTex", settings.baseSettings.baseTexture != null ? settings.baseSettings.baseTexture : Texture2D.whiteTexture);
            reflectionMat.SetFloat("_StudioTextureStrength", settings.baseSettings.baseTexture != null ? settings.baseSettings.textureStrength : 0);
            var studio = settings.reflections.studio;
            reflectionMat.SetFloat("_StudioEnabled", studio.enabled ? 1 : 0);
            reflectionMat.SetColor("_MetalColor", studio.metalColor);
            reflectionMat.SetColor("_ShadowColor", studio.shadowColor);
            reflectionMat.SetVector("_Surface", new Vector4(studio.softness, studio.distortion, studio.grain, studio.iridescence));
            reflectionMat.SetVector("_Pattern", new Vector4((float)studio.pattern, studio.patternScale, studio.patternStrength, studio.patternDetail));
            reflectionMat.SetFloat("_PatternSeed", studio.patternSeed);
            reflectionMat.SetVector("_PatternControl", new Vector4(studio.patternWarp, studio.patternGlow, studio.patternRotation * Mathf.Deg2Rad, studio.patternContrast));
            reflectionMat.SetFloat("_PatternWrap", studio.wrapPattern ? 1 : 0);
            reflectionMat.SetColor("_PatternColor", studio.patternColor);
            reflectionMat.SetVector("_Studio", new Vector4(studio.rotation * Mathf.Deg2Rad, studio.satin, 0, 0));
            for (int i = 0; i < 3; i++)
            {
                var panel = studio.panels[i];
                reflectionMat.SetVector("_Panel" + i, new Vector4(panel.direction.x, panel.direction.y, panel.direction.z, panel.enabled ? panel.intensity : 0));
                reflectionMat.SetVector("_PanelSize" + i, new Vector4(panel.size.x, panel.size.y, panel.rotation * Mathf.Deg2Rad, (float)panel.shape));
                reflectionMat.SetColor("_PanelColor" + i, panel.color);
            }
            reflectionMat.SetFloat("_Smoothness", settings.reflections.smoothness);
            reflectionMat.SetFloat("_Intensity", settings.reflections.intensity);
            reflectionMat.SetColor("_ReflectionColor", settings.reflections.reflectionColor);
            reflectionMat.SetFloat("_FresnelBias", settings.reflections.fresnelBias);
            reflectionMat.SetFloat("_FresnelPower", settings.reflections.fresnelPower);
            reflectionMat.SetColor("_HorizonColor", settings.reflections.horizonColor);
            reflectionMat.SetColor("_GroundColor", settings.reflections.groundColor);

            Graphics.Blit(null, target, reflectionMat);
        }

        private void RenderEnvironmentLayer(RenderTexture target)
        {
            if (settings.environment.uvMode == EnvironmentUVMode.Cubemap)
            {
                if (envCubemapMat == null || settings.environment.cubemap == null) return;

                envCubemapMat.SetTexture("_CubeMap", settings.environment.cubemap);
                envCubemapMat.SetFloat("_Intensity", settings.environment.intensity);
                envCubemapMat.SetFloat("_Blur", settings.environment.blur); 
                envCubemapMat.SetVector("_Rotation", new Vector4(0, settings.environment.rotation, 0, 0));

                Graphics.Blit(null, target, envCubemapMat);
            }
            else
            {
                if (envMat == null || settings.environment.envMap == null) return;

                envMat.SetTexture("_EnvMap", settings.environment.envMap);
                envMat.SetFloat("_Intensity", settings.environment.intensity);
                envMat.SetFloat("_Blur", settings.environment.blur);
                envMat.SetFloat("_Rotation", settings.environment.rotation);
                envMat.SetInt("_UVMode", (int)settings.environment.uvMode);
                envMat.SetFloat("_Border", settings.environment.border);
                envMat.SetFloat("_OffsetX", settings.environment.offset.x);
                envMat.SetFloat("_OffsetY", settings.environment.offset.y);
                envMat.SetFloat("_TilingX", settings.environment.tiling.x);
                envMat.SetFloat("_TilingY", settings.environment.tiling.y);

                if (settings.environment.normalMap != null)
                {
                    envMat.SetTexture("_NormalMap", settings.environment.normalMap);
                    envMat.SetFloat("_UseNormalMap", 1f);
                    envMat.SetFloat("_NormalStrength", settings.environment.normalStrength);
                }
                else
                {
                    envMat.SetFloat("_UseNormalMap", 0f);
                }

                Graphics.Blit(null, target, envMat);
            }
        }

        private void RenderNoiseLayer(RenderTexture target)
        {
            if (noiseMat == null || settings.noise.noiseTex == null) return;

            noiseMat.SetTexture("_NoiseTex", settings.noise.noiseTex);
            noiseMat.SetFloat("_Strength", settings.noise.strength);
            noiseMat.SetFloat("_Blur", settings.noise.blur);
            noiseMat.SetFloat("_TilingX", settings.noise.tiling.x);
            noiseMat.SetFloat("_TilingY", settings.noise.tiling.y);

            Graphics.Blit(null, target, noiseMat);
        }

        private void CompositeToFinal(RenderTexture[] layers, RenderTexture final)
        {
            var previous = RenderTexture.active;
            var result = RenderTexture.GetTemporary(final.width, final.height, 0, final.format);
            try
            {
                Graphics.Blit(layers[0], result);
                foreach (int index in layerOrder)
                {
                    if (ShouldCompositeLayer(index))
                        result = BlendLayer(result, layers[index], GetLayerBlendMode(index), GetLayerOpacity(index));
                }
                Graphics.Blit(result, final);
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(result); }
        }

        private bool ShouldCompositeLayer(int layerIndex)
        {
            switch (layerIndex)
            {
                case 1: return settings.lighting.enabled;
                case 2: return settings.fresnel.enabled;
                case 3: return settings.rim.enabled;
                case 4: return settings.reflections.enabled;
                case 5:
                    bool hasEnvTexture = settings.environment.uvMode == EnvironmentUVMode.Cubemap
                        ? settings.environment.cubemap != null
                        : settings.environment.envMap != null;
                    return settings.environment.enabled && hasEnvTexture;
                case 6: return settings.noise.enabled && settings.noise.noiseTex != null;
                case 7: return settings.sceneCapture.enabled && settings.sceneCapture.captureMode == SceneCaptureMode.Material && settings.sceneCapture.capturedTexture != null;
                default: return false;
            }
        }

        private RenderTexture BlendLayer(RenderTexture baseRT, RenderTexture blendRT, BlendMode mode, float opacity)
        {
            blendMat.SetTexture("_BaseTex", baseRT);
            blendMat.SetTexture("_BlendTex", blendRT);
            blendMat.SetInt("_BlendMode", (int)mode);
            blendMat.SetFloat("_Opacity", opacity);
            var result = RenderTexture.GetTemporary(baseRT.width, baseRT.height, 0, baseRT.format);
            try { Graphics.Blit(baseRT, result, blendMat); }
            catch { RenderTexture.ReleaseTemporary(result); throw; }
            RenderTexture.ReleaseTemporary(baseRT);
            return result;
        }
        #endregion

        #region Helper Methods
        private void DrawSection(string title, Action content)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"<b>{title}</b>", sectionHeaderStyle);
                EditorGUILayout.Space(2);
                content?.Invoke();
            }
            EditorGUILayout.Space(5);
        }

        private T OwnTexture<T>(T texture) where T : Texture
        {
            ownedTextures.Add(texture);
            return texture;
        }

        private void SavePresetAsset(string path)
        {
            var preset = CreateInstance<MatCapMakerPreset>();
            try
            {
                preset.CopyFromSettings(settings);
                preset.layerOrder = (int[])layerOrder.Clone();
                AssetDatabase.CreateAsset(preset, path);
                var copies = new System.Collections.Generic.Dictionary<Texture, Texture>();
                foreach (var field in typeof(MatCapMakerPreset).GetFields())
                {
                    if (!typeof(Texture).IsAssignableFrom(field.FieldType)) continue;
                    var texture = field.GetValue(preset) as Texture;
                    if (texture == null || AssetDatabase.Contains(texture)) continue;
                    Texture copy;
                    if (!copies.TryGetValue(texture, out copy))
                    {
                        copy = Instantiate(texture);
                        copy.name = texture.name;
                        copy.hideFlags = HideFlags.None;
                        AssetDatabase.AddObjectToAsset(copy, preset);
                        copies.Add(texture, copy);
                    }
                    field.SetValue(preset, copy);
                }
                EditorUtility.SetDirty(preset);
                AssetDatabase.SaveAssetIfDirty(preset);
            }
            catch
            {
                if (AssetDatabase.Contains(preset)) AssetDatabase.DeleteAsset(path);
                else DestroyImmediate(preset);
                throw;
            }
        }

        private void RecordUndo(string actionName)
        {
            if (settingsWrapper != null)
            {
                Undo.RecordObject(settingsWrapper, "MatCap: " + actionName);
            }
        }
        #endregion
    }
}

