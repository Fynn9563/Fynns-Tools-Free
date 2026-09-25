using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The one thing the pet keeps track of about itself.
	internal sealed class PetEnergy
	{
		// Asleep restores much faster than awake drains.
		private const float SleepRecoveryMultiplier = 4f;

		// Standing about is restful but not restorative.
		private const float IdleDrainMultiplier = 0.25f;

		private float _value = 1f;

		public float Value => _value;

		// Two thresholds rather than one, and this matters.
		public bool WantsSleep { get; private set; }

		public void Load(float value)
		{
			_value = Mathf.Clamp01(value);
			WantsSleep = _value <= UnityPetFrameworkSettings.SleepThreshold;
		}

		public void Step(float deltaTime, bool asleep, bool active)
		{
			float drainPerSecond = 1f / Mathf.Max(1f, UnityPetFrameworkSettings.EnergyDrainMinutes * 60f);

			if (asleep)
			{
				_value = Mathf.Clamp01(_value + drainPerSecond * SleepRecoveryMultiplier * deltaTime);
			}
			else
			{
				float rate = active ? 1f : IdleDrainMultiplier;
				_value = Mathf.Clamp01(_value - drainPerSecond * rate * deltaTime);
			}

			float sleepAt = UnityPetFrameworkSettings.SleepThreshold;

			// The gap is a quarter of the scale.
			float wakeAt = Mathf.Min(1f, sleepAt + 0.25f);

			if (!WantsSleep && _value <= sleepAt)
			{
				WantsSleep = true;
			}
			else if (WantsSleep && _value >= wakeAt)
			{
				WantsSleep = false;
			}
		}

		// Petting and a successful compile both perk the pet up a little.
		public void Add(float amount)
		{
			_value = Mathf.Clamp01(_value + amount);
		}
	}
}
