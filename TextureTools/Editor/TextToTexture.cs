using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEditor;
using Object = UnityEngine.Object;

namespace TextToTexture
{
    public class TextToTexture : ScriptableObject
    {
        #region Constants

        private const string POIYOMI_SHADER_NAME = ".poiyomi/\u2022 Poiyomi Toon \u2022";
        private const string FALLBACK_SHADER_NAME = "Standard";

        private const string DUMMY_PARENT_NAME = "(Temp) TTT_Objects";

        private const string EDITORPREFS_SAVEPATH = "Text2TextureSavePath";
        private const string EDITORPREFS_DEFAULTSAVEPATH = "Assets/Texture Tools/Text";

        private const string RESOURCE_FONTPATH = "Assets/FynnsToolkit/TextToTexture/Resources/TTT_DancingScript TMP.asset";
        private const string RESOURCE_FONTGUID = "e7d6a5821a387564caacc9929e10eb22";

        private const string RESOURCE_TEMPLATEPATH = "Assets/FynnsToolkit/TextToTexture/Resources/TTT_CanvasTemplate.prefab";
        private const string RESOURCE_TEMPLATEGUID = "2cd48bc473a380748ad104409753555e";

        private readonly string[] DimensionPresets =
        {
            "128x128",
            "256x256",
            "512x512",
            "1024x1024",
            "2048x2048",
            "4096x4096",
        };

        #endregion

        #region Automated Variables

        public static string savePath;

        private static GUIContent resetIcon;
        private static bool showExtra;
        private static bool isInCustomizing;

        private static Camera tempCamera;
        private static TextMeshProUGUI tempText;
        private static GameObject tempParent;
        private static Material tempMaterial;
        private static RenderTexture tempRenderTexture;
        private static UnityEngine.SceneManagement.Scene previewScene;
        private static readonly List<Texture2D> captures = new List<Texture2D>();

        #endregion

        #region Input Variables
        public static string textInput = "";
        public static string outputName = "";
        public static TextureGenerationType generationType = TextureGenerationType.GenerateMaterial;

        public static TMP_FontAsset customFont;
        public static float outlineThickness = 0.075f;
        public static float extraPadding = 5;
        public static Vector2 offset;
        public static int resolutionWidth = 256;
        public static int resolutionHeight = 256;

        public static float verticalMargin = 100;
        public static float horizontalMargin = 100;

        public static bool invert;
        public static bool wrapText = true;
        #endregion

        #region Declarations
        public enum TextureGenerationType
        {
            GenerateNormalMap,
            GenerateMask,
            GenerateCustomTexture,
            GenerateMaterial
        }
        #endregion

        public static void showWindow() { FynnsTextureTools.TextureToolsWindow.Open(0); }

        public void DrawPanel()
        {
            using (new EditorGUI.DisabledScope(isInCustomizing))
            {
                EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);
                textInput = EditorGUILayout.TextArea(textInput, GUILayout.MinHeight(65));
                generationType = (TextureGenerationType)EditorGUILayout.EnumPopup("Output", generationType);
                customFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font", customFont, typeof(TMP_FontAsset), false);
                using (new EditorGUI.DisabledScope(generationType != TextureGenerationType.GenerateNormalMap && generationType != TextureGenerationType.GenerateMaterial))
                    outlineThickness = EditorGUILayout.Slider("Bevel Width", outlineThickness, 0, 1);
                extraPadding = EditorGUILayout.FloatField("Padding", extraPadding);
                offset = EditorGUILayout.Vector2Field("Offset", offset);
                horizontalMargin = EditorGUILayout.FloatField("Width (%)", horizontalMargin);
                verticalMargin = EditorGUILayout.FloatField("Height (%)", verticalMargin);
                wrapText = EditorGUILayout.Toggle("Wrap Text", wrapText);
                invert = EditorGUILayout.Toggle("Invert", invert);
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
                outputName = EditorGUILayout.TextField(new GUIContent("Output Name", "Leave blank to use the text."), outputName);
                resolutionWidth = Mathf.Clamp(EditorGUILayout.IntField("Width", resolutionWidth), 1, 8192);
                resolutionHeight = Mathf.Clamp(EditorGUILayout.IntField("Height", resolutionHeight), 1, 8192);
                savePath = AssetFolderPath(savePath, "Save To", EDITORPREFS_SAVEPATH, false);
            }

            if (!isInCustomizing)
            {
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(textInput)))
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Generate"))
                    {
                        switch (generationType)
                        {
                            case TextureGenerationType.GenerateNormalMap:
                            case TextureGenerationType.GenerateMask:
                                GenerateInit();
                                break;
                            case TextureGenerationType.GenerateCustomTexture:
                                isInCustomizing = true;
                                GenerateInit();
                                break;
                            case TextureGenerationType.GenerateMaterial:

                                GenerateTextMaterial();
                                break;
                        }
                        
                    }
                }
            }
            else
            {
                if (!tempCamera) isInCustomizing = false;
                if (tempText) EditorGUILayout.ObjectField("Custom Text", tempText, typeof(TextMeshProUGUI), true);
                if (tempMaterial) EditorGUILayout.ObjectField("Custom Material", tempMaterial, typeof(Material), false);
                if (GUILayout.Button("Save Texture")) GenerateTexture();
                if (GUILayout.Button("Cancel")) Cleanup();
            }




        }

        public Material GenerateTextMaterial()
        {
            var oldType = generationType; bool savedInvert = invert;
            try {
                                generationType = TextureGenerationType.GenerateNormalMap;
                                Texture2D normalTexture = GenerateInit();

                                generationType = TextureGenerationType.GenerateMask;
                                bool oldInvert = invert;
                                invert = true;
                                Texture2D maskTexture = GenerateInit();
                                invert = oldInvert;

                                generationType = TextureGenerationType.GenerateMaterial;

                                Shader currentShader = Shader.Find(POIYOMI_SHADER_NAME) ?? Shader.Find(FALLBACK_SHADER_NAME);
                                if (!currentShader) throw new InvalidOperationException("A compatible material shader is missing.");
                                Material newMaterial = new Material(currentShader);
                                #region Standard/Generic Property Set
                                newMaterial.SetTexture("_MainTex", maskTexture);
                                newMaterial.SetTexture("_MetallicGlossMap", maskTexture);
                                newMaterial.SetTexture("_BumpMap", normalTexture);
                                newMaterial.SetFloat("_Glossiness", 0.85f);
                                newMaterial.SetFloat("_GlossMapScale", 0.85f);
                                #endregion

                                newMaterial.EnableKeyword("_NORMALMAP");
                                if (currentShader.name == FALLBACK_SHADER_NAME) newMaterial.EnableKeyword("_METALLICGLOSSMAP");
                                if (currentShader.name == POIYOMI_SHADER_NAME) {
                                #region Poiyomi Propert Set
                                newMaterial.EnableKeyword("VIGNETTE_CLASSIC");
                                newMaterial.SetTexture("_BRDFSpecularMap", maskTexture);
                                newMaterial.SetTexture("_BRDFMetallicMap", maskTexture);
                                newMaterial.SetFloat("_LightingMode", 1);
                                newMaterial.SetFloat("_LightingStandardSmoothness", 0.85f);
                                newMaterial.SetFloat("_EnableBRDF", 1);
                                newMaterial.SetFloat("_BRDFMetallic", 1);
                                newMaterial.SetFloat("_BRDFGlossiness", 0.85f);
                                #endregion
                                }
                                string newMatName = FynnsTextureTools.TextureIO.FileName(outputName, textInput);
                                string newMatPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/{newMatName}.mat");
                                AssetDatabase.CreateAsset(newMaterial, newMatPath);
                                EditorGUIUtility.PingObject(newMaterial);
                                return newMaterial;

            } finally { generationType = oldType; invert = savedInvert; Cleanup(); }
        }

        private bool previewOnly;
        public Texture2D BuildPreview()
        {
            if (string.IsNullOrWhiteSpace(textInput)) return null;
            if (isInCustomizing && tempCamera)
            {
                var previous = RenderTexture.active;
                try { tempCamera.targetTexture = tempRenderTexture; RenderTexture.active = tempRenderTexture; tempCamera.Render();
                    var result = new Texture2D(tempRenderTexture.width, tempRenderTexture.height, TextureFormat.RGBA32, false);
                    result.ReadPixels(new Rect(0, 0, result.width, result.height), 0, 0); result.Apply(); return result;
                } finally { RenderTexture.active = previous; }
            }
            int w = resolutionWidth, h = resolutionHeight;
            var type = generationType;
            try
            {
                float scale = Mathf.Min(1, 512f / Mathf.Max(w, h));
                resolutionWidth = Mathf.Max(1, Mathf.RoundToInt(w * scale));
                resolutionHeight = Mathf.Max(1, Mathf.RoundToInt(h * scale));
                if (type == TextureGenerationType.GenerateMaterial) generationType = TextureGenerationType.GenerateNormalMap;
                previewOnly = true;
                return GenerateInit();
            }
            finally { previewOnly = false; resolutionWidth = w; resolutionHeight = h; generationType = type; }
        }

        private Texture2D GenerateInit()
        {
            GameObject canvasTemplate = ReadyResource<GameObject>(RESOURCE_TEMPLATEPATH, RESOURCE_TEMPLATEGUID);
            customFont = customFont ?? ReadyResource<TMP_FontAsset>(RESOURCE_FONTPATH, RESOURCE_FONTGUID);

            bool customizing = isInCustomizing; Cleanup(); isInCustomizing = customizing;
            FynnsTextureTools.TextureIO.ValidateSize(resolutionWidth, resolutionHeight);
            if (!canvasTemplate || !customFont) throw new InvalidOperationException("Text resources are missing.");
            previewScene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            tempRenderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

            float resolutionRatio = (float) resolutionWidth / resolutionHeight;


            tempParent = new GameObject(DUMMY_PARENT_NAME)
            {
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild,
                transform =
                {
                    position = new Vector3(420, 666, 69)
                }
            };

            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(tempParent, previewScene);
            GameObject canvas = Instantiate(canvasTemplate, tempParent.transform);
            if (generationType == TextureGenerationType.GenerateCustomTexture) EditorGUIUtility.PingObject(canvas.GetInstanceID());

            tempCamera = canvas.GetComponentInChildren<Camera>();
            tempCamera.scene = previewScene;
            tempText = canvas.GetComponentInChildren<TextMeshProUGUI>();
            RectTransform textRect = tempText.GetComponent<RectTransform>();

            tempCamera.transform.Translate(-offset);
            tempCamera.orthographicSize = 80 + extraPadding * 2;

            tempText.enableWordWrapping = wrapText;
            tempText.font = customFont;
            tempText.text = textInput;

            switch (generationType)
            {
                case TextureGenerationType.GenerateNormalMap:
                    CreateTemporaryMaterial(tempText.fontSharedMaterial, true, true);
                    break;
                case TextureGenerationType.GenerateMask:
                    CreateTemporaryMaterial(tempText.fontSharedMaterial, true, false);
                    break;
                case TextureGenerationType.GenerateCustomTexture:
                    CreateTemporaryMaterial(tempText.fontSharedMaterial, false, false);
                    break;
            }


            textRect.sizeDelta = new Vector2(160 * resolutionRatio * (horizontalMargin/100), 160 * (verticalMargin / 100));


            return isInCustomizing ? null : GenerateTexture();
        }

        private Texture2D GenerateTexture()
        {
            isInCustomizing = false;
            tempCamera.targetTexture = tempRenderTexture;
            var previousRT = RenderTexture.active;
            try
            {
            RenderTexture.active = tempRenderTexture;

            if (!previewOnly) ReadyPath(savePath);
            byte[] textureBytes = null;

            switch (generationType)
            {
                case TextureGenerationType.GenerateNormalMap:
                {

                    tempMaterial.SetFloat("_LightAngle", 0);
                    Texture2D bottomTexture = CaptureRender();

                    tempMaterial.SetFloat("_LightAngle", 6.28f / 4);
                    Texture2D rightTexture = CaptureRender();

                    tempMaterial.SetFloat("_LightAngle", 6.28f / 4 * 2);
                    Texture2D topTexture = CaptureRender();

                    tempMaterial.SetFloat("_LightAngle", 6.28f / 4 * 3);
                    Texture2D leftTexture = CaptureRender();


                    Color[] bottomPixels = bottomTexture.GetPixels();
                    Color[] rightPixels = rightTexture.GetPixels();
                    Color[] topPixels = topTexture.GetPixels();
                    Color[] leftPixels = leftTexture.GetPixels();

                    Color[] newColors = new Color[resolutionHeight * resolutionWidth];

                    Parallel.For(0, resolutionHeight, i =>
                    {
                        Parallel.For(0, resolutionWidth, j =>
                        {
                            int currentIndex = i * resolutionWidth + j;
                            float horizontalInfluence = 0.5f + leftPixels[currentIndex].r / 2 - rightPixels[currentIndex].r / 2;
                            float verticalInfluence = 0.5f + bottomPixels[currentIndex].r / 2 - topPixels[currentIndex].r / 2;
                            float blueColor = 1 - ((Mathf.Abs(horizontalInfluence - 0.5f) + Mathf.Abs(verticalInfluence - 0.5f)) / 2);
                            newColors[currentIndex] = new Color(horizontalInfluence, verticalInfluence, blueColor);
                        });
                    });

                    DestroyImmediate(rightTexture);
                    DestroyImmediate(topTexture);
                    DestroyImmediate(leftTexture);

                    bottomTexture.SetPixels(newColors);
                    bottomTexture.Apply();
                    textureBytes = bottomTexture.EncodeToPNG();
                    DestroyImmediate(bottomTexture);
                    break;
                }
                case TextureGenerationType.GenerateMask:
                {
                    CreateTemporaryMaterial(tempText.fontSharedMaterial, true, false);

                    tempCamera.backgroundColor = invert ? Color.white : Color.black;
                    Texture2D maskTexture = CaptureRender();
                    textureBytes = maskTexture.EncodeToPNG();
                    DestroyImmediate(maskTexture);

                    break;
                }
                case TextureGenerationType.GenerateCustomTexture:
                {
                    Texture2D newTexture = CaptureRender();
                    textureBytes = newTexture.EncodeToPNG();
                    DestroyImmediate(newTexture);
                    break;
                }
            }

            RenderTexture.active = previousRT;

            if (previewOnly)
            {
                var preview = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                preview.LoadImage(textureBytes); preview.hideFlags = HideFlags.HideAndDontSave;
                return preview;
            }
            Texture2D generatedTexture = SaveTexture(textureBytes);
            EditorGUIUtility.PingObject(generatedTexture);

            if (tempParent)
                DestroyImmediate(tempParent);

            return generatedTexture;
            }
            finally { RenderTexture.active = previousRT; Cleanup(); }
        }

        private static Texture2D SaveTexture(byte[] textureBytes)
        {

            string suffix;
            switch (generationType)
            {
                case TextureGenerationType.GenerateNormalMap:
                    suffix = " Normal";
                    break;
                case TextureGenerationType.GenerateMask:
                    suffix = " Mask";
                    break;
                default:
                    suffix = string.Empty;
                    break;
            }
            string texturePath = AssetDatabase.GenerateUniqueAssetPath(savePath + "/" + FynnsTextureTools.TextureIO.FileName(outputName, textInput) + suffix + ".png");
            try
            {
                File.WriteAllBytes(texturePath, textureBytes);
            }
            catch
            {
                texturePath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/Generated TextTexture.png");
                File.WriteAllBytes(texturePath, textureBytes);
            }

            AssetDatabase.ImportAsset(texturePath);

            TextureImporter textureSettings = (TextureImporter)AssetImporter.GetAtPath(texturePath);

            if (generationType == TextureGenerationType.GenerateNormalMap) textureSettings.textureType = TextureImporterType.NormalMap;
            if (generationType == TextureGenerationType.GenerateMask) textureSettings.sRGBTexture = false;

            textureSettings.npotScale = TextureImporterNPOTScale.None;
            textureSettings.maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(resolutionWidth, resolutionHeight));
            textureSettings.crunchedCompression = false;
            textureSettings.textureCompression = TextureImporterCompression.Uncompressed;
            textureSettings.compressionQuality = 100;

            textureSettings.streamingMipmaps = true;
            textureSettings.SaveAndReimport();

            return (Texture2D)AssetDatabase.LoadMainAssetAtPath(texturePath);
        }

        private static Texture2D CaptureRender()
        {
            tempCamera.Render();
            Texture2D capturedTexture = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGBA32, false);
            captures.Add(capturedTexture);
            capturedTexture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
            return capturedTexture;
        }

        private static void SetRequiredMaterialSettings(Material m, bool isNormal)
        {
            foreach (var key in m.shaderKeywords)
                m.DisableKeyword(key);

            if (isNormal)
            {
                m.EnableKeyword("BEVEL_ON");
                m.SetFloat("_OutlineWidth", outlineThickness);
            }

            m.SetFloat("_ShaderFlags", 0);
            m.SetFloat("_Bevel", isNormal ? invert ? -1 : 1 : 0);
            m.SetFloat("_BevelOffset", 0);
            m.SetFloat("_BevelClamp", 0);
            m.SetFloat("_BevelRoundness", 0);
            m.SetFloat("_BevelWidth", 0.5f);

            m.SetFloat("_Ambient", 1);
            m.SetFloat("_SpecularPower", 1);
            m.SetFloat("_Reflectivity", 5);
            m.SetFloat("_Diffuse", 0);

            m.SetFloat("_BumpFace", 0);
            m.SetFloat("_BumpOutline", 0);

            m.SetColor("_FaceColor", isNormal ? Color.clear : invert ? Color.black : Color.white);
            m.SetColor("_OutlineColor", isNormal ? Color.black : Color.clear);
            m.SetColor("_ReflectFaceColor", Color.clear);
            m.SetColor("_ReflectOutlineColor", Color.clear);
            m.SetColor("_SpecularColor", new Color(1.498039f, 1.498039f, 1.498039f));
        }

        private static void CreateTemporaryMaterial(Material original, bool ForceSettings, bool isNormal)
        {
            var next = new Material(original) { hideFlags = HideFlags.HideAndDontSave };
            if (isNormal && (!next.HasProperty("_Bevel") || !next.HasProperty("_LightAngle")))
            {
                var shader = Shader.Find("TextMeshPro/Distance Field");
                if (!shader) { DestroyImmediate(next); throw new InvalidOperationException("TextMeshPro Distance Field shader is required for text normals."); }
                next.shader = shader;
            }
            if (tempMaterial) DestroyImmediate(tempMaterial);
            tempMaterial = next;
            if (ForceSettings) SetRequiredMaterialSettings(tempMaterial, isNormal);
            tempText.fontSharedMaterial = tempMaterial;
        }

        private void OnEnable()
        {
            savePath = EditorPrefs.GetString(EDITORPREFS_SAVEPATH, EDITORPREFS_DEFAULTSAVEPATH);
            resetIcon = new GUIContent(EditorGUIUtility.IconContent("d_Refresh")) {tooltip = "Reset Dimensions"};
        }

        #region DSHelper Methods

        private static T ReadyResource<T>(string resourcePath, string resourceGUID = "", string msg = "") where T : Object
        {
            T asset = Resources.Load<T>(System.IO.Path.GetFileNameWithoutExtension(resourcePath));
            if (asset) return asset;

            if (!string.IsNullOrWhiteSpace(resourceGUID))
            {
                asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(resourceGUID));
                if (asset) return asset;
            }

            if (string.IsNullOrWhiteSpace(msg)) msg = $"Required Resources Asset at Resource Path {resourcePath} not found! Execution may not proceed.";
            EditorUtility.DisplayDialog("Error", msg, "Heck");
            throw new NullReferenceException(msg);
        }

        private static void ReadyPath(string path)
        {
            FynnsTextureTools.TextureIO.ValidateOutput(path + "/output.png");
            if (Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
            AssetDatabase.ImportAsset(path);
        }

        

        private static string AssetFolderPath(string variable, string title, string playerpref, bool isPlayerPrefs = true)
        {
            TextureUtility.TextureUtility.AssetFolderPath(ref variable, title, playerpref);
            return variable;
        }
        public static void Cleanup()
        {
            foreach (var capture in captures) if (capture) DestroyImmediate(capture);
            captures.Clear();
            if (tempParent) DestroyImmediate(tempParent);
            if (tempMaterial) DestroyImmediate(tempMaterial);
            if (tempRenderTexture) { tempRenderTexture.Release(); DestroyImmediate(tempRenderTexture); }
            if (previewScene.IsValid()) UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(previewScene);
            tempParent = null; tempMaterial = null; tempRenderTexture = null; tempCamera = null; tempText = null; isInCustomizing = false;
        }
        private void OnDisable() { Cleanup(); }

        private static void DrawSeparator(int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
            r.height = thickness;
            r.y += padding / 2f;
            r.x -= 2;
            r.width += 6;
            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out Color lineColor);
            EditorGUI.DrawRect(r, lineColor);
        }

        

        #endregion
    }
}
