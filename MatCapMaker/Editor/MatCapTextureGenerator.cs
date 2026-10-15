using UnityEngine;
using UnityEditor;

namespace MatCapMaker
{
    public static class MatCapTextureGenerator
    {
        #region Environment Maps (Gradient-based)

        public static Texture2D GenerateGradientEnvironment(int size, Color leftColor, Color rightColor, string name = "Gradient Environment")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = (float)x / (size - 1);
                    Color c = Color.Lerp(leftColor, rightColor, t);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        public static Texture2D GenerateRadialEnvironment(int size, Color centerColor, Color edgeColor, string name = "Radial Environment")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / maxDist);
                    Color c = Color.Lerp(centerColor, edgeColor, t);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        public static Texture2D GenerateStudioEnvironment(int size, string name = "Studio Environment")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            Color skyColor = new Color(0.9f, 0.92f, 0.95f, 1f);
            Color horizonColor = new Color(0.7f, 0.72f, 0.75f, 1f);
            Color groundColor = new Color(0.15f, 0.15f, 0.18f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = (float)y / (size - 1);
                    Color c;

                    if (t > 0.5f)
                    {
                        float skyT = (t - 0.5f) * 2f;
                        c = Color.Lerp(horizonColor, skyColor, skyT);
                    }
                    else
                    {
                        float groundT = t * 2f;
                        c = Color.Lerp(groundColor, horizonColor, groundT);
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        public static Texture2D GenerateIridescentEnvironment(int size, string name = "Iridescent Environment")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = ((float)x / (size - 1) + (float)y / (size - 1)) * 0.5f;
                    Color c = Color.HSVToRGB(t, 0.6f, 0.9f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        public static Texture2D GenerateSunsetEnvironment(int size, string name = "Sunset Environment")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            Color deepOrange = new Color(0.95f, 0.4f, 0.15f, 1f);
            Color brightYellow = new Color(1f, 0.9f, 0.5f, 1f);
            Color purple = new Color(0.4f, 0.2f, 0.5f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float t = (float)y / (size - 1);
                    Color c;

                    if (t > 0.6f)
                    {
                        float upper = (t - 0.6f) / 0.4f;
                        c = Color.Lerp(brightYellow, purple, upper);
                    }
                    else
                    {
                        float lower = t / 0.6f;
                        c = Color.Lerp(deepOrange, brightYellow, lower);
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        #endregion

        #region Noise Textures

        public static Texture2D GeneratePerlinNoise(int size, float scale, string name = "Perlin Noise")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            float offsetX = Random.Range(0f, 10000f);
            float offsetY = Random.Range(0f, 10000f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float xCoord = offsetX + (float)x / size * scale;
                    float yCoord = offsetY + (float)y / size * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    Color c = new Color(sample, sample, sample, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D GenerateFractalNoise(int size, float scale, int octaves, float persistence, string name = "Fractal Noise")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            float offsetX = Random.Range(0f, 10000f);
            float offsetY = Random.Range(0f, 10000f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;
                    float maxValue = 0f;

                    for (int i = 0; i < octaves; i++)
                    {
                        float xCoord = offsetX + (float)x / size * scale * frequency;
                        float yCoord = offsetY + (float)y / size * scale * frequency;
                        float perlinValue = Mathf.PerlinNoise(xCoord, yCoord);

                        noiseHeight += perlinValue * amplitude;
                        maxValue += amplitude;

                        amplitude *= persistence;
                        frequency *= 2f;
                    }

                    float sample = noiseHeight / maxValue;
                    Color c = new Color(sample, sample, sample, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D GenerateCellularNoise(int size, int cellCount, string name = "Cellular Noise")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            Vector2[] cellPoints = new Vector2[cellCount * cellCount];
            for (int cy = 0; cy < cellCount; cy++)
            {
                for (int cx = 0; cx < cellCount; cx++)
                {
                    float cellX = (cx + Random.Range(0.2f, 0.8f)) / cellCount;
                    float cellY = (cy + Random.Range(0.2f, 0.8f)) / cellCount;
                    cellPoints[cy * cellCount + cx] = new Vector2(cellX, cellY);
                }
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = (float)x / size;
                    float py = (float)y / size;
                    Vector2 point = new Vector2(px, py);

                    float minDist = float.MaxValue;
                    foreach (var cellPoint in cellPoints)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                            {
                                Vector2 wrapped = cellPoint + new Vector2(ox, oy);
                                float dist = Vector2.Distance(point, wrapped);
                                if (dist < minDist)
                                    minDist = dist;
                            }
                        }
                    }

                    float maxPossibleDist = 1f / cellCount;
                    float sample = Mathf.Clamp01(minDist / maxPossibleDist);

                    Color c = new Color(sample, sample, sample, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D GenerateMarbleNoise(int size, float scale, float turbulence, string name = "Marble Noise")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            float offsetX = Random.Range(0f, 10000f);
            float offsetY = Random.Range(0f, 10000f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float xCoord = offsetX + (float)x / size * scale;
                    float yCoord = offsetY + (float)y / size * scale;

                    float noise = 0f;
                    float amp = 1f;
                    float freq = 1f;
                    for (int i = 0; i < 4; i++)
                    {
                        noise += Mathf.PerlinNoise(xCoord * freq, yCoord * freq) * amp;
                        amp *= 0.5f;
                        freq *= 2f;
                    }

                    float marble = Mathf.Sin((float)x / size * 10f + noise * turbulence);
                    float sample = (marble + 1f) * 0.5f;

                    Color c = new Color(sample, sample, sample, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D GenerateBrushedMetal(int size, float intensity, string name = "Brushed Metal")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            float offsetX = Random.Range(0f, 10000f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise1 = Mathf.PerlinNoise(offsetX + (float)x / size * 100f, (float)y / size * 2f);
                    float noise2 = Mathf.PerlinNoise(offsetX + (float)x / size * 200f, (float)y / size * 4f) * 0.5f;

                    float sample = 0.5f + (noise1 + noise2 - 0.75f) * intensity;
                    sample = Mathf.Clamp01(sample);

                    Color c = new Color(sample, sample, sample, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        public static Texture2D GenerateGrainNoise(int size, string name = "Grain Noise")
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float sample = Random.Range(0f, 1f);
                    Color c = new Color(sample, sample, sample, 1f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        #endregion

        #region Utility Methods

        public static string SaveTextureAsAsset(Texture2D texture, string folderPath, string fileName)
        {
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{fileName}.png");

            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(fullPath, pngData);

            AssetDatabase.ImportAsset(fullPath);

            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = texture.wrapMode;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return fullPath;
        }

        #endregion
    }

    public sealed class MatCapTextureGeneratorPanel : System.IDisposable
    {
        private enum TextureType
        {
            GradientEnvironment,
            RadialEnvironment,
            StudioEnvironment,
            IridescentEnvironment,
            SunsetEnvironment,
            PerlinNoise,
            FractalNoise,
            CellularNoise,
            MarbleNoise,
            BrushedMetal,
            GrainNoise
        }

        private TextureType selectedType = TextureType.StudioEnvironment;
        private int textureSize = 256;
        private Texture2D previewTexture;
        private string savePath = "Assets/MatCaps/Textures";

        private Color color1 = Color.white;
        private Color color2 = Color.gray;
        private float noiseScale = 5f;
        private int octaves = 4;
        private float persistence = 0.5f;
        private int cellCount = 8;
        private float turbulence = 5f;
        private float intensity = 1f;

        private readonly System.Action repaint;
        private readonly System.Action<Texture2D, int> applyTexture;

        public MatCapTextureGeneratorPanel(System.Action repaint, System.Action<Texture2D, int> applyTexture)
        {
            this.repaint = repaint;
            this.applyTexture = applyTexture;
            GeneratePreview();
        }

        public void DrawGUI()
        {
            GUILayout.Label("Texture Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            EditorGUI.BeginChangeCheck();
            selectedType = (TextureType)EditorGUILayout.EnumPopup("Pattern", selectedType);
            textureSize = EditorGUILayout.IntPopup("Resolution", textureSize,
                new[] { "64", "128", "256", "512", "1024" }, new[] { 64,128,256,512,1024 });
            DrawTypeParameters();
            if (EditorGUI.EndChangeCheck()) GeneratePreview();
            GUILayout.Space(10);
            if (previewTexture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(200,200,GUILayout.ExpandWidth(true));
                rect.x += (rect.width-200)*.5f; rect.width=200;
                EditorGUI.DrawPreviewTexture(rect,previewTexture);
            }
            GUILayout.Space(8);
            if (GUILayout.Button("Regenerate",GUILayout.Height(26))) GeneratePreview();
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use as Environment")) applyTexture(previewTexture,0);
                if (GUILayout.Button("Use as Base Texture")) applyTexture(previewTexture,1);
                if (GUILayout.Button("Use as Noise")) applyTexture(previewTexture,2);
            }
            GUILayout.Space(12);
            using (new GUILayout.HorizontalScope())
            {
                savePath = EditorGUILayout.TextField("Save To",savePath);
                if (GUILayout.Button("...",GUILayout.Width(28)))
                {
                    string chosen=EditorUtility.OpenFolderPanel("Save Texture",savePath,"");
                    string relative=FileUtil.GetProjectRelativePath(chosen);
                    if (relative=="Assets" || relative.StartsWith("Assets/")) savePath=relative;
                }
            }
            if (GUILayout.Button("Save Texture as PNG",GUILayout.Height(28))) SaveTexture();
        }

        private void DrawTypeParameters()
        {
            switch (selectedType)
            {
                case TextureType.GradientEnvironment:
                    color1 = EditorGUILayout.ColorField("Left Color", color1);
                    color2 = EditorGUILayout.ColorField("Right Color", color2);
                    break;

                case TextureType.RadialEnvironment:
                    color1 = EditorGUILayout.ColorField("Center Color", color1);
                    color2 = EditorGUILayout.ColorField("Edge Color", color2);
                    break;

                case TextureType.PerlinNoise:
                    noiseScale = EditorGUILayout.Slider("Scale", noiseScale, 1f, 20f);
                    break;

                case TextureType.FractalNoise:
                    noiseScale = EditorGUILayout.Slider("Scale", noiseScale, 1f, 20f);
                    octaves = EditorGUILayout.IntSlider("Octaves", octaves, 1, 8);
                    persistence = EditorGUILayout.Slider("Persistence", persistence, 0.1f, 1f);
                    break;

                case TextureType.CellularNoise:
                    cellCount = EditorGUILayout.IntSlider("Cell Count", cellCount, 2, 16);
                    break;

                case TextureType.MarbleNoise:
                    noiseScale = EditorGUILayout.Slider("Scale", noiseScale, 1f, 20f);
                    turbulence = EditorGUILayout.Slider("Turbulence", turbulence, 1f, 20f);
                    break;

                case TextureType.BrushedMetal:
                    intensity = EditorGUILayout.Slider("Intensity", intensity, 0.1f, 2f);
                    break;

            }
        }

        private void GeneratePreview()
        {
            if (previewTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(previewTexture);
            }

            switch (selectedType)
            {
                case TextureType.GradientEnvironment:
                    previewTexture = MatCapTextureGenerator.GenerateGradientEnvironment(textureSize, color1, color2);
                    break;
                case TextureType.RadialEnvironment:
                    previewTexture = MatCapTextureGenerator.GenerateRadialEnvironment(textureSize, color1, color2);
                    break;
                case TextureType.StudioEnvironment:
                    previewTexture = MatCapTextureGenerator.GenerateStudioEnvironment(textureSize);
                    break;
                case TextureType.IridescentEnvironment:
                    previewTexture = MatCapTextureGenerator.GenerateIridescentEnvironment(textureSize);
                    break;
                case TextureType.SunsetEnvironment:
                    previewTexture = MatCapTextureGenerator.GenerateSunsetEnvironment(textureSize);
                    break;
                case TextureType.PerlinNoise:
                    previewTexture = MatCapTextureGenerator.GeneratePerlinNoise(textureSize, noiseScale);
                    break;
                case TextureType.FractalNoise:
                    previewTexture = MatCapTextureGenerator.GenerateFractalNoise(textureSize, noiseScale, octaves, persistence);
                    break;
                case TextureType.CellularNoise:
                    previewTexture = MatCapTextureGenerator.GenerateCellularNoise(textureSize, cellCount);
                    break;
                case TextureType.MarbleNoise:
                    previewTexture = MatCapTextureGenerator.GenerateMarbleNoise(textureSize, noiseScale, turbulence);
                    break;
                case TextureType.BrushedMetal:
                    previewTexture = MatCapTextureGenerator.GenerateBrushedMetal(textureSize, intensity);
                    break;
                case TextureType.GrainNoise:
                    previewTexture = MatCapTextureGenerator.GenerateGrainNoise(textureSize);
                    break;
            }

            repaint();
        }

        private void SaveTexture()
        {
            if (previewTexture == null)
            {
                GeneratePreview();
            }

            string fileName = $"{selectedType}_{textureSize}";
            string path = MatCapTextureGenerator.SaveTextureAsAsset(previewTexture, savePath, fileName);

            Debug.Log($"<color=#5ef02f>[MatCap Texture Generator]</color> Texture saved to: {path}");

            var savedAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (savedAsset != null)
            {
                EditorGUIUtility.PingObject(savedAsset);
            }
        }

        public void Dispose()
        {
            if (previewTexture != null) UnityEngine.Object.DestroyImmediate(previewTexture);
            previewTexture = null;
        }
    }
}
