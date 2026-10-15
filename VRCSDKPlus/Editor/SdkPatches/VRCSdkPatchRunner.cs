#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.VRCSDKPlus
{
	// Reapply saved patch preferences after domain reload.
	[InitializeOnLoad]
	internal static class VRCSdkPatchRunner
	{
		internal const string LogPrefix = "<color=#5ef02f>[VRC SDK Patches]</color>";

		private const string HarmonyId = "com.fynn.fynnstools.vrcsdkpatches";

		private static readonly Harmony HarmonyInstance = new Harmony(HarmonyId);

		public static readonly IReadOnlyList<VRCSdkPatch> Patches = new VRCSdkPatch[]
		{
			new ShaderErrorSelectPatch(),
		};

		static VRCSdkPatchRunner()
		{
			// Wait for SDK assemblies before resolving patch targets.
			EditorApplication.delayCall += SyncAll;
			AssemblyReloadEvents.beforeAssemblyReload += RevertAll;
		}

		public static void SyncAll()
		{
			foreach (VRCSdkPatch patch in Patches)
			{
				Sync(patch);
			}
		}

		public static void Sync(VRCSdkPatch patch)
		{
			bool wanted = VRCSdkPatchesSettings.IsEnabled(patch.Id);
			try
			{
				if (wanted && !patch.IsApplied)
				{
					patch.Apply(HarmonyInstance);
				}
				else if (!wanted && patch.IsApplied)
				{
					patch.Revert(HarmonyInstance);
				}
			}
			catch (Exception exception)
			{
				patch.Revert(HarmonyInstance);
				VRCSdkPatchesSettings.SetEnabled(patch.Id, false);
				Debug.LogWarning($"{LogPrefix} Could not apply \"{patch.Title}\", so it has been turned back off.\n{exception}");
			}
		}

		private static void RevertAll()
		{
			foreach (VRCSdkPatch patch in Patches)
			{
				if (patch.IsApplied)
				{
					patch.Revert(HarmonyInstance);
				}
			}
		}
	}
}
#endif
