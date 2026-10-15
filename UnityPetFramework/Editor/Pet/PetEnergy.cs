using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal sealed class PetEnergy
	{
		private const float SleepRecoveryMultiplier = 4f;

		private const float IdleDrainMultiplier = 0.25f;

		private float _value = 1f;

		public float Value => _value;

		// Separate sleep/wake thresholds prevent rapid state changes.
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

		public void Add(float amount)
		{
			_value = Mathf.Clamp01(_value + amount);
		}
	}
}
