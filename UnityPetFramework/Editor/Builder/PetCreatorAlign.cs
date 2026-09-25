using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Nudging individual frames until an animation stops jittering.
	internal sealed partial class PetCreatorPage
	{
		private const int NudgeStep = 1;
		private const int NudgeStepFast = 5;

		private bool _aligning;

		// Position within the clip rather than a cell index.
		private int _alignPosition;

		private void DrawAlignment(PetClipDefinition clip)
		{
			EditorGUILayout.Space(4);

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Frame Alignment", EditorStyles.miniBoldLabel);
				GUILayout.FlexibleSpace();

				bool wanted = GUILayout.Toggle(_aligning, _aligning ? "Done" : "Align Frames", EditorStyles.miniButton, GUILayout.Width(90f));
				if (wanted != _aligning) SetAligning(wanted, clip);
			}

			if (!_aligning) return;

			if (clip.FrameCount == 0)
			{
				EditorGUILayout.LabelField("This animation has no frames yet.", EditorStyles.miniLabel);
				return;
			}

			_alignPosition = Mathf.Clamp(_alignPosition, 0, clip.FrameCount - 1);

			// Re-pinned every pass rather than only when something changes.
			_preview.Pin(clip.name, _alignPosition);

			DrawAlignFramePicker(clip);
			DrawNudgePad(clip);
			HandleNudgeKeys(clip);
		}

		// Entering stops playback and pins the pad to one frame.
		private void SetAligning(bool aligning, PetClipDefinition clip)
		{
			_aligning = aligning;
			_preview.ShowOnionSkin = aligning;

			if (!aligning)
			{
				_preview.Unpin();
				return;
			}

			_alignPosition = 0;
			_preview.SetDefinition(_definition);

			// So a text field elsewhere on the page does not swallow the arrow keys.
			GUIUtility.keyboardControl = 0;
		}
		private void DrawAlignFramePicker(PetClipDefinition clip)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(_alignPosition <= 0))
				{
					if (GUILayout.Button("<", EditorStyles.miniButton, GUILayout.Width(24f))) _alignPosition--;
				}

				int cell = clip.frames[_alignPosition].cell;
				EditorGUILayout.LabelField(
					string.Format("Frame {0} of {1}  (cell {2})", _alignPosition + 1, clip.FrameCount, cell),
					EditorStyles.miniLabel);

				using (new EditorGUI.DisabledScope(_alignPosition >= clip.FrameCount - 1))
				{
					if (GUILayout.Button(">", EditorStyles.miniButton, GUILayout.Width(24f))) _alignPosition++;
				}
			}
		}

		private void DrawNudgePad(PetClipDefinition clip)
		{
			PetSpriteFrame frame = AlignedFrame(clip);
			if (frame == null) return;

			// Shown as how far the artwork has moved.
			float restX = frame.width * .5f;
			float restY = frame.height;

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(
					string.Format("Offset {0:0}, {1:0}", restX - frame.pivotX, restY - frame.pivotY),
					EditorStyles.miniLabel, GUILayout.Width(90f));

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("←", EditorStyles.miniButton, GUILayout.Width(26f))) Nudge(clip, -1, 0, NudgeStep);
				if (GUILayout.Button("→", EditorStyles.miniButton, GUILayout.Width(26f))) Nudge(clip, 1, 0, NudgeStep);
				if (GUILayout.Button("↑", EditorStyles.miniButton, GUILayout.Width(26f))) Nudge(clip, 0, -1, NudgeStep);
				if (GUILayout.Button("↓", EditorStyles.miniButton, GUILayout.Width(26f))) Nudge(clip, 0, 1, NudgeStep);

				GUILayout.Space(6f);

				_preview.ShowOnionSkin = GUILayout.Toggle(
					_preview.ShowOnionSkin,
					new GUIContent("Ghost", "Shows the frame before this one faded behind it, to line the two up against each other."),
					EditorStyles.miniButton, GUILayout.Width(50f));

				if (GUILayout.Button(new GUIContent("Reset", "Puts this frame back where slicing left it."), EditorStyles.miniButton, GUILayout.Width(50f)))
				{
					frame.pivotX = restX;
					frame.pivotY = restY;
					AlignmentChanged();
				}
			}

			EditorGUILayout.LabelField(
				"Arrow keys nudge a pixel, hold Shift for five. The faded shape behind is the frame before this one. The offset belongs to the frame, so it applies anywhere that frame is used.",
				EditorStyles.wordWrappedMiniLabel);
		}

		private void HandleNudgeKeys(PetClipDefinition clip)
		{
			Event e = Event.current;
			if (e.type != EventType.KeyDown) return;

			int step = e.shift ? NudgeStepFast : NudgeStep;

			switch (e.keyCode)
			{
				case KeyCode.LeftArrow: Nudge(clip, -1, 0, step); break;
				case KeyCode.RightArrow: Nudge(clip, 1, 0, step); break;
				case KeyCode.UpArrow: Nudge(clip, 0, -1, step); break;
				case KeyCode.DownArrow: Nudge(clip, 0, 1, step); break;
				default: return;
			}

			e.Use();
		}

		// dx and dy are which way the artwork should move on screen.
		private void Nudge(PetClipDefinition clip, int dx, int dy, int step)
		{
			PetSpriteFrame frame = AlignedFrame(clip);
			if (frame == null) return;

			frame.pivotX = Mathf.Clamp(frame.pivotX - dx * step, 0f, frame.width);
			frame.pivotY = Mathf.Clamp(frame.pivotY - dy * step, 0f, frame.height);
			AlignmentChanged();
		}

		private PetSpriteFrame AlignedFrame(PetClipDefinition clip)
		{
			if (_alignPosition < 0 || _alignPosition >= clip.FrameCount) return null;

			int cell = clip.frames[_alignPosition].cell;
			return cell >= 0 && cell < _definition.spriteFrames.Count ? _definition.spriteFrames[cell] : null;
		}

		private void AlignmentChanged()
		{
			// The preview measures its canvas from the anchors.
			_preview.SetDefinition(_definition);
			SaveWorkingPet();
			_container?.MarkDirtyRepaint();
		}
	}
}
