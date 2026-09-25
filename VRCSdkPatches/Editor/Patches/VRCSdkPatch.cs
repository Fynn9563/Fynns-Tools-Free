#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace FynnsTools.VRCSdkPatches
{
	// Base class for one patch. Revert unpatches _patched, so clearing a tick box takes effect
	// without a domain reload.
	internal abstract class VRCSdkPatch
	{
		private readonly List<MethodBase> _patched = new List<MethodBase>();

		public abstract string Id { get; }

		public abstract string Title { get; }

		public abstract string Description { get; }

		public string UnavailableReason { get; private set; }

		public bool IsAvailable => string.IsNullOrEmpty(UnavailableReason);

		public bool IsApplied => _patched.Count > 0;

		public void Refresh()
		{
			UnavailableReason = CheckAvailability();
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

		protected abstract string CheckAvailability();

		// Add every patched method to the list or Revert will miss it.
		protected abstract void ApplyPatches(Harmony harmony, List<MethodBase> patched);
	}
}
#endif
