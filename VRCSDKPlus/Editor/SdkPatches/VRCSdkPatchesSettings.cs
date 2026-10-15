#if VRC_SDK_VRCSDK3 && !UDON
using UnityEditor;

namespace FynnsTools.VRCSDKPlus
{
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
