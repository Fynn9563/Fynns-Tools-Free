using UnityEditor;

namespace FynnsTools.ClientSimStorageCleaner
{
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
