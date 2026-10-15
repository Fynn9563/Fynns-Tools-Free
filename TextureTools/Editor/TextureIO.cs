using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTextureTools
{
    public static class TextureIO
    {
        public static string FileName(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) value = fallback;
            var invalid = Path.GetInvalidFileNameChars();
            var name = new string(value.ToCharArray().Where(c => !invalid.Contains(c)).ToArray()).Trim().TrimEnd('.');
            if (string.IsNullOrWhiteSpace(name)) name = "Generated Texture";
            if (name.Length > 100) name = name.Substring(0, 100);
            return name;
        }

        public static void ValidateSize(int width, int height)
        {
            if (width < 1 || height < 1 || width > 8192 || height > 8192)
                throw new ArgumentOutOfRangeException("size", "Texture dimensions must be between 1 and 8192.");
        }

        public static Texture2D Read(Texture source, int width, int height, bool linear)
        {
            if (!source) throw new ArgumentNullException("source");
            ValidateSize(width, height);
            var previous = RenderTexture.active;
            bool srgb = GL.sRGBWrite;
            RenderTexture temporary = null;
            Texture2D result = null;
            try
            {
                temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32,
                    linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
                GL.sRGBWrite = !linear && QualitySettings.activeColorSpace == ColorSpace.Linear;
                Graphics.Blit(source, temporary);
                RenderTexture.active = temporary;
                result = new Texture2D(width, height, TextureFormat.RGBA32, false, linear);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply();
                return result;
            }
            catch { if (result) UnityEngine.Object.DestroyImmediate(result); throw; }
            finally
            {
                RenderTexture.active = previous;
                GL.sRGBWrite = srgb;
                if (temporary) RenderTexture.ReleaseTemporary(temporary);
            }
        }

        public static void ValidateOutput(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Choose an output file.");
            var project = Path.GetFullPath(Application.dataPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var full = Path.GetFullPath(path);
            if (!path.Replace('\\', '/').StartsWith("Assets/", StringComparison.Ordinal) ||
                !full.StartsWith(project, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Output must be inside this project's Assets folder.");
        }

        public static void Write(byte[] data, string path, bool normal = false, bool linear = false)
        {
            ValidateOutput(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, data);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer)
            {
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normal && !linear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 8192;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.SaveAndReimport();
            }
        }

        public static void CopySampling(Texture2D source, string path)
        {
            var from = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source)) as TextureImporter;
            var to = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!from || !to) return;
            to.wrapModeU = from.wrapModeU; to.wrapModeV = from.wrapModeV;
            to.filterMode = from.filterMode; to.anisoLevel = from.anisoLevel;
            to.mipmapEnabled = from.mipmapEnabled;
            to.SaveAndReimport();
        }
    }
}
