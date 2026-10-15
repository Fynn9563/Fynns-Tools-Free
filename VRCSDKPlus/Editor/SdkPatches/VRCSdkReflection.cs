#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Reflection;
using HarmonyLib;

namespace FynnsTools.VRCSDKPlus
{
	// Resolve SDK targets without an assembly dependency.
	internal static class VRCSdkReflection
	{
		private const string AvatarsAssemblyPrefix = "VRC.SDK3A";

		private const string ControlPanelTypeName = "VRCSdkControlPanel";

		public static bool IsAvatarsSdkInstalled
		{
			get
			{
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.GetName().Name.StartsWith(AvatarsAssemblyPrefix, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}

				return false;
			}
		}

		public static Type ControlPanelType => AccessTools.TypeByName(ControlPanelTypeName);
	}
}
#endif
