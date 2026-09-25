#if VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;

namespace FynnsTools.VRCSdkPatches
{
	// Toggling a box applies or reverts immediately, so there is nothing to save.
	internal sealed class VRCSdkPatchesWindow : EditorWindow
	{
		private const string WindowTitle = "VRC SDK Patches";

		private static readonly Vector2 MinWindowSize = new Vector2(380, 200);

		public static void Open()
		{
			VRCSdkPatchesWindow window = GetWindow<VRCSdkPatchesWindow>(WindowTitle);
			window.minSize = MinWindowSize;
			window.Show();
		}

		private void OnEnable()
		{
			RefreshPatches();
		}

		// Availability changes when an SDK package is installed or removed.
		private void OnFocus()
		{
			RefreshPatches();
		}

		private static void RefreshPatches()
		{
			foreach (VRCSdkPatch patch in VRCSdkPatchRunner.Patches)
			{
				patch.Refresh();
			}
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Patches", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(
				"Each patch changes how the VRChat SDK behaves in the editor only. Nothing is written to the SDK itself, and turning one off restores the original behaviour immediately.",
				EditorStyles.wordWrappedMiniLabel);
			EditorGUILayout.Space();

			foreach (VRCSdkPatch patch in VRCSdkPatchRunner.Patches)
			{
				DrawPatch(patch);
				EditorGUILayout.Space();
			}
		}

		private static void DrawPatch(VRCSdkPatch patch)
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				bool enabled = VRCSdkPatchesSettings.IsEnabled(patch.Id);

				using (new EditorGUI.DisabledScope(!patch.IsAvailable))
				{
					bool wanted = EditorGUILayout.ToggleLeft(patch.Title, enabled, EditorStyles.boldLabel);
					if (wanted != enabled)
					{
						VRCSdkPatchesSettings.SetEnabled(patch.Id, wanted);
						VRCSdkPatchRunner.Sync(patch);
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
