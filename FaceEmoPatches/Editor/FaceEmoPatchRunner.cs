#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.FaceEmoPatches
{
	// Harmony holds its patches in memory, and a domain reload throws that memory away.
	// So the enabled set lives in EditorPrefs and is re-applied on every load, rather
	// than being applied once and assumed to stick.
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
			// FaceEmo's assemblies are not guaranteed to be loaded while static
			// constructors run, and every patch looks its targets up by name, so the first
			// pass waits until the editor has finished starting up.
			EditorApplication.delayCall += SyncAll;

			// Patches are handed back before the domain goes away. Harmony would discard
			// them regardless, but unpatching keeps FaceEmo's methods in a known state if
			// the reload is cancelled part way through.
			AssemblyReloadEvents.beforeAssemblyReload += RevertAll;
		}

		public static void SyncAll()
		{
			foreach (FaceEmoPatch patch in Patches)
			{
				Sync(patch);
			}
		}

		// Brings one patch in line with its tick box. Every apply and revert in the tool
		// goes through here, so this is the only place that has to handle a patch failing.
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
				// A patch that throws half way through would leave FaceEmo partly rewritten,
				// and would try again on every reload. Undo what it managed and turn the tick
				// box off, so the editor is left exactly as it would be without this tool.
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
