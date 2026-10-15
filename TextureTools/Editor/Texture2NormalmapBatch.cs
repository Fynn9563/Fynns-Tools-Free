using UnityEditor;
namespace FynnsToolkit {
 public static class Texture2NormalmapBatch {
  [MenuItem("Assets/Fynn's Tools/Textures/Batch Normal Maps", false, 2000)]
  public static void ConvertToNormalMap() { FynnsTextureTools.TextureToolsWindow.OpenBatch(); }
 }
}