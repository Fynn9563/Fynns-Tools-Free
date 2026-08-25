#if VRC_SDK_VRCSDK3
using UnityEditor;

namespace FynnsTools.FaceEmoPatches
{
	// Which patches are on is stored in EditorPrefs rather than in the project, because
	// it changes how the editor behaves for one person and nothing in the built avatar
	// depends on it. Nothing is enabled by default: rewriting another package's methods
	// is the kind of thing that should happen only after someone asks for it.
	internal static class FaceEmoPatchesSettings
	{
		private const string KeyPrefix = "FynnsTools.FaceEmoPatches.";

		public static bool IsEnabled(string id)
		{
			return EditorPrefs.GetBool(KeyPrefix + id, false);
		}

		public static void SetEnabled(string id, bool enabled)
		{
			EditorPrefs.SetBool(KeyPrefix + id, enabled);
		}
	}
}
#endif
