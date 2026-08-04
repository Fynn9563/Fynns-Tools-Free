#if NDMF && VRC_SDK_VRCSDK3
using System;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.GoGoLocoPoseChanger
{
	// Resolves the tool's own folder by locating its assembly definition, so assets
	// load from wherever the tool was installed. Paths are never hardcoded, and a
	// name search is never used: an older copy of this tool in the same project
	// would ship identically named assets, and a search could return that one.
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

				foreach (string guid in AssetDatabase.FindAssets("FynnsTools.GoGoLocoPoseChanger.Editor"))
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					if (!path.EndsWith(AsmdefFileName, StringComparison.Ordinal))
					{
						continue;
					}

					// <root>/Editor/<asmdef>, so step up twice to reach the tool root.
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
