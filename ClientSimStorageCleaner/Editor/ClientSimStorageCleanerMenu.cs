using UnityEditor;
using UnityEngine;

namespace FynnsTools.ClientSimStorageCleaner
{
	public static class ClientSimStorageCleanerMenu
	{
		private const string ToolsMenu = "Tools/Fynn's Tools/ClientSim Storage Cleaner/";

		[MenuItem(ToolsMenu + "Settings", false, 20)]
		private static void OpenSettings()
		{
			ClientSimStorageCleanerWindow.Open();
		}

		[MenuItem(ToolsMenu + "Clear Now", false, 40)]
		private static void ClearNow()
		{
			if (!ClientSimStorage.Clear(out string error))
			{
				Debug.LogWarning("[ClientSim Storage Cleaner] Could not delete " + ClientSimStorage.FolderPath + ": " + error);
			}
		}

		// Not while playing.
		[MenuItem(ToolsMenu + "Clear Now", true)]
		private static bool CanClearNow()
		{
			return !EditorApplication.isPlayingOrWillChangePlaymode;
		}
	}
}
