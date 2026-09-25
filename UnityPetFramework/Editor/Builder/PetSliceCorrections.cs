using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Every edit the Smart Slice review can make, as operations on the model.
	internal static class PetSliceCorrections
	{
		public static PetSliceFrame FrameAt(PetSliceModel model, int row, int frame)
		{
			if (row < 0 || row >= model.rows.Count) return null;
			if (frame < 0 || frame >= model.rows[row].frames.Count) return null;

			return model.rows[row].frames[frame];
		}

		// Folds several frames of one row into the earliest of them.
		public static void MergeFrames(PetSliceModel model, int row, List<int> frames)
		{
			if (row < 0 || row >= model.rows.Count || frames.Count < 2) return;

			frames.Sort();
			PetSliceRow target = model.rows[row];
			PetSliceFrame keep = target.frames[frames[0]];

			for (int i = frames.Count - 1; i >= 1; i--)
			{
				int index = frames[i];
				if (index < 0 || index >= target.frames.Count) continue;

				keep.components.AddRange(target.frames[index].components);
				target.frames.RemoveAt(index);
			}
		}

		// One frame back into its separate pieces, in reading order.
		public static void SplitFrame(PetSliceModel model, int row, int frame)
		{
			PetSliceFrame source = FrameAt(model, row, frame);
			if (source == null || source.components.Count < 2) return;

			List<int> ids = new List<int>(source.components);
			ids.Sort((a, b) => model.Component(a).x.CompareTo(model.Component(b).x));

			model.rows[row].frames.RemoveAt(frame);

			for (int i = 0; i < ids.Count; i++)
			{
				PetSliceFrame piece = new PetSliceFrame();
				piece.components.Add(ids[i]);
				model.rows[row].frames.Insert(frame + i, piece);
			}
		}

		// Removes a frame and ignores what was in it.
		public static void DeleteFrame(PetSliceModel model, int row, int frame)
		{
			PetSliceFrame target = FrameAt(model, row, frame);
			if (target == null) return;

			foreach (int id in target.components)
			{
				PetSliceComponent component = model.Component(id);
				if (component != null) component.ignored = true;
			}

			model.rows[row].frames.RemoveAt(frame);
		}

		// Takes a component out of whatever frame holds it, without deciding what happens to it next.
		public static void Detach(PetSliceModel model, int id)
		{
			foreach (PetSliceRow row in model.rows)
			{
				foreach (PetSliceFrame frame in row.frames) frame.components.Remove(id);
			}
		}

		public static void Assign(PetSliceModel model, int id, int row, int frame)
		{
			PetSliceFrame target = FrameAt(model, row, frame);
			if (target == null) return;

			Detach(model, id);

			PetSliceComponent component = model.Component(id);
			if (component != null) component.ignored = false;

			target.components.Add(id);
		}

		// A new frame from the given components, placed in reading order within the row.
		public static void AddFrame(PetSliceModel model, int row, IEnumerable<int> components)
		{
			if (row < 0 || row >= model.rows.Count) return;

			PetSliceFrame frame = new PetSliceFrame();
			int left = int.MaxValue;

			foreach (int id in components)
			{
				PetSliceComponent component = model.Component(id);
				if (component == null) continue;

				Detach(model, id);
				component.ignored = false;
				frame.components.Add(id);
				left = Mathf.Min(left, component.x);
			}

			if (frame.components.Count == 0) return;

			int at = model.rows[row].frames.Count;
			for (int i = 0; i < model.rows[row].frames.Count; i++)
			{
				if (model.FrameBounds(model.rows[row].frames[i]).x <= left) continue;

				at = i;
				break;
			}

			model.rows[row].frames.Insert(at, frame);
		}
		// Moves a component one frame along its row.
		public static void Nudge(PetSliceModel model, int id, int direction)
		{
			PetSliceComponent component = model.Component(id);
			if (component == null || component.row < 0) return;

			int target = component.frame + direction;
			if (target < 0 || target >= model.rows[component.row].frames.Count) return;

			Assign(model, id, component.row, target);
		}

		public static void Ignore(PetSliceModel model, IEnumerable<int> components)
		{
			foreach (int id in components)
			{
				PetSliceComponent component = model.Component(id);
				if (component == null) continue;

				component.ignored = true;
				Detach(model, id);
			}
		}

		public static void MoveFrame(PetSliceModel model, int row, int frame, int toRow)
		{
			PetSliceFrame moving = FrameAt(model, row, frame);
			if (moving == null || toRow < 0 || toRow >= model.rows.Count || toRow == row) return;

			model.rows[row].frames.RemoveAt(frame);
			model.rows[toRow].frames.Add(moving);
			SortReadingOrder(model, model.rows[toRow]);
		}

		public static void ReorderFrame(PetSliceModel model, int row, int frame, int direction)
		{
			PetSliceFrame moving = FrameAt(model, row, frame);
			int target = frame + direction;
			if (moving == null || target < 0 || target >= model.rows[row].frames.Count) return;

			model.rows[row].frames.RemoveAt(frame);
			model.rows[row].frames.Insert(target, moving);
		}

		public static void MergeRows(PetSliceModel model, int row)
		{
			if (row < 0 || row + 1 >= model.rows.Count) return;

			PetSliceRow first = model.rows[row];
			PetSliceRow second = model.rows[row + 1];

			first.frames.AddRange(second.frames);
			first.top = Mathf.Min(first.top, second.top);
			first.bottom = Mathf.Max(first.bottom, second.bottom);
			model.rows.RemoveAt(row + 1);

			SortReadingOrder(model, first);
		}

		public static void DeleteRow(PetSliceModel model, int row)
		{
			if (row < 0 || row >= model.rows.Count) return;

			foreach (PetSliceFrame frame in model.rows[row].frames)
			{
				foreach (int id in frame.components)
				{
					PetSliceComponent component = model.Component(id);
					if (component != null) component.ignored = true;
				}
			}

			model.rows.RemoveAt(row);
		}

		// A row built from components detection left out, for a band it missed entirely.
		public static void CreateRow(PetSliceModel model, IEnumerable<int> components)
		{
			List<int> ids = new List<int>(components);
			if (ids.Count == 0) return;

			int top = int.MaxValue, bottom = int.MinValue;
			foreach (int id in ids)
			{
				PetSliceComponent component = model.Component(id);
				if (component == null) continue;

				top = Mathf.Min(top, component.y);
				bottom = Mathf.Max(bottom, component.y + component.height);
			}

			if (top > bottom) return;

			int at = model.rows.Count;
			for (int i = 0; i < model.rows.Count; i++)
			{
				if (model.rows[i].top <= top) continue;

				at = i;
				break;
			}

			model.rows.Insert(at, new PetSliceRow { name = "Row " + (at + 1), top = top, bottom = bottom });
			AddFrame(model, at, ids);
		}

		// Left to right.
		private static void SortReadingOrder(PetSliceModel model, PetSliceRow row)
		{
			row.frames.Sort((a, b) => model.FrameBounds(a).x.CompareTo(model.FrameBounds(b).x));
		}
	}
}
