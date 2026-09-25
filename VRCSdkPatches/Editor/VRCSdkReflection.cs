#if VRC_SDK_VRCSDK3
using System;
using System.Reflection;
using HarmonyLib;

namespace FynnsTools.VRCSdkPatches
{
	// Targets are looked up by name, so the tool needs no reference to the SDK's assemblies.
	internal static class VRCSdkReflection
	{
		private const string AvatarsAssemblyPrefix = "VRC.SDK3A";

		private const string ControlPanelTypeName = "VRCSdkControlPanel";

		// VRC_SDK_VRCSDK3 covers com.vrchat.base. Avatars is a separate package.
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

		// Global namespace, so the bare name is the full name.
		public static Type ControlPanelType => AccessTools.TypeByName(ControlPanelTypeName);
	}
}
#endif
