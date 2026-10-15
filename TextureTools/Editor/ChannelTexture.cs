using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TextureUtility
{
    [System.Serializable]
    public class ChannelTexture
    {
        public string name;
        public Texture2D texture;
        public bool invert;
        public Color defaultColor;
        public ColorMode mode = ColorMode.Red;

        public enum ColorMode
        {
            Red,
            Green,
            Blue,
            Alpha
        }

        public ChannelTexture(string n, int mode)
        {
            name = n;
            SetMode(mode, true);
            if (n == "Alpha")
                defaultColor = Color.white;
        }

        public void SetMode(int i, bool ignoreSave = false)
        {
            switch (i)
            {
                case 0:
                    mode = ColorMode.Red;
                    break;
                case 1:
                    mode = ColorMode.Green;
                    break;
                case 2:
                    mode = ColorMode.Blue;
                    break;
                case 3:
                    mode = ColorMode.Alpha;
                    break;
            }

            if (!ignoreSave)
            {
                EditorPrefs.SetInt("TextureUtilityChannel" + name, i);
            }
        }

        public Texture2D GetChannelColors(int width, int height, out float[] colors, bool unloadTempTexture)
        {
            Texture2D textureToUse;
            if (texture)
                textureToUse = texture;
            else
            {
                textureToUse = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                textureToUse.SetPixel(0, 0, defaultColor);
                textureToUse.Apply();
            }

            Texture2D newTexture = TextureUtility.GetColors(textureToUse, width, height, out Color[] myColors, unloadTempTexture);
            colors = myColors.Select(c =>
            {
                switch (mode)
                {
                    case ColorMode.Red:
                        return c.r;
                    case ColorMode.Green:
                        return c.g;
                    case ColorMode.Blue:
                        return c.b;
                    default:
                        return c.a;
                }
            }).ToArray();
            if (invert)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = 1 - colors[i];
                }
            }

            if (!texture && unloadTempTexture)
                Object.DestroyImmediate(textureToUse);

            return newTexture;

        }

        public void DrawGUI()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(name + " Output", EditorStyles.boldLabel);
                texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false);
                mode = (ColorMode)EditorGUILayout.EnumPopup("Read Channel", mode);
                if (!texture) defaultColor = EditorGUILayout.ColorField("Constant", defaultColor);
                invert = EditorGUILayout.Toggle("Invert", invert);
            }
        }
    }
}
