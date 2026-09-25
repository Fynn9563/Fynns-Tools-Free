#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.VRCSdkPatches
{
	// Patches do not survive a domain reload. The enabled set lives in EditorPrefs and is
	// re-applied on every load.
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
			// delayCall because patches resolve their targets by name and the SDK's assemblies
			// may not be loaded yet.
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
				// Roll back and clear the tick box. A half-applied patch would retry on every reload.
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
