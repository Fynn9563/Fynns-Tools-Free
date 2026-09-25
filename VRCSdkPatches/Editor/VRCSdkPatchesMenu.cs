#if VRC_SDK_VRCSDK3
using UnityEditor;

namespace FynnsTools.VRCSdkPatches
{
	public static class VRCSdkPatchesMenu
	{
		private const string ToolsMenu = "Tools/Fynn's Tools/VRC SDK Patches/Settings";

		[MenuItem(ToolsMenu, false, 20)]
		private static void OpenSettings()
		{
			VRCSdkPatchesWindow.Open();
		}
	}
}
#endif
