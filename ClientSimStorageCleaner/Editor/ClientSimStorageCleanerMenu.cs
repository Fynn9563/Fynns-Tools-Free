using UnityEditor;
using UnityEngine;

namespace FynnsTools.ClientSimStorageCleaner
{
	public static class ClientSimStorageCleanerMenu
	{
		private const string ToolsMenu = "Tools/Fynn's Tools/ClientSim Storage Cleaner/";

		[MenuItem(ToolsMenu + "Settings", false, 93)]
		private static void OpenSettings()
		{
			ClientSimStorageCleanerWindow.Open();
		}

		[MenuItem(ToolsMenu + "Clear Now", false, 94)]
		private static void ClearNow()
		{
			if (!ClientSimStorage.Clear(out string error))
			{
				Debug.LogWarning("[ClientSim Storage Cleaner] Could not delete " + ClientSimStorage.FolderPath + ": " + error);
			}
		}

		[MenuItem(ToolsMenu + "Clear Now", true, 94)]
		private static bool CanClearNow()
		{
			return !EditorApplication.isPlayingOrWillChangePlaymode;
		}

		[MenuItem(ToolsMenu + "Reset Runtime Caches Now", false, 95)]
		private static void ResetRuntimeCachesNow()
		{
			ClientSimRuntimeCaches.Reset();
		}

		[MenuItem(ToolsMenu + "Reset Runtime Caches Now", true, 95)]
		private static bool CanResetRuntimeCachesNow()
		{
			return !EditorApplication.isPlayingOrWillChangePlaymode;
		}
	}
}
