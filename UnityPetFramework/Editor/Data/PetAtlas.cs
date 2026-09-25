using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// A loaded sprite sheet.
	internal sealed class PetAtlas : IDisposable
	{
		// A cell counts as empty when no pixel in it is more opaque than this.
		private const byte EmptyAlphaThreshold = 8;

		private Texture2D _sheet;
		private Color32[] _pixels;
		private bool[] _cellEmpty;
		private IList<PetSpriteFrame> _frames;
		public bool HasRectangles => _frames != null;
		public Color32[] Pixels => _pixels;

		// Explicit per-frame rectangles, laid over the sheet.
		public void SetFrames(IList<PetSpriteFrame> frames)
		{
			_frames = frames;
			_cellEmpty = null;
		}

		public PetSpriteFrame Frame(int index) => _frames[index];

		public static PetAtlas FromFile(string path, PetDefinition definition, out string error)
		{
			var atlas = FromFile(path, definition.HasRectangles ? 1 : definition.columns, definition.HasRectangles ? 1 : definition.rows, out error);
			if (atlas != null && definition.HasRectangles) atlas.SetFrames(definition.spriteFrames);
			return atlas;
		}
		public static PetAtlas FromBytes(byte[] png, PetDefinition definition, out string error)
		{
			var atlas = FromBytes(png, definition.HasRectangles ? 1 : definition.columns, definition.HasRectangles ? 1 : definition.rows, out error);
			if (atlas != null && definition.HasRectangles) atlas.SetFrames(definition.spriteFrames);
			return atlas;
		}

		private PetAtlas()
		{
		}

		// The decoded sheet.
		public Texture2D Sheet => _sheet;

		public int Columns { get; private set; }

		public int Rows { get; private set; }

		public int CellWidth { get; private set; }

		public int CellHeight { get; private set; }

		public int CellCount => _frames != null ? _frames.Count : Columns * Rows;

		public static PetAtlas FromFile(string absolutePath, int columns, int rows, out string error)
		{
			if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
			{
				error = "The sprite sheet could not be found at " + (absolutePath ?? "<null>") + ".";
				return null;
			}

			byte[] png;
			try
			{
				png = File.ReadAllBytes(absolutePath);
			}
			catch (Exception exception)
			{
				error = "The sprite sheet could not be read: " + exception.Message;
				return null;
			}

			return FromBytes(png, columns, rows, out error);
		}

		public static PetAtlas FromBytes(byte[] png, int columns, int rows, out string error)
		{
			if (png == null || png.Length == 0)
			{
				error = "The sprite sheet is empty.";
				return null;
			}

			if (columns <= 0 || rows <= 0)
			{
				error = "The grid must have at least one column and one row.";
				return null;
			}
			Texture2D sheet = new Texture2D(2, 2, TextureFormat.RGBA32, false, false)
			{
				hideFlags = HideFlags.HideAndDontSave,
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

			if (!sheet.LoadImage(png, false))
			{
				UnityEngine.Object.DestroyImmediate(sheet);
				error = "The sprite sheet is not a PNG or JPEG that Unity can read.";
				return null;
			}

			// Read before any DestroyImmediate below.
			int sheetWidth = sheet.width;
			int sheetHeight = sheet.height;

			// Exact division or nothing.
			if (sheetWidth % columns != 0 || sheetHeight % rows != 0)
			{
				UnityEngine.Object.DestroyImmediate(sheet);
				error = string.Format(
					"This sheet is {0}x{1}, which does not divide evenly into {2} columns by {3} rows. " +
					"That would need cells of {4:0.##}x{5:0.##}, leaving {6} pixels across and {7} down unaccounted for. " +
					"Check the frame size, or the number of columns and rows.",
					sheetWidth, sheetHeight, columns, rows,
					sheetWidth / (float)columns, sheetHeight / (float)rows,
					sheetWidth % columns, sheetHeight % rows);
				return null;
			}

			error = null;
			return new PetAtlas
			{
				_sheet = sheet,
				_pixels = sheet.GetPixels32(),
				Columns = columns,
				Rows = rows,
				CellWidth = sheetWidth / columns,
				CellHeight = sheetHeight / rows
			};
		}

		public bool IsValidCell(int cellIndex)
		{
			return cellIndex >= 0 && cellIndex < CellCount;
		}

		// Where a cell sits in the texture, in Unity's bottom-up pixel space.
		public RectInt CellPixelRect(int cellIndex)
		{
			if (_frames != null)
			{
				var f = _frames[cellIndex];
				return new RectInt(f.x, _sheet.height - f.y - f.height, f.width, f.height);
			}
			int column = cellIndex % Columns;
			int row = cellIndex / Columns;
			return new RectInt(
				column * CellWidth,
				_sheet.height - (row + 1) * CellHeight,
				CellWidth,
				CellHeight);
		}

		// The same rect as 0..1 coordinates, for GUI.DrawTextureWithTexCoords.
		public Rect CellUvRect(int cellIndex)
		{
			RectInt pixels = CellPixelRect(cellIndex);
			return new Rect(
				pixels.x / (float)_sheet.width,
				pixels.y / (float)_sheet.height,
				pixels.width / (float)_sheet.width,
				pixels.height / (float)_sheet.height);
		}

		// Cells with nothing drawn in them.
		public bool IsCellEmpty(int cellIndex)
		{
			if (!IsValidCell(cellIndex))
			{
				return true;
			}

			if (_cellEmpty == null)
			{
				_cellEmpty = new bool[CellCount];
				for (int i = 0; i < CellCount; i++)
				{
					_cellEmpty[i] = ScanIsEmpty(i);
				}
			}

			return _cellEmpty[cellIndex];
		}

		// The union of what is drawn across the given cells.
		public RectInt ComputeContentBounds(IEnumerable<int> cellIndices)
		{
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;

			foreach (int cellIndex in cellIndices)
			{
				if (!IsValidCell(cellIndex))
				{
					continue;
				}

				RectInt cell = CellPixelRect(cellIndex);
				for (int y = 0; y < cell.height; y++)
				{
					int rowStart = (cell.y + y) * _sheet.width + cell.x;
					for (int x = 0; x < cell.width; x++)
					{
						if (_pixels[rowStart + x].a <= EmptyAlphaThreshold)
						{
							continue;
						}

						// Flip to top-down so the result reads the way the sheet looks.
						int localY = cell.height - 1 - y;
						if (x < left) { left = x; }
						if (x > right) { right = x; }
						if (localY < top) { top = localY; }
						if (localY > bottom) { bottom = localY; }
					}
				}
			}

			if (left > right || top > bottom)
			{
				return new RectInt(0, 0, 0, 0);
			}

			return new RectInt(left, top, right - left + 1, bottom - top + 1);
		}

		// Cell-local top-down pixels to a pixel index into the sheet's bottom-up buffer.
		public int PixelIndex(int cellIndex, int localX, int localY)
		{
			RectInt cell = CellPixelRect(cellIndex);
			int y = cell.y + (cell.height - 1 - localY);
			return y * _sheet.width + cell.x + localX;
		}

		public Color32 GetPixel(int cellIndex, int localX, int localY)
		{
			return _pixels[PixelIndex(cellIndex, localX, localY)];
		}

		public void Dispose()
		{
			if (_sheet != null)
			{
				UnityEngine.Object.DestroyImmediate(_sheet);
				_sheet = null;
			}

			_pixels = null;
			_cellEmpty = null;
		}

		private bool ScanIsEmpty(int cellIndex)
		{
			RectInt cell = CellPixelRect(cellIndex);
			for (int y = 0; y < cell.height; y++)
			{
				int rowStart = (cell.y + y) * _sheet.width + cell.x;
				for (int x = 0; x < cell.width; x++)
				{
					if (_pixels[rowStart + x].a > EmptyAlphaThreshold)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
