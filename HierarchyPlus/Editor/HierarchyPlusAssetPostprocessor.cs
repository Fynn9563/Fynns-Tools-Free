using UnityEditor;
using UnityEngine;
using System.Linq;

namespace HierarchyPlus
{
    public class HierarchyPlusAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool shouldRefresh = false;
            
            foreach (string asset in importedAssets)
            {
                if (IsIconAsset(asset))
                {
                    shouldRefresh = true;
                    break;
                }
            }
            
            if (!shouldRefresh)
            {
                foreach (string asset in deletedAssets)
                {
                    if (IsIconAsset(asset))
                    {
                        shouldRefresh = true;
                        break;
                    }
                }
            }
            
            if (!shouldRefresh)
            {
                foreach (string asset in movedAssets)
                {
                    if (IsIconAsset(asset))
                    {
                        shouldRefresh = true;
                        break;
                    }
                }
            }
            
            if (!shouldRefresh)
            {
                foreach (string asset in importedAssets)
                {
                    if (asset.EndsWith(".cs") || asset.EndsWith(".dll"))
                    {
                        shouldRefresh = true;
                        break;
                    }
                }
            }
            
            if (shouldRefresh)
            {
                EditorApplication.delayCall += () =>
                {
                    if (HierarchyPlus.RefreshIconsDelayed != null)
                        HierarchyPlus.RefreshIconsDelayed();
                };
            }
        }
        
        private static bool IsIconAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
                
            string lowerPath = assetPath.ToLower();
            
            if (lowerPath.EndsWith(".png") || lowerPath.EndsWith(".jpg") || 
                lowerPath.EndsWith(".jpeg") || lowerPath.EndsWith(".tga") ||
                lowerPath.EndsWith(".tif") || lowerPath.EndsWith(".tiff") ||
                lowerPath.EndsWith(".psd") || lowerPath.EndsWith(".svg"))
            {
                if (lowerPath.Contains("icon") || lowerPath.Contains("gizmo") || 
                    lowerPath.Contains("editor") || lowerPath.Contains("customicons"))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}