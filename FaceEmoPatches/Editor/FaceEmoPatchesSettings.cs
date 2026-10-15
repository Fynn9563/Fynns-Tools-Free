#if VRC_SDK_VRCSDK3
using UnityEditor;

namespace FynnsTools.FaceEmoPatches
{
	// Editor-only preferences are per machine and default to on.
	internal static class FaceEmoPatchesSettings
	{
		private const string KeyPrefix = "FynnsTools.FaceEmoPatches.";

		public static bool IsEnabled(string id)
		{
			return EditorPrefs.GetBool(KeyPrefix + id, true);
		}

		public static void SetEnabled(string id, bool enabled)
		{
			EditorPrefs.SetBool(KeyPrefix + id, enabled);
		}
	}
}
#endif
