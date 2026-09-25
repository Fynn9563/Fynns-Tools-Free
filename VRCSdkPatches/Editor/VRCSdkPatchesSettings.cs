#if VRC_SDK_VRCSDK3
using UnityEditor;

namespace FynnsTools.VRCSdkPatches
{
	// Editor-wide, defaults to on.
	internal static class VRCSdkPatchesSettings
	{
		private const string KeyPrefix = "FynnsTools.VRCSdkPatches.";

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
