using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Rebuilds a pet's sheet with only what it actually uses.
	internal static class PetAtlasRepacker
	{
		internal sealed class Result
		{
			public PetDefinition Definition;
			public byte[] AtlasPng;
			public int CellsBefore;
			public int CellsAfter;
			public long PixelsBefore;
			public long PixelsAfter;
		}

		// Keeps the sheet within what any GPU will accept.
		private const int MaxSheetDimension = 4096;

		public static bool TryRepack(PetAtlas atlas, PetDefinition definition, out Result result, out string error)
		{
			result = null;

			if (atlas == null || definition == null)
			{
				error = "There is no pet to repack.";
				return false;
			}

			if (definition.HasRectangles) return PetRectanglePacker.TryPack(atlas, definition, out result, out error);

			List<int> used = CollectUsedCells(definition, atlas);
			if (used.Count == 0)
			{
				error = "This pet has no frames, so there is nothing to pack.";
				return false;
			}

			// Across the referenced cells only.
			RectInt content = atlas.ComputeContentBounds(used);
			if (content.width <= 0 || content.height <= 0)
			{
				error = "Nothing is drawn in any of the frames this pet uses.";
				return false;
			}

			int columns = ChooseColumns(used.Count, content.width, content.height);
			int rows = Mathf.CeilToInt(used.Count / (float)columns);

			if (columns * content.width > MaxSheetDimension || rows * content.height > MaxSheetDimension)
			{
				error = "The packed sheet would be larger than " + MaxSheetDimension + " pixels on a side.";
				return false;
			}

			Dictionary<int, int> remap = new Dictionary<int, int>(used.Count);
			for (int i = 0; i < used.Count; i++)
			{
				remap[used[i]] = i;
			}

			byte[] png = BuildSheet(atlas, used, content, columns, rows, out string buildError);
			if (png == null)
			{
				error = buildError;
				return false;
			}

			result = new Result
			{
				Definition = Rewrite(definition, remap, columns, rows, content),
				AtlasPng = png,
				CellsBefore = atlas.CellCount,
				CellsAfter = used.Count,
				PixelsBefore = (long)atlas.CellCount * atlas.CellWidth * atlas.CellHeight,
				PixelsAfter = (long)used.Count * content.width * content.height
			};

			error = null;
			return true;
		}

		// In first-use order, so frames that play together stay near each other on the sheet.
		private static List<int> CollectUsedCells(PetDefinition definition, PetAtlas atlas)
		{
			List<int> used = new List<int>();
			HashSet<int> seen = new HashSet<int>();

			foreach (PetClipDefinition clip in definition.AllSequences())
			{
				if (clip?.frames == null)
				{
					continue;
				}

				foreach (PetFrameRef frame in clip.frames)
				{
					if (atlas.IsValidCell(frame.cell) && seen.Add(frame.cell))
					{
						used.Add(frame.cell);
					}
				}
			}

			return used;
		}
		private static int ChooseColumns(int count, int cellWidth, int cellHeight)
		{
			int best = 1;
			float bestScore = float.MaxValue;

			for (int columns = 1; columns <= count; columns++)
			{
				int rows = Mathf.CeilToInt(count / (float)columns);
				float width = columns * cellWidth;
				float height = rows * cellHeight;

				// Penalise both lopsidedness and the empty tail of the last row.
				float aspect = Mathf.Max(width, height) / Mathf.Min(width, height);
				float waste = (columns * rows - count) / (float)count;
				float score = aspect + waste * 2f;

				if (score < bestScore)
				{
					bestScore = score;
					best = columns;
				}
			}

			return best;
		}

		private static byte[] BuildSheet(PetAtlas atlas, List<int> used, RectInt content, int columns, int rows, out string error)
		{
			int width = columns * content.width;
			int height = rows * content.height;

			Texture2D packed = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
			{
				hideFlags = HideFlags.HideAndDontSave,
				filterMode = FilterMode.Point
			};

			try
			{
				Color32[] pixels = new Color32[width * height];

				for (int slot = 0; slot < used.Count; slot++)
				{
					int destinationColumn = slot % columns;
					int destinationRow = slot / columns;

					for (int y = 0; y < content.height; y++)
					{
						for (int x = 0; x < content.width; x++)
						{
							Color32 source = atlas.GetPixel(used[slot], content.x + x, content.y + y);

							// The destination is bottom-up like every Unity texture.
							int destinationX = destinationColumn * content.width + x;
							int destinationY = height - 1 - (destinationRow * content.height + y);
							pixels[destinationY * width + destinationX] = source;
						}
					}
				}

				packed.SetPixels32(pixels);
				packed.Apply(false, false);

				error = null;
				return packed.EncodeToPNG();
			}
			catch (System.Exception exception)
			{
				error = "The packed sheet could not be built: " + exception.Message;
				return null;
			}
			finally
			{
				Object.DestroyImmediate(packed);
			}
		}

		// A copy with the new grid and every frame index rewritten.
		private static PetDefinition Rewrite(PetDefinition source, Dictionary<int, int> remap, int columns, int rows, RectInt content)
		{
			PetDefinition packed = JsonUtility.FromJson<PetDefinition>(JsonUtility.ToJson(source)).Normalise();

			packed.columns = columns;
			packed.rows = rows;
			packed.cellWidth = content.width;
			packed.cellHeight = content.height;

			// The cells are now cropped to exactly the drawn area.
			packed.contentLeft = 0;
			packed.contentTop = 0;
			packed.contentWidth = content.width;
			packed.contentHeight = content.height;

			foreach (PetClipDefinition clip in packed.AllSequences())
			{
				if (clip?.frames == null)
				{
					continue;
				}

				for (int i = 0; i < clip.frames.Count; i++)
				{
					PetFrameRef frame = clip.frames[i];
					if (remap.TryGetValue(frame.cell, out int moved))
					{
						frame.cell = moved;
						clip.frames[i] = frame;
					}
				}
			}

			return packed;
		}
	}
}
