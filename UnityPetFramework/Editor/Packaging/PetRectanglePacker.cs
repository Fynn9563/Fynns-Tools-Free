using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Best-short-side-fit MaxRects without rotation or resampling.
	internal static class PetRectanglePacker
	{
		public static bool TryPack(PetAtlas atlas, PetDefinition definition, out PetAtlasRepacker.Result result, out string error)
		{
			result = null;
			if (!PetDefinitionValidator.Validate(definition, atlas.Sheet.width, atlas.Sheet.height, out error)) return false;
			var used = definition.UsedCells();

			// Frames that are all one size go out as a plain grid rather than through the packer.
			if (TryPackUniform(atlas, definition, used, out result, out error)) return true;
			if (error != null) return false;
			used.Sort((a, b) => (definition.spriteFrames[b].width * definition.spriteFrames[b].height).CompareTo(definition.spriteFrames[a].width * definition.spriteFrames[a].height));
			Dictionary<int, RectInt> best = null;
			int bestWidth = 0, bestHeight = 0, area = int.MaxValue;
			for (int width = 64; width <= 4096; width *= 2)
			{
				var free = new List<RectInt> { new RectInt(0, 0, width, 4096) };
				var placed = new Dictionary<int, RectInt>();
				int extentX = 0, extentY = 0;
				foreach (int id in used)
				{
					var f = definition.spriteFrames[id];
					int index = -1, score = int.MaxValue;
					for (int i = 0; i < free.Count; i++)
					{
						var r = free[i];
						if (r.width < f.width + 2 || r.height < f.height + 2) continue;
						int s = Mathf.Min(r.width - f.width - 2, r.height - f.height - 2);
						if (s < score) { score = s; index = i; }
					}
					if (index < 0) break;
					var allocation = new RectInt(free[index].x, free[index].y, f.width + 2, f.height + 2);
					placed[id] = new RectInt(allocation.x + 1, allocation.y + 1, f.width, f.height);
					extentX = Mathf.Max(extentX, allocation.xMax); extentY = Mathf.Max(extentY, allocation.yMax);
					SplitFree(free, allocation);
				}
				if (placed.Count == used.Count && extentX * extentY < area)
				{ best = placed; bestWidth = extentX; bestHeight = extentY; area = extentX * extentY; }
			}
			if (best == null || used.Count == 0) { error = "The selected frames do not fit within a 4096 x 4096 atlas."; return false; }
			Texture2D texture = null;
			try
			{
				var packed = JsonUtility.FromJson<PetDefinition>(JsonUtility.ToJson(definition)).Normalise();
				packed.spriteFrames.Clear();
				packed.columns = packed.rows = packed.cellWidth = packed.cellHeight = 0;
				var remap = new Dictionary<int, int>();
				var pixels = new Color32[area];
				foreach (int id in used)
				{
					var r = best[id];
					var f = definition.spriteFrames[id].Copy();
					f.x = r.x; f.y = r.y;
					remap[id] = packed.spriteFrames.Count;
					packed.spriteFrames.Add(f);
					for (int y = 0; y < f.height; y++) for (int x = 0; x < f.width; x++)
						pixels[(bestHeight - 1 - r.y - y) * bestWidth + r.x + x] = atlas.GetPixel(id, x, y);
				}
				foreach (var clip in packed.AllSequences()) for (int i = 0; i < clip.frames.Count; i++)
				{
					var f = clip.frames[i]; f.cell = remap[f.cell]; clip.frames[i] = f;
				}
				texture = new Texture2D(bestWidth, bestHeight, TextureFormat.RGBA32, false);
				texture.SetPixels32(pixels); texture.Apply(false, false);
				result = new PetAtlasRepacker.Result { Definition = packed, AtlasPng = texture.EncodeToPNG(), CellsBefore = atlas.CellCount, CellsAfter = used.Count,
					PixelsBefore = (long)atlas.Sheet.width * atlas.Sheet.height, PixelsAfter = area };
				error = null;
				return true;
			}
			catch (Exception ex) { error = "Atlas generation failed: " + ex.Message; return false; }
			finally { if (texture != null) UnityEngine.Object.DestroyImmediate(texture); }
		}

		// Lays every frame on an even grid, near square and within 4096 on each side.
		private static bool TryPackUniform(PetAtlas atlas, PetDefinition definition, List<int> used, out PetAtlasRepacker.Result result, out string error)
		{
			result = null;
			error = null;
			if (used.Count == 0) return false;

			int cellWidth = definition.spriteFrames[used[0]].width;
			int cellHeight = definition.spriteFrames[used[0]].height;
			foreach (int id in used)
			{
				var frame = definition.spriteFrames[id];
				if (frame.width != cellWidth || frame.height != cellHeight) return false;
			}

			int columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(used.Count)));
			columns = Mathf.Min(columns, Mathf.Max(1, 4096 / Mathf.Max(1, cellWidth)));
			int rows = Mathf.CeilToInt(used.Count / (float)columns);

			int width = columns * cellWidth;
			int height = rows * cellHeight;
			if (width > 4096 || height > 4096)
			{
				error = "The selected frames do not fit within a 4096 x 4096 atlas.";
				return false;
			}

			Texture2D texture = null;
			try
			{
				var packed = JsonUtility.FromJson<PetDefinition>(JsonUtility.ToJson(definition)).Normalise();
				packed.spriteFrames.Clear();
				packed.columns = packed.rows = packed.cellWidth = packed.cellHeight = 0;

				var remap = new Dictionary<int, int>();
				var pixels = new Color32[width * height];

				for (int i = 0; i < used.Count; i++)
				{
					int id = used[i];
					int left = i % columns * cellWidth;
					int top = i / columns * cellHeight;

					var frame = definition.spriteFrames[id].Copy();
					frame.x = left;
					frame.y = top;
					remap[id] = packed.spriteFrames.Count;
					packed.spriteFrames.Add(frame);

					// Both buffers are bottom-up; the loop counts top-down.
					for (int y = 0; y < cellHeight; y++)
					for (int x = 0; x < cellWidth; x++)
					{
						pixels[(height - 1 - top - y) * width + left + x] = atlas.GetPixel(id, x, y);
					}
				}

				foreach (var clip in packed.AllSequences())
				for (int i = 0; i < clip.frames.Count; i++)
				{
					var frame = clip.frames[i];
					frame.cell = remap[frame.cell];
					clip.frames[i] = frame;
				}

				texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
				texture.SetPixels32(pixels);
				texture.Apply(false, false);

				result = new PetAtlasRepacker.Result
				{
					Definition = packed,
					AtlasPng = texture.EncodeToPNG(),
					CellsBefore = atlas.CellCount,
					CellsAfter = used.Count,
					PixelsBefore = (long)atlas.Sheet.width * atlas.Sheet.height,
					PixelsAfter = (long)width * height
				};

				return true;
			}
			catch (Exception exception)
			{
				error = "Atlas generation failed: " + exception.Message;
				return false;
			}
			finally
			{
				if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
			}
		}

		private static void SplitFree(List<RectInt> free, RectInt used)
		{
			for (int i = free.Count - 1; i >= 0; i--)
			{
				var r = free[i];
				if (!r.Overlaps(used)) continue;
				free.RemoveAt(i);
				if (used.x > r.x) free.Add(new RectInt(r.x, r.y, used.x - r.x, r.height));
				if (used.xMax < r.xMax) free.Add(new RectInt(used.xMax, r.y, r.xMax - used.xMax, r.height));
				if (used.y > r.y) free.Add(new RectInt(r.x, r.y, r.width, used.y - r.y));
				if (used.yMax < r.yMax) free.Add(new RectInt(r.x, used.yMax, r.width, r.yMax - used.yMax));
			}
			for (int i = free.Count - 1; i >= 0; i--)
				for (int j = 0; j < free.Count; j++)
					if (i != j && free[i].x >= free[j].x && free[i].y >= free[j].y && free[i].xMax <= free[j].xMax && free[i].yMax <= free[j].yMax)
					{ free.RemoveAt(i); break; }
		}
	}
}
