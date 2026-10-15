#if NDMF && VRC_SDK_VRCSDK3
using System;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.GoGoLocoPoseChanger
{
	internal static class GoGoLocoPoseChangerPaths
	{
		private const string AsmdefFileName = "FynnsTools.GoGoLocoPoseChanger.Editor.asmdef";

		private static string _toolRoot;

		public static string ToolRoot
		{
			get
			{
				if (_toolRoot != null)
				{
					return _toolRoot;
				}

				var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(GoGoLocoPoseChangerPaths).Assembly);
				if (package != null)
				{
					_toolRoot = package.assetPath;
					return _toolRoot;
				}

				foreach (string guid in AssetDatabase.FindAssets("FynnsTools.GoGoLocoPoseChanger.Editor"))
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					if (!path.EndsWith(AsmdefFileName, StringComparison.Ordinal))
					{
						continue;
					}

					string editorDir = path.Substring(0, path.LastIndexOf('/'));
					_toolRoot = editorDir.Substring(0, editorDir.LastIndexOf('/'));
					return _toolRoot;
				}

				Debug.LogWarning("<color=#5ef02f>[GoGoLoco Pose Changer]</color> Could not locate the tool folder, so its prefab and inspector layout are unavailable. Was part of the folder moved or deleted?");
				_toolRoot = "";
				return _toolRoot;
			}
		}

		public static string PrefabPath => InToolFolder("Runtime/GoGoLocoPoseChanger.prefab");

		public static string InspectorUxmlPath => InToolFolder("Editor/UI/GoGoLocoPoseChangerNdmf.uxml");

		private static string InToolFolder(string relativePath)
		{
			return string.IsNullOrEmpty(ToolRoot) ? null : ToolRoot + "/" + relativePath;
		}
	}
}
#endif
