#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.FaceEmoPatches
{
	// Reapply saved patch preferences after domain reload.
	[InitializeOnLoad]
	internal static class FaceEmoPatchRunner
	{
		internal const string LogPrefix = "<color=#5ef02f>[FaceEmo Patches]</color>";

		private const string HarmonyId = "com.fynn.fynnstools.faceemopatches";

		private static readonly Harmony HarmonyInstance = new Harmony(HarmonyId);

		public static readonly IReadOnlyList<FaceEmoPatch> Patches = new FaceEmoPatch[]
		{
			new TransparentThumbnailPatch(),
			new BlendShapePopupSizePatch(),
		};

		static FaceEmoPatchRunner()
		{
			// Wait until FaceEmo assemblies are loaded before resolving targets.
			EditorApplication.delayCall += SyncAll;

			// Unpatch before reload, including cancelled reloads.
			AssemblyReloadEvents.beforeAssemblyReload += RevertAll;
		}

		public static void SyncAll()
		{
			foreach (FaceEmoPatch patch in Patches)
			{
				Sync(patch);
			}
		}

		public static void Sync(FaceEmoPatch patch)
		{
			bool wanted = FaceEmoPatchesSettings.IsEnabled(patch.Id);
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
				// Revert partial patches and disable failed preferences.
				patch.Revert(HarmonyInstance);
				FaceEmoPatchesSettings.SetEnabled(patch.Id, false);
				Debug.LogWarning($"{LogPrefix} Could not apply \"{patch.Title}\", so it has been turned back off.\n{exception}");
			}
		}

		private static void RevertAll()
		{
			foreach (FaceEmoPatch patch in Patches)
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
