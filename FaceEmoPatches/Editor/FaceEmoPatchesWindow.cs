#if VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;

namespace FynnsTools.FaceEmoPatches
{
	// The tick boxes. Toggling one applies or undoes the patch straight away, so there is
	// nothing to save and no reason to restart Unity.
	internal sealed class FaceEmoPatchesWindow : EditorWindow
	{
		private const string WindowTitle = "FaceEmo Patches";

		private static readonly Vector2 MinWindowSize = new Vector2(380, 240);

		public static void Open()
		{
			FaceEmoPatchesWindow window = GetWindow<FaceEmoPatchesWindow>(WindowTitle);
			window.minSize = MinWindowSize;
			window.Show();
		}

		private void OnEnable()
		{
			RefreshPatches();
		}

		// FaceEmo can be installed, updated or removed while the window sits open, and
		// that changes which patches are applicable, so availability is re-checked on
		// focus rather than only when the window is first created.
		private void OnFocus()
		{
			RefreshPatches();
		}

		private static void RefreshPatches()
		{
			foreach (FaceEmoPatch patch in FaceEmoPatchRunner.Patches)
			{
				patch.Refresh();
			}
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			if (!FaceEmoReflection.IsInstalled)
			{
				EditorGUILayout.HelpBox(
					"FaceEmo was not found in this project. Install FaceEmo to use these patches.",
					MessageType.Info);
				return;
			}

			EditorGUILayout.LabelField("Patches", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(
				"Each patch changes FaceEmo's behaviour in the editor only. Nothing is written to FaceEmo itself, and turning one off restores the original behaviour immediately.",
				EditorStyles.wordWrappedMiniLabel);
			EditorGUILayout.Space();

			foreach (FaceEmoPatch patch in FaceEmoPatchRunner.Patches)
			{
				DrawPatch(patch);
				EditorGUILayout.Space();
			}
		}

		private static void DrawPatch(FaceEmoPatch patch)
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				bool enabled = FaceEmoPatchesSettings.IsEnabled(patch.Id);

				using (new EditorGUI.DisabledScope(!patch.IsAvailable))
				{
					bool wanted = EditorGUILayout.ToggleLeft(patch.Title, enabled, EditorStyles.boldLabel);
					if (wanted != enabled)
					{
						FaceEmoPatchesSettings.SetEnabled(patch.Id, wanted);
						FaceEmoPatchRunner.Sync(patch);
					}
				}

				EditorGUILayout.LabelField(patch.Description, EditorStyles.wordWrappedMiniLabel);

				if (!patch.IsAvailable)
				{
					EditorGUILayout.HelpBox(patch.UnavailableReason, MessageType.Warning);
				}
			}
		}
	}
}
#endif
