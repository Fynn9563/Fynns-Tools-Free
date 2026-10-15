using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	[Serializable]
	internal sealed class PetSliceComponent
	{
		public int id;

		public int x, y, width, height;
		public int pixels;

		public bool ignored;

		public bool primary;

		public int row = -1;
		public int frame = -1;

		public RectInt Bounds => new RectInt(x, y, width, height);

		public Vector2 Centre => new Vector2(x + width * .5f, y + height * .5f);
	}

	[Serializable]
	internal sealed class PetSliceFrame
	{
		public List<int> components = new List<int>();
	}

	[Serializable]
	internal sealed class PetSliceRow
	{
		public string name;

		// Source pixel bounds are top-down.
		public int top;
		public int bottom;

		public List<PetSliceFrame> frames = new List<PetSliceFrame>();
	}

	[Serializable]
	internal sealed class PetSliceIgnoreRegion
	{
		public int x, y, width, height;

		public RectInt Rect => new RectInt(x, y, width, height);
	}
	internal sealed class PetSliceModel
	{
		public int width;
		public int height;

		// Top-down component IDs; zero means transparent.
		public int[] labels;

		public List<PetSliceComponent> components = new List<PetSliceComponent>();

		public List<PetSliceRow> rows = new List<PetSliceRow>();

		public List<PetSliceIgnoreRegion> ignoreRegions = new List<PetSliceIgnoreRegion>();

		public int IgnoredCount
		{
			get
			{
				int count = 0;
				foreach (PetSliceComponent component in components)
				{
					if (component.ignored) count++;
				}

				return count;
			}
		}

		public int FrameCount
		{
			get
			{
				int count = 0;
				foreach (PetSliceRow row in rows) count += row.frames.Count;
				return count;
			}
		}

		public int MaxFramesPerRow
		{
			get
			{
				int most = 0;
				foreach (PetSliceRow row in rows) most = Mathf.Max(most, row.frames.Count);
				return most;
			}
		}

		public PetSliceComponent Component(int id)
		{
			return id >= 1 && id <= components.Count ? components[id - 1] : null;
		}

		public RectInt FrameBounds(PetSliceFrame frame)
		{
			bool any = false;
			int left = 0, top = 0, right = 0, bottom = 0;

			foreach (int id in frame.components)
			{
				PetSliceComponent component = Component(id);
				if (component == null || component.ignored) continue;

				if (!any)
				{
					left = component.x;
					top = component.y;
					right = component.x + component.width;
					bottom = component.y + component.height;
					any = true;
					continue;
				}

				left = Mathf.Min(left, component.x);
				top = Mathf.Min(top, component.y);
				right = Mathf.Max(right, component.x + component.width);
				bottom = Mathf.Max(bottom, component.y + component.height);
			}

			return any ? new RectInt(left, top, right - left, bottom - top) : new RectInt(0, 0, 0, 0);
		}

		public Vector2Int CellSize()
		{
			int cellWidth = 1;
			int cellHeight = 1;

			foreach (PetSliceRow row in rows)
			{
				foreach (PetSliceFrame frame in row.frames)
				{
					RectInt bounds = FrameBounds(frame);
					cellWidth = Mathf.Max(cellWidth, bounds.width);
					cellHeight = Mathf.Max(cellHeight, bounds.height);
				}
			}

			return new Vector2Int(cellWidth, cellHeight);
		}
		public void Rebuild()
		{
			foreach (PetSliceComponent component in components)
			{
				component.row = -1;
				component.frame = -1;
			}

			foreach (PetSliceRow row in rows)
			{
				for (int f = row.frames.Count - 1; f >= 0; f--)
				{
					PetSliceFrame frame = row.frames[f];
					frame.components.RemoveAll(id => Component(id) == null);

					bool alive = false;
					foreach (int id in frame.components)
					{
						if (!Component(id).ignored) { alive = true; break; }
					}

					if (!alive) row.frames.RemoveAt(f);
				}
			}
			rows.RemoveAll(row => row.frames.Count == 0);

			for (int r = 0; r < rows.Count; r++)
			{
				PetSliceRow row = rows[r];
				if (string.IsNullOrEmpty(row.name)) row.name = "Row " + (r + 1);

				for (int f = 0; f < row.frames.Count; f++)
				{
					foreach (int id in row.frames[f].components)
					{
						PetSliceComponent component = Component(id);
						component.row = r;
						component.frame = f;
					}
				}
			}
		}

		public bool InIgnoredRegion(RectInt bounds)
		{
			Vector2Int centre = new Vector2Int(
				Mathf.RoundToInt(bounds.x + bounds.width * .5f),
				Mathf.RoundToInt(bounds.y + bounds.height * .5f));

			foreach (PetSliceIgnoreRegion region in ignoreRegions)
			{
				if (region.Rect.Contains(centre)) return true;
			}

			return false;
		}

		public void ApplyIgnoreRegions()
		{
			foreach (PetSliceComponent component in components)
			{
				if (InIgnoredRegion(component.Bounds)) component.ignored = true;
			}
		}
	}

	// Snapshot editable state for the bounded Undo history.
	[Serializable]
	internal sealed class PetSliceSnapshot
	{
		public List<PetSliceRow> rows = new List<PetSliceRow>();
		public List<PetSliceIgnoreRegion> ignoreRegions = new List<PetSliceIgnoreRegion>();

		public List<int> flags = new List<int>();

		public static PetSliceSnapshot Capture(PetSliceModel model)
		{
			PetSliceSnapshot snapshot = new PetSliceSnapshot();
			snapshot.rows = CopyRows(model.rows);
			snapshot.ignoreRegions = CopyRegions(model.ignoreRegions);

			foreach (PetSliceComponent component in model.components)
			{
				snapshot.flags.Add((component.ignored ? 1 : 0) | (component.primary ? 2 : 0));
			}

			return snapshot;
		}

		public void Restore(PetSliceModel model)
		{
			model.rows = CopyRows(rows);
			model.ignoreRegions = CopyRegions(ignoreRegions);

			for (int i = 0; i < model.components.Count && i < flags.Count; i++)
			{
				model.components[i].ignored = (flags[i] & 1) != 0;
				model.components[i].primary = (flags[i] & 2) != 0;
			}

			model.Rebuild();
		}

		private static List<PetSliceRow> CopyRows(List<PetSliceRow> source)
		{
			List<PetSliceRow> copy = new List<PetSliceRow>(source.Count);
			foreach (PetSliceRow row in source)
			{
				PetSliceRow clone = new PetSliceRow { name = row.name, top = row.top, bottom = row.bottom };
				foreach (PetSliceFrame frame in row.frames)
				{
					clone.frames.Add(new PetSliceFrame { components = new List<int>(frame.components) });
				}

				copy.Add(clone);
			}

			return copy;
		}

		private static List<PetSliceIgnoreRegion> CopyRegions(List<PetSliceIgnoreRegion> source)
		{
			List<PetSliceIgnoreRegion> copy = new List<PetSliceIgnoreRegion>(source.Count);
			foreach (PetSliceIgnoreRegion region in source)
			{
				copy.Add(new PetSliceIgnoreRegion { x = region.x, y = region.y, width = region.width, height = region.height });
			}

			return copy;
		}
	}
}
