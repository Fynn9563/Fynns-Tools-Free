using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.UnityPetFramework
{
	// Everything the user can change about the pet.
	internal sealed class PetSettingsPage
	{
		private readonly PetDeferredActions _deferred = new PetDeferredActions();
		private IMGUIContainer _container;
		private Vector2 _scroll;

		// How long the size has to stop changing before the pet is rebuilt at it.
		private const double ScaleSettleSeconds = .2;

		private bool _scaleDirty;
		private double _scaleChangedAt;

		// A blank line inside a dialog message.
		public static readonly string Break = "\n\n";

		public VisualElement Build()
		{
			_container = new IMGUIContainer(OnGUI);
			_container.style.flexGrow = 1f;
			return _container;
		}

		private void OnGUI()
		{
			DrawPage();
			if (_deferred.Run()) _container?.MarkDirtyRepaint();
		}

		private void DrawPage()
		{
			using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(_scroll))
			{
				_scroll = scroll.scrollPosition;

				DrawAppearance();
				EditorGUILayout.Space(6);
				DrawBehaviour();
				EditorGUILayout.Space(6);
				DrawSound();
				EditorGUILayout.Space(6);
				DrawReset();
			}
		}

		private void DrawAppearance()
		{
			EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUI.BeginChangeCheck();
				float scale = EditorGUILayout.Slider(
					new GUIContent("Size", "How big the pet is drawn."),
					UnityPetFrameworkSettings.Scale, 0.15f, 2f);

				if (EditorGUI.EndChangeCheck())
				{
					UnityPetFrameworkSettings.Scale = scale;
					MarkScaleDirty();
				}

				EditorGUI.BeginChangeCheck();
				UnityPetFrameworkSettings.AnimationSpeed = EditorGUILayout.Slider(
					new GUIContent("Animation Speed", "A multiplier on every animation."),
					UnityPetFrameworkSettings.AnimationSpeed, 0.25f, 3f);

				UnityPetFrameworkSettings.ShowEnergyBar = EditorGUILayout.Toggle(
					new GUIContent("Show Energy Bar", "A small bar in the bottom left corner of the Unity window."),
					UnityPetFrameworkSettings.ShowEnergyBar);

				bool clickThrough = EditorGUILayout.Toggle(
					new GUIContent("Ignore Clicks", "Clicks pass straight through the pet to whatever is underneath."),
					UnityPetFrameworkSettings.ClickThrough);

				if (EditorGUI.EndChangeCheck())
				{
					UnityPetFrameworkSettings.ClickThrough = clickThrough;
					PetOverlayHost.ApplyClickThrough();
				}
			}
		}

		private static void DrawBehaviour()
		{
			EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUI.BeginChangeCheck();
				UnityPetFrameworkSettings.MovementMode = (PetMovementMode)EditorGUILayout.EnumPopup(
					new GUIContent("Movement", "Safe UI keeps the pet inside the Scene and Game views. Free Roam lets it go anywhere in the Unity window."),
					UnityPetFrameworkSettings.MovementMode);

				if (EditorGUI.EndChangeCheck())
				{
					// Where the pet may walk has just changed completely.
					PetOverlayHost.RefreshMovementArea();
				}

				UnityPetFrameworkSettings.RunAwayFromMouse = EditorGUILayout.Toggle(
					new GUIContent("Run From Mouse", "The pet walks away when the pointer reaches it."),
					UnityPetFrameworkSettings.RunAwayFromMouse);

				UnityPetFrameworkSettings.IdleFrequency = EditorGUILayout.Slider(
					new GUIContent("Calmness", "How much of the time the pet stands about rather than walking."),
					UnityPetFrameworkSettings.IdleFrequency, 0f, 1f);

				UnityPetFrameworkSettings.EnergyDrainMinutes = EditorGUILayout.Slider(
					new GUIContent("Energy Lasts", "Roughly how many minutes of wandering it takes to tire the pet out."),
					UnityPetFrameworkSettings.EnergyDrainMinutes, 2f, 240f);

				UnityPetFrameworkSettings.SleepThreshold = EditorGUILayout.Slider(
					new GUIContent("Sleeps Below", "The energy level the pet starts looking for a nap at."),
					UnityPetFrameworkSettings.SleepThreshold, 0f, 0.6f);

				UnityPetFrameworkSettings.ReactToEditorEvents = EditorGUILayout.Toggle(
					new GUIContent("React To Unity", "The pet thinks while Unity compiles, and reacts to errors and play mode."),
					UnityPetFrameworkSettings.ReactToEditorEvents);
			}
		}

		private static void DrawSound()
		{
			EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				UnityPetFrameworkSettings.SoundEnabled = EditorGUILayout.Toggle(
					new GUIContent("Sound", "Plays any sounds the pet comes with."),
					UnityPetFrameworkSettings.SoundEnabled);

				using (new EditorGUI.DisabledScope(!UnityPetFrameworkSettings.SoundEnabled))
				{
					UnityPetFrameworkSettings.SoundVolume = EditorGUILayout.Slider(
						"Volume", UnityPetFrameworkSettings.SoundVolume, 0f, 1f);
				}

				EditorGUILayout.LabelField(
					"The pets included with the tool have no sounds. A pet made in the Pet Creator can have one per animation.",
					EditorStyles.wordWrappedMiniLabel);
			}
		}

		// Size decides how big the pet's window and every baked frame are.
		private void MarkScaleDirty()
		{
			_scaleChangedAt = EditorApplication.timeSinceStartup;
			if (_scaleDirty) return;

			_scaleDirty = true;
			EditorApplication.update += TickScale;
		}

		private void TickScale()
		{
			if (!_scaleDirty)
			{
				EditorApplication.update -= TickScale;
				return;
			}

			// The window closed part way through a change.
			if (_container == null || _container.panel == null)
			{
				_scaleDirty = false;
				EditorApplication.update -= TickScale;
				return;
			}

			// Non-zero for as long as the slider owns the drag.
			if (GUIUtility.hotControl != 0) return;

			// The settle is what stops a typed value rebuilding the window once per keystroke.
			if (EditorApplication.timeSinceStartup - _scaleChangedAt < ScaleSettleSeconds) return;

			_scaleDirty = false;
			EditorApplication.update -= TickScale;
			PetOverlayHost.Restart();
		}

		private void DrawReset()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button(
					new GUIContent(
						"Reset Everything",
						"Puts every setting back, empties the pet's energy back to full, forgets where it was standing, and lets it take its own default size again. Your pets and anything you have made are left alone."),
					GUILayout.Width(150f)))
				{
					_deferred.Defer(ConfirmReset);
				}
			}
		}

		private static void ConfirmReset()
		{
			if (EditorUtility.DisplayDialog(
				"Reset the pet",
				"Put every setting back to how it started?" + Break +
				"This also refills the pet's energy, forgets where it was standing, and lets it take its own default size again." + Break +
				"Your pets, and anything you have made in the Pet Creator, are not touched.",
				"Reset",
				"Cancel"))
			{
				PetOverlayHost.ResetEverything();
			}
		}
	}
}
