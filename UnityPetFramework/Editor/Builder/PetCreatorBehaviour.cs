using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// How the pet behaves.
	internal sealed partial class PetCreatorPage
	{
		private bool _mappingExpanded;

		private static readonly string[] FacingLabels = { "Right", "Left" };

		// Which way the artwork is drawn, for the pet as a whole.
		private void DrawFacingRow()
		{
			EditorGUI.BeginChangeCheck();

			int facing = EditorGUILayout.Popup(
				new GUIContent("Artwork Faces", "Which way this pet is drawn looking. Everything is mirrored from here, so if the pet walks backwards, this is the setting."),
				_definition.artFacesRight ? 0 : 1,
				FacingLabels);

			bool pickup = EditorGUILayout.Toggle(
				new GUIContent("Allow Pickup", "Lets the pet be picked up and dragged, using its Hang animation."),
				_definition.allowPickup);

			bool jump = EditorGUILayout.Toggle(
				new GUIContent("Physical Jump", "The pet travels through the air when it jumps, rather than playing the jump on the spot."),
				_definition.physicalJump);

			if (!EditorGUI.EndChangeCheck()) return;

			_definition.artFacesRight = facing == 0;
			_definition.allowPickup = pickup;
			_definition.physicalJump = jump;
			SaveWorkingPet();
		}

		private void DrawBehaviourMappings()
		{
			_mappingExpanded = EditorGUILayout.Foldout(_mappingExpanded, "Behaviour Mappings", true);
			if (!_mappingExpanded) return;

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(
					"What the pet plays for each thing it does. Default uses the usual animation for that behaviour; Disabled means the pet simply does not do it.",
					EditorStyles.wordWrappedMiniLabel);

				List<string> options = new List<string> { "Default", "Disabled" };
				foreach (PetClipDefinition clip in _definition.clips)
				{
					if (clip.FrameCount > 0) options.Add(clip.name);
				}

				string[] choices = options.ToArray();

				foreach (string behaviour in PetBehaviours.Names)
				{
					PetBehaviourMapping mapping = _definition.behaviours.Find(m => m.behaviour == behaviour);

					int current = mapping == null
						? 0
						: string.IsNullOrEmpty(mapping.animation) ? 1 : options.IndexOf(mapping.animation);

					int next = EditorGUILayout.Popup(behaviour, Mathf.Max(0, current), choices);
					if (next == current) continue;

					_definition.behaviours.RemoveAll(m => m.behaviour == behaviour);
					if (next != 0)
					{
						_definition.behaviours.Add(new PetBehaviourMapping
						{
							behaviour = behaviour,
							animation = next == 1 ? "" : options[next]
						});
					}

					SaveWorkingPet();
				}
			}
		}
	}
}
