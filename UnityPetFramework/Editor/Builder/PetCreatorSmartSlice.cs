using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Smart Slice: the review and correction stage.
	internal sealed partial class PetCreatorPage
	{
		private const float MinCanvasWidth = 330f;
		private const int MaxSliceUndo = 32;

		private readonly PetSliceCanvas _sliceCanvas = new PetSliceCanvas();
		private readonly List<PetSliceSnapshot> _sliceUndo = new List<PetSliceSnapshot>();

		private readonly PetSplitter _smartSplitter = new PetSplitter(
			"FynnsTools.UnityPetFramework.Builder.SmartPanelWidth", 340f, 260f, MinCanvasWidth);

		private PetSliceModel _slice;
		private PetSliceSensitivity _sensitivity = PetSliceSensitivity.Normal;
		private bool _ignoreMetadata = true;
		private int _alphaThreshold = 8;
		private Vector2 _smartScroll;
		private int _sliceRow;

		// The generated sheet, kept until the model changes.
		private PetSheetGenerator.Result _generated;
		private Texture2D _generatedTexture;
		private string _generateError;
		private int _sliceRevision;
		private int _generatedRevision = -1;

		private string SliceStateKey => WorkingPetKey + ".Slice";

		private void ResetSmartState()
		{
			_slice = null;
			_sliceUndo.Clear();
			_sliceCanvas.Reset();
			DropGenerated();
			SessionState.EraseString(SliceStateKey);
		}

		private void DropGenerated()
		{
			if (_generatedTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(_generatedTexture);
				_generatedTexture = null;
			}

			_generated = null;
			_generatedRevision = -1;
			_generateError = null;
		}

		private void SliceChanged()
		{
			_slice?.Rebuild();
			_sliceRevision++;
			SaveSliceState();
			_container?.MarkDirtyRepaint();
		}

		private void PushSliceUndo()
		{
			if (_slice == null) return;

			_sliceUndo.Add(PetSliceSnapshot.Capture(_slice));
			if (_sliceUndo.Count > MaxSliceUndo) _sliceUndo.RemoveAt(0);
		}

		private void UndoSlice()
		{
			if (_slice == null || _sliceUndo.Count == 0) return;

			_sliceUndo[_sliceUndo.Count - 1].Restore(_slice);
			_sliceUndo.RemoveAt(_sliceUndo.Count - 1);
			_sliceCanvas.Selection.Clear();
			SliceChanged();
		}

		// Only the corrections are stored.
		private void SaveSliceState()
		{
			if (_slice == null)
			{
				SessionState.EraseString(SliceStateKey);
				return;
			}

			SessionState.SetString(SliceStateKey, JsonUtility.ToJson(PetSliceSnapshot.Capture(_slice)));
			SessionState.SetInt(SliceStateKey + ".Alpha", _alphaThreshold);
		}
		// Rebuilds the review state after an assembly reload.
		private bool EnsureSliceModel()
		{
			if (_slice != null) return true;
			if (_atlas == null) return false;

			string json = SessionState.GetString(SliceStateKey, "");
			if (string.IsNullOrEmpty(json)) return false;

			try
			{
				PetSliceSnapshot snapshot = JsonUtility.FromJson<PetSliceSnapshot>(json);
				if (snapshot == null || snapshot.flags.Count == 0) return false;

				int alpha = SessionState.GetInt(SliceStateKey + ".Alpha", _alphaThreshold);
				PetSliceModel model = PetSmartSlicer.Scan(_atlas.Pixels, _atlas.Sheet.width, _atlas.Sheet.height, (byte)alpha);

				// The scan has to produce the same components it did before.
				if (model.components.Count != snapshot.flags.Count)
				{
					SessionState.EraseString(SliceStateKey);
					return false;
				}

				snapshot.Restore(model);
				_slice = model;
				_sliceRevision++;
				return true;
			}
			catch (Exception)
			{
				SessionState.EraseString(SliceStateKey);
				return false;
			}
		}
		private const float SliceBarOneRowWidth = 720f;

		private void DrawSmartSliceBar()
		{
			float available = _container != null && _container.contentRect.width > 1f
				? _container.contentRect.width
				: EditorGUIUtility.currentViewWidth;

			if (available >= SliceBarOneRowWidth)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					DrawSliceSettings();
					DrawSliceButton();
				}
			}
			else
			{
				DrawSliceSettings();
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					DrawSliceButton();
				}
			}

			DrawSliceOutcome();
		}

		private void DrawSliceSettings()
		{
			_sensitivity = (PetSliceSensitivity)EditorGUILayout.EnumPopup(
				new GUIContent("Mode", "How eagerly detached pieces such as hearts or question marks are treated as part of a nearby sprite. Normal suits most sheets."),
				_sensitivity);

			_ignoreMetadata = EditorGUILayout.Toggle(
				new GUIContent("Ignore text", "Leaves out wide flat panels and runs of small aligned shapes, which are usually credits or labels rather than frames."),
				_ignoreMetadata);

			_alphaThreshold = EditorGUILayout.IntSlider(
				new GUIContent("Alpha threshold", "How opaque a pixel has to be to count as drawn. Raise it for a sheet with faint halos around the artwork."),
				_alphaThreshold, 0, 254);
		}

		private void DrawSliceButton()
		{
			using (new EditorGUI.DisabledScope(_atlas == null))
			{
				if (GUILayout.Button("Smart Slice", GUILayout.Width(110f))) _deferred.Defer(RunSmartSlice);
			}
		}

		// What detection found, and the one button that commits it.
		private void DrawSliceOutcome()
		{
			if (!EnsureSliceModel()) return;

			EnsureGenerated();

			if (_generateError != null)
			{
				EditorGUILayout.HelpBox(_generateError, MessageType.Warning);
				return;
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(
					string.Format("Detected {0} frames in {1} rows. Generates {2} x {3} cells, {4} columns by {5} rows.",
						_slice.FrameCount, _slice.rows.Count,
						_generated.Grid.cellWidth, _generated.Grid.cellHeight,
						_generated.Grid.columns, _generated.Grid.rows),
					EditorStyles.miniLabel);

				if (GUILayout.Button("Accept", GUILayout.Width(90f))) _deferred.Defer(AcceptSlice);
			}
		}

		private void RunSmartSlice()
		{
			if (_atlas == null) return;

			try
			{
				// Regions the user drew survive a re-detect.
				List<PetSliceIgnoreRegion> regions = _slice?.ignoreRegions;

				_slice = PetSmartSlicer.Detect(
					_atlas.Pixels, _atlas.Sheet.width, _atlas.Sheet.height,
					_sensitivity, _ignoreMetadata, (byte)_alphaThreshold, regions);

				_sliceUndo.Clear();
				_sliceCanvas.Reset();
				SliceChanged();

				SetStatus(string.Format(
					"Found {0} frames across {1} rows, leaving out {2} pieces as text or noise. Review them, then press Accept.",
					_slice.FrameCount, _slice.rows.Count, _slice.IgnoredCount),
					MessageType.Info);
			}
			catch (Exception exception)
			{
				SetStatus(exception.Message, MessageType.Warning);
			}
		}

		private void EnsureGenerated()
		{
			if (_generatedRevision == _sliceRevision) return;

			DropGenerated();
			_generatedRevision = _sliceRevision;

			if (!PetSheetGenerator.TryGenerate(_slice, _atlas.Pixels, out _generated, out _generateError)) return;

			_generatedTexture = new Texture2D(_generated.Width, _generated.Height, TextureFormat.RGBA32, false, false)
			{
				hideFlags = HideFlags.HideAndDontSave,
				filterMode = FilterMode.Point
			};

			_generatedTexture.SetPixels32(_generated.Pixels);
			_generatedTexture.Apply(false, false);
		}
		// Writes the generated sheet to the draft folder.
		private void AcceptSlice()
		{
			if (_slice == null || _atlas == null) return;

			EnsureGenerated();
			if (_generated == null)
			{
				SetStatus(_generateError ?? "Nothing was detected to accept.", MessageType.Warning);
				return;
			}

			string path;
			try
			{
				string folder = WorkingFolder;
				Directory.CreateDirectory(folder);

				// A fresh name each time.
				path = Path.Combine(folder, "sliced-" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".png");
				File.WriteAllBytes(path, PetSheetGenerator.EncodePng(_generated));
			}
			catch (Exception exception)
			{
				SetStatus("The generated sheet could not be written: " + exception.Message, MessageType.Warning);
				return;
			}

			// Cell numbers come from the new sheet.
			_definition.clips.Clear();
			_definition.icon = null;
			_definition.behaviours.Clear();

			_gridSlice = _generated.Grid.Copy();
			_sheetPath = path;
			_sliceMode = PetSliceMode.Grid;
			_gridApplied = false;

			EditorPrefs.SetString(SheetPathKey, _sheetPath);
			ResetSmartState();
			ReloadAtlas();
			ApplyGrid();

			SetStatus(string.Format(
				"Accepted. The sheet is now {0} columns by {1} rows of {2} x {3}, and can be used like any other grid.",
				_gridSlice.columns, _gridSlice.rows, _gridSlice.cellWidth, _gridSlice.cellHeight),
				MessageType.Info);
		}

		private void DrawSmartPage()
		{
			if (_atlas == null)
			{
				EditorGUILayout.HelpBox(_atlasError ?? "Choose a source sheet, then press Smart Slice.", MessageType.Info);
				return;
			}

			if (!EnsureSliceModel())
			{
				EditorGUILayout.HelpBox(
					"Press Smart Slice to find the frames on this sheet. Nothing is changed until you press Accept.",
					MessageType.Info);
				return;
			}

			float available = _container != null && _container.contentRect.width > 1f
				? _container.contentRect.width
				: EditorGUIUtility.currentViewWidth;

			float panel = _smartSplitter.Resolve(available);

			if (available >= _smartSplitter.MinimumForSplit)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					DrawSourceReview(panel);
					_smartSplitter.DrawHandle();
					using (new EditorGUILayout.VerticalScope()) DrawCorrections();
				}

				if (_smartSplitter.IsDragging && _container != null) _container.MarkDirtyRepaint();
			}
			else
			{
				DrawSourceReview(0f);
				DrawCorrections();
			}

			if (!string.IsNullOrEmpty(_status)) EditorGUILayout.HelpBox(_status, _statusType);

			Event e = Event.current;
			if (e.type == EventType.KeyDown && (e.control || e.command) && e.keyCode == KeyCode.Z)
			{
				UndoSlice();
				e.Use();
			}
		}

		// Left: the source with rows, sprites, effects and frames marked on it.
		private void DrawSourceReview(float width)
		{
			GUILayoutOption[] options = width > 0f ? new[] { GUILayout.Width(width) } : new GUILayoutOption[0];

			using (new EditorGUILayout.VerticalScope(options))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Source", EditorStyles.boldLabel, GUILayout.Width(50f));
					_sliceCanvas.ActiveTool = (PetSliceCanvas.Tool)EditorGUILayout.EnumPopup(_sliceCanvas.ActiveTool);
					if (GUILayout.Button("Fit", GUILayout.Width(40f))) _sliceCanvas.Fit();
				}

				Rect area = GUILayoutUtility.GetRect(100f, 260f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				PetSliceIgnoreRegion drawn = _sliceCanvas.Draw(area, _atlas, _slice);

				if (drawn != null)
				{
					PushSliceUndo();
					_slice.ignoreRegions.Add(drawn);
					_slice.ApplyIgnoreRegions();
					SliceChanged();
				}

				EditorGUILayout.LabelField(
					_sliceCanvas.ZoomLabel() + "    click a piece, shift-click to add, scroll to zoom, middle or alt drag to pan",
					EditorStyles.miniLabel);
			}
		}
		// Right: the sheet this would generate, and everything that can be corrected before it is.
		private void DrawCorrections()
		{
			using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(
				_smartScroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView))
			{
				_smartScroll = scroll.scrollPosition;

				DrawNormalizedPreview();
				DrawSelectionTools();
				DrawRowTools();
				DrawIgnoreTools();
			}
		}

		private void DrawNormalizedPreview()
		{
			EnsureGenerated();

			EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);

			if (_generateError != null)
			{
				EditorGUILayout.HelpBox(_generateError, MessageType.Warning);
				return;
			}

			Rect area = GUILayoutUtility.GetRect(100f, 200f, GUILayout.ExpandWidth(true));
			EditorGUI.DrawRect(area, new Color(.1f, .1f, .1f));

			if (_generatedTexture == null) return;

			float scale = Mathf.Min(area.width / _generatedTexture.width, area.height / _generatedTexture.height);
			Rect image = new Rect(
				area.center.x - _generatedTexture.width * scale * .5f,
				area.center.y - _generatedTexture.height * scale * .5f,
				_generatedTexture.width * scale,
				_generatedTexture.height * scale);

			GUI.DrawTexture(image, _generatedTexture, ScaleMode.StretchToFill, true);
			Color lines = new Color(1f, 1f, 1f, .12f);
			for (int c = 1; c < _generated.Grid.columns; c++)
			{
				float x = image.x + c * _generated.Grid.cellWidth * scale;
				EditorGUI.DrawRect(new Rect(x, image.y, 1f, image.height), lines);
			}

			for (int r = 1; r < _generated.Grid.rows; r++)
			{
				float y = image.y + r * _generated.Grid.cellHeight * scale;
				EditorGUI.DrawRect(new Rect(image.x, y, image.width, 1f), lines);
			}

			EditorGUILayout.LabelField(
				string.Format("{0} x {1} cells, {2} columns by {3} rows, {4} frames",
					_generated.Grid.cellWidth, _generated.Grid.cellHeight,
					_generated.Grid.columns, _generated.Grid.rows, _generated.Notes.Count),
				EditorStyles.miniLabel);
		}

		// Which frames the current selection sits in.
		private bool SelectedFrames(out int row, out List<int> frames)
		{
			row = -1;
			frames = new List<int>();

			foreach (int id in _sliceCanvas.Selection)
			{
				PetSliceComponent component = _slice.Component(id);
				if (component == null || component.row < 0) continue;

				if (row < 0) row = component.row;
				else if (component.row != row) return false;

				if (!frames.Contains(component.frame)) frames.Add(component.frame);
			}

			return row >= 0 && frames.Count > 0;
		}

		private void Correct(Action edit)
		{
			PushSliceUndo();
			edit();
			SliceChanged();
		}
		private void DrawSelectionTools()
		{
			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("Selected pieces", EditorStyles.boldLabel);

			bool any = _sliceCanvas.Selection.Count > 0;

			// A correction that spans rows has no single answer for which row the result belongs to.
			bool sameRow = SelectedFrames(out int selectedRow, out List<int> selectedFrames);
			if (!sameRow)
			{
				selectedRow = -1;
				selectedFrames.Clear();
			}

			EditorGUILayout.LabelField(
				any
					? string.Format("{0} selected{1}", _sliceCanvas.Selection.Count,
						sameRow ? string.Format(" in {0}, frame {1}", _slice.rows[selectedRow].name,
							Describe(selectedFrames)) : " across several rows")
					: "Click a piece on the source to select it.",
				EditorStyles.miniLabel);

			DrawFrameDetail(selectedRow, selectedFrames);

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(selectedFrames.Count < 2))
				{
					if (GUILayout.Button(new GUIContent("Merge Frames", "Joins the selected frames into one.")))
						Correct(() => PetSliceCorrections.MergeFrames(_slice, selectedRow, selectedFrames));
				}

				using (new EditorGUI.DisabledScope(selectedFrames.Count != 1))
				{
					if (GUILayout.Button(new GUIContent("Split Frame", "Breaks the frame back into its separate pieces.")))
						Correct(() => PetSliceCorrections.SplitFrame(_slice, selectedRow, selectedFrames[0]));
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(!sameRow))
				{
					if (GUILayout.Button(new GUIContent("Delete Frame", "Removes the frame and leaves its pieces out.")))
					{
						Correct(() =>
						{
							selectedFrames.Sort();
							for (int i = selectedFrames.Count - 1; i >= 0; i--)
								PetSliceCorrections.DeleteFrame(_slice, selectedRow, selectedFrames[i]);
						});
					}

					if (GUILayout.Button(new GUIContent("New Frame", "Makes the selected pieces a frame of their own.")))
						Correct(() => PetSliceCorrections.AddFrame(_slice, selectedRow, new List<int>(_sliceCanvas.Selection)));
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(!any))
				{
					// The commonest correction by far.
					if (GUILayout.Button(new GUIContent("Move Left", "Moves the selected pieces to the previous frame in the row.")))
						Correct(() => NudgeSelection(-1));

					if (GUILayout.Button(new GUIContent("Move Right", "Moves the selected pieces to the next frame in the row.")))
						Correct(() => NudgeSelection(1));
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(!sameRow || selectedFrames.Count != 1))
				{
					if (GUILayout.Button(new GUIContent("Frame Earlier", "Moves this frame one place earlier in its row.")))
						Correct(() => PetSliceCorrections.ReorderFrame(_slice, selectedRow, selectedFrames[0], -1));

					if (GUILayout.Button(new GUIContent("Frame Later", "Moves this frame one place later in its row.")))
						Correct(() => PetSliceCorrections.ReorderFrame(_slice, selectedRow, selectedFrames[0], 1));
				}
			}

			using (new EditorGUI.DisabledScope(!sameRow || selectedFrames.Count != 1 || _slice.rows.Count < 2))
			{
				int target = EditorGUILayout.Popup("Move frame to", -1, RowNames());
				if (target >= 0)
					Correct(() => PetSliceCorrections.MoveFrame(_slice, selectedRow, selectedFrames[0], target));
			}
		}

		// The metadata the spec asks be kept for each generated frame.
		private void DrawFrameDetail(int row, List<int> frames)
		{
			if (row < 0 || frames.Count != 1 || _generated == null) return;

			foreach (PetSheetGenerator.FrameNote note in _generated.Notes)
			{
				if (note.Row != row || note.IndexInRow != frames[0]) continue;

				EditorGUILayout.LabelField(
					string.Format("source {0},{1} {2}x{3}  ->  cell {4} at {5},{6}, content offset {7},{8}, {9} pieces",
						note.Source.x, note.Source.y, note.Source.width, note.Source.height,
						note.Cell, note.CellRect.x, note.CellRect.y,
						note.ContentOffset.x, note.ContentOffset.y, note.Components.Count),
					EditorStyles.miniLabel);
				return;
			}
		}

		private void NudgeSelection(int direction)
		{
			foreach (int id in _sliceCanvas.Selection) PetSliceCorrections.Nudge(_slice, id, direction);
		}

		private static string Describe(List<int> frames)
		{
			if (frames.Count == 1) return (frames[0] + 1).ToString();

			string[] parts = new string[frames.Count];
			for (int i = 0; i < frames.Count; i++) parts[i] = (frames[i] + 1).ToString();
			return string.Join(", ", parts);
		}

		private string[] RowNames()
		{
			string[] names = new string[_slice.rows.Count];
			for (int i = 0; i < names.Length; i++) names[i] = _slice.rows[i].name + " (" + _slice.rows[i].frames.Count + ")";
			return names;
		}
		private void DrawRowTools()
		{
			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("Rows", EditorStyles.boldLabel);

			_sliceRow = Mathf.Clamp(_sliceRow, 0, Mathf.Max(0, _slice.rows.Count - 1));
			if (_slice.rows.Count == 0)
			{
				EditorGUILayout.LabelField("No rows were found.", EditorStyles.miniLabel);
				return;
			}

			_sliceRow = EditorGUILayout.Popup("Row", _sliceRow, RowNames());
			PetSliceRow row = _slice.rows[_sliceRow];

			string renamed = EditorGUILayout.DelayedTextField("Name", row.name);
			if (renamed != row.name) Correct(() => row.name = renamed);
			using (new EditorGUILayout.HorizontalScope())
			{
				int top = EditorGUILayout.IntField("Top", row.top);
				int bottom = EditorGUILayout.IntField("Bottom", row.bottom);
				if (top != row.top || bottom != row.bottom)
				{
					Correct(() =>
					{
						row.top = Mathf.Clamp(top, 0, _slice.height);
						row.bottom = Mathf.Clamp(bottom, row.top + 1, _slice.height);
					});
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(_sliceRow + 1 >= _slice.rows.Count))
				{
					if (GUILayout.Button(new GUIContent("Merge With Next", "Joins this row and the one below into one row.")))
						Correct(() => PetSliceCorrections.MergeRows(_slice, _sliceRow));
				}

				if (GUILayout.Button(new GUIContent("Delete Row", "Removes the row and leaves its pieces out.")))
					Correct(() => PetSliceCorrections.DeleteRow(_slice, _sliceRow));
			}

			using (new EditorGUI.DisabledScope(_sliceCanvas.Selection.Count == 0))
			{
				if (GUILayout.Button(new GUIContent("New Row From Selection", "Makes a row out of the selected pieces, for a band that was missed.")))
					Correct(() => PetSliceCorrections.CreateRow(_slice, new List<int>(_sliceCanvas.Selection)));
			}
		}

		private void DrawIgnoreTools()
		{
			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("Left out", EditorStyles.boldLabel);

			EditorGUILayout.LabelField(
				string.Format("{0} pieces and {1} regions are being left out.", _slice.IgnoredCount, _slice.ignoreRegions.Count),
				EditorStyles.miniLabel);

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(_sliceCanvas.Selection.Count == 0))
				{
					if (GUILayout.Button(new GUIContent("Ignore Piece", "Leaves the selected pieces out of every frame.")))
						Correct(() => PetSliceCorrections.Ignore(_slice, new List<int>(_sliceCanvas.Selection)));
				}

				using (new EditorGUI.DisabledScope(_slice.ignoreRegions.Count == 0))
				{
					if (GUILayout.Button(new GUIContent("Clear Regions", "Removes every ignore region. Pieces inside them stay out until you detect again.")))
						Correct(() => _slice.ignoreRegions.Clear());
				}
			}

			EditorGUILayout.LabelField(
				"Switch the source tool to Ignore Region and drag a box over credits or notes.",
				EditorStyles.wordWrappedMiniLabel);

			EditorGUILayout.Space(4);
			using (new EditorGUI.DisabledScope(_sliceUndo.Count == 0))
			{
				if (GUILayout.Button("Undo correction")) UndoSlice();
			}
		}
	}
}
