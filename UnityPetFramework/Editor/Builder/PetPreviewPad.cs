using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Plays one of a pet's animations.
	internal sealed class PetPreviewPad
	{
		private enum Backdrop
		{
			Transparent,
			Dark,
			Light
		}

		private const float CheckerSize = 8f;

		// Wide enough for the longest slot label with the playing marker in front of it.
		private const float ButtonWidth = 128f;
		private const float ButtonSpacing = 4f;

		private static readonly Color CheckerA = new Color(0.24f, 0.24f, 0.24f, 1f);
		private static readonly Color CheckerB = new Color(0.29f, 0.29f, 0.29f, 1f);
		private static readonly Color DarkBackdrop = new Color(0.12f, 0.12f, 0.12f, 1f);
		private static readonly Color LightBackdrop = new Color(0.86f, 0.86f, 0.86f, 1f);
		private static readonly Color ContentOutline = new Color(0.3f, 0.7f, 1f, 0.8f);
		private static readonly Color GroundLine = new Color(1f, 0.76f, 0.03f, 0.9f);

		private readonly PetAnimator _animator = new PetAnimator();

		private Backdrop _backdrop = Backdrop.Transparent;
		private bool _showAnchor = true;

		// Which way the pet is pretending to face.
		private PetFacing _facing = PetFacing.Right;

		private PetDefinition _definition;

		// True while something is on screen.
		private PetClipDefinition _pinnedClip;
		private int _pinnedPosition;

		public bool IsPinned => _pinnedClip != null;

		public void Pin(string clipName, int position)
		{
			_pinnedClip = _definition?.FindClip(clipName);
			_pinnedPosition = position;
		}

		public void Unpin()
		{
			_pinnedClip = null;
		}

		private const float OnionSkinAlpha = .3f;

		public bool ShowOnionSkin { get; set; }

		// True while something is moving, so the owning window knows whether to keep repainting.
		public bool NeedsRepaint => !IsPinned && _animator.IsPlaying;

		public void SetDefinition(PetDefinition definition)
		{
			_definition = definition;
			_animator.SetDefinition(definition);
		}

		// Playing anything leaves the pinned view.
		public void Play(string name)
		{
			_pinnedClip = null;
			_animator.Play(name, true);
		}

		public void SetSpeed(float speed)
		{
			_animator.Speed = speed;
		}

		public void Tick(float deltaTime)
		{
			if (IsPinned) return;

			_animator.Step(deltaTime);
		}

		public void Draw(Rect area, PetAtlas atlas)
		{
			DrawBackdrop(area);

			if (atlas == null || _definition == null)
			{
				return;
			}

			PetFrameRef current = CurrentRef();
			if (current.cell < 0 || !atlas.IsValidCell(current.cell))
			{
				DrawCentredHint(area, "Pick an animation to play it.");
				return;
			}

			if (_definition.HasRectangles)
			{
				var f = _definition.spriteFrames[current.cell];
				var canvas = PetFrameGeometry.Canvas(_definition, ActiveClipName == PetClipSlots.PetIcon ? _definition.icon : null);
				bool flip = MirrorFor(current);

				// Behind the real frame, so the one being aligned stays readable.
				if (ShowOnionSkin) DrawOnionSkin(area, atlas, canvas);

				Rect draw = PetFrameGeometry.DrawRect(area, canvas, f, flip);
				GUI.DrawTextureWithTexCoords(draw, atlas.Sheet, FrameUv(atlas, current, flip), true);
				if (_showAnchor)
				{
					float scale = draw.width / f.width;
					float x = draw.x + (flip ? f.width - f.pivotX : f.pivotX) * scale;
					float y = draw.y + f.pivotY * scale;
					EditorGUI.DrawRect(new Rect(area.x, y, area.width, 1), GroundLine);
					EditorGUI.DrawRect(new Rect(x, area.y, 1, area.height), GroundLine);
				}
				return;
			}
			Rect frame = FitCell(area, atlas);
			GUI.DrawTextureWithTexCoords(frame, atlas.Sheet, FrameUv(atlas, current, MirrorFor(current)), true);

			if (_showAnchor)
			{
				DrawAnchorGuides(frame, atlas);
			}
		}

		// A mirrored frame is drawn by running its texture coordinates backwards across the cell.
		private void DrawOnionSkin(Rect area, PetAtlas atlas, RectInt canvas)
		{
			if (!TryPreviousFrame(out PetFrameRef previous) || !atlas.IsValidCell(previous.cell)) return;

			PetSpriteFrame frame = _definition.spriteFrames[previous.cell];
			bool flip = MirrorFor(previous);
			Rect ghost = PetFrameGeometry.DrawRect(area, canvas, frame, flip);

			Color previousColour = GUI.color;
			GUI.color = new Color(1f, 1f, 1f, OnionSkinAlpha);

			try
			{
				GUI.DrawTextureWithTexCoords(ghost, atlas.Sheet, FrameUv(atlas, previous, flip), true);
			}
			finally
			{
				GUI.color = previousColour;
			}
		}

		// The frame immediately before the one on screen.
		private bool TryPreviousFrame(out PetFrameRef frame)
		{
			frame = default;

			PetClipDefinition clip = ActiveClip;
			int index = ActivePosition;
			if (clip == null || index < 1 || index >= clip.FrameCount) return false;

			frame = clip.frames[index - 1];
			return true;
		}

		private PetClipDefinition ActiveClip => _pinnedClip ?? _animator.CurrentClip;

		private string ActiveClipName => _pinnedClip != null ? _pinnedClip.name : _animator.CurrentClipName;

		private int ActivePosition => _pinnedClip != null ? _pinnedPosition : _animator.CurrentFrameIndex;

		private PetFrameRef CurrentRef()
		{
			PetClipDefinition clip = ActiveClip;
			int index = ActivePosition;
			if (clip == null || index < 0 || index >= clip.FrameCount) return new PetFrameRef(-1, false);

			return clip.frames[index];
		}

		public static Rect FrameUv(PetAtlas atlas, PetFrameRef frame, bool mirrored)
		{
			Rect uv = atlas.CellUvRect(frame.cell);
			if (mirrored)
			{
				uv.x += uv.width;
				uv.width = -uv.width;
			}

			return uv;
		}

		private bool MirrorFor(PetFrameRef frame)
		{
			return PetMirror.ShouldMirror(_definition, ActiveClip, frame, _facing);
		}

		// availableWidth is the width of the area the pad was actually given.
		public void DrawControls(PetAtlas atlas, float availableWidth)
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				DrawClipButtons(availableWidth);
				EditorGUILayout.Space(4);
				DrawTransport();
				EditorGUILayout.Space(2);
				DrawReadout(atlas);
			}
		}

		// Every slot gets a button whether the pet fills it or not, disabled when it has no frames.
		private void DrawClipButtons(float availableWidth)
		{
			if (_definition == null)
			{
				return;
			}

			var clips = new System.Collections.Generic.List<PetClipDefinition>(_definition.AllSequences());
			int selected = clips.FindIndex(c => c.name == _animator.CurrentClipName);
			string[] labels = clips.ConvertAll(c => c.name + " (" + c.FrameCount + ")").ToArray();
			using (new EditorGUILayout.HorizontalScope())
			{
				int next = EditorGUILayout.Popup("Animation", selected, labels);
				if (next >= 0 && next != selected) _animator.Play(clips[next].name, true);
				if (GUILayout.Button("Play / Restart", GUILayout.Width(100)) && clips.Count > 0)
					_animator.Play(clips[Mathf.Max(0, next)].name, true);
			}
		}

		private void DrawClipButton(PetClipSlots.Slot slot)
		{
			PetClipDefinition clip = _definition.FindClip(slot.Name);
			int frameCount = clip?.FrameCount ?? 0;

			using (new EditorGUI.DisabledScope(frameCount == 0))
			{
				string tooltip = frameCount == 0
					? slot.Description + "\n\nNo frames yet."
					: string.Format("{0}\n\n{1} frames at {2:0.#} fps", slot.Description, frameCount, clip.fps);

				bool playing = _animator.CurrentClipName == slot.Name;
				GUIStyle style = playing ? EditorStyles.miniButtonMid : EditorStyles.miniButton;

				if (GUILayout.Button(new GUIContent(playing ? "> " + slot.Label : slot.Label, tooltip), style, GUILayout.Width(ButtonWidth)))
				{
					// Plays straight away, replacing whatever was on screen.
					_animator.Play(slot.Name, true);
				}
			}
		}

		private void DrawTransport()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(!_animator.IsPlaying && _animator.CurrentCell < 0))
				{
					if (GUILayout.Button("Stop", EditorStyles.miniButton, GUILayout.Width(60f)))
					{
						_animator.Stop();
					}
				}

				_showAnchor = GUILayout.Toggle(_showAnchor, new GUIContent("Anchor", "Shows the drawn area and the line the pet stands on."), EditorStyles.miniButton, GUILayout.Width(60f));

				bool facingLeft = GUILayout.Toggle(
					_facing == PetFacing.Left,
					new GUIContent("Face Left", "Previews the animation mirrored, the way it is drawn when the pet walks the other way."),
					EditorStyles.miniButton,
					GUILayout.Width(70f));

				_facing = facingLeft ? PetFacing.Left : PetFacing.Right;

				GUILayout.FlexibleSpace();

				_backdrop = (Backdrop)EditorGUILayout.EnumPopup(_backdrop, GUILayout.Width(90f));
			}
		}

		private void DrawReadout(PetAtlas atlas)
		{
			string playing = _animator.CurrentClipName ?? "Nothing";
			string frames = _animator.CurrentFrameCount == 0
				? "-"
				: string.Format("{0} / {1}", _animator.CurrentFrameIndex, _animator.CurrentFrameCount - 1);

			PetFrameRef current = _animator.CurrentFrame;
			EditorGUILayout.LabelField(
				string.Format(
					"Playing {0}    frame {1}    cell {2}{3}    {4:0.#} fps",
					playing,
					frames,
					current.cell < 0 ? "-" : current.cell.ToString(),
					current.flip ? " mirrored" : "",
					_animator.CurrentFps),
				EditorStyles.miniLabel);
		}

		private void DrawBackdrop(Rect area)
		{
			switch (_backdrop)
			{
				case Backdrop.Dark:
					EditorGUI.DrawRect(area, DarkBackdrop);
					return;

				case Backdrop.Light:
					EditorGUI.DrawRect(area, LightBackdrop);
					return;
			}

			// A checkerboard rather than a flat colour.
			EditorGUI.DrawRect(area, CheckerA);
			int columns = Mathf.CeilToInt(area.width / CheckerSize);
			int rows = Mathf.CeilToInt(area.height / CheckerSize);
			for (int y = 0; y < rows; y++)
			{
				for (int x = y % 2; x < columns; x += 2)
				{
					Rect square = new Rect(
						area.x + x * CheckerSize,
						area.y + y * CheckerSize,
						CheckerSize,
						CheckerSize);

					// Clipped by hand: the last row and column hang over the edge.
					square.width = Mathf.Min(square.width, area.xMax - square.x);
					square.height = Mathf.Min(square.height, area.yMax - square.y);
					if (square.width > 0f && square.height > 0f)
					{
						EditorGUI.DrawRect(square, CheckerB);
					}
				}
			}
		}

		private static Rect FitCell(Rect area, PetAtlas atlas)
		{
			float scale = Mathf.Min(
				area.width / atlas.CellWidth,
				area.height / atlas.CellHeight);

			float width = atlas.CellWidth * scale;
			float height = atlas.CellHeight * scale;
			return new Rect(
				area.x + (area.width - width) * 0.5f,
				area.y + (area.height - height) * 0.5f,
				width,
				height);
		}

		// The box the art actually occupies, and the line the pet stands on.
		private void DrawAnchorGuides(Rect frame, PetAtlas atlas)
		{
			if (_definition.contentWidth <= 0 || _definition.contentHeight <= 0)
			{
				return;
			}

			float scaleX = frame.width / atlas.CellWidth;
			float scaleY = frame.height / atlas.CellHeight;

			Rect content = new Rect(
				frame.x + _definition.contentLeft * scaleX,
				frame.y + _definition.contentTop * scaleY,
				_definition.contentWidth * scaleX,
				_definition.contentHeight * scaleY);

			EditorGUI.DrawRect(new Rect(content.x, content.y, content.width, 1f), ContentOutline);
			EditorGUI.DrawRect(new Rect(content.x, content.yMax - 1f, content.width, 1f), ContentOutline);
			EditorGUI.DrawRect(new Rect(content.x, content.y, 1f, content.height), ContentOutline);
			EditorGUI.DrawRect(new Rect(content.xMax - 1f, content.y, 1f, content.height), ContentOutline);

			float ground = frame.y + _definition.GroundBaseline * scaleY;
			EditorGUI.DrawRect(new Rect(frame.x, ground - 1f, frame.width, 2f), GroundLine);
		}

		private static void DrawCentredHint(Rect area, string message)
		{
			GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleCenter,
				wordWrap = true
			};

			GUI.Label(area, message, style);
		}
	}
}
