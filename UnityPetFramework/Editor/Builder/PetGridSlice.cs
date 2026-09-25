using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// A uniform grid laid over a sprite sheet.
	[Serializable]
	internal sealed class PetGridSlice
	{
		// Matches the per-pet limit in PetDefinitionValidator.
		public const int MaxCells = 4096;

		public int cellWidth = 64;
		public int cellHeight = 64;
		public int columns = 4;
		public int rows = 4;
		public int offsetX;
		public int offsetY;
		public int spacingX;
		public int spacingY;

		public int CellCount => columns * rows;

		// The smallest sheet this grid fits on.
		public int RequiredWidth => offsetX + columns * cellWidth + Mathf.Max(0, columns - 1) * spacingX;

		public int RequiredHeight => offsetY + rows * cellHeight + Mathf.Max(0, rows - 1) * spacingY;

		// No padding anywhere.
		public bool IsTight => offsetX == 0 && offsetY == 0 && spacingX == 0 && spacingY == 0;

		public PetGridSlice Copy()
		{
			return (PetGridSlice)MemberwiseClone();
		}

		public bool Equivalent(PetGridSlice other)
		{
			return other != null && other.cellWidth == cellWidth && other.cellHeight == cellHeight &&
				other.columns == columns && other.rows == rows && other.offsetX == offsetX &&
				other.offsetY == offsetY && other.spacingX == spacingX && other.spacingY == spacingY;
		}

		// Top-left pixel rectangle of one cell.
		public RectInt CellRect(int index)
		{
			int column = index % Mathf.Max(1, columns);
			int row = index / Mathf.Max(1, columns);
			return new RectInt(
				offsetX + column * (cellWidth + spacingX),
				offsetY + row * (cellHeight + spacingY),
				cellWidth,
				cellHeight);
		}

		// Every cell as a frame, anchored bottom centre.
		public List<PetSpriteFrame> BuildFrames()
		{
			List<PetSpriteFrame> frames = new List<PetSpriteFrame>(CellCount);
			for (int index = 0; index < CellCount; index++)
			{
				RectInt cell = CellRect(index);
				frames.Add(new PetSpriteFrame
				{
					x = cell.x,
					y = cell.y,
					width = cell.width,
					height = cell.height,
					pivotX = cell.width * .5f,
					pivotY = cell.height
				});
			}

			return frames;
		}

		// Everything the rest of the creator assumes about a grid, checked in one place.
		public bool Validate(int sheetWidth, int sheetHeight, out string error)
		{
			if (cellWidth < 1 || cellHeight < 1)
			{
				error = "A cell has to be at least one pixel on each side.";
				return false;
			}

			if (columns < 1 || rows < 1)
			{
				error = "The grid needs at least one column and one row.";
				return false;
			}

			if (offsetX < 0 || offsetY < 0 || spacingX < 0 || spacingY < 0)
			{
				error = "Offset and spacing cannot be negative.";
				return false;
			}

			if ((long)columns * rows > MaxCells)
			{
				error = string.Format(
					"{0} columns by {1} rows is {2} cells. At most {3} are supported.",
					columns, rows, (long)columns * rows, MaxCells);
				return false;
			}

			if (sheetWidth > 0 && RequiredWidth > sheetWidth)
			{
				error = string.Format(
					"This grid needs {0} pixels across but the sheet is only {1}. " +
					"{2} columns of {3} with {4} spacing and {5} offset runs {6} pixels past the right edge.",
					RequiredWidth, sheetWidth, columns, cellWidth, spacingX, offsetX, RequiredWidth - sheetWidth);
				return false;
			}

			if (sheetHeight > 0 && RequiredHeight > sheetHeight)
			{
				error = string.Format(
					"This grid needs {0} pixels down but the sheet is only {1}. " +
					"{2} rows of {3} with {4} spacing and {5} offset runs {6} pixels past the bottom edge.",
					RequiredHeight, sheetHeight, rows, cellHeight, spacingY, offsetY, RequiredHeight - sheetHeight);
				return false;
			}

			error = null;
			return true;
		}

		// How much of the sheet the grid does not cover.
		public string Leftover(int sheetWidth, int sheetHeight)
		{
			int across = sheetWidth - RequiredWidth;
			int down = sheetHeight - RequiredHeight;
			if (across <= 0 && down <= 0)
			{
				return null;
			}

			return string.Format("{0} pixels across and {1} down are outside the grid.", Mathf.Max(0, across), Mathf.Max(0, down));
		}

		// Recovers the grid a set of frames came from, for reopening a pet that was saved earlier.
		public static bool TryInfer(IList<PetSpriteFrame> frames, out PetGridSlice grid)
		{
			grid = null;
			if (frames == null || frames.Count == 0) return false;

			int cellWidth = frames[0].width;
			int cellHeight = frames[0].height;
			int left = int.MaxValue, top = int.MaxValue;

			foreach (PetSpriteFrame frame in frames)
			{
				if (frame.width != cellWidth || frame.height != cellHeight) return false;

				left = Mathf.Min(left, frame.x);
				top = Mathf.Min(top, frame.y);
			}

			// Distinct column and row positions, which give the counts and the pitch.
			SortedSet<int> columnsAt = new SortedSet<int>();
			SortedSet<int> rowsAt = new SortedSet<int>();
			foreach (PetSpriteFrame frame in frames)
			{
				columnsAt.Add(frame.x);
				rowsAt.Add(frame.y);
			}

			if (!TryPitch(columnsAt, cellWidth, out int spacingX)) return false;
			if (!TryPitch(rowsAt, cellHeight, out int spacingY)) return false;

			grid = new PetGridSlice
			{
				cellWidth = cellWidth,
				cellHeight = cellHeight,
				columns = columnsAt.Count,
				rows = rowsAt.Count,
				offsetX = left,
				offsetY = top,
				spacingX = spacingX,
				spacingY = spacingY
			};

			return true;
		}

		// Even steps or nothing.
		private static bool TryPitch(SortedSet<int> positions, int cell, out int spacing)
		{
			spacing = 0;
			if (positions.Count < 2) return true;

			int[] sorted = new int[positions.Count];
			positions.CopyTo(sorted);
			int step = sorted[1] - sorted[0];
			if (step < cell) return false;

			for (int i = 2; i < sorted.Length; i++)
			{
				if (sorted[i] - sorted[i - 1] != step) return false;
			}

			spacing = step - cell;
			return true;
		}

		// The most cells of this size that fit in the space, given the padding.
		public static int FitCount(int available, int cell, int offset, int spacing)
		{
			if (cell < 1 || available - offset < cell)
			{
				return 0;
			}

			return (available - offset + spacing) / (cell + spacing);
		}

		// The largest cell size that fits this many cells in the space.
		public static int FitCell(int available, int count, int offset, int spacing)
		{
			if (count < 1)
			{
				return 0;
			}

			int usable = available - offset - Mathf.Max(0, count - 1) * spacing;
			return usable < count ? 0 : usable / count;
		}
	}
}
