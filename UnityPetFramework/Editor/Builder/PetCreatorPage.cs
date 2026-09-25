using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.UnityPetFramework
{
	// Fills in a pet's animations by clicking cells on its sprite sheet.
	internal sealed partial class PetCreatorPage : IDisposable
	{
		private const string PetPathKey = "FynnsTools.UnityPetFramework.Builder.PetPath";
		private const string SheetPathKey = "FynnsTools.UnityPetFramework.Builder.SheetPath";
		private const string WorkingPetKey = "FynnsTools.UnityPetFramework.Builder.WorkingPet";

		private const int MaxUndoSteps = 64;
		private readonly PetSplitter _gridSplitter = new PetSplitter(
			"FynnsTools.UnityPetFramework.Builder.GridPanelWidth", 300f, 240f, 300f);
		private const float PreviewHeight = 190f;
		private const float ChipWidth = 38f;
		private const float ChipHeight = 20f;

		// Index order matches PetDrawnFacing.
		private static readonly string[] DrawnFacingLabels = { "Same as pet", "Right", "Left" };

		// A newline inside a dialog string, kept as a constant so the escape is not repeated in prose.
		private static readonly string Newline = "\n";

		private readonly PetFrameGrid _grid = new PetFrameGrid();
		private readonly PetPreviewPad _preview = new PetPreviewPad();
		private readonly List<List<PetFrameRef>> _undo = new List<List<PetFrameRef>>();
		private readonly PetDeferredActions _deferred = new PetDeferredActions();

		private IMGUIContainer _container;
		private double _lastTickTime;

		private PetAtlas _atlas;
		private string _atlasError;

		private string _petPath;
		private string _sheetPath;

		private PetDefinition _definition;

		// Which slot the cells being clicked go into.
		private string _selectedSlot = PetClipSlots.Stand1;

		// Whether the next cell clicked is added mirrored.
		private bool _addMirrored;

		private bool _identityExpanded;

		private Vector2 _sidePanelScroll;

		// The panel's width this frame.
		private float _sidePanelMeasured = 300f;
		private string _status;
		private MessageType _statusType = MessageType.None;

		public PetCreatorPage()
		{
			_petPath = EditorPrefs.GetString(PetPathKey, "");
			_sheetPath = EditorPrefs.GetString(SheetPathKey, "");
			LoadSlicingPrefs();

			_definition = LoadWorkingPet() ?? NewDefinition();

			_grid.CellClicked += AppendFrame;
			_grid.CellRemoveRequested += RemoveFrameByCell;
			_preview.SetDefinition(_definition);

			if (!string.IsNullOrEmpty(_sheetPath))
			{
				ReloadAtlas();
			}
		}

		public VisualElement Build()
		{
			_container = new IMGUIContainer(OnGUI);
			_container.style.flexGrow = 1f;

			_lastTickTime = EditorApplication.timeSinceStartup;
			EditorApplication.update += Tick;
			return _container;
		}

		public void Dispose()
		{
			EditorApplication.update -= Tick;

			SaveWorkingPet();

			_grid.CellClicked -= AppendFrame;
			_grid.CellRemoveRequested -= RemoveFrameByCell;

			DropGenerated();
			_atlas?.Dispose();
			_atlas = null;
			_container = null;
		}

		// Drives the preview and repaints, but only while something is actually animating.
		private void Tick()
		{
			double now = EditorApplication.timeSinceStartup;
			float delta = (float)(now - _lastTickTime);
			_lastTickTime = now;

			if (!_preview.NeedsRepaint)
			{
				return;
			}

			// Clamped for the same reason the live pet clamps.
			_preview.Tick(Mathf.Clamp(delta, 0f, 0.25f));
			_container?.MarkDirtyRepaint();
		}

		private void OnGUI()
		{
			DrawPage();

			// Anything that opens a modal picker runs here, outside every layout scope.
			if (_deferred.Run()) _container?.MarkDirtyRepaint();
		}

		private void DrawPage()
		{
			DrawPetSection();
			DrawSourceSection();
			if (_sliceMode == PetSliceMode.Smart) { DrawSmartPage(); return; }

			if (!string.IsNullOrEmpty(_atlasError))
			{
				EditorGUILayout.HelpBox(_atlasError, MessageType.Warning);
				return;
			}

			if (_atlas == null)
			{
				EditorGUILayout.HelpBox(
					"Choose a source sheet to start.",
					MessageType.Info);
				return;
			}

			// The frame table is what the slot list and the grid both read.
			if (!_gridApplied || !_definition.HasRectangles) return;

			// Measured from the container, never from the window.
			float available = _container != null && _container.contentRect.width > 1f
				? _container.contentRect.width
				: EditorGUIUtility.currentViewWidth;

			float panel = _gridSplitter.Resolve(available);

			if (available >= _gridSplitter.MinimumForSplit)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					DrawSidePanel(panel);
					_gridSplitter.DrawHandle();

					using (new EditorGUILayout.VerticalScope())
					{
						DrawGrid();
						DrawPreview();
					}
				}

				if (_gridSplitter.IsDragging && _container != null) _container.MarkDirtyRepaint();
			}
			else
			{
				// Too narrow for both.
				DrawSidePanel(0f);
				DrawGrid();
				DrawPreview();
			}

			if (!string.IsNullOrEmpty(_status))
			{
				EditorGUILayout.HelpBox(_status, _statusType);
			}
		}

		// The row of pet actions needs about 520 pixels laid out in one line.
		private const float PetActionRowWidth = 520f;

		private void DrawPetSection()
		{
			float available = _container != null && _container.contentRect.width > 1f
				? _container.contentRect.width
				: EditorGUIUtility.currentViewWidth;

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (available >= PetActionRowWidth)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						DrawPetName(true);
						DrawPetActions();
					}

					return;
				}

				DrawPetName(false);
				using (new EditorGUILayout.HorizontalScope()) DrawPetActions();
			}
		}

		private void DrawPetName(bool fixedWidth)
		{
			var options = fixedWidth
				? new[] { GUILayout.Width(160f) }
				: new[] { GUILayout.ExpandWidth(true) };

			EditorGUILayout.LabelField(
				string.IsNullOrEmpty(_petPath) ? "Unsaved pet" : Path.GetFileName(Path.GetDirectoryName(_petPath)),
				EditorStyles.boldLabel,
				options);
		}

		private void DrawPetActions()
		{
			if (GUILayout.Button(new GUIContent("New Pet", "Starts again with an empty pet. Nothing already saved is touched."), EditorStyles.miniButton, GUILayout.Width(70f)))
			{
				_deferred.Defer(NewPet);
			}

			if (GUILayout.Button("Open Pet", EditorStyles.miniButton, GUILayout.Width(80f)))
			{
				_deferred.Defer(BrowseForPet);
			}

			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_petPath)))
			{
				if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.Width(60f)))
				{
					_deferred.Defer(() => SavePet(_petPath));
				}
			}

			if (GUILayout.Button("Save As", EditorStyles.miniButton, GUILayout.Width(70f)))
			{
				_deferred.Defer(BrowseToSavePet);
			}

			GUILayout.FlexibleSpace();
		}

		// Who the pet is.
		private void DrawIdentity()
		{
			_identityExpanded = EditorGUILayout.Foldout(_identityExpanded, "Pet Details", true);
			if (!_identityExpanded)
			{
				return;
			}

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUI.BeginChangeCheck();

				_definition.displayName = EditorGUILayout.TextField(
					new GUIContent("Name", "What the pet is called in the list."), _definition.displayName);

				_definition.id = EditorGUILayout.TextField(
					new GUIContent("Id", "Identifies the pet when it is shared. Letters, numbers, dots and dashes."),
					_definition.id);

				_definition.author = EditorGUILayout.TextField("Author", _definition.author);
				_definition.version = EditorGUILayout.TextField("Version", _definition.version);
				_definition.license = EditorGUILayout.TextField("Licence", _definition.license);

				_definition.defaultScale = EditorGUILayout.Slider(
					new GUIContent("Default Size", "The size this pet is shown at before anyone changes it."),
					Mathf.Approximately(_definition.defaultScale, 0f) ? 1f : _definition.defaultScale,
					0.15f, 2f);

				if (EditorGUI.EndChangeCheck())
				{
					// An id has to survive being used as a folder name.
					_definition.id = PetLibrary.SanitiseId(_definition.id);
					SaveWorkingPet();
				}

				DrawFacingRow();
			}
		}

		// A width of zero means take the whole row, which the stacked layout uses.
		private void DrawSidePanel(float width)
		{
			GUILayoutOption[] options = width > 0f ? new[] { GUILayout.Width(width) } : new GUILayoutOption[0];
			float measured = width > 0f ? width : EditorGUIUtility.currentViewWidth;
			_sidePanelMeasured = measured;

			// The default label width is about 150.
			float previousLabel = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = Mathf.Clamp(measured * .42f, 80f, 150f);

			try
			{
				using (new EditorGUILayout.VerticalScope(options))

				// Horizontal scrolling off.
				using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(
					_sidePanelScroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView))
				{
					_sidePanelScroll = scroll.scrollPosition;

					DrawIdentity();
					EditorGUILayout.Space(6);
					DrawSlotList();
					EditorGUILayout.Space(6);
					DrawSelectedSlot();
					EditorGUILayout.Space(6);
					DrawBehaviourMappings();
				}
			}
			finally
			{
				EditorGUIUtility.labelWidth = previousLabel;
			}
		}

		// Every slot, always, in a fixed order.
		private void DrawSlotList()
		{
			EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				foreach (PetClipSlots.Slot slot in PetClipSlots.All)
				{
					DrawSlotRow(slot);
				}

				EditorGUILayout.Space(4);
				EditorGUILayout.LabelField("Thumbnail", EditorStyles.miniBoldLabel);
				DrawSlotRow(PetClipSlots.Icon);
			}
		}

		private void DrawSlotRow(PetClipSlots.Slot slot)
		{
			PetClipDefinition clip = _definition.FindClip(slot.Name);
			int frameCount = clip?.FrameCount ?? 0;
			bool selected = slot.Name == _selectedSlot;

			using (new EditorGUILayout.HorizontalScope())
			{
				GUIStyle style = selected ? EditorStyles.boldLabel : EditorStyles.label;

				string marker = frameCount > 0
					? frameCount.ToString()
					: slot.Required ? "needed" : "-";

				if (GUILayout.Button(
					new GUIContent((selected ? "> " : "   ") + slot.Label, slot.Description),
					style,
					GUILayout.ExpandWidth(true)))
				{
					_selectedSlot = slot.Name;
					_undo.Clear();

					// Alignment is about one animation.
					_aligning = false;
					_preview.ShowOnionSkin = false;
					_preview.Unpin();
				}

				Color previous = GUI.color;
				if (frameCount == 0 && slot.Required)
				{
					// The one state worth colouring: a pet missing this will not load.
					GUI.color = new Color(1f, 0.76f, 0.03f);
				}

				GUILayout.Label(marker, EditorStyles.miniLabel, GUILayout.Width(46f));
				GUI.color = previous;
			}
		}

		private void DrawSelectedSlot()
		{
			PetClipSlots.Slot slot = PetClipSlots.Find(_selectedSlot);
			if (slot == null)
			{
				return;
			}

			EditorGUILayout.LabelField(slot.Label, EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(slot.Description, EditorStyles.wordWrappedMiniLabel);
				EditorGUILayout.Space(2);

				PetClipDefinition clip = _definition.FindClip(_selectedSlot);
				if (clip == null)
				{
					EditorGUILayout.LabelField("Click cells on the sheet to fill this in.", EditorStyles.miniLabel);
					return;
				}

				EditorGUI.BeginChangeCheck();
				// Rounded as it is set.
				clip.fps = Mathf.Round(EditorGUILayout.Slider("FPS", clip.fps, 1f, 24f) * 10f) / 10f;
				clip.loop = EditorGUILayout.Toggle(
					new GUIContent("Loop", "Off means it plays once and then hands control back."),
					clip.loop);

				// Only meaningful for a one-shot: a loop has no last frame to rest on.
				using (new EditorGUI.DisabledScope(clip.loop))
				{
					clip.holdSeconds = Mathf.Round(EditorGUILayout.Slider(
						new GUIContent("Hold", "Stays on the last frame for this long after the animation finishes. Use it to let a short reaction land, instead of slowing the whole animation down."),
						clip.holdSeconds, 0f, 5f) * 10f) / 10f;
				}

				// The icon is never mirrored for facing and never plays a sound.
				bool isIcon = _selectedSlot == PetClipSlots.PetIcon;

				if (!isIcon)
				{
					clip.allowFlip = EditorGUILayout.Toggle(
						new GUIContent(
							"Allow Mirroring",
							"The pet is drawn facing one way and this animation is flipped when it faces the other. " +
							"Turn it off for artwork that reads wrong backwards, such as lettering, and for a turn."),
						clip.allowFlip);
					using (new EditorGUI.DisabledScope(!clip.allowFlip))
					{
						clip.drawnFacing = EditorGUILayout.Popup(
							new GUIContent("Drawn Facing", "Which way this animation is drawn, when it disagrees with the rest of the pet."),
							Mathf.Clamp(clip.drawnFacing, 0, DrawnFacingLabels.Length - 1),
							DrawnFacingLabels);
					}
				}

				if (EditorGUI.EndChangeCheck()) SaveWorkingPet();

				if (!isIcon) DrawSoundRow(clip);

				EditorGUILayout.Space(4);
				DrawFrameSequence(clip);
				DrawAlignment(clip);

				EditorGUILayout.Space(4);
				DrawClipActions(clip);
			}
		}

		// One optional sound per animation, played as it starts.
		private void DrawSoundRow(PetClipDefinition clip)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel("Sound");
				EditorGUILayout.LabelField(
					string.IsNullOrEmpty(clip.sound) ? "None" : Path.GetFileName(clip.sound),
					EditorStyles.miniLabel);

				if (GUILayout.Button("Choose", EditorStyles.miniButton, GUILayout.Width(60f)))
				{
					_deferred.Defer(() => ChooseSound(clip));
				}

				using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(clip.sound)))
				{
					if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50f)))
					{
						clip.sound = "";
						SaveWorkingPet();
					}
				}
			}
		}

		// The animation as the user built it.
		private void DrawFrameSequence(PetClipDefinition clip)
		{
			EditorGUILayout.LabelField(
				string.Format("Frames ({0})", clip.FrameCount),
				EditorStyles.miniBoldLabel);

			if (clip.FrameCount == 0)
			{
				EditorGUILayout.LabelField("Click cells on the sheet to add them.", EditorStyles.miniLabel);
				return;
			}

			int perRow = Mathf.Max(1, Mathf.FloorToInt((_sidePanelMeasured - 34f) / (ChipWidth + 2f)));
			for (int start = 0; start < clip.frames.Count; start += perRow)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					for (int i = start; i < Mathf.Min(start + perRow, clip.frames.Count); i++)
					{
						DrawFrameChip(clip, i);
					}

					GUILayout.FlexibleSpace();
				}
			}
		}

		private void DrawFrameChip(PetClipDefinition clip, int position)
		{
			PetFrameRef frame = clip.frames[position];

			// One short line.
			string label = frame.flip ? position + "<" : position.ToString();
			string tooltip = string.Format(
				"Position {0}, cell {1}{2}\n\nClick for options.",
				position, frame.cell, frame.flip ? ", mirrored" : "");

			if (!GUILayout.Button(new GUIContent(label, tooltip), EditorStyles.miniButton, GUILayout.Width(ChipWidth), GUILayout.Height(ChipHeight)))
			{
				return;
			}

			// A menu rather than modifier clicks.
			int index = position;
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent(frame.flip ? "Draw normally" : "Draw mirrored"), false, () => ToggleFrameFlip(clip, index));
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Remove"), false, () => RemoveFrameAt(clip, index));
			menu.ShowAsContext();
		}

		private void DrawClipActions(PetClipDefinition clip)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(_undo.Count == 0))
				{
					if (GUILayout.Button("Undo", EditorStyles.miniButton))
					{
						PopUndo(clip);
					}
				}

				using (new EditorGUI.DisabledScope(clip.FrameCount == 0))
				{
					if (GUILayout.Button(new GUIContent("Mirror All", "Mirrors every frame. Builds a left facing animation from a right facing one."), EditorStyles.miniButton))
					{
						PushUndo(clip);
						for (int i = 0; i < clip.frames.Count; i++)
						{
							PetFrameRef frame = clip.frames[i];
							frame.flip = !frame.flip;
							clip.frames[i] = frame;
						}

						SaveWorkingPet();
					}

					if (GUILayout.Button(new GUIContent("Reverse", "Plays the frames the other way round. Turns a turn one way into the turn back."), EditorStyles.miniButton))
					{
						PushUndo(clip);
						clip.frames.Reverse();
						SaveWorkingPet();
					}

					if (GUILayout.Button("Clear", EditorStyles.miniButton))
					{
						PushUndo(clip);
						clip.frames.Clear();
						SaveWorkingPet();
					}
				}
			}
		}

		private void DrawGrid()
		{
			using (new EditorGUILayout.VerticalScope())
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField(
						"Filling in: " + (PetClipSlots.Find(_selectedSlot)?.Label ?? "-"),
						EditorStyles.miniBoldLabel);

					GUILayout.FlexibleSpace();

					_addMirrored = GUILayout.Toggle(
						_addMirrored,
						new GUIContent("Add mirrored", "Frames clicked from now on are drawn mirrored left to right."),
						EditorStyles.miniButton,
						GUILayout.Width(100f));
				}

				Rect area = GUILayoutUtility.GetRect(
					GUIContent.none,
					GUIStyle.none,
					GUILayout.ExpandWidth(true),
					GUILayout.ExpandHeight(true),
					GUILayout.MinHeight(240f));

				PetClipDefinition clip = _definition.FindClip(_selectedSlot);
				_grid.Draw(area, _atlas, clip?.frames);

				EditorGUILayout.LabelField(
					string.Format(
						"{0}    click to add, ctrl+click to remove, scroll to zoom, middle or alt drag to pan",
						_grid.ZoomLabel()),
					EditorStyles.miniLabel);
			}
		}

		private void DrawPreview()
		{
			EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

			Rect pad = GUILayoutUtility.GetRect(
				GUIContent.none,
				GUIStyle.none,
				GUILayout.ExpandWidth(true),
				GUILayout.Height(PreviewHeight));

			_preview.Draw(pad, _atlas);

			// The pad's own rect is the available width.
			_preview.DrawControls(_atlas, pad.width);
		}

		// Starts again with nothing.
		private void NewPet()
		{
			if (!EditorUtility.DisplayDialog(
				"Start a new pet",
				"Clear the Pet Creator and start again?" + Newline + Newline +
				"Anything not saved is lost. Pets you have already saved are not affected.",
				"Start New",
				"Cancel"))
			{
				return;
			}

			_atlas?.Dispose();
			_atlas = null;
			_atlasError = null;

			_definition = NewDefinition();
			_petPath = "";
			_sheetPath = "";

			_selectedSlot = PetClipSlots.Stand1;
			ResetSmartState();
			_addMirrored = false;
			_undo.Clear();
			_grid.Reset();
			_preview.SetDefinition(_definition);

			EditorPrefs.DeleteKey(PetPathKey);
			EditorPrefs.DeleteKey(SheetPathKey);
			SessionState.EraseString(WorkingPetKey);

			SetStatus("Started a new pet. Choose a sprite sheet to begin.", MessageType.Info);
		}

		private string FirstFilledSlot()
		{
			foreach (PetClipSlots.Slot slot in PetClipSlots.All)
			{
				if (_definition.HasClip(slot.Name))
				{
					return slot.Name;
				}
			}

			return null;
		}

		private void BrowseForSheet()
		{
			string start = string.IsNullOrEmpty(_sheetPath)
				? UnityPetFrameworkPaths.ToAbsolute(UnityPetFrameworkPaths.BundledPetsFolder) ?? Application.dataPath
				: Path.GetDirectoryName(_sheetPath);

			string picked = EditorUtility.OpenFilePanel("Choose a sprite sheet", start, "png");
			if (string.IsNullOrEmpty(picked))
			{
				return;
			}

			_sheetPath = picked;

			// A different sheet means different cell numbers.
			_definition.clips.Clear();
			_definition.icon = null;
			_definition.spriteFrames.Clear();
			_definition.behaviours.Clear();
			_gridApplied = false;
			ResetSmartState();

			EditorPrefs.SetString(SheetPathKey, _sheetPath);
			SetStatus(null, MessageType.None);
			ReloadAtlas();
		}

		private int CountEmptyCells()
		{
			int empty = 0;
			for (int i = 0; i < _atlas.CellCount; i++)
			{
				if (_atlas.IsCellEmpty(i))
				{
					empty++;
				}
			}

			return empty;
		}

		// Slots hold no clip until something is put in one.
		private PetClipDefinition EnsureClip(string slotName)
		{
			PetClipDefinition clip = _definition.FindClip(slotName);
			if (clip != null)
			{
				return clip;
			}

			clip = new PetClipDefinition
			{
				name = slotName,
				fps = 7f,
				loop = DefaultLoopFor(slotName)
			};

			// The icon has its own field and is not one of the pet's animations.
			if (slotName == PetClipSlots.PetIcon) _definition.icon = clip;
			else _definition.clips.Add(clip);

			_preview.SetDefinition(_definition);
			return clip;
		}

		// Walking and idling carry on until something stops them.
		private static bool DefaultLoopFor(string slotName)
		{
			switch (slotName)
			{
				case PetClipSlots.Idle:
				case PetClipSlots.Walk:
				case PetClipSlots.Thinking:
				case PetClipSlots.Sleeping:
					return true;

				default:
					return false;
			}
		}

		private void AppendFrame(int cellIndex)
		{
			PetClipDefinition clip = EnsureClip(_selectedSlot);
			PushUndo(clip);
			clip.frames.Add(new PetFrameRef(cellIndex, _addMirrored));
			SetStatus(null, MessageType.None);
			SaveWorkingPet();
		}

		// Ctrl+click on the sheet takes out the last use of that cell.
		private void RemoveFrameByCell(int cellIndex)
		{
			PetClipDefinition clip = _definition.FindClip(_selectedSlot);
			if (clip == null)
			{
				return;
			}

			for (int i = clip.frames.Count - 1; i >= 0; i--)
			{
				if (clip.frames[i].cell != cellIndex)
				{
					continue;
				}

				PushUndo(clip);
				clip.frames.RemoveAt(i);
				SaveWorkingPet();
				return;
			}
		}

		private void RemoveFrameAt(PetClipDefinition clip, int position)
		{
			if (position < 0 || position >= clip.frames.Count)
			{
				return;
			}

			PushUndo(clip);
			clip.frames.RemoveAt(position);
			SaveWorkingPet();
			_container?.MarkDirtyRepaint();
		}

		private void ToggleFrameFlip(PetClipDefinition clip, int position)
		{
			if (position < 0 || position >= clip.frames.Count)
			{
				return;
			}

			PushUndo(clip);
			PetFrameRef frame = clip.frames[position];
			frame.flip = !frame.flip;
			clip.frames[position] = frame;
			SaveWorkingPet();
			_container?.MarkDirtyRepaint();
		}

		private void PushUndo(PetClipDefinition clip)
		{
			// Whole-list snapshots.
			_undo.Add(new List<PetFrameRef>(clip.frames));
			if (_undo.Count > MaxUndoSteps)
			{
				_undo.RemoveAt(0);
			}
		}

		private void PopUndo(PetClipDefinition clip)
		{
			if (_undo.Count == 0)
			{
				return;
			}

			clip.frames.Clear();
			clip.frames.AddRange(_undo[_undo.Count - 1]);
			_undo.RemoveAt(_undo.Count - 1);
			SaveWorkingPet();
		}

		private void SetStatus(string message, MessageType type)
		{
			_status = message;
			_statusType = type;
		}

		private static PetDefinition NewDefinition()
		{
			return new PetDefinition
			{
				schemaVersion = UnityPetFrameworkInfo.PetSchemaVersion,
				frameworkVersion = UnityPetFrameworkInfo.Version,
				id = "new-pet",
				displayName = "New Pet",
				defaultScale = 1f
			};
		}

		// The work in progress lives in SessionState.
		private void SaveWorkingPet()
		{
			SessionState.SetString(WorkingPetKey, JsonUtility.ToJson(_definition));
		}

		private static PetDefinition LoadWorkingPet()
		{
			string json = SessionState.GetString(WorkingPetKey, "");
			if (string.IsNullOrEmpty(json))
			{
				return null;
			}

			try
			{
				PetDefinition restored = JsonUtility.FromJson<PetDefinition>(json)?.Normalise();
				if (restored?.clips == null)
				{
					return null;
				}

				return restored;
			}
			catch (Exception)
			{
				// A stored blob from an older layout is not worth a message.
				return null;
			}
		}
	}
}
