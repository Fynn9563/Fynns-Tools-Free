using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsToolkit
{
    public class Texture2Normalmap : ScriptableObject
    {
        #region Private Variables
        [SerializeField] private Texture2D inputTexture;
        [SerializeField] private string outputName = "";
        [SerializeField] private float     heightScale         = 1.0f;
        public enum      Channel { R, G, B, A, Grayscale }
        [SerializeField] private Channel   selectedChannel     = Channel.Grayscale;
        [SerializeField] private bool      invertX             = false;
        [SerializeField] private bool      invertY             = false;
        public enum      Algorithm { CentralDifference, Sobel, Prewitt, Scharr, Laplacian, Roberts, Kirsch, FreiChen, MultiScaleEdgeDetection, LocalContrastEnhancement, CannyEdgeDetection, BilateralFilter, NonLocalMeans }
        [SerializeField] private Algorithm conversionAlgorithm = Algorithm.CentralDifference;

        [SerializeField] private bool  showAdvancedParameters = false;
        [SerializeField] private float sobelInnerWeight       = 2f;
        [SerializeField] private float sobelOuterWeight       = 1f;
        [SerializeField] private float prewittWeight          = 1f;
        [SerializeField] private float scharrLowWeight        = 3f;
        [SerializeField] private float scharrHighWeight       = 10f;
        [SerializeField] private float laplacianCenterWeight  = 4f;
        [SerializeField] private float laplacianNeighborWeight= 1f;
        [SerializeField] private float robertsScale           = 1f;
        [SerializeField] private float kirschScale            = 1f;
        [SerializeField] private float freiChenScale          = 1f;
        [SerializeField] private float multiScaleWeight1 = 1.0f;
        [SerializeField] private float multiScaleWeight2 = 0.5f;
        [SerializeField] private float contrastEnhancementFactor = 1.0f;
        [SerializeField] private float cannyLowThreshold = 0.1f;
        [SerializeField] private float cannyHighThreshold = 0.3f;
        [SerializeField] private float bilateralSigmaSpatial = 2.0f;
        [SerializeField] private float bilateralSigmaRange = 0.1f;
        [SerializeField] private float nlmH = 0.1f;

        [SerializeField] private bool applyPreFilter = false;
        [SerializeField] private int preFilterKernelSize = 5;
        [SerializeField] private float preFilterSigma = 1.0f;

        private RenderTexture previewRT;
        [SerializeField] private Texture2D     lastInputTexture;
        [SerializeField] private float         previewScale = 1.0f;
        [SerializeField] private Texture2NormalmapPreset batchPreset;
        [SerializeField] private bool           showBatchSettings = false;
        [SerializeField] private Vector2        scrollPos;

        private static RenderTexture poolHeightRT;
        private static RenderTexture poolNormalRT;
        private static RenderTexture poolBentRT;
        private static ComputeShader normalMapCS, preFilterCS;
        private static Material     heightMat;

        private static readonly Color accentColor = new Color(0.3f, 0.7f, 1f);

        private static readonly string[] algorithmLabels = {
            "Central Difference",
            "Sobel",
            "Prewitt",
            "Scharr",
            "Laplacian",
            "Roberts",
            "Kirsch",
            "Frei-Chen",
            "Multi-Scale",
            "Local Contrast Enhancement",
            "Canny",
            "Bilateral",
            "Non-Local Means"
        };
        #endregion

        #region Window Setup
        public static void ShowWindow() { FynnsTextureTools.TextureToolsWindow.Open(1); }
        #endregion

        private void OnDestroy() => ReleasePools();
        public void UseSource(Texture2D input) { inputTexture = input; lastInputTexture = null; }

        #region GUI
        public void DrawPanel()
        {




            DrawTextureInput();

            if (inputTexture != null && ValidateImportSettings(inputTexture))
            {
                if (inputTexture != lastInputTexture || previewRT == null)
                {
                    lastInputTexture = inputTexture;
                }

                DrawParameters();
                DrawAdvancedSection();
                DrawPresetButtons();
                DrawSaveButton();


            }




        }

        

        private void DrawTextureInput()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    richText = true
                };

                EditorGUILayout.LabelField("<b>Input Texture</b>", sectionStyle);
                EditorGUILayout.Space(2);

                EditorGUI.BeginChangeCheck();
                inputTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", inputTexture, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (EditorGUI.EndChangeCheck())
                {
                    if (inputTexture != null)
                    {
                        lastInputTexture = inputTexture;
                    }
                }
            }

            EditorGUILayout.Space(5);
        }

        private bool ValidateImportSettings(Texture2D tex)
        {
            if (!SystemInfo.supportsComputeShaders) { EditorGUILayout.HelpBox("Normal conversion requires compute shader support.", MessageType.Error); return false; }
            return tex != null;
        }

        private void DrawParameters()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    richText = true
                };

                EditorGUILayout.LabelField("<b>Parameters</b>", sectionStyle);
                EditorGUILayout.Space(2);

                EditorGUI.BeginChangeCheck();

                heightScale = EditorGUILayout.Slider("Strength", heightScale, 0f, 5f);
                selectedChannel = (Channel)EditorGUILayout.EnumPopup("Channel", selectedChannel);

                using (new GUILayout.HorizontalScope())
                {
                    invertX = EditorGUILayout.ToggleLeft("Invert X", invertX);
                    invertY = EditorGUILayout.ToggleLeft("Invert Y", invertY);
                }

                conversionAlgorithm = (Algorithm)EditorGUILayout.Popup("Algorithm", (int)conversionAlgorithm, algorithmLabels);

                if (EditorGUI.EndChangeCheck() && inputTexture != null)
                {
                    lastInputTexture = inputTexture;
                    FynnsTextureTools.TextureToolsWindow.RepaintOpen();
                }
            }

            EditorGUILayout.Space(5);
        }

        private void DrawAdvancedSection()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                showAdvancedParameters = EditorGUILayout.Foldout(showAdvancedParameters, "Advanced Settings", true, EditorStyles.foldoutHeader);

                if (showAdvancedParameters)
                {
                    EditorGUILayout.Space(2);

                    EditorGUI.BeginChangeCheck();

                    applyPreFilter = EditorGUILayout.Toggle("Apply Pre-Filter", applyPreFilter);

                    if (applyPreFilter)
                    {
                        EditorGUI.indentLevel++;
                        preFilterKernelSize = EditorGUILayout.IntSlider("Kernel Size", preFilterKernelSize, 3, 11);
                        preFilterSigma = EditorGUILayout.Slider("Sigma", preFilterSigma, 0.1f, 5f);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space(5);

                    DrawAlgorithmParameters();

                    if (EditorGUI.EndChangeCheck() && inputTexture != null)
                    {
                        lastInputTexture = inputTexture;
                        FynnsTextureTools.TextureToolsWindow.RepaintOpen();
                    }
                }
            }

            EditorGUILayout.Space(5);
        }

        private void DrawAlgorithmParameters()
        {
            switch (conversionAlgorithm)
            {
                case Algorithm.Sobel:
                    sobelInnerWeight = EditorGUILayout.Slider("Inner Weight", sobelInnerWeight, 0.5f, 5f);
                    sobelOuterWeight = EditorGUILayout.Slider("Outer Weight", sobelOuterWeight, 0.5f, 5f);
                    break;
                case Algorithm.Prewitt:
                    prewittWeight = EditorGUILayout.Slider("Weight", prewittWeight, 0.5f, 5f);
                    break;
                case Algorithm.Scharr:
                    scharrLowWeight = EditorGUILayout.Slider("Low Weight", scharrLowWeight, 0.5f, 10f);
                    scharrHighWeight = EditorGUILayout.Slider("High Weight", scharrHighWeight, 0.5f, 20f);
                    break;
                case Algorithm.Laplacian:
                    laplacianCenterWeight = EditorGUILayout.Slider("Center Weight", laplacianCenterWeight, 1f, 10f);
                    laplacianNeighborWeight = EditorGUILayout.Slider("Neighbor Weight", laplacianNeighborWeight, 0.1f, 5f);
                    break;
                case Algorithm.Roberts:
                    robertsScale = EditorGUILayout.Slider("Scale", robertsScale, 0.1f, 5f);
                    break;
                case Algorithm.Kirsch:
                    kirschScale = EditorGUILayout.Slider("Scale", kirschScale, 0.1f, 5f);
                    break;
                case Algorithm.FreiChen:
                    freiChenScale = EditorGUILayout.Slider("Scale", freiChenScale, 0.1f, 5f);
                    break;
                case Algorithm.MultiScaleEdgeDetection:
                    multiScaleWeight1 = EditorGUILayout.Slider("Small Scale Weight", multiScaleWeight1, 0.1f, 5f);
                    multiScaleWeight2 = EditorGUILayout.Slider("Large Scale Weight", multiScaleWeight2, 0.1f, 5f);
                    break;
                case Algorithm.LocalContrastEnhancement:
                    contrastEnhancementFactor = EditorGUILayout.Slider("Contrast Enhancement", contrastEnhancementFactor, 0.1f, 5f);
                    break;
                case Algorithm.CannyEdgeDetection:
                    cannyLowThreshold = EditorGUILayout.Slider("Low Threshold", cannyLowThreshold, 0.0f, 1f);
                    cannyHighThreshold = EditorGUILayout.Slider("High Threshold", cannyHighThreshold, 0.0f, 1f);
                    break;
                case Algorithm.BilateralFilter:
                    bilateralSigmaSpatial = EditorGUILayout.Slider("Sigma (Spatial)", bilateralSigmaSpatial, 0.1f, 10f);
                    bilateralSigmaRange = EditorGUILayout.Slider("Sigma (Range)", bilateralSigmaRange, 0.01f, 1f);
                    break;
                case Algorithm.NonLocalMeans:
                    nlmH = EditorGUILayout.Slider("NLM h", nlmH, 0.01f, 1f);
                    break;
                default:
                    GUIStyle infoStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontStyle = FontStyle.Italic,
                        normal = { textColor = Color.gray }
                    };
                    EditorGUILayout.LabelField("No advanced parameters for this algorithm.", infoStyle);
                    break;
            }
        }

        private void DrawPresetButtons()
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Preset", GUILayout.Height(25)))
                    SavePresetAsset();
                if (GUILayout.Button("Load Preset", GUILayout.Height(25)))
                    LoadPresetAsset();
                if (GUILayout.Button("Reset", GUILayout.Height(25)))
                    ResetPreset();
            }

            EditorGUILayout.Space(5);
        }

        private void DrawSaveButton()
        {
            outputName = EditorGUILayout.TextField(new GUIContent("Output Name", "Leave blank to use the source name."), outputName);
            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = accentColor;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                fixedHeight = 35
            };

            if (GUILayout.Button("Save Normal Map", buttonStyle))
            {
                if (inputTexture == null)
                    EditorUtility.DisplayDialog("Error", "Please select a texture.", "OK");
                else
                    ConvertTextureToNormalMap();
            }

            GUI.backgroundColor = originalBg;
            EditorGUILayout.Space(5);
        }

        

        private void DrawPreview()
        {
            if (previewRT == null) return;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    richText = true
                };

                EditorGUILayout.LabelField("<b>Preview</b>", sectionStyle);
                EditorGUILayout.Space(2);

                previewScale = EditorGUILayout.Slider("Preview Scale", previewScale, 0.1f, 5f);

                var maxSize = 256f;
                var aspect = (float)previewRT.width / previewRT.height;
                var w = aspect > 1f ? maxSize : maxSize * aspect;
                var h = aspect > 1f ? maxSize / aspect : maxSize;
                w *= previewScale;
                h *= previewScale;

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    var rect = GUILayoutUtility.GetRect(w, h);
                    EditorGUI.DrawPreviewTexture(rect, previewRT, null, ScaleMode.ScaleToFit);
                    GUILayout.FlexibleSpace();
                }
            }
        }

        
        #endregion

        #region Normal Map Generation
        public Texture2NormalmapPreset CaptureSettings()
        {
            var p = CreateInstance<Texture2NormalmapPreset>();
            p.heightScale = heightScale;
p.selectedChannel = (int)selectedChannel;
p.invertX = invertX;
p.invertY = invertY;
p.conversionAlgorithm = (int)conversionAlgorithm;
p.sobelInnerWeight = sobelInnerWeight;
p.sobelOuterWeight = sobelOuterWeight;
p.prewittWeight = prewittWeight;
p.scharrLowWeight = scharrLowWeight;
p.scharrHighWeight = scharrHighWeight;
p.laplacianCenterWeight = laplacianCenterWeight;
p.laplacianNeighborWeight = laplacianNeighborWeight;
p.robertsScale = robertsScale;
p.kirschScale = kirschScale;
p.freiChenScale = freiChenScale;
p.multiScaleWeight1 = multiScaleWeight1;
p.multiScaleWeight2 = multiScaleWeight2;
p.contrastEnhancementFactor = contrastEnhancementFactor;
p.cannyLowThreshold = cannyLowThreshold;
p.cannyHighThreshold = cannyHighThreshold;
p.bilateralSigmaSpatial = bilateralSigmaSpatial;
p.bilateralSigmaRange = bilateralSigmaRange;
p.nlmH = nlmH;
p.applyPreFilter = applyPreFilter;
p.preFilterKernelSize = preFilterKernelSize;
p.preFilterSigma = preFilterSigma;
            return p;
        }
        private RenderTexture GetPooledNormalMapRT(Texture2D input)
        {
            var p = CaptureSettings();
            try
            {
                var next = FynnsTextureTools.NormalPipeline.Render(input, p);
                if (poolNormalRT) { poolNormalRT.Release(); DestroyImmediate(poolNormalRT); }
                poolNormalRT = next;
                return next;
            }
            finally { DestroyImmediate(p); }
        }
        public Texture2D PreviewSource { get { return inputTexture; } }
        public Texture2D BuildPreview(Texture2D source = null)
        {
            source = source ? source : inputTexture;
            if (!source) return null;
            var settings = CaptureSettings();
            Texture2D small = null; RenderTexture rendered = null;
            try
            {
                float scale = Mathf.Min(1, 512f / Mathf.Max(source.width, source.height));
                small = FynnsTextureTools.TextureIO.Read(source, Mathf.Max(1, Mathf.RoundToInt(source.width * scale)), Mathf.Max(1, Mathf.RoundToInt(source.height * scale)), false);
                rendered = FynnsTextureTools.NormalPipeline.Render(small, settings);
                return FynnsTextureTools.TextureIO.Read(rendered, rendered.width, rendered.height, true);
            }
            finally { if (small) DestroyImmediate(small); if (rendered) { rendered.Release(); DestroyImmediate(rendered); } DestroyImmediate(settings); }
        }
        public void ConvertBatch(Texture2D input, string path)
        {
            var p = CaptureSettings();
            try { FynnsTextureTools.NormalPipeline.Save(input, p, path); }
            finally { DestroyImmediate(p); }
        }

        private void ConvertTextureToNormalMap()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Normal Map", FynnsTextureTools.TextureIO.FileName(outputName, inputTexture.name + "_NormalMap"), "png", "Choose an output file.");
            if (string.IsNullOrEmpty(path)) return;
            ConvertBatch(inputTexture, path);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture2D>(path));
        }

        private static void ReleasePools()
        {
            if (poolNormalRT) { poolNormalRT.Release(); DestroyImmediate(poolNormalRT); }
            poolNormalRT = null;
        }
        #endregion

        #region Preset Management
        private void SavePresetAsset()
        {
            var preset = CreateInstance<Texture2NormalmapPreset>();
            preset.heightScale = heightScale;
            preset.selectedChannel = (int)selectedChannel;
            preset.invertX = invertX;
            preset.invertY = invertY;
            preset.conversionAlgorithm = (int)conversionAlgorithm;
            preset.sobelInnerWeight = sobelInnerWeight;
            preset.sobelOuterWeight = sobelOuterWeight;
            preset.prewittWeight = prewittWeight;
            preset.scharrLowWeight = scharrLowWeight;
            preset.scharrHighWeight = scharrHighWeight;
            preset.laplacianCenterWeight = laplacianCenterWeight;
            preset.laplacianNeighborWeight = laplacianNeighborWeight;
            preset.robertsScale = robertsScale;
            preset.kirschScale = kirschScale;
            preset.freiChenScale = freiChenScale;
            preset.multiScaleWeight1 = multiScaleWeight1;
            preset.multiScaleWeight2 = multiScaleWeight2;
            preset.contrastEnhancementFactor = contrastEnhancementFactor;
            preset.cannyLowThreshold = cannyLowThreshold;
            preset.cannyHighThreshold = cannyHighThreshold;
            preset.bilateralSigmaSpatial = bilateralSigmaSpatial;
            preset.bilateralSigmaRange = bilateralSigmaRange;
            preset.nlmH = nlmH;
            preset.applyPreFilter = applyPreFilter;
            preset.preFilterKernelSize = preFilterKernelSize;
            preset.preFilterSigma = preFilterSigma;

            var path = EditorUtility.SaveFilePanelInProject("Save Preset", "NormalMapPreset", "asset", "Specify a filename for the preset.");
            if (string.IsNullOrEmpty(path)) { DestroyImmediate(preset); return; }
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#5ef02f>[Texture To Normalmap]</color> Preset saved: " + path);
        }

        private void LoadPresetAsset()
        {
            var path = EditorUtility.OpenFilePanel("Load Preset", "Assets", "asset");
            if (string.IsNullOrEmpty(path)) return;
            var projectPath = FileUtil.GetProjectRelativePath(path);
            if (!projectPath.StartsWith("Assets/")) return;
            var preset = AssetDatabase.LoadAssetAtPath<Texture2NormalmapPreset>(projectPath);
            if (preset == null) return;

            heightScale = preset.heightScale;
            selectedChannel = (Channel)preset.selectedChannel;
            invertX = preset.invertX;
            invertY = preset.invertY;
            conversionAlgorithm = (Algorithm)preset.conversionAlgorithm;
            sobelInnerWeight = preset.sobelInnerWeight;
            sobelOuterWeight = preset.sobelOuterWeight;
            prewittWeight = preset.prewittWeight;
            scharrLowWeight = preset.scharrLowWeight;
            scharrHighWeight = preset.scharrHighWeight;
            laplacianCenterWeight = preset.laplacianCenterWeight;
            laplacianNeighborWeight = preset.laplacianNeighborWeight;
            robertsScale = preset.robertsScale;
            kirschScale = preset.kirschScale;
            freiChenScale = preset.freiChenScale;
            multiScaleWeight1 = preset.multiScaleWeight1;
            multiScaleWeight2 = preset.multiScaleWeight2;
            contrastEnhancementFactor = preset.contrastEnhancementFactor;
            cannyLowThreshold = preset.cannyLowThreshold;
            cannyHighThreshold = preset.cannyHighThreshold;
            bilateralSigmaSpatial = preset.bilateralSigmaSpatial;
            bilateralSigmaRange = preset.bilateralSigmaRange;
            nlmH = preset.nlmH;
            applyPreFilter = preset.applyPreFilter;
            preFilterKernelSize = preset.preFilterKernelSize;
            preFilterSigma = preset.preFilterSigma;

            if (inputTexture != null)
            {
                lastInputTexture = inputTexture;
                lastInputTexture = inputTexture;
            }
            FynnsTextureTools.TextureToolsWindow.RepaintOpen();
            Debug.Log("<color=#5ef02f>[Texture To Normalmap]</color> Preset loaded: " + projectPath);
        }

        private void ResetPreset()
        {
            heightScale = 1.0f;
            selectedChannel = Channel.Grayscale;
            invertX = false;
            invertY = false;
            conversionAlgorithm = Algorithm.CentralDifference;
            sobelInnerWeight = 2f;
            sobelOuterWeight = 1f;
            prewittWeight = 1f;
            scharrLowWeight = 3f;
            scharrHighWeight = 10f;
            laplacianCenterWeight = 4f;
            laplacianNeighborWeight = 1f;
            robertsScale = 1f;
            kirschScale = 1f;
            freiChenScale = 1f;
            multiScaleWeight1 = 1.0f;
            multiScaleWeight2 = 0.5f;
            contrastEnhancementFactor = 1.0f;
            cannyLowThreshold = 0.1f;
            cannyHighThreshold = 0.3f;
            bilateralSigmaSpatial = 2.0f;
            bilateralSigmaRange = 0.1f;
            nlmH = 0.1f;
            applyPreFilter = false;
            preFilterKernelSize = 5;
            preFilterSigma = 1.0f;

            if (inputTexture != null)
            {
                lastInputTexture = inputTexture;
                lastInputTexture = inputTexture;
            }
            FynnsTextureTools.TextureToolsWindow.RepaintOpen();
            Debug.Log("<color=#5ef02f>[Texture To Normalmap]</color> Parameters reset to defaults.");
        }

        

        


        #endregion
    }
}
