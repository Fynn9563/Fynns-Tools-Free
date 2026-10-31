using UnityEditor;

namespace FynnsTools.ClientSimStorageCleaner
{
	internal static class ClientSimStorageCleanerSettings
	{
		private const string EnabledKey = "FynnsTools.ClientSimStorageCleaner.Enabled";
		private const string ResetRuntimeCachesKey = "FynnsTools.ClientSimStorageCleaner.ResetRuntimeCaches";
		private const string ProtectAssemblyReloadsKey = "FynnsTools.ClientSimStorageCleaner.ProtectAssemblyReloads";

		public static bool ProtectAssemblyReloads
		{
			get { return EditorPrefs.GetBool(ProtectAssemblyReloadsKey, true); }
			set { EditorPrefs.SetBool(ProtectAssemblyReloadsKey, value); }
		}

		public static bool ResetRuntimeCaches
		{
			get { return EditorPrefs.GetBool(ResetRuntimeCachesKey, true); }
			set { EditorPrefs.SetBool(ResetRuntimeCachesKey, value); }
		}

		public static bool Enabled
		{
			get { return EditorPrefs.GetBool(EnabledKey, true); }
			set { EditorPrefs.SetBool(EnabledKey, value); }
		}
	}
}
