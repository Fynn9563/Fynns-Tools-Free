using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.UnityPetFramework
{
	// What the pet responds to, in one place the user can read.
	internal sealed class PetAboutPage
	{
		private static readonly string[][] Mouse =
		{
			new[] { "Click", "A reaction. Which one is the Click behaviour." },
			new[] { "Click and hold", "Petting. It carries on for as long as you hold, and stops when you let go. Adds energy." },
			new[] { "Click again and again", "It puts up with three, then gets annoyed. Leave it a couple of seconds and it forgets." },
			new[] { "Double click", "A different reaction, then the pet walks somewhere else." },
			new[] { "Double click and hold", "Picks the pet up. It follows the cursor until you let go." },
			new[] { "Move the cursor at it", "Runs away, if Run From Mouse is on in Settings." }
		};

		private static readonly string[][] Editor =
		{
			new[] { "Compile started", "Compile" },
			new[] { "Compile finished cleanly", "Success" },
			new[] { "Compile failed", "Failure" },
			new[] { "Console error or exception", "Failure, at most once every few seconds" },
			new[] { "Entered play mode", "Success" },
			new[] { "Returned to edit mode", "Success" }
		};

		private static readonly string[][] Idle =
		{
			new[] { "Nothing for a while", "Wanders, idles, thinks, sits down for a rest, and occasionally jumps." },
			new[] { "Energy runs low", "Warns once, then sleeps. Petting and good news put it back." },
			new[] { "Rarely", "Eating, playing, a joke, or something special, if the pet has one." }
		};

		public VisualElement Build()
		{
			IMGUIContainer container = new IMGUIContainer(OnGUI);
			container.style.flexGrow = 1f;
			return container;
		}

		private void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("Mouse", EditorStyles.boldLabel);
				DrawRows(Mouse);

				EditorGUILayout.Space(4);
				EditorGUILayout.LabelField(
					"A press that starts somewhere else never grabs the pet, so dragging across it from the Scene View does nothing.",
					EditorStyles.wordWrappedMiniLabel);
			}

			EditorGUILayout.Space(6);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("Reacts to the editor", EditorStyles.boldLabel);
				DrawRows(Editor);

				EditorGUILayout.Space(4);
				EditorGUILayout.LabelField(
					"Each of these picks an animation through the behaviour named beside it. " +
					"Which animation that is can be changed per pet under Behaviour Mappings in the Pet Creator, " +
					"and switched off there entirely. Reactions as a whole can be turned off in Settings.",
					EditorStyles.wordWrappedMiniLabel);
			}

			EditorGUILayout.Space(6);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("On its own", EditorStyles.boldLabel);
				DrawRows(Idle);
			}
		}

		private static void DrawRows(string[][] rows)
		{
			// The label column is wide enough for the longest entry.
			float previous = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 170f;

			try
			{
				foreach (string[] row in rows)
				{
					EditorGUILayout.LabelField(row[0], row[1], EditorStyles.wordWrappedMiniLabel);
				}
			}
			finally
			{
				EditorGUIUtility.labelWidth = previous;
			}
		}
	}
}
