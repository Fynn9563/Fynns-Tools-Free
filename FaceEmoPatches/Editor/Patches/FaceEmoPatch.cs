#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace FynnsTools.FaceEmoPatches
{
	// One tick box in the tool window, and the change to FaceEmo behind it.
	//
	// Harmony rewrites methods in memory, so a patch is undone by handing back the same
	// methods it took. Each subclass records what it patched, which lets a tick box be
	// cleared without a domain reload and lets a half-finished apply be rolled back.
	internal abstract class FaceEmoPatch
	{
		private readonly List<MethodBase> _patched = new List<MethodBase>();

		public abstract string Id { get; }

		public abstract string Title { get; }

		public abstract string Description { get; }

		// Empty while the patch can be applied, otherwise the reason it cannot be, worded
		// for the person reading the tool window rather than for a stack trace.
		public string UnavailableReason { get; private set; }

		public bool IsAvailable => string.IsNullOrEmpty(UnavailableReason);

		public bool IsApplied => _patched.Count > 0;

		public void Refresh()
		{
			UnavailableReason = FaceEmoReflection.IsInstalled
				? CheckAvailability()
				: "FaceEmo was not found in this project.";
		}

		public void Apply(Harmony harmony)
		{
			if (IsApplied)
			{
				return;
			}

			Refresh();
			if (!IsAvailable)
			{
				return;
			}

			ApplyPatches(harmony, _patched);
		}

		public void Revert(Harmony harmony)
		{
			foreach (MethodBase method in _patched)
			{
				harmony.Unpatch(method, HarmonyPatchType.All, harmony.Id);
			}

			_patched.Clear();
		}

		// Returns null when every FaceEmo member this patch needs was found. A version of
		// FaceEmo that moved one of them makes the patch unavailable rather than broken.
		protected abstract string CheckAvailability();

		// Adds every method it patches to the given list, so Revert can hand them back.
		protected abstract void ApplyPatches(Harmony harmony, List<MethodBase> patched);
	}
}
#endif
