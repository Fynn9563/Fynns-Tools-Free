using UnityEditor;

namespace FynnsTools.UnityPetFramework
{
	public static class UnityPetFrameworkMenu
	{
		private const string ToolsMenu = "Tools/Fynn's Tools/Unity Pet Framework";

		[MenuItem(ToolsMenu, false, 20)]
		private static void OpenWindow()
		{
			UnityPetFrameworkWindow.Open();
		}
	}
}
