using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Resolves the tool's own folder by locating its assembly definition.
	internal static class UnityPetFrameworkPaths
	{
		private const string AsmdefFileName = "FynnsTools.UnityPetFramework.Editor.asmdef";

		private static string _toolRoot;
		private static string _importedPetsFolder;
		private static string _draftFolder;

		public static string ToolRoot
		{
			get
			{
				if (_toolRoot != null)
				{
					return _toolRoot;
				}

				foreach (string guid in AssetDatabase.FindAssets("FynnsTools.UnityPetFramework.Editor"))
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

				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "Could not locate the tool folder, so the window layout and the bundled pet are unavailable. Was part of the folder moved or deleted?");
				_toolRoot = "";
				return _toolRoot;
			}
		}

		public static string WindowUxmlPath => InToolFolder("Editor/UI/UnityPetFramework.uxml");

		public static string BundledPetsFolder => InToolFolder("Pets");

		// Pets the user imported are not project assets.
		public static string ImportedPetsFolder
		{
			get
			{
				if (_importedPetsFolder != null)
				{
					return _importedPetsFolder;
				}

				string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				_importedPetsFolder = Path.Combine(localAppData, "FynnsTools", "UnityPetFramework", "Pets");
				return _importedPetsFolder;
			}
		}
		public static string DraftFolder
		{
			get
			{
				if (_draftFolder != null) return _draftFolder;

				string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				_draftFolder = Path.Combine(localAppData, "FynnsTools", "UnityPetFramework", "Draft");
				return _draftFolder;
			}
		}

		// Turns a path under the tool folder into one the file APIs can read.
		public static string ToAbsolute(string assetPath)
		{
			if (string.IsNullOrEmpty(assetPath))
			{
				return null;
			}

			// Application.dataPath ends at Assets, and an asset path starts with it.
			string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
			return projectRoot == null ? null : Path.GetFullPath(Path.Combine(projectRoot, assetPath));
		}

		private static string InToolFolder(string relativePath)
		{
			return string.IsNullOrEmpty(ToolRoot) ? null : ToolRoot + "/" + relativePath;
		}
	}
}
