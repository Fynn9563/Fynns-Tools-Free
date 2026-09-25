using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal enum PetMovementMode
	{
		FreeRoam,
		SafeUi
	}

	// Everything the user can change, stored per machine.
	internal static class UnityPetFrameworkSettings
	{
		private const string Prefix = "FynnsTools.UnityPetFramework.";

		public static bool Enabled
		{
			get => EditorPrefs.GetBool(Prefix + "Enabled", false);
			set => EditorPrefs.SetBool(Prefix + "Enabled", value);
		}

		// The pet folder's id.
		public static string SelectedPetId
		{
			get => EditorPrefs.GetString(Prefix + "SelectedPetId", "");
			set => EditorPrefs.SetString(Prefix + "SelectedPetId", value);
		}

		// Which pet the Scale below was last set up for.
		public static string ScaleInitialisedFor
		{
			get => EditorPrefs.GetString(Prefix + "ScaleInitialisedFor", "");
			set => EditorPrefs.SetString(Prefix + "ScaleInitialisedFor", value);
		}

		// Continuous rather than a few fixed steps, so the pet can be sized to taste.
		public static float Scale
		{
			get => Mathf.Clamp(EditorPrefs.GetFloat(Prefix + "Scale", 0.5f), 0.15f, 2f);
			set => EditorPrefs.SetFloat(Prefix + "Scale", Mathf.Clamp(value, 0.15f, 2f));
		}

		public static float AnimationSpeed
		{
			get => Mathf.Clamp(EditorPrefs.GetFloat(Prefix + "AnimationSpeed", 1f), 0.25f, 3f);
			set => EditorPrefs.SetFloat(Prefix + "AnimationSpeed", Mathf.Clamp(value, 0.25f, 3f));
		}

		public static PetMovementMode MovementMode
		{
			get => (PetMovementMode)EditorPrefs.GetInt(Prefix + "MovementMode", (int)PetMovementMode.SafeUi);
			set => EditorPrefs.SetInt(Prefix + "MovementMode", (int)value);
		}

		public static bool RunAwayFromMouse
		{
			get => EditorPrefs.GetBool(Prefix + "RunAwayFromMouse", false);
			set => EditorPrefs.SetBool(Prefix + "RunAwayFromMouse", value);
		}

		public static bool ShowEnergyBar
		{
			get => EditorPrefs.GetBool(Prefix + "ShowEnergyBar", false);
			set => EditorPrefs.SetBool(Prefix + "ShowEnergyBar", value);
		}

		// Full to empty in roughly this many minutes of walking about.
		public static float EnergyDrainMinutes
		{
			get => Mathf.Clamp(EditorPrefs.GetFloat(Prefix + "EnergyDrainMinutes", 25f), 2f, 240f);
			set => EditorPrefs.SetFloat(Prefix + "EnergyDrainMinutes", Mathf.Clamp(value, 2f, 240f));
		}

		// How much of the time the pet stands about rather than walking.
		public static float IdleFrequency
		{
			get => Mathf.Clamp01(EditorPrefs.GetFloat(Prefix + "IdleFrequency", 0.45f));
			set => EditorPrefs.SetFloat(Prefix + "IdleFrequency", Mathf.Clamp01(value));
		}

		// Energy below which the pet starts looking for a nap.
		public static float SleepThreshold
		{
			get => Mathf.Clamp(EditorPrefs.GetFloat(Prefix + "SleepThreshold", 0.1f), 0f, 0.6f);
			set => EditorPrefs.SetFloat(Prefix + "SleepThreshold", Mathf.Clamp(value, 0f, 0.6f));
		}

		public static bool ReactToEditorEvents
		{
			get => EditorPrefs.GetBool(Prefix + "ReactToEditorEvents", true);
			set => EditorPrefs.SetBool(Prefix + "ReactToEditorEvents", value);
		}

		public static bool ClickThrough
		{
			get => EditorPrefs.GetBool(Prefix + "ClickThrough", false);
			set => EditorPrefs.SetBool(Prefix + "ClickThrough", value);
		}

		public static bool SoundEnabled
		{
			get => EditorPrefs.GetBool(Prefix + "SoundEnabled", true);
			set => EditorPrefs.SetBool(Prefix + "SoundEnabled", value);
		}

		public static float SoundVolume
		{
			get => Mathf.Clamp01(EditorPrefs.GetFloat(Prefix + "SoundVolume", 0.5f));
			set => EditorPrefs.SetFloat(Prefix + "SoundVolume", Mathf.Clamp01(value));
		}

		// Kept across restarts.
		public static float Energy
		{
			get => Mathf.Clamp01(EditorPrefs.GetFloat(Prefix + "Energy", 1f));
			set => EditorPrefs.SetFloat(Prefix + "Energy", Mathf.Clamp01(value));
		}

		// Puts every setting back to its default.
		public static void ResetAll()
		{
			EditorPrefs.DeleteKey(Prefix + "ScaleInitialisedFor");
			EditorPrefs.DeleteKey(Prefix + "Scale");
			EditorPrefs.DeleteKey(Prefix + "AnimationSpeed");
			EditorPrefs.DeleteKey(Prefix + "MovementMode");
			EditorPrefs.DeleteKey(Prefix + "RunAwayFromMouse");
			EditorPrefs.DeleteKey(Prefix + "ShowEnergyBar");
			EditorPrefs.DeleteKey(Prefix + "EnergyDrainMinutes");
			EditorPrefs.DeleteKey(Prefix + "IdleFrequency");
			EditorPrefs.DeleteKey(Prefix + "SleepThreshold");
			EditorPrefs.DeleteKey(Prefix + "ReactToEditorEvents");
			EditorPrefs.DeleteKey(Prefix + "ClickThrough");
			EditorPrefs.DeleteKey(Prefix + "SoundEnabled");
			EditorPrefs.DeleteKey(Prefix + "SoundVolume");
			EditorPrefs.DeleteKey(Prefix + "Energy");
		}
	}
}
