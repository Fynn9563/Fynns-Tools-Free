using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Turns a reviewed slice model into an ordinary uniform sprite sheet.
	internal static class PetSheetGenerator
	{
		// Generous, and only here to turn an absurd model into a message instead of an out of memory.
		private const int MaxAxis = 8192;
		private const long MaxPixels = 32L * 1024 * 1024;

		// What a generated frame was made from.
		internal sealed class FrameNote
		{
			public int Row;
			public int IndexInRow;
			public int Cell;
			public RectInt Source;
			public RectInt CellRect;
			public Vector2Int ContentOffset;
			public List<int> Components = new List<int>();
		}

		internal sealed class Result
		{
			public Color32[] Pixels;
			public int Width;
			public int Height;
			public PetGridSlice Grid;
			public List<FrameNote> Notes = new List<FrameNote>();
		}

		public static bool TryGenerate(PetSliceModel model, Color32[] source, out Result result, out string error)
		{
			result = null;

			if (model == null || source == null || model.labels == null ||
				source.Length != model.width * model.height)
			{
				error = "The detection result does not match the sheet it came from. Detect again.";
				return false;
			}

			int frames = model.FrameCount;
			if (frames == 0)
			{
				error = "No frames were detected, so there is nothing to generate.";
				return false;
			}

			Vector2Int cell = model.CellSize();
			int columns = model.MaxFramesPerRow;
			int rows = model.rows.Count;

			long width = (long)columns * cell.x;
			long height = (long)rows * cell.y;

			if (width > MaxAxis || height > MaxAxis || width * height > MaxPixels)
			{
				error = string.Format(
					"The generated sheet would be {0} x {1}, which is too large. " +
					"Usually that means one frame has absorbed something far away: check the widest frames in the review.",
					width, height);
				return false;
			}

			if ((long)columns * rows > PetGridSlice.MaxCells)
			{
				error = string.Format(
					"{0} columns by {1} rows is {2} cells, and at most {3} are supported.",
					columns, rows, (long)columns * rows, PetGridSlice.MaxCells);
				return false;
			}

			result = new Result
			{
				Pixels = new Color32[width * height],
				Width = (int)width,
				Height = (int)height,
				Grid = new PetGridSlice
				{
					cellWidth = cell.x,
					cellHeight = cell.y,
					columns = columns,
					rows = rows
				}
			};

			Paint(model, source, result);
			error = null;
			return true;
		}

		private static void Paint(PetSliceModel model, Color32[] source, Result result)
		{
			int columns = result.Grid.columns;
			HashSet<int> wanted = new HashSet<int>();

			for (int r = 0; r < model.rows.Count; r++)
			{
				PetSliceRow row = model.rows[r];

				for (int f = 0; f < row.frames.Count; f++)
				{
					PetSliceFrame frame = row.frames[f];
					RectInt bounds = model.FrameBounds(frame);
					if (bounds.width <= 0 || bounds.height <= 0) continue;

					wanted.Clear();
					foreach (int id in frame.components)
					{
						PetSliceComponent component = model.Component(id);
						if (component != null && !component.ignored) wanted.Add(id);
					}

					int index = r * columns + f;
					RectInt cellRect = result.Grid.CellRect(index);

					// Centred across, sat on the floor.
					int offsetX = (cellRect.width - bounds.width) / 2;
					int offsetY = cellRect.height - bounds.height;

					CopyFrame(model, source, result, bounds, wanted, cellRect.x + offsetX, cellRect.y + offsetY);

					FrameNote note = new FrameNote
					{
						Row = r,
						IndexInRow = f,
						Cell = index,
						Source = bounds,
						CellRect = cellRect,
						ContentOffset = new Vector2Int(offsetX, offsetY)
					};

					note.Components.AddRange(wanted);
					result.Notes.Add(note);
				}
			}
		}

		// Copies one frame's pixels.
		private static void CopyFrame(
			PetSliceModel model, Color32[] source, Result result,
			RectInt bounds, HashSet<int> wanted, int destLeft, int destTop)
		{
			for (int y = 0; y < bounds.height; y++)
			{
				int sourceY = bounds.y + y;
				if (sourceY < 0 || sourceY >= model.height) continue;

				int destY = destTop + y;
				if (destY < 0 || destY >= result.Height) continue;

				int sourceRow = (model.height - 1 - sourceY) * model.width;
				int labelRow = sourceY * model.width;
				int destRow = (result.Height - 1 - destY) * result.Width;

				for (int x = 0; x < bounds.width; x++)
				{
					int sourceX = bounds.x + x;
					if (sourceX < 0 || sourceX >= model.width) continue;

					// The one line this whole class exists for.
					if (!wanted.Contains(model.labels[labelRow + sourceX])) continue;

					int destX = destLeft + x;
					if (destX < 0 || destX >= result.Width) continue;

					result.Pixels[destRow + destX] = source[sourceRow + sourceX];
				}
			}
		}

		// PNG bytes for the generated sheet.
		public static byte[] EncodePng(Result result)
		{
			Texture2D texture = new Texture2D(result.Width, result.Height, TextureFormat.RGBA32, false, false)
			{
				hideFlags = HideFlags.HideAndDontSave
			};

			try
			{
				texture.SetPixels32(result.Pixels);
				texture.Apply(false, false);
				return texture.EncodeToPNG();
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(texture);
			}
		}
	}
}
