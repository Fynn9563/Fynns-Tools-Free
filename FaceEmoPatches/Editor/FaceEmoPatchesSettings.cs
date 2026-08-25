#if VRC_SDK_VRCSDK3
using UnityEditor;

namespace FynnsTools.FaceEmoPatches
{
	// Which patches are on is stored in EditorPrefs rather than in the project, because
	// it changes how the editor behaves for one person and nothing in the built avatar
	// depends on it. Both default to on, since installing a tool called FaceEmo Patches is
	// already the request for the patches.
	//
	// Turning one off still sticks: the default below only applies while the key is absent,
	// and SetEnabled writes the key explicitly the moment a tick box is touched.
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
