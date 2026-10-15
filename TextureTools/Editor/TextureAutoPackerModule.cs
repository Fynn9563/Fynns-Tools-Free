using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TextureUtility
{
    [CreateAssetMenu(fileName = "Texture Pack Recipes", menuName = "Fynn's Tools/Texture Pack Recipes")]
    public class TextureAutoPackerModule : ScriptableObject
    {
        public List<AutoPackedTexture> packedTextures = new List<AutoPackedTexture>();
    }

    [Serializable]
    public class AutoPackedTexture
    {
        public bool expanded = true;
        public bool forward = true;
        public string name = "Packed Texture";
        public ChannelTexture[] channels = { new ChannelTexture("Red", 0), new ChannelTexture("Green", 1), new ChannelTexture("Blue", 2), new ChannelTexture("Alpha", 3) };
        public string[] channelsHashes = new string[5];
        public bool forceModified;
        public Texture2D packed;
        public TextureUtility.TexEncoding encoding;
        public int jpgQuality = 75;
        public bool automatic = true;
        public bool linear = true;
        public int width = 256, height = 256;
        public bool sourceSize = true;
        public string outputPath;
        public string fingerprint;

        public string Output
        {
            get
            {
                string path = packed ? AssetDatabase.GetAssetPath(packed) : outputPath;
                if (string.IsNullOrEmpty(path)) return path;
                return System.IO.Path.ChangeExtension(path, encoding == TextureUtility.TexEncoding.SaveAsJPG ? ".jpg" : encoding == TextureUtility.TexEncoding.SaveAsTGA ? ".tga" : ".png").Replace('\\', '/');
            }
        }
        public string Fingerprint()
        {
            var b = new StringBuilder();
            b.Append((int)encoding).Append('|').Append(jpgQuality).Append('|').Append(linear).Append('|').Append(width).Append('|').Append(height).Append('|').Append(sourceSize).Append('|').Append(Output);
            foreach (var channel in channels)
            {
                b.Append('|').Append((int)channel.mode).Append('|').Append(channel.invert).Append('|').Append(JsonUtility.ToJson(channel.defaultColor));
                if (channel.texture)
                {
                    var path = AssetDatabase.GetAssetPath(channel.texture);
                    b.Append('|').Append(path).Append('|').Append(AssetDatabase.GetAssetDependencyHash(path)).Append('|').Append(channel.texture.imageContentsHash);
                }
            }
            return Hash128.Compute(b.ToString()).ToString();
        }
        public bool WasModified() { return forceModified || !packed || fingerprint != Fingerprint(); }
        public Texture2D Render(int maxSize = 0)
        {
            int w = width, h = height;
            bool foundSize = false;
            foreach (var channel in channels)
            {
                if (sourceSize && channel.texture && !foundSize) { w = channel.texture.width; h = channel.texture.height; foundSize = true; }
            }
            FynnsTextureTools.TextureIO.ValidateSize(w, h);
            if (maxSize > 0) { float scale = Mathf.Min(1, (float)maxSize / Mathf.Max(w, h)); w = Mathf.Max(1, Mathf.RoundToInt(w * scale)); h = Mathf.Max(1, Mathf.RoundToInt(h * scale)); }
            Texture2D result = null;
            try
            {
                var values = new float[4][];
                for (int c = 0; c < 4; c++)
                {
                    var channel = channels[c];
                    Texture2D copy = null;
                    try
                    {
                        var colors = channel.texture ? (copy = FynnsTextureTools.TextureIO.Read(channel.texture, w, h, linear)).GetPixels() : null;
                        values[c] = new float[w * h];
                        for (int i = 0; i < values[c].Length; i++)
                        {
                            var color = colors == null ? channel.defaultColor : colors[i];
                            float value = color[(int)channel.mode];
                            values[c][i] = channel.invert ? 1 - value : value;
                        }
                    }
                    finally { if (copy) UnityEngine.Object.DestroyImmediate(copy); }
                }
                result = new Texture2D(w, h, TextureFormat.RGBA32, false, linear);
                var pixels = new Color[w * h];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(values[0][i], values[1][i], values[2][i], values[3][i]);
                result.SetPixels(pixels); result.Apply();
                var ready = result; result = null; return ready;
            }
            finally { if (result) UnityEngine.Object.DestroyImmediate(result); }
        }
        public string Pack()
        {
            if (string.IsNullOrEmpty(Output)) throw new InvalidOperationException("Choose an output file first.");
            string ext = encoding == TextureUtility.TexEncoding.SaveAsJPG ? ".jpg" : encoding == TextureUtility.TexEncoding.SaveAsTGA ? ".tga" : ".png";
            string path = System.IO.Path.ChangeExtension(Output, ext).Replace('\\', '/');
            FynnsTextureTools.TextureIO.ValidateOutput(path);
            foreach (var channel in channels)
                if (channel.texture && AssetDatabase.GetAssetPath(channel.texture) == path) throw new InvalidOperationException("The output cannot also be an input channel.");
            Texture2D result = Render();
            try
            {
                var bytes = encoding == TextureUtility.TexEncoding.SaveAsJPG ? result.EncodeToJPG(jpgQuality) : encoding == TextureUtility.TexEncoding.SaveAsTGA ? result.EncodeToTGA() : result.EncodeToPNG();
                FynnsTextureTools.TextureIO.Write(bytes, path, false, linear);
                packed = AssetDatabase.LoadAssetAtPath<Texture2D>(path); outputPath = path;
                fingerprint = Fingerprint(); forceModified = false;
                return path;
            }
            finally { if (result) UnityEngine.Object.DestroyImmediate(result); }
        }
    }
}
