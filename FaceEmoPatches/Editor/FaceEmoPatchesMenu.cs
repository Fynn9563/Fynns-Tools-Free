#if VRC_SDK_VRCSDK3
using UnityEditor;

namespace FynnsTools.FaceEmoPatches
{
	public static class FaceEmoPatchesMenu
	{
		private const string ToolsMenu = "Tools/Fynn's Tools/FaceEmo Patches/Settings";

		[MenuItem(ToolsMenu, false, 33)]
		private static void OpenSettings()
		{
			FaceEmoPatchesWindow.Open();
		}
	}
}
#endif
