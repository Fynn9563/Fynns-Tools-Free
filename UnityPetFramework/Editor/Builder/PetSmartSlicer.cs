using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal enum PetSliceSensitivity
	{
		Strict,
		Normal,
		Loose
	}

	// Works out where the frames are on a sheet that was never laid out on a grid.
	internal static class PetSmartSlicer
	{
		// The knobs sensitivity turns.
		internal readonly struct Tuning
		{
			private Tuning(float reach, float primary, float merge, float rowGap)
			{
				Reach = reach;
				Primary = primary;
				Merge = merge;
				RowGap = rowGap;
			}

			// How far from a frame a loose piece may sit and still be attached to it.
			public float Reach { get; }
			public float Primary { get; }

			// How close two sprites must be.
			public float Merge { get; }

			// How large a vertical gap is still treated as being inside one row.
			public float RowGap { get; }

			public static Tuning For(PetSliceSensitivity sensitivity)
			{
				switch (sensitivity)
				{
					// Leaves more pieces standing alone.
					case PetSliceSensitivity.Strict:
						return new Tuning(.35f, .10f, .22f, .18f);

					// Absorbs larger and more distant pieces.
					case PetSliceSensitivity.Loose:
						return new Tuning(1f, .30f, .55f, .45f);

					default:
						return new Tuning(.60f, .18f, .38f, .30f);
				}
			}
		}

		public static PetSliceModel Detect(
			Color32[] pixels,
			int width,
			int height,
			PetSliceSensitivity sensitivity = PetSliceSensitivity.Normal,
			bool ignoreMetadata = true,
			byte alpha = 8,
			List<PetSliceIgnoreRegion> ignoreRegions = null)
		{
			PetSliceModel model = Scan(pixels, width, height, alpha);
			if (ignoreRegions != null) model.ignoreRegions.AddRange(ignoreRegions);
			if (!ignoreMetadata)
			{
				foreach (PetSliceComponent component in model.components) component.ignored = false;
			}

			model.ApplyIgnoreRegions();

			Tuning tuning = Tuning.For(sensitivity);
			int typical = TypicalSize(model);

			List<Vector2Int> bands = Bands(model, tuning, typical);
			AssignToBands(model, bands);
			BuildRows(model, bands, tuning, typical);
			model.Rebuild();
			return model;
		}

		// The size of a component that is probably a sprite.
		private static int TypicalSize(PetSliceModel model)
		{
			List<int> sizes = new List<int>();
			foreach (PetSliceComponent component in model.components)
			{
				if (!component.ignored && component.pixels >= 16) sizes.Add(component.pixels);
			}

			if (sizes.Count == 0) return 16;

			sizes.Sort();
			return sizes[(sizes.Count - 1) * 3 / 4];
		}
		public static PetSliceModel Scan(Color32[] pixels, int width, int height, byte alpha = 8)
		{
			if (width <= 0 || height <= 0 || (long)width * height > 32 * 1024 * 1024 || pixels == null || pixels.Length != width * height)
			{
				throw new ArgumentException("Smart Slice supports images up to 32 megapixels.");
			}

			PetSliceModel model = new PetSliceModel { width = width, height = height, labels = new int[pixels.Length] };
			Queue<int> queue = new Queue<int>();

			for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++)
			{
				int start = y * width + x;

				// The pixel buffer is bottom-up.
				if (model.labels[start] != 0 || pixels[(height - 1 - y) * width + x].a <= alpha) continue;

				if (model.components.Count >= 65536)
				{
					throw new ArgumentException("Too many separate shapes on this sheet. Raise the alpha threshold, or clean up the noise around the artwork.");
				}

				int id = model.components.Count + 1;
				int left = x, top = y, right = x, bottom = y, count = 0, grey = 0;
				model.labels[start] = id;
				queue.Enqueue(start);

				while (queue.Count > 0)
				{
					int p = queue.Dequeue();
					int px = p % width, py = p / width;
					count++;

					Color32 colour = pixels[(height - 1 - py) * width + px];
					if (Mathf.Max(colour.r, Mathf.Max(colour.g, colour.b)) - Mathf.Min(colour.r, Mathf.Min(colour.g, colour.b)) < 18) grey++;

					left = Mathf.Min(left, px); right = Mathf.Max(right, px);
					top = Mathf.Min(top, py); bottom = Mathf.Max(bottom, py);

					for (int yy = Mathf.Max(0, py - 1); yy <= Mathf.Min(height - 1, py + 1); yy++)
					for (int xx = Mathf.Max(0, px - 1); xx <= Mathf.Min(width - 1, px + 1); xx++)
					{
						int q = yy * width + xx;
						if (model.labels[q] != 0 || pixels[(height - 1 - yy) * width + xx].a <= alpha) continue;

						model.labels[q] = id;
						queue.Enqueue(q);
					}
				}

				int w = right - left + 1, h = bottom - top + 1;

				// A wide, flat, almost colourless solid block is a credits bar.
				bool panel = w > h * 2.5f && count > 256 && grey > count * .94f && count > w * h * .82f;

				model.components.Add(new PetSliceComponent
				{
					id = id, x = left, y = top, width = w, height = h, pixels = count, ignored = panel
				});
			}

			FlagTextRuns(model);
			return model;
		}

		// A run of small, similarly sized, bottom-aligned islands is a line of text.
		private static void FlagTextRuns(PetSliceModel model)
		{
			foreach (PetSliceComponent component in model.components)
			{
				if (component.width > 14 || component.height > 14) continue;

				int neighbours = 0;
				foreach (PetSliceComponent other in model.components)
				{
					if (other.width > 14 || other.height > 14) continue;
					if (Mathf.Abs(other.y + other.height - component.y - component.height) > 2) continue;
					if (Mathf.Abs(other.Centre.x - component.Centre.x) > 45) continue;

					neighbours++;
				}

				if (neighbours >= 6) component.ignored = true;
			}
		}
		// Step 2: the horizontal bands the sheet's animations are laid out in.
		private static List<Vector2Int> Bands(PetSliceModel model, Tuning tuning, int typical)
		{
			int floor = Mathf.Max(24, Mathf.RoundToInt(typical * .15f));
			int[] coverage = new int[model.height];

			foreach (PetSliceComponent component in model.components)
			{
				if (component.ignored || component.pixels < floor) continue;

				for (int y = component.y; y < component.y + component.height && y < model.height; y++) coverage[y]++;
			}

			List<Vector2Int> runs = new List<Vector2Int>();
			for (int y = 0; y < model.height; y++)
			{
				if (coverage[y] == 0) continue;

				int start = y;
				while (y < model.height && coverage[y] > 0) y++;
				runs.Add(new Vector2Int(start, y));
			}

			if (runs.Count == 0) return runs;

			List<int> heights = new List<int>();
			foreach (Vector2Int run in runs) heights.Add(run.y - run.x);
			heights.Sort();
			int median = heights[heights.Count / 2];

			// Measured before the gaps are closed.
			float pitch = RowPitch(runs, median);

			List<Vector2Int> merged = new List<Vector2Int> { runs[0] };
			for (int i = 1; i < runs.Count; i++)
			{
				Vector2Int last = merged[merged.Count - 1];
				if (runs[i].x - last.y < median * tuning.RowGap) merged[merged.Count - 1] = new Vector2Int(last.x, runs[i].y);
				else merged.Add(runs[i]);
			}

			List<Vector2Int> bands = new List<Vector2Int>();
			foreach (Vector2Int run in merged) Split(run, coverage, pitch, bands);
			return bands;
		}

		// The distance from one row of the sheet to the next.
		private static float RowPitch(List<Vector2Int> runs, int median)
		{
			if (runs.Count < 3) return median;

			List<int> gaps = new List<int>();
			for (int i = 1; i < runs.Count; i++) gaps.Add(runs[i].x - runs[i - 1].x);

			gaps.Sort();
			return Mathf.Max(gaps[gaps.Count / 2], median);
		}

		// Cuts an over-tall band at the emptiest pixel rows near where the boundaries ought to be.
		private static void Split(Vector2Int run, int[] coverage, float pitch, List<Vector2Int> bands)
		{
			int height = run.y - run.x;
			if (height <= pitch * 1.45f || pitch < 1f)
			{
				bands.Add(run);
				return;
			}

			int parts = Mathf.Max(2, Mathf.RoundToInt(height / pitch));
			int window = Mathf.Max(1, Mathf.RoundToInt(pitch * .3f));
			int previous = run.x;

			for (int i = 1; i < parts; i++)
			{
				float ideal = run.x + height * i / (float)parts;
				int low = Mathf.Max(previous + 1, Mathf.RoundToInt(ideal) - window);
				int high = Mathf.Min(run.y - 1, Mathf.RoundToInt(ideal) + window);
				if (high <= low) continue;

				int best = low;
				for (int y = low; y < high; y++)
				{
					if (coverage[y] < coverage[best] ||
						(coverage[y] == coverage[best] && Mathf.Abs(y - ideal) < Mathf.Abs(best - ideal)))
					{
						best = y;
					}
				}

				if (best <= previous) continue;

				bands.Add(new Vector2Int(previous, best));
				previous = best;
			}

			bands.Add(new Vector2Int(previous, run.y));
		}

		// Everything goes to the band holding its vertical centre.
		private static void AssignToBands(PetSliceModel model, List<Vector2Int> bands)
		{
			foreach (PetSliceComponent component in model.components)
			{
				if (component.ignored) continue;

				float centre = component.Centre.y;
				component.row = -1;

				for (int i = 0; i < bands.Count; i++)
				{
					if (centre < bands[i].x || centre >= bands[i].y) continue;

					component.row = i;
					break;
				}

				if (component.row < 0) component.ignored = true;
			}
		}
		// Steps 3 to 5.
		private static void BuildRows(PetSliceModel model, List<Vector2Int> bands, Tuning tuning, int typical)
		{
			model.rows.Clear();

			for (int b = 0; b < bands.Count; b++)
			{
				List<PetSliceComponent> members = new List<PetSliceComponent>();
				foreach (PetSliceComponent component in model.components)
				{
					if (component.row == b && !component.ignored) members.Add(component);
				}

				if (members.Count == 0) continue;

				// Step 3.
				int largest = 0;
				foreach (PetSliceComponent component in members) largest = Mathf.Max(largest, component.pixels);

				List<PetSliceComponent> primaries = new List<PetSliceComponent>();
				foreach (PetSliceComponent component in members)
				{
					component.primary = component.pixels >= largest * tuning.Primary &&
						component.pixels >= typical * .05f;

					if (component.primary) primaries.Add(component);
				}

				if (primaries.Count == 0)
				{
					// A band with nothing sprite-sized in it is not an animation row.
					foreach (PetSliceComponent component in members) component.ignored = true;
					continue;
				}

				primaries.Sort((a, c) => a.Centre.x.CompareTo(c.Centre.x));

				float spacing = FrameSpacing(primaries);
				PetSliceRow row = new PetSliceRow { name = "Row " + (model.rows.Count + 1), top = bands[b].x, bottom = bands[b].y };
				MergePrimaries(primaries, spacing, tuning.Merge, row);
				AttachEffects(members, row, model, spacing, tuning.Reach);
				model.rows.Add(row);
			}
		}

		// Step 4: how far apart consecutive frames sit in this row.
		private static float FrameSpacing(List<PetSliceComponent> primaries)
		{
			int largest = 0;
			int widest = 1;
			foreach (PetSliceComponent component in primaries)
			{
				largest = Mathf.Max(largest, component.pixels);
				widest = Mathf.Max(widest, component.width);
			}

			List<PetSliceComponent> bodies = new List<PetSliceComponent>();
			int widestBody = 1;
			foreach (PetSliceComponent component in primaries)
			{
				if (component.pixels < largest * .5f) continue;

				bodies.Add(component);
				widestBody = Mathf.Max(widestBody, component.width);
			}

			// Too few to measure a pitch from, so fall back to sprite width.
			if (bodies.Count < 3) return Mathf.Max(widestBody * 1.6f, widest * 1.2f);

			List<float> gaps = new List<float>();
			for (int i = 1; i < bodies.Count; i++) gaps.Add(bodies[i].Centre.x - bodies[i - 1].Centre.x);

			gaps.Sort();
			return Mathf.Max(gaps[gaps.Count / 2], widestBody * .9f);
		}

		// Consecutive sprites that sit far closer together than one frame pitch.
		private static void MergePrimaries(List<PetSliceComponent> primaries, float spacing, float merge, PetSliceRow row)
		{
			PetSliceFrame frame = new PetSliceFrame();
			frame.components.Add(primaries[0].id);
			row.frames.Add(frame);

			for (int i = 1; i < primaries.Count; i++)
			{
				PetSliceComponent previous = primaries[i - 1];
				PetSliceComponent current = primaries[i];

				float overlap = Mathf.Min(current.x + current.width, previous.x + previous.width) - Mathf.Max(current.x, previous.x);
				bool near = current.Centre.x - previous.Centre.x < spacing * merge;
				bool nested = overlap > Mathf.Min(current.width, previous.width) * .5f;

				if (!near && !nested)
				{
					frame = new PetSliceFrame();
					row.frames.Add(frame);
				}

				frame.components.Add(current.id);
			}
		}
		private static void AttachEffects(
			List<PetSliceComponent> members, PetSliceRow row, PetSliceModel model, float spacing, float reach)
		{
			List<RectInt> boxes = new List<RectInt>();
			foreach (PetSliceFrame frame in row.frames) boxes.Add(model.FrameBounds(frame));

			float limit = spacing * reach;

			foreach (PetSliceComponent component in members)
			{
				if (component.primary) continue;

				int right = component.x + component.width;
				int bottom = component.y + component.height;
				int best = -1;
				float bestScore = float.MaxValue;

				for (int i = 0; i < boxes.Count; i++)
				{
					RectInt box = boxes[i];
					float dx = Mathf.Max(0, Mathf.Max(box.xMin - right, component.x - box.xMax));
					if (dx > limit) continue;

					float dy = Mathf.Max(0, Mathf.Max(box.yMin - bottom, component.y - box.yMax));
					float width = Mathf.Max(box.xMax, right) - Mathf.Min(box.xMin, component.x);
					float overflow = Mathf.Max(0f, width - spacing);

					float score = dx * 2f + dy * .35f +
						Mathf.Abs(component.Centre.x - box.center.x) * .5f +
						overflow * 1.5f;

					if (score >= bestScore) continue;

					bestScore = score;
					best = i;
				}

				if (best < 0)
				{
					component.ignored = true;
					continue;
				}

				row.frames[best].components.Add(component.id);

				RectInt grown = boxes[best];
				int left = Mathf.Min(grown.xMin, component.x);
				int top = Mathf.Min(grown.yMin, component.y);
				boxes[best] = new RectInt(left, top, Mathf.Max(grown.xMax, right) - left, Mathf.Max(grown.yMax, bottom) - top);
			}
		}
	}
}
