using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace TextureUtility
{
    public class TextureAutoPackerData : ScriptableObject
    {
        public bool active = true;
        public List<TextureAutoPackerModule> activeModules = new List<TextureAutoPackerModule>();
        public const string SavePath = "Assets/FynnsToolkit/Saved Data/Texture Utility/TextureAutoPackerData.asset";
        public static TextureAutoPackerData Find() { return AssetDatabase.LoadAssetAtPath<TextureAutoPackerData>(SavePath); }
        public static TextureAutoPackerData GetInstance()
        {
            var data = Find();
            if (!data)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SavePath));
                data = CreateInstance<TextureAutoPackerData>();
                AssetDatabase.CreateAsset(data, SavePath);
            }
            return data;
        }
    }
}
