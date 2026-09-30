using UnityEditor;

namespace FynnsTools.ClientSimStorageCleaner
{
	// On by default.
	internal static class ClientSimStorageCleanerSettings
	{
		private const string EnabledKey = "FynnsTools.ClientSimStorageCleaner.Enabled";

		public static bool Enabled
		{
			get { return EditorPrefs.GetBool(EnabledKey, true); }
			set { EditorPrefs.SetBool(EnabledKey, value); }
		}
	}
}
