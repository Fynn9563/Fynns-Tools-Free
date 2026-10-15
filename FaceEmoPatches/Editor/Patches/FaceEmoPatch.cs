#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace FynnsTools.FaceEmoPatches
{
	internal abstract class FaceEmoPatch
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

		protected abstract string CheckAvailability();

		protected abstract void ApplyPatches(Harmony harmony, List<MethodBase> patched);
	}
}
#endif
