using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The source sheet with what detection made of it drawn on top.
	internal sealed class PetSliceCanvas
	{
		internal enum Tool
		{
			Select,
			IgnoreRegion
		}

		private static readonly Color BandColour = new Color(.35f, .7f, 1f, .07f);
		private static readonly Color BandEdge = new Color(.35f, .7f, 1f, .45f);
		private static readonly Color FrameColour = new Color(.4f, 1f, .5f, .9f);
		private static readonly Color PrimaryColour = new Color(1f, 1f, 1f, .35f);
		private static readonly Color EffectColour = new Color(1f, .75f, .2f, .8f);
		private static readonly Color IgnoredColour = new Color(1f, .3f, .3f, .55f);
		private static readonly Color RegionColour = new Color(1f, .2f, .2f, .12f);

		// Labels only once there is room to read them.
		private const float LabelZoom = .35f;

		public readonly HashSet<int> Selection = new HashSet<int>();

		public Tool ActiveTool;

		private float _zoom;
		private Vector2 _pan;
		private Vector2 _dragStart;
		private bool _dragging;

		public void Reset()
		{
			Selection.Clear();
			_zoom = 0f;
			_pan = Vector2.zero;
			_dragging = false;
		}

		public void Fit()
		{
			_zoom = 0f;
		}

		public string ZoomLabel()
		{
			return Mathf.RoundToInt(_zoom * 100f) + "%";
		}

		// Returns the region the user just finished drawing, or null.
		public PetSliceIgnoreRegion Draw(Rect area, PetAtlas atlas, PetSliceModel model)
		{
			if (_zoom <= 0f)
			{
				_zoom = Mathf.Min(area.width / atlas.Sheet.width, area.height / atlas.Sheet.height);
				_pan = new Vector2(
					(area.width - atlas.Sheet.width * _zoom) * .5f,
					(area.height - atlas.Sheet.height * _zoom) * .5f);
			}

			EditorGUI.DrawRect(area, new Color(.1f, .1f, .1f));

			Rect sheet = new Rect(area.position + _pan, new Vector2(atlas.Sheet.width, atlas.Sheet.height) * _zoom);

			GUI.BeginClip(area);
			Rect local = new Rect(sheet.position - area.position, sheet.size);
			GUI.DrawTexture(local, atlas.Sheet, ScaleMode.StretchToFill, true);

			DrawBands(local, model);
			DrawComponents(local, model);
			DrawFrames(local, model);
			DrawRegions(local, model);

			if (_dragging)
			{
				Vector2 mouse = (Event.current.mousePosition - local.position) / _zoom;
				Outline(Screen(Between(_dragStart, mouse), local), Color.white);
			}

			GUI.EndClip();

			return HandleInput(area, sheet, model);
		}
		private void DrawBands(Rect sheet, PetSliceModel model)
		{
			foreach (PetSliceRow row in model.rows)
			{
				Rect band = new Rect(sheet.x, sheet.y + row.top * _zoom, sheet.width, Mathf.Max(1f, (row.bottom - row.top) * _zoom));

				EditorGUI.DrawRect(band, BandColour);
				EditorGUI.DrawRect(new Rect(band.x, band.y, band.width, 1f), BandEdge);
				EditorGUI.DrawRect(new Rect(band.x, band.yMax - 1f, band.width, 1f), BandEdge);

				if (_zoom >= LabelZoom) GUI.Label(new Rect(band.x + 2f, band.y, 120f, 14f), row.name, EditorStyles.whiteMiniLabel);
			}
		}

		private void DrawComponents(Rect sheet, PetSliceModel model)
		{
			foreach (PetSliceComponent component in model.components)
			{
				Rect box = Screen(component.Bounds, sheet);

				if (Selection.Contains(component.id))
				{
					Outline(box, Color.yellow);
					continue;
				}

				if (component.ignored)
				{
					Outline(box, IgnoredColour);
					continue;
				}

				// Primaries only set frame positions, so they are marked faintly.
				Outline(box, component.primary ? PrimaryColour : EffectColour);
			}
		}

		private void DrawFrames(Rect sheet, PetSliceModel model)
		{
			foreach (PetSliceRow row in model.rows)
			{
				for (int f = 0; f < row.frames.Count; f++)
				{
					RectInt bounds = model.FrameBounds(row.frames[f]);
					if (bounds.width <= 0) continue;

					Rect box = Screen(bounds, sheet);
					Outline(box, FrameColour);

					if (_zoom >= LabelZoom)
					{
						GUI.Label(new Rect(box.x, box.y - 13f, 60f, 13f), (f + 1).ToString(), EditorStyles.whiteMiniLabel);
					}
				}
			}
		}

		private void DrawRegions(Rect sheet, PetSliceModel model)
		{
			foreach (PetSliceIgnoreRegion region in model.ignoreRegions)
			{
				Rect box = Screen(region.Rect, sheet);
				EditorGUI.DrawRect(box, RegionColour);
				Outline(box, IgnoredColour);
			}
		}

		private PetSliceIgnoreRegion HandleInput(Rect area, Rect sheet, PetSliceModel model)
		{
			Event e = Event.current;
			Vector2 point = (e.mousePosition - sheet.position) / _zoom;
			bool inside = area.Contains(e.mousePosition);

			if (e.type == EventType.ScrollWheel && inside)
			{
				// Zoomed around the cursor, so whatever is being looked at stays put.
				float next = Mathf.Clamp(_zoom * Mathf.Pow(1.12f, -e.delta.y), .02f, 16f);
				_pan = e.mousePosition - area.position - point * next;
				_zoom = next;
				e.Use();
			}

			if (e.type == EventType.MouseDrag && inside && (e.button == 2 || e.alt))
			{
				_pan += e.delta;
				e.Use();
			}

			if (e.type == EventType.MouseDown && inside && e.button == 0 && !e.alt)
			{
				if (ActiveTool == Tool.IgnoreRegion)
				{
					_dragStart = point;
					_dragging = true;
				}
				else
				{
					int hit = ComponentAt(model, point);
					if (!e.shift) Selection.Clear();
					if (hit > 0 && !Selection.Add(hit)) Selection.Remove(hit);
				}

				e.Use();
			}

			if (e.type == EventType.MouseUp && e.button == 0 && _dragging)
			{
				_dragging = false;
				e.Use();

				RectInt drawn = Clamp(Between(_dragStart, point), model);
				if (drawn.width > 2 && drawn.height > 2)
				{
					return new PetSliceIgnoreRegion { x = drawn.x, y = drawn.y, width = drawn.width, height = drawn.height };
				}
			}

			return null;
		}

		// Smallest component under the cursor, so a heart drawn over a sprite can still be picked.
		private static int ComponentAt(PetSliceModel model, Vector2 point)
		{
			Vector2Int pixel = Vector2Int.FloorToInt(point);
			int best = 0;
			long smallest = long.MaxValue;

			foreach (PetSliceComponent component in model.components)
			{
				if (!component.Bounds.Contains(pixel)) continue;

				long size = (long)component.width * component.height;
				if (size >= smallest) continue;

				smallest = size;
				best = component.id;
			}

			return best;
		}

		private Rect Screen(RectInt rect, Rect sheet)
		{
			return new Rect(sheet.x + rect.x * _zoom, sheet.y + rect.y * _zoom, rect.width * _zoom, rect.height * _zoom);
		}

		private static RectInt Between(Vector2 a, Vector2 b)
		{
			return new RectInt(
				Mathf.FloorToInt(Mathf.Min(a.x, b.x)),
				Mathf.FloorToInt(Mathf.Min(a.y, b.y)),
				Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(a.x - b.x))),
				Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(a.y - b.y))));
		}

		private static RectInt Clamp(RectInt rect, PetSliceModel model)
		{
			int x = Mathf.Clamp(rect.x, 0, Mathf.Max(0, model.width - 1));
			int y = Mathf.Clamp(rect.y, 0, Mathf.Max(0, model.height - 1));
			return new RectInt(x, y, Mathf.Clamp(rect.xMax - x, 1, model.width - x), Mathf.Clamp(rect.yMax - y, 1, model.height - y));
		}

		private static void Outline(Rect rect, Color colour)
		{
			EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), colour);
			EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), colour);
			EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), colour);
			EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), colour);
		}
	}
}
