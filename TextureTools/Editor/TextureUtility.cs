using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace TextureUtility
{
    public class TextureUtility : ScriptableObject
    {
        private readonly string[] DimensionPresets = {
            "128x128",
            "256x256",
            "512x512",
            "1024x1024",
            "2048x2048",
            "4096x4096",
        };

        private static GUIContent resetIcon;
        private static Texture2D titleTexture;

        private static Texture2D mainTexture;
        private static Texture2D maskTexture;

        private static int jpgQuality = 75;
        private static string outputName = "";
        private static int editChannel;
        public int EditChannel
        {
            get { return editChannel; }
            set
            {
                int next = Mathf.Clamp(value, 0, 4);
                if (next == editChannel) return;
                StoreCurrentAdjustment();
                editChannel = next;
                LoadAdjustment(channelAdjustments[editChannel]);
            }
        }

        private static int texWidth = 256, texHeight = 256;
        private static bool copyImport = true;
        private static bool pingTexture = true;

        private static bool rotating;
        private static TexRotation rotationType;

        private static bool inverting;
        private static bool maskInvert=true;
        private static bool invertRedS = true, invertGreenS = true, invertBlueS = true, invertAlphaS;

        private static bool unpacking, packing = true;
        private static bool packLinear = true, packSourceSize = true;
        private static bool editingTab=true, packingTab,creatingTab;

        
        private static readonly ChannelTexture redChannel = new ChannelTexture("Red", 0);
        private static readonly ChannelTexture greenChannel = new ChannelTexture("Green", 1);
        private static readonly ChannelTexture blueChannel = new ChannelTexture("Blue", 2);
        private static readonly ChannelTexture alphaChannel = new ChannelTexture("Alpha", 0);
        private static readonly ChannelTexture[] channelTextures = { redChannel, greenChannel, blueChannel, alphaChannel };

        private static bool selectColor;
        private static Color selectedColor = new Color(1, .25f, .65f, 1);
        private static float colorTolerance = .1f, colorSoftness = .1f;

        private static bool hueShifting;
        private static bool maskHueShift=true;
        private static float hueShiftFloat;

        private static bool saturating;
        private static SaturationMethodEnum saturationMethod = SaturationMethodEnum.Poiyomi;
        private static bool maskSaturate=true;
        private static float saturationFloat;

        private static bool colorizing;
        private static bool maskColorize=true;
        private static bool textureColorize;
        private static bool alphaColorize;
        private static float colorizeFloat=0.5f;
        private static Color colorizeColor = Color.black;
        private static Texture2D colorizeTexture;

        [System.Serializable]
        private class ColorAdjustment
        {
            public bool selectColor = false;
            public Color selectedColor = new Color(1, .25f, .65f, 1);
            public float colorTolerance = .1f, colorSoftness = .1f;
            public bool inverting = false;
            public bool maskInvert = true;
            public bool invertRedS = true;
            public bool invertGreenS = true;
            public bool invertBlueS = true;
            public bool invertAlphaS = false;
            public bool hueShifting = false;
            public bool maskHueShift = true;
            public float hueShiftFloat = 0;
            public bool saturating = false;
            public SaturationMethodEnum saturationMethod = SaturationMethodEnum.Poiyomi;
            public bool maskSaturate = true;
            public float saturationFloat = 0;
            public bool colorizing = false;
            public bool maskColorize = true;
            public bool textureColorize = false;
            public bool alphaColorize = false;
            public float colorizeFloat = .5f;
            public Color colorizeColor = Color.black;
            public Texture2D colorizeTexture = null;
            public bool Enabled { get { return hueShifting || saturating || colorizing || (inverting && (invertRedS || invertGreenS || invertBlueS || invertAlphaS)); } }
        }
        private static ColorAdjustment[] channelAdjustments = Enumerable.Range(0, 5).Select(i => new ColorAdjustment()).ToArray();
        private static void StoreCurrentAdjustment()
        {
            channelAdjustments[editChannel] = new ColorAdjustment {
                selectColor = selectColor,
                selectedColor = selectedColor,
                colorTolerance = colorTolerance,
                colorSoftness = colorSoftness,
                inverting = inverting,
                maskInvert = maskInvert,
                invertRedS = invertRedS,
                invertGreenS = invertGreenS,
                invertBlueS = invertBlueS,
                invertAlphaS = invertAlphaS,
                hueShifting = hueShifting,
                maskHueShift = maskHueShift,
                hueShiftFloat = hueShiftFloat,
                saturating = saturating,
                saturationMethod = saturationMethod,
                maskSaturate = maskSaturate,
                saturationFloat = saturationFloat,
                colorizing = colorizing,
                maskColorize = maskColorize,
                textureColorize = textureColorize,
                alphaColorize = alphaColorize,
                colorizeFloat = colorizeFloat,
                colorizeColor = colorizeColor,
                colorizeTexture = colorizeTexture
            };
        }
        private static void LoadAdjustment(ColorAdjustment adjustment)
        {
            selectColor = adjustment.selectColor;
            selectedColor = adjustment.selectedColor;
            colorTolerance = adjustment.colorTolerance;
            colorSoftness = adjustment.colorSoftness;
            inverting = adjustment.inverting;
            maskInvert = adjustment.maskInvert;
            invertRedS = adjustment.invertRedS;
            invertGreenS = adjustment.invertGreenS;
            invertBlueS = adjustment.invertBlueS;
            invertAlphaS = adjustment.invertAlphaS;
            hueShifting = adjustment.hueShifting;
            maskHueShift = adjustment.maskHueShift;
            hueShiftFloat = adjustment.hueShiftFloat;
            saturating = adjustment.saturating;
            saturationMethod = adjustment.saturationMethod;
            maskSaturate = adjustment.maskSaturate;
            saturationFloat = adjustment.saturationFloat;
            colorizing = adjustment.colorizing;
            maskColorize = adjustment.maskColorize;
            textureColorize = adjustment.textureColorize;
            alphaColorize = adjustment.alphaColorize;
            colorizeFloat = adjustment.colorizeFloat;
            colorizeColor = adjustment.colorizeColor;
            colorizeTexture = adjustment.colorizeTexture;
        }
        public void ResetCurrentAdjustment()
        {
            channelAdjustments[editChannel] = new ColorAdjustment();
            LoadAdjustment(channelAdjustments[editChannel]);
        }
        private static Color AdjustColor(Color source, ColorAdjustment adjustment, Color mask, Color tint, Color selectionSource)
        {
            var color = source;
            if (adjustment.hueShifting || adjustment.saturating)
            {
                Color.RGBToHSV(source, out float h, out float saturation, out float value);
                if (adjustment.hueShifting) h = Mathf.Repeat(h + adjustment.hueShiftFloat * (adjustment.maskHueShift ? mask.r : 1), 1);
                if (adjustment.saturating && adjustment.saturationMethod == SaturationMethodEnum.Gimp)
                    saturation = Mathf.Clamp01(saturation * (1 + adjustment.saturationFloat * (adjustment.maskSaturate ? mask.r : 1)));
                color = Color.HSVToRGB(h, saturation, value);
                if (adjustment.saturating && adjustment.saturationMethod == SaturationMethodEnum.Poiyomi)
                {
                    float dot = color.r * .3f + color.g * .59f + color.b * .11f;
                    float factor = Mathf.Lerp(1, adjustment.saturationFloat + 1, adjustment.maskSaturate ? mask.r : 1);
                    color = new Color(dot + factor * (color.r - dot), dot + factor * (color.g - dot), dot + factor * (color.b - dot), source.a);
                }
                color.a = source.a;
            }
            if (adjustment.colorizing)
            {
                float alpha = color.a;
                color = Color.Lerp(color, adjustment.textureColorize ? tint : adjustment.colorizeColor, adjustment.colorizeFloat * (adjustment.maskColorize ? mask.r : 1));
                if (!adjustment.alphaColorize) color.a = alpha;
            }
            if (adjustment.inverting)
            {
                if (adjustment.invertRedS) color.r = Mathf.Lerp(color.r, 1 - color.r, adjustment.maskInvert ? mask.r : 1);
                if (adjustment.invertGreenS) color.g = Mathf.Lerp(color.g, 1 - color.g, adjustment.maskInvert ? mask.g : 1);
                if (adjustment.invertBlueS) color.b = Mathf.Lerp(color.b, 1 - color.b, adjustment.maskInvert ? mask.b : 1);
                if (adjustment.invertAlphaS) color.a = Mathf.Lerp(color.a, 1 - color.a, adjustment.maskInvert ? mask.a : 1);
            }
            if (adjustment.selectColor)
            {
                float dr = selectionSource.r - adjustment.selectedColor.r;
                float dg = selectionSource.g - adjustment.selectedColor.g;
                float db = selectionSource.b - adjustment.selectedColor.b;
                float distance = Mathf.Sqrt((dr * dr + dg * dg + db * db) / 3f);
                float weight = distance <= adjustment.colorTolerance ? 1f : 0f;
                if (distance > adjustment.colorTolerance && adjustment.colorSoftness > 0)
                    weight = 1 - Mathf.SmoothStep(0, 1, (distance - adjustment.colorTolerance) / adjustment.colorSoftness);
                color = Color.Lerp(source, color, weight);
            }
            return color;
        }

        private static Color originalGUIColor;

        private static TexEncoding encoding = TexEncoding.SaveAsPNG;
        public enum TexEncoding
        {
            SaveAsPNG,
            SaveAsJPG,
            SaveAsTGA
        }

        public enum TexRotation
        {
            Clockwise90,
            CClockwise90,
            Rotate180,
            FlipHorizontal,
            FlipVertical
        }
        
        public enum SaturationMethodEnum
        {
            Poiyomi,
            Gimp
        }

        #region Creating Tab Variables
        private static bool creatingCustomSize = true;
        private static bool creatingReverse;
        private static string creatingPath;
        private static Color solidColor=Color.black;
        private static Gradient gradientColor = new Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.black, 1) } };

        private static TextureCreatingMode creatingMode = TextureCreatingMode.SolidColor;

        private enum TextureCreatingMode
        {
            SolidColor,
            HorizontalGradient,
            VerticalGradient
        }
        #endregion

        public static void Open() { FynnsTextureTools.TextureToolsWindow.Open(2); }

        public void DrawPanel(int tab)
        {
            originalGUIColor = GUI.backgroundColor;
            editingTab = tab == 2; creatingTab = tab == 3; packingTab = tab == 4;
            if (editingTab) DrawEditingTab();
            if (creatingTab) DrawCreatingTab();
            if (packingTab) DrawPackingTab();
        }
        public void ApplyBatch(Texture2D input, bool unpack, bool keepSize)
        {
            var old = mainTexture; int w = texWidth, h = texHeight;
            try { mainTexture = input; if (keepSize) ResetDimensions(); if (unpack) UnpackTexture(); else ApplyTexture(); }
            finally { mainTexture = old; texWidth = w; texHeight = h; }
        }
        public void UseSource(Texture2D input) { mainTexture = input; ResetDimensions(); }

        
        public Texture2D PreviewSource { get { return mainTexture; } }
        public Texture2D BuildPreview(int tab, Texture2D input = null, bool keepSize = true)
        {
            var old = mainTexture; int w = texWidth, h = texHeight;
            try
            {
                if (input) { mainTexture = input; if (keepSize) ResetDimensions(); }
                float scale = Mathf.Min(1, 512f / Mathf.Max(texWidth, texHeight));
                texWidth = Mathf.Max(1, Mathf.RoundToInt(texWidth * scale)); texHeight = Mathf.Max(1, Mathf.RoundToInt(texHeight * scale));
                if (tab == 3) return BuildCreated(true);
                if (tab == 4 && packing) return new AutoPackedTexture { channels = channelTextures, linear = packLinear, sourceSize = packSourceSize, width = texWidth, height = texHeight }.Render(512);
                if (!mainTexture) return null;
                if (tab == 4 || tab == 7)
                {
                    scale = Mathf.Min(1, 512f / Mathf.Max(mainTexture.width, mainTexture.height));
                    texWidth = Mathf.Max(1, Mathf.RoundToInt(mainTexture.width * scale)); texHeight = Mathf.Max(1, Mathf.RoundToInt(mainTexture.height * scale));
                }
                if (tab == 4 || tab == 7) return FynnsTextureTools.TextureIO.Read(mainTexture, texWidth, texHeight, false);
                return BuildEdited(true);
            }
            finally { mainTexture = old; texWidth = w; texHeight = h; }
        }

        private void DrawEditingTab()
        {
            EditorGUI.BeginChangeCheck();
            mainTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", mainTexture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck()) ResetDimensions();
            maskTexture = (Texture2D)EditorGUILayout.ObjectField("Mask (optional)", maskTexture, typeof(Texture2D), false);
            using (new GUILayout.HorizontalScope()) DrawDimensionsGUI();
            DrawExportSettings();
            rotating = EditorGUILayout.ToggleLeft("Rotate / Flip", rotating);
            if (rotating) rotationType = (TexRotation)EditorGUILayout.EnumPopup("Direction", rotationType);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new[] { "RGBA Adjustments", "Red Adjustments", "Green Adjustments", "Blue Adjustments", "Alpha Adjustments" }[editChannel], EditorStyles.boldLabel);
                if (GUILayout.Button("Reset", GUILayout.Width(55))) { ResetCurrentAdjustment(); FynnsTextureTools.TextureToolsWindow.RepaintOpen(); }
            }
            selectColor = EditorGUILayout.ToggleLeft("Only Affect Selected Color", selectColor);
            if (selectColor)
            {
                selectedColor = EditorGUILayout.ColorField(new GUIContent("Target Color", "Use the color picker's eyedropper to sample a color."), selectedColor, true, false, false);
                colorTolerance = EditorGUILayout.Slider("Tolerance", colorTolerance, 0, 1);
                colorSoftness = EditorGUILayout.Slider("Edge Softness", colorSoftness, 0, 1);
                EditorGUILayout.LabelField("Selects colors from the original image. Combines with the optional mask.", EditorStyles.wordWrappedMiniLabel);
            }
            inverting = EditorGUILayout.ToggleLeft("Invert Channels", inverting);
            if (inverting)
            {
                using (new GUILayout.HorizontalScope())
                {
                    invertRedS = EditorGUILayout.ToggleLeft("Red", invertRedS); invertGreenS = EditorGUILayout.ToggleLeft("Green", invertGreenS);
                    invertBlueS = EditorGUILayout.ToggleLeft("Blue", invertBlueS); invertAlphaS = EditorGUILayout.ToggleLeft("Alpha", invertAlphaS);
                }
                if (maskTexture) maskInvert = EditorGUILayout.Toggle("Use Mask for Invert", maskInvert);
            }
            hueShifting = EditorGUILayout.ToggleLeft("Hue Shift", hueShifting);
            if (hueShifting) { hueShiftFloat = EditorGUILayout.Slider("Hue", hueShiftFloat, 0, 1); if (maskTexture) maskHueShift = EditorGUILayout.Toggle("Use Mask for Hue", maskHueShift); }
            saturating = EditorGUILayout.ToggleLeft("Saturation", saturating);
            if (saturating)
            {
                saturationMethod = (SaturationMethodEnum)EditorGUILayout.EnumPopup("Method", saturationMethod);
                saturationFloat = EditorGUILayout.Slider("Amount", saturationFloat, -1, saturationMethod == SaturationMethodEnum.Poiyomi ? 10 : 1);
                if (maskTexture) maskSaturate = EditorGUILayout.Toggle("Use Mask for Saturation", maskSaturate);
            }
            colorizing = EditorGUILayout.ToggleLeft("Colorize", colorizing);
            if (colorizing)
            {
                textureColorize = EditorGUILayout.Toggle("Use Color Texture", textureColorize);
                if (textureColorize) colorizeTexture = (Texture2D)EditorGUILayout.ObjectField("Color Texture", colorizeTexture, typeof(Texture2D), false);
                else colorizeColor = EditorGUILayout.ColorField("Color", colorizeColor);
                colorizeFloat = EditorGUILayout.Slider("Amount", colorizeFloat, 0, 1);
                alphaColorize = EditorGUILayout.Toggle("Include Alpha", alphaColorize);
                if (maskTexture) maskColorize = EditorGUILayout.Toggle("Use Mask for Colorize", maskColorize);
            }
            using (new EditorGUI.DisabledScope(!mainTexture)) if (GUILayout.Button("Save Edited Copy", GUILayout.Height(30))) ApplyTexture();
        }
        private void DrawExportSettings()
        {
            outputName = EditorGUILayout.TextField(new GUIContent("Output Name", "Leave blank for an automatic name."), outputName);
            encoding = (TexEncoding)EditorGUILayout.EnumPopup("Format", encoding);
            if (encoding == TexEncoding.SaveAsJPG) jpgQuality = EditorGUILayout.IntSlider("JPEG Quality", jpgQuality, 1, 100);
            copyImport = EditorGUILayout.Toggle("Copy Import Settings", copyImport);
            pingTexture = EditorGUILayout.Toggle("Highlight Output", pingTexture);
        }

        private void DrawCreatingTab()
        {
            EditorGUILayout.LabelField("Texture", EditorStyles.boldLabel);
            creatingMode = (TextureCreatingMode)EditorGUILayout.EnumPopup("Pattern", creatingMode);
            if (creatingMode == TextureCreatingMode.SolidColor) solidColor = EditorGUILayout.ColorField("Color", solidColor);
            else { gradientColor = EditorGUILayout.GradientField("Gradient", gradientColor); creatingReverse = EditorGUILayout.Toggle("Reverse", creatingReverse); }
            creatingCustomSize = EditorGUILayout.Toggle("Custom Dimensions", creatingCustomSize);
            if (creatingCustomSize) { using (new GUILayout.HorizontalScope()) DrawDimensionsGUI(false); }
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
            DrawExportSettings();
            AssetFolderPath(ref creatingPath, "Save To", "TextureUtilityCreatingPath");
            if (GUILayout.Button("Save Texture", GUILayout.Height(30))) CreateTexture();
        }

        private void DrawPackingTab()
        {
            int mode = GUILayout.Toolbar(packing ? 0 : 1, new[] { "Pack Channels", "Unpack Channels" });
            packing = mode == 0; unpacking = !packing;
            DrawExportSettings();
            if (packing)
            {
                foreach (var channel in channelTextures) channel.DrawGUI();
                packSourceSize = EditorGUILayout.Toggle("Use Source Dimensions", packSourceSize);
                if (!packSourceSize) { using (new GUILayout.HorizontalScope()) DrawDimensionsGUI(false); }
                packLinear = EditorGUILayout.Toggle("Linear Data Texture", packLinear);
                if (encoding == TexEncoding.SaveAsJPG) EditorGUILayout.HelpBox("JPEG drops the alpha channel and compresses channel values.", MessageType.Info);
                if (GUILayout.Button("Save Packed Texture", GUILayout.Height(30)))
                {
                    string ext = encoding == TexEncoding.SaveAsJPG ? "jpg" : encoding == TexEncoding.SaveAsTGA ? "tga" : "png";
                    string path = EditorUtility.SaveFilePanelInProject("Pack channels", FynnsTextureTools.TextureIO.FileName(outputName, "Packed Texture"), ext, "Choose an output file.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var recipe = new AutoPackedTexture { channels = channelTextures, encoding = encoding, jpgQuality = jpgQuality, linear = packLinear, sourceSize = packSourceSize, width = texWidth, height = texHeight, outputPath = path };
                        string output = recipe.Pack();
                        if (copyImport)
                        {
                            var first = channelTextures.FirstOrDefault(c => c.texture);
                            if (first != null) FynnsTextureTools.TextureIO.CopySampling(first.texture, output);
                        }
                        if (pingTexture) EditorGUIUtility.PingObject(recipe.packed);
                    }
                }
            }
            else
            {
                mainTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", mainTexture, typeof(Texture2D), false);
                using (new EditorGUI.DisabledScope(!mainTexture)) if (GUILayout.Button("Unpack Channels", GUILayout.Height(30))) UnpackTexture();
            }
        }

        private void DrawDimensionsGUI(bool drawReset=true)
        {
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            GUIStyle iconStyle = new GUIStyle(GUI.skin.label) { padding = new RectOffset(), margin = new RectOffset(), imagePosition = ImagePosition.ImageOnly };
            
                EditorGUI.BeginDisabledGroup(!mainTexture && !creatingTab && !packingTab);
                if (drawReset)
                {
                    if (GUILayout.Button(resetIcon, iconStyle, GUILayout.Height(16), GUILayout.Width(16)))
                        ResetDimensions();
                }
                EditorGUIUtility.labelWidth = 20;
                texWidth = EditorGUILayout.IntField(new GUIContent("W","Width"), texWidth);
                texHeight = EditorGUILayout.IntField(new GUIContent("H", "Height"), texHeight);
                EditorGUIUtility.labelWidth = previousLabelWidth;

                texWidth = Mathf.Clamp(texWidth, 1, 8192); texHeight = Mathf.Clamp(texHeight, 1, 8192);
                int dummy = -1;
                EditorGUI.BeginChangeCheck();
                dummy = EditorGUILayout.Popup(dummy, DimensionPresets,GUILayout.Width(17));
                if (EditorGUI.EndChangeCheck())
                {
                string[] dimensions = ((string)DimensionPresets.GetValue(dummy)).Split('x');
                texWidth = int.Parse(dimensions[0]);
                texHeight = int.Parse(dimensions[1]);
                }

                EditorGUI.EndDisabledGroup();
            
        }

        public static Texture2D GetColors(Texture2D texture, out Color[] Colors, bool unloadTempTexture = false)
        {
            return GetColors(texture, texture.width, texture.height, out Colors, unloadTempTexture);
        }

        public static Texture2D GetColors(Texture2D texture, int width, int height, out Color[] Colors, bool unloadTempTexture = false)
        {
            var result = FynnsTextureTools.TextureIO.Read(texture, width, height, false);
            Colors = result.GetPixels();
            if (unloadTempTexture) { DestroyImmediate(result); return null; }
            return result;
        }

        private static void SaveTexture(byte[] textureEncoding, string path, bool refresh=false, bool ping=false)
        {
            FynnsTextureTools.TextureIO.ValidateOutput(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, textureEncoding);
            if (refresh)
            {
                AssetDatabase.Refresh();
                if (ping)
                {
                    Ping(path);
                }
            }

        }
        private static void Ping(string path)
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
        }

        private void ApplyTexture() { var result = BuildEdited(false); if (result) DestroyImmediate(result); }
        private Texture2D BuildEdited(bool preview)
        {
            StoreCurrentAdjustment();
            var adjustments = channelAdjustments.ToArray();
            for (int channel = 0; channel < adjustments.Length; channel++)
                if (adjustments[channel].colorizing && adjustments[channel].textureColorize && !adjustments[channel].colorizeTexture)
                    throw new System.InvalidOperationException(new[] { "RGBA", "Red", "Green", "Blue", "Alpha" }[channel] + ": choose a color texture before colorizing.");

            string destinationPath = GetDestinationFolder(mainTexture);
            if (!destinationPath.StartsWith("Assets")) destinationPath = "Assets/Texture Tools/Generated";
            if (!preview) ReadyFolder(destinationPath);
            string texPath = AssetDatabase.GetAssetPath(mainTexture);

            Texture2D newTexture = GetColors(mainTexture, texWidth, texHeight, out Color[] myColors);
            try {

            if (rotating)
            {
                List<Color> rotatedColors = new List<Color>();
                switch (rotationType)
                {
                    case TexRotation.Clockwise90:
                        for (int i = texWidth-1; i >=0; i--)
                        {
                            rotatedColors.AddRange(newTexture.GetPixels(i, 0, 1, texHeight));
                        }
                        myColors = rotatedColors.ToArray();
                        DestroyImmediate(newTexture);
                        newTexture = new Texture2D(texHeight, texWidth, TextureFormat.RGBA32, false);
                        break;

                    case TexRotation.CClockwise90:
                        for (int i = 0; i < texWidth; i++)
                        {
                            rotatedColors.AddRange(ReverseArray(newTexture.GetPixels(i, 0, 1, texHeight)));
                        }
                        myColors = rotatedColors.ToArray();
                        DestroyImmediate(newTexture);
                        newTexture = new Texture2D(texHeight, texWidth, TextureFormat.RGBA32, false);
                        break;

                    case TexRotation.Rotate180:
                        myColors = ReverseArray(myColors);
                        break;

                    case TexRotation.FlipHorizontal:
                        for (int i = 0; i < texHeight; i++)
                        {
                            rotatedColors.AddRange(ReverseArray(newTexture.GetPixels(0, i, texWidth, 1)));
                        }
                        myColors = rotatedColors.ToArray();
                        break;

                    case TexRotation.FlipVertical:
                        for (int i = texHeight - 1; i >= 0; i--)
                        {
                            rotatedColors.AddRange(newTexture.GetPixels(0, i, texWidth, 1));
                        }
                        myColors = rotatedColors.ToArray();
                        break;
                }
                
            }

            bool colorInverting = adjustments.Any(a => a.inverting);
            Color[] maskColors = null;
            if (maskTexture) GetColors(maskTexture, newTexture.width, newTexture.height, out maskColors, true);
            var tintColors = new Color[5][];
            for (int channel = 0; channel < adjustments.Length; channel++)
                if (adjustments[channel].colorizing && adjustments[channel].textureColorize)
                    GetColors(adjustments[channel].colorizeTexture, newTexture.width, newTexture.height, out tintColors[channel], true);
            var newColors = new Color[myColors.Length];
            Parallel.For(0, myColors.Length, i =>
            {
                var mask = maskColors == null ? Color.white : maskColors[i];
                var common = AdjustColor(myColors[i], adjustments[0], mask, tintColors[0] == null ? Color.white : tintColors[0][i], myColors[i]);
                var mixed = common;
                for (int channel = 0; channel < 4; channel++)
                {
                    if (!adjustments[channel + 1].Enabled) continue;
                    var adjusted = AdjustColor(common, adjustments[channel + 1], mask, tintColors[channel + 1] == null ? Color.white : tintColors[channel + 1][i], myColors[i]);
                    mixed[channel] = adjusted[channel];
                }
                newColors[i] = mixed;
            });
            newTexture.SetPixels(newColors);
            newTexture.Apply();

            if (preview) { var result = newTexture; newTexture = null; return result; }
            GetEncoding(newTexture, encoding, out byte[] data, out string ext);
            DestroyImmediate(newTexture);

            string newTexturePath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + "/" + FynnsTextureTools.TextureIO.FileName(outputName, mainTexture.name + (colorInverting ? " Inverted" : "")) + ext);
            
            SaveTexture(data, newTexturePath, true, pingTexture);

            if (copyImport)
            {
                CopyTextureSettings(texPath, newTexturePath);
            }
            return null;
                    } finally { if (newTexture) DestroyImmediate(newTexture); }
        }

        private static void GetEncoding(Texture2D texture, TexEncoding encodingType, out byte[] data, out string ext)
        {
            switch ((int)encodingType)
            {
                default:
                    ext = ".png";
                    data = texture.EncodeToPNG();
                    break;
                case 1:
                    ext = ".jpg";
                    data = texture.EncodeToJPG(jpgQuality);
                    break;
                case 2:
                    ext = ".tga";
                    data = texture.EncodeToTGA();
                    break;
            }
        }


        private void CreateTexture() { var result = BuildCreated(false); if (result) DestroyImmediate(result); }
        private Texture2D BuildCreated(bool preview)
        {
            Texture2D newTexture = null;
            try {
            int w = creatingCustomSize ? texWidth : 0;
            int h = creatingCustomSize ? texHeight : 0;

            Color[] myColors = null;
            switch ((int)creatingMode)
            {
                case 0:
                    if (!creatingCustomSize)
                    {
                        w = h = 4;
                    }
                    newTexture = new Texture2D(w, h);

                    myColors = CreateFilledArray(solidColor, w * h);
                    newTexture.SetPixels(0, 0, w, h, myColors);
                    break;
                case 1:
                    {
                        if (!creatingCustomSize)
                        {
                            w = 256;
                            h = 4;
                        }
                        newTexture = new Texture2D(w, h);

                        int i = creatingReverse ? w - 1 : 0;
                        int istep = creatingReverse ? -1 : 1;

                        float xstepValue = (1f / Mathf.Max(1, w - 1));
                        float xcurrentStep = 0;
                        for (; i < w && i >= 0; i += istep)
                        {
                            newTexture.SetPixels(i, 0, 1, h, CreateFilledArray(gradientColor.Evaluate(xcurrentStep), h));
                            xcurrentStep += xstepValue;
                        }
                    }
                    break;
                case 2:
                    {
                        if (!creatingCustomSize)
                        {
                            w = 4;
                            h = 256;
                        }
                        newTexture = new Texture2D(w, h);

                        int i = creatingReverse ? h - 1 : 0;
                        int istep = creatingReverse ? -1 : 1;

                        float ystepValue = 1f / Mathf.Max(1, h - 1);
                        float ycurrentStep = 0;
                        for (; i < h && i >= 0; i += istep)
                        {
                            newTexture.SetPixels(0, i, w, 1, CreateFilledArray(gradientColor.Evaluate(ycurrentStep), w));
                            ycurrentStep += ystepValue;
                        }
                    }
                    break;
            }

            newTexture.Apply();
            if (preview) { var result = newTexture; newTexture = null; return result; }
            GetEncoding(newTexture, encoding, out byte[] data, out string ext);
            DestroyImmediate(newTexture);

            ReadyFolder(creatingPath);
            SaveTexture(data, AssetDatabase.GenerateUniqueAssetPath(creatingPath + "/" + FynnsTextureTools.TextureIO.FileName(outputName, "Generated Texture") + ext), true, pingTexture);
            return null;
                    } finally { if (newTexture) DestroyImmediate(newTexture); }
        }

        private void UnpackTexture()
        {
            string destinationPath = GetDestinationFolder(mainTexture);
            if (!destinationPath.StartsWith("Assets")) destinationPath = "Assets/Texture Tools/Generated";
            ReadyFolder(destinationPath);
            string texPath = AssetDatabase.GetAssetPath(mainTexture);
            int x = mainTexture.width, y = mainTexture.height;
            Texture2D newTexture = GetColors(mainTexture, x, y, out Color[] myColors);
            List<System.Tuple<string, string>> copyFromTo = new List<System.Tuple<string, string>>();

            bool isRedPass = true, isGreenPass, isBluePass, isAlphaPass;
            isGreenPass = isBluePass = isAlphaPass = false;
            try
            {
                AssetDatabase.StartAssetEditing();

                do
                {
                    Color[] newColors = new Color[myColors.Length];

                    bool hasAlpha = false;
                    for (int i = 0; i < myColors.Length; i++)
                    {
                        Color currentColor = myColors[i];

                        float r = currentColor.r;
                        float g = currentColor.g;
                        float b = currentColor.b;
                        float a = currentColor.a;

                        if (isRedPass)
                        {
                            g = b = r;
                            a = 1;
                        }
                        if (isGreenPass)
                        {
                            r = b = g;
                            a = 1;
                        }
                        if (isBluePass)
                        {
                            r = g = b;
                            a = 1;
                        }
                        if (isAlphaPass)
                        {
                            r = g = b = a;
                            a = 1;
                            if (currentColor.a != 1)
                                hasAlpha = true;
                        }

                        newColors[i] = new Color(r, g, b, a);
                    }

                    if (isAlphaPass && !hasAlpha)
                    {
                        isAlphaPass = false;
                        goto Skip;
                    }

                    newTexture.SetPixels(newColors);
                    newTexture.Apply();

                    GetEncoding(newTexture, encoding, out byte[] data, out string ext);

                    string newTexturePath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + "/" + mainTexture.name
                        + (isRedPass ? "-Red" : isGreenPass ? "-Green" : isBluePass ? "-Blue" : "-Alpha") + ext);

                    SaveTexture(data, newTexturePath);

                    if (copyImport)
                    {
                        copyFromTo.Add(new System.Tuple<string, string>(texPath, newTexturePath));
                    }

                    if (isAlphaPass)
                        isAlphaPass = false;
                    if (isBluePass)
                    {
                        isBluePass = false;
                        isAlphaPass = true;
                    }
                    if (isGreenPass)
                    {
                        isGreenPass = false;
                        isBluePass = true;
                    }
                    if (isRedPass)
                    {
                        isRedPass = false;
                        isGreenPass = true;
                    }



                    Skip:;

                } while (isRedPass || isGreenPass || isBluePass || isAlphaPass);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                DestroyImmediate(newTexture);
            }
            AssetDatabase.Refresh();
            if (copyImport)
            {
                for (int i = 0; i < copyFromTo.Count; i++)
                {
                    CopyTextureSettings(copyFromTo[i].Item1, copyFromTo[i].Item2);
                }
            }
        }

        public void PackTexture(ChannelTexture[] channels)
        {
            int firstIndex = 0;
            for (int i = 3; i >= 0; i--)
            {
                if (channels[i].texture)
                    firstIndex = i;
            }
            ChannelTexture firstChannel = channels[firstIndex];
            int w = firstChannel.texture.width;
            int h = firstChannel.texture.height;
            PackTexture(channels, AssetDatabase.GetAssetPath(firstChannel.texture), w, h, encoding);
        }

        public static string PackTexture(ChannelTexture[] channels, TexEncoding encodingType, bool refresh=true, bool copyImportSettings=true)
        {
            int firstIndex = -1;
            for (int i = 3; i >= 0; i--)
            {
                if (channels[i].texture)
                    firstIndex = i;
            }
            if (firstIndex < 0)
                return string.Empty;
            ChannelTexture firstChannel = channels[firstIndex];
            int w = firstChannel.texture.width;
            int h = firstChannel.texture.height;
            return PackTexture(channels, AssetDatabase.GetAssetPath(firstChannel.texture), w, h, encodingType,refresh,false,copyImportSettings);
        }

        public static string PackTexture(ChannelTexture[] channels, string destination,int width, int height, TexEncoding encodingType, bool refresh=true,bool overwrite=false, bool copyImportSettings=true)
        {
            int firstIndex = -1;
            for (int i = 3; i >= 0; i--)
            {
                if (channels[i].texture)
                    firstIndex = i;
            }
            if (firstIndex < 0)
                return string.Empty;

            ChannelTexture firstChannel = channels[firstIndex];

            
            Texture2D newTexture = new Texture2D(width, height);
            channels[0].GetChannelColors(width, height, out float[] reds, true);
            channels[1].GetChannelColors(width, height, out float[] greens, true);
            channels[2].GetChannelColors(width, height, out float[] blues, true);
            channels[3].GetChannelColors(width, height, out float[] alphas, true);
            Color[] finalColors = new Color[width*height];

            for (int i=0;i< finalColors.Length;i++)
            {
                finalColors[i].r = (reds!=null) ? reds[i] : 0;
                finalColors[i].g = (greens != null) ? greens[i] : 0;
                finalColors[i].b = (blues != null) ? blues[i] : 0;
                finalColors[i].a = (alphas != null) ? alphas[i] : 1;
            }
            newTexture.SetPixels(finalColors);
            newTexture.Apply();

            GetEncoding(newTexture, encodingType, out byte[] data, out string ext);

            string newTexturePath = GetDestinationFolder(destination)+"/"+System.IO.Path.GetFileNameWithoutExtension(destination)+ext;
            if (!overwrite)
                newTexturePath = AssetDatabase.GenerateUniqueAssetPath(newTexturePath);
            SaveTexture(data, newTexturePath);
            DestroyImmediate(newTexture);
            if (refresh)
                AssetDatabase.Refresh();
            

            if (copyImportSettings)
            {
                CopyTextureSettings(AssetDatabase.GetAssetPath(firstChannel.texture), newTexturePath);
            }
            return newTexturePath;
        }

        private static void CopyTextureSettings(string from, string to)
        {
            TextureImporter source = (TextureImporter)AssetImporter.GetAtPath(from);
            TextureImporterSettings sourceSettings = new TextureImporterSettings();
            source.ReadTextureSettings(sourceSettings);
            
            TextureImporter destination = (TextureImporter)AssetImporter.GetAtPath(to);
            destination.SetTextureSettings(sourceSettings);
            destination.maxTextureSize = source.maxTextureSize;
            destination.textureCompression = source.textureCompression;
            destination.crunchedCompression = source.crunchedCompression;
            destination.SaveAndReimport();
        }

        private static string GetDestinationFolder(Object o)
        {
            string path = AssetDatabase.GetAssetPath(o);
            return GetDestinationFolder(path);
        }
        private static string GetDestinationFolder(string path)
        {
            return path.Substring(0, path.LastIndexOf('/'));
        }

        private void ResetDimensions()
        {
            if (mainTexture)
            {
                texHeight = mainTexture.height;
                texWidth = mainTexture.width;
            }
        }

        private void SetColorIcon(bool value)
        {
            if (value)
                GUI.backgroundColor = Color.green;
            else
                GUI.backgroundColor = Color.grey;
        }

        private void OnEnable()
        {
            resetIcon = new GUIContent(EditorGUIUtility.IconContent("d_Refresh")) { tooltip = "Reset Dimensions" };
            creatingPath = EditorPrefs.GetString("TextureUtilityCreatingPath", "Assets/Texture Tools/Generated");

            foreach (var channel in channelTextures)
            {
                channel.SetMode(EditorPrefs.GetInt("TextureUtilityChannel" + channel.name, (int)channel.mode));
            }
        }

        private static T[] CreateFilledArray<T>(T variable,int length)
        {
            T[] myArray = new T[length];
            for (int i=0;i< myArray.Length;i++)
            {
                myArray[i] = variable;
            }
            return myArray;
        }

        private static T[] ReverseArray<T>(T[] array)
        {
            T[] reversed = new T[array.Length];
            int index = array.Length - 1;
            for (int i = 0; i < reversed.Length; i++)
            {
                reversed[i] = array[index];
                index--;
            }
            return reversed;
        }

        public static void AssetFolderPath(ref string variable, string title, string playerpref)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField(title, variable);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    var folder = EditorUtility.OpenFolderPanel(title, Application.dataPath, "");
                    if (!string.IsNullOrEmpty(folder))
                    {
                        var relative = FileUtil.GetProjectRelativePath(folder).Replace('\\', '/');
                        if (relative == "Assets" || relative.StartsWith("Assets/")) { variable = relative; EditorPrefs.SetString(playerpref, variable); }
                        else EditorUtility.DisplayDialog("Save folder", "Choose a folder inside this project's Assets folder.", "OK");
                    }
                }
            }
        }

        public static void ReadyFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);  
        }

        private static void Credit()
        {
            GUIStyle creditLabelStyle = new GUIStyle(GUI.skin.label) { richText = true };
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<b>Made by Fynn</b>", creditLabelStyle))
                    Application.OpenURL("https://github.com/Fynn9563");
            }
        }
    }
}
