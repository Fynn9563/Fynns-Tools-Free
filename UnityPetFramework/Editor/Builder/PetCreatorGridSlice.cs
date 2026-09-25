using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// How a sprite sheet is cut into frames.
	internal enum PetSliceMode
	{
		Grid,
		Smart
	}

	internal sealed partial class PetCreatorPage
	{
		private const string SliceModeKey = "FynnsTools.UnityPetFramework.Builder.SliceMode";
		private const string GridKey = "FynnsTools.UnityPetFramework.Builder.Grid";
		private const float PairedFieldWidth = 380f;

		private PetSliceMode _sliceMode = PetSliceMode.Grid;
		private PetGridSlice _gridSlice = new PetGridSlice();

		// Set once a grid has actually been applied to this sheet.
		private bool _gridApplied;
		private bool _gridPaddingExpanded;

		// False when a pet was opened whose frames are not on any regular grid.
		private bool _gridInferred = true;

		private void LoadSlicingPrefs()
		{
			_sliceMode = (PetSliceMode)EditorPrefs.GetInt(SliceModeKey, (int)PetSliceMode.Grid);

			// Restored from the working pet rather than from a preference.
			_gridApplied = _definition != null && _definition.HasRectangles;

			string json = EditorPrefs.GetString(GridKey, "");
			if (!string.IsNullOrEmpty(json))
			{
				PetGridSlice stored = JsonUtility.FromJson<PetGridSlice>(json);
				if (stored != null) _gridSlice = stored;
			}
		}

		private void SaveSlicingPrefs()
		{
			EditorPrefs.SetInt(SliceModeKey, (int)_sliceMode);
			EditorPrefs.SetString(GridKey, JsonUtility.ToJson(_gridSlice));
		}

		// Recovers the grid of a pet being reopened.
		private void AdoptLoadedSlicing(PetDefinition loaded)
		{
			ResetSmartState();
			_sliceMode = PetSliceMode.Grid;
			_gridApplied = loaded.HasRectangles;

			_gridInferred = false;
			if (loaded.HasRectangles && PetGridSlice.TryInfer(loaded.spriteFrames, out PetGridSlice inferred))
			{
				_gridSlice = inferred;
				_gridInferred = true;
			}

			SaveSlicingPrefs();
			ApplyFramesToAtlas();
		}

		// Both modes finish with explicit rectangles.
		private void ApplyFramesToAtlas()
		{
			if (_atlas != null && _definition.HasRectangles) _atlas.SetFrames(_definition.spriteFrames);
		}

		// Loads the sheet whole.
		private void ReloadAtlas()
		{
			_atlas?.Dispose();
			_atlas = null;
			_atlasError = null;
			_grid.Reset();

			if (string.IsNullOrEmpty(_sheetPath) || !File.Exists(_sheetPath))
			{
				if (!string.IsNullOrEmpty(_sheetPath)) _atlasError = "The sprite sheet is no longer at " + _sheetPath + ".";

				return;
			}

			// One cell covering the whole sheet, purely to decode it.
			_atlas = PetAtlas.FromFile(_sheetPath, 1, 1, out _atlasError);
			if (_atlas == null) return;

			_definition.atlas = Path.GetFileName(_sheetPath);
			ApplyFramesToAtlas();
		}

		private void DrawSourceSection()
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Source Sheet");
					EditorGUILayout.SelectableLabel(
						string.IsNullOrEmpty(_sheetPath) ? "None" : Path.GetFileName(_sheetPath),
						EditorStyles.textField,
						GUILayout.Height(EditorGUIUtility.singleLineHeight));

					if (GUILayout.Button("Browse", GUILayout.Width(70f))) _deferred.Defer(BrowseForSheet);
				}

				if (_atlas != null)
				{
					EditorGUILayout.LabelField(
						string.Format("{0} x {1} pixels", _atlas.Sheet.width, _atlas.Sheet.height),
						EditorStyles.miniLabel);
				}

				EditorGUI.BeginChangeCheck();
				PetSliceMode mode = (PetSliceMode)GUILayout.Toolbar((int)_sliceMode, SliceModeLabels);
				if (EditorGUI.EndChangeCheck())
				{
					_sliceMode = mode;
					SaveSlicingPrefs();
				}

				if (_sliceMode == PetSliceMode.Smart)
				{
					DrawSmartSliceBar();
					return;
				}

				DrawGridControls();
			}
		}

		private static readonly string[] SliceModeLabels =
		{
			"Grid Slice",
			"Smart Slice"
		};
		// Cell size and count are two views of one number when there is no padding.
		private void DrawGridControls()
		{
			int sheetWidth = _atlas?.Sheet != null ? _atlas.Sheet.width : 0;
			int sheetHeight = _atlas?.Sheet != null ? _atlas.Sheet.height : 0;

			using (Paired())
			{
				EditorGUI.BeginChangeCheck();
				int cellWidth = Mathf.Max(1, EditorGUILayout.IntField("Cell Width", _gridSlice.cellWidth));
				int cellHeight = Mathf.Max(1, EditorGUILayout.IntField("Cell Height", _gridSlice.cellHeight));
				if (EditorGUI.EndChangeCheck())
				{
					_gridSlice.cellWidth = cellWidth;
					_gridSlice.cellHeight = cellHeight;
					if (_gridSlice.IsTight) FitCounts(sheetWidth, sheetHeight);
					SaveSlicingPrefs();
				}
			}

			using (Paired())
			{
				EditorGUI.BeginChangeCheck();
				int columns = Mathf.Max(1, EditorGUILayout.IntField("Columns", _gridSlice.columns));
				int rows = Mathf.Max(1, EditorGUILayout.IntField("Rows", _gridSlice.rows));
				if (EditorGUI.EndChangeCheck())
				{
					_gridSlice.columns = columns;
					_gridSlice.rows = rows;
					if (_gridSlice.IsTight) FitCellSize(sheetWidth, sheetHeight);
					SaveSlicingPrefs();
				}
			}

			_gridPaddingExpanded = EditorGUILayout.Foldout(_gridPaddingExpanded, "Offset and spacing", true);
			if (_gridPaddingExpanded) DrawGridPadding();

			DrawGridSummary(sheetWidth, sheetHeight);
		}

		private void DrawGridPadding()
		{
			EditorGUI.BeginChangeCheck();

			using (Paired())
			{
				_gridSlice.offsetX = Mathf.Max(0, EditorGUILayout.IntField(
					new GUIContent("Offset X", "Blank pixels before the first column."), _gridSlice.offsetX));
				_gridSlice.offsetY = Mathf.Max(0, EditorGUILayout.IntField(
					new GUIContent("Offset Y", "Blank pixels above the first row."), _gridSlice.offsetY));
			}

			using (Paired())
			{
				_gridSlice.spacingX = Mathf.Max(0, EditorGUILayout.IntField(
					new GUIContent("Spacing X", "Blank pixels between one column and the next."), _gridSlice.spacingX));
				_gridSlice.spacingY = Mathf.Max(0, EditorGUILayout.IntField(
					new GUIContent("Spacing Y", "Blank pixels between one row and the next."), _gridSlice.spacingY));
			}

			if (EditorGUI.EndChangeCheck()) SaveSlicingPrefs();

			if (!_gridSlice.IsTight)
			{
				EditorGUILayout.LabelField(
					"With padding set, cell size and counts stay exactly as entered.",
					EditorStyles.miniLabel);
			}
		}
		// A row when there is room for one, a column when there is not.
		private IDisposable Paired()
		{
			float available = _container != null && _container.contentRect.width > 1f
				? _container.contentRect.width
				: EditorGUIUtility.currentViewWidth;

			return available >= PairedFieldWidth
				? (IDisposable)new EditorGUILayout.HorizontalScope()
				: new EditorGUILayout.VerticalScope();
		}

		private void DrawGridSummary(int sheetWidth, int sheetHeight)
		{
			bool valid = _gridSlice.Validate(sheetWidth, sheetHeight, out string error);

			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(_atlas == null))
				{
					if (GUILayout.Button(
						new GUIContent("Fit To Sheet", "Sets columns and rows to as many cells of this size as the sheet holds."),
						GUILayout.Width(100f)))
					{
						FitCounts(sheetWidth, sheetHeight);
						SaveSlicingPrefs();
					}
				}

				GUILayout.FlexibleSpace();

				using (new EditorGUI.DisabledScope(_atlas == null || !valid))
				{
					if (GUILayout.Button("Apply Grid", GUILayout.Width(100f))) _deferred.Defer(ApplyGrid);
				}
			}

			if (_atlas == null)
			{
				EditorGUILayout.HelpBox("Choose a source sheet to begin.", MessageType.Info);
				return;
			}

			if (!valid)
			{
				EditorGUILayout.HelpBox(error, MessageType.Warning);
				return;
			}

			string leftover = _gridSlice.Leftover(sheetWidth, sheetHeight);
			EditorGUILayout.LabelField(
				string.Format("{0} cells of {1} x {2}. {3}",
					_gridSlice.CellCount, _gridSlice.cellWidth, _gridSlice.cellHeight,
					leftover ?? "The grid covers the whole sheet."),
				EditorStyles.miniLabel);

			if (!_gridApplied)
			{
				EditorGUILayout.HelpBox("Press Apply Grid to start picking frames.", MessageType.Info);
				return;
			}

			if (!_gridInferred)
			{
				EditorGUILayout.HelpBox(
					"This pet's frames are not on an even grid, so the numbers above do not describe it. " +
					"Its animations still work. Applying a grid would replace those frames.",
					MessageType.Info);
			}
		}

		private void FitCounts(int sheetWidth, int sheetHeight)
		{
			int columns = PetGridSlice.FitCount(sheetWidth, _gridSlice.cellWidth, _gridSlice.offsetX, _gridSlice.spacingX);
			int rows = PetGridSlice.FitCount(sheetHeight, _gridSlice.cellHeight, _gridSlice.offsetY, _gridSlice.spacingY);

			// Zero means not even one cell fits.
			if (columns > 0) _gridSlice.columns = columns;
			if (rows > 0) _gridSlice.rows = rows;
		}

		private void FitCellSize(int sheetWidth, int sheetHeight)
		{
			int cellWidth = PetGridSlice.FitCell(sheetWidth, _gridSlice.columns, _gridSlice.offsetX, _gridSlice.spacingX);
			int cellHeight = PetGridSlice.FitCell(sheetHeight, _gridSlice.rows, _gridSlice.offsetY, _gridSlice.spacingY);

			if (cellWidth > 0) _gridSlice.cellWidth = cellWidth;
			if (cellHeight > 0) _gridSlice.cellHeight = cellHeight;
		}

		// Turns the grid into the frame table everything downstream reads.
		private void ApplyGrid()
		{
			if (_atlas == null)
			{
				SetStatus("Choose a source sheet first.", MessageType.Warning);
				return;
			}

			if (!_gridSlice.Validate(_atlas.Sheet.width, _atlas.Sheet.height, out string error))
			{
				SetStatus(error, MessageType.Warning);
				return;
			}

			bool assigned = _definition.UsedCells().Count > 0;
			bool renumbers = assigned &&
				(!PetGridSlice.TryInfer(_definition.spriteFrames, out PetGridSlice current) || !_gridSlice.Equivalent(current));

			if (renumbers && !EditorUtility.DisplayDialog(
				"Apply a different grid",
				"Changing the grid renumbers every cell, so the frames already picked for your animations would point somewhere else." + Newline + Newline +
				"Those animations will be emptied. Pets you have already saved are not affected.",
				"Apply Grid",
				"Cancel"))
			{
				return;
			}

			if (renumbers)
			{
				_definition.clips.Clear();
				_definition.icon = null;
				_definition.behaviours.Clear();
			}

			_definition.spriteFrames = _gridSlice.BuildFrames();
			_definition.schemaVersion = UnityPetFrameworkInfo.PetSchemaVersion;
			_gridApplied = true;
			_gridInferred = true;

			ApplyFramesToAtlas();
			_grid.Reset();
			_preview.SetDefinition(_definition);
			SaveWorkingPet();
			SaveSlicingPrefs();

			SetStatus(string.Format(
				"Grid applied: {0} columns by {1} rows of {2} x {3}, {4} cells, {5} blank.",
				_gridSlice.columns, _gridSlice.rows, _gridSlice.cellWidth, _gridSlice.cellHeight,
				_gridSlice.CellCount, CountEmptyCells()),
				MessageType.Info);
		}
	}
}
