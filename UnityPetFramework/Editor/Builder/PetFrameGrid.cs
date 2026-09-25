using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The sprite sheet drawn as a clickable grid, with pan and zoom.
	internal sealed class PetFrameGrid
	{
		private const float MinZoom = 0.05f;
		private const float MaxZoom = 8f;
		private const float ZoomPerNotch = 0.06f;

		private static readonly Color GridLineColor = new Color(1f, 1f, 1f, 0.08f);
		private static readonly Color EmptyCellTint = new Color(0f, 0f, 0f, 0.55f);
		private static readonly Color SelectedCellTint = new Color(0.3f, 0.7f, 1f, 0.25f);
		private static readonly Color SelectedCellOutline = new Color(0.3f, 0.7f, 1f, 0.9f);
		private static readonly Color HoverOutline = new Color(1f, 1f, 1f, 0.5f);
		private static readonly Color BackgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);

		private Vector2 _pan;
		private float _zoom;
		private bool _framed;
		private int _hoveredCell = -1;

		private GUIStyle _badgeStyle;

		// Raised with the atlas cell index that was clicked.
		public event Action<int> CellClicked;

		public event Action<int> CellRemoveRequested;

		// Drops the framing so the next Draw fits the sheet again.
		public void Reset()
		{
			_framed = false;
			_pan = Vector2.zero;
			_hoveredCell = -1;
		}

		public void Draw(Rect area, PetAtlas atlas, IList<PetFrameRef> selectedFrames)
		{
			if (atlas == null || atlas.Sheet == null)
			{
				return;
			}

			EditorGUI.DrawRect(area, BackgroundColor);

			if (!_framed)
			{
				FrameToFit(area, atlas);
			}

			Rect content = ContentRect(area, atlas);
			GUI.BeginClip(area);
			Rect localContent = new Rect(content.x - area.x, content.y - area.y, content.width, content.height);
			DrawSheet(localContent, atlas, selectedFrames);
			GUI.EndClip();

			HandleInput(area, content, atlas);
		}

		// Draws a one-line summary of the view for a status strip.
		public string ZoomLabel()
		{
			return Mathf.RoundToInt(_zoom * 100f) + "%";
		}

		private void DrawSheet(Rect content, PetAtlas atlas, IList<PetFrameRef> selectedFrames)
		{
			// One draw call for the whole sheet.
			GUI.DrawTexture(content, atlas.Sheet, ScaleMode.StretchToFill, true);

			float scaleX = content.width / atlas.Sheet.width;
			float scaleY = content.height / atlas.Sheet.height;

			// Where each selected cell sits in the clip, so a cell used twice can show both positions.
			Dictionary<int, List<string>> orderByCell = BuildOrderLookup(selectedFrames);

			for (int index = 0; index < atlas.CellCount; index++)
			{
				Rect cell = CellRect(content, atlas, index, scaleX, scaleY);

				if (atlas.IsCellEmpty(index))
				{
					// Dimmed rather than hidden.
					EditorGUI.DrawRect(cell, EmptyCellTint);
				}

				if (orderByCell.TryGetValue(index, out List<string> positions))
				{
					EditorGUI.DrawRect(cell, SelectedCellTint);
					DrawOutline(cell, SelectedCellOutline);
					DrawBadge(cell, positions);
				}

				if (index == _hoveredCell)
				{
					DrawOutline(cell, HoverOutline);
				}
			}

			DrawGridLines(content, atlas, scaleX, scaleY);
		}

		private static Dictionary<int, List<string>> BuildOrderLookup(IList<PetFrameRef> selectedFrames)
		{
			Dictionary<int, List<string>> lookup = new Dictionary<int, List<string>>();
			if (selectedFrames == null)
			{
				return lookup;
			}

			for (int position = 0; position < selectedFrames.Count; position++)
			{
				PetFrameRef frame = selectedFrames[position];
				if (!lookup.TryGetValue(frame.cell, out List<string> positions))
				{
					positions = new List<string>();
					lookup[frame.cell] = positions;
				}

				// The arrow marks a mirrored frame.
				positions.Add(frame.flip ? position + "<" : position.ToString());
			}

			return lookup;
		}

		private void DrawBadge(Rect cell, List<string> positions)
		{
			if (cell.width < 18f || cell.height < 14f)
			{
				// Below this the label is unreadable and only adds clutter.
				return;
			}

			if (_badgeStyle == null)
			{
				_badgeStyle = new GUIStyle(EditorStyles.miniLabel)
				{
					alignment = TextAnchor.UpperLeft,
					fontStyle = FontStyle.Bold,
					normal = { textColor = Color.white }
				};
			}

			StringBuilder text = new StringBuilder();
			for (int i = 0; i < positions.Count; i++)
			{
				if (i > 0)
				{
					text.Append(',');
				}

				// Shown as the position in the animation, which is what the user picked.
				text.Append(positions[i]);

				if (i == 2 && positions.Count > 3)
				{
					text.Append("...");
					break;
				}
			}

			Rect label = new Rect(cell.x + 2f, cell.y + 1f, cell.width - 4f, 14f);
			EditorGUI.DrawRect(new Rect(label.x - 1f, label.y, Mathf.Min(label.width, text.Length * 7f + 6f), 13f), new Color(0f, 0f, 0f, 0.6f));
			GUI.Label(label, text.ToString(), _badgeStyle);
		}

		// An outline per cell rather than lines across the whole sheet.
		private static void DrawGridLines(Rect content, PetAtlas atlas, float scaleX, float scaleY)
		{
			for (int index = 0; index < atlas.CellCount; index++)
			{
				Rect cell = CellRect(content, atlas, index, scaleX, scaleY);

				// Below this the outlines are the only thing visible.
				if (cell.width < 6f || cell.height < 6f) return;

				DrawOutline(cell, GridLineColor);
			}
		}

		private static void DrawOutline(Rect rect, Color color)
		{
			EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), color);
			EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
			EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), color);
			EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
		}

		// Read from the cell's own pixel rectangle rather than worked out from the column count.
		private static Rect CellRect(Rect content, PetAtlas atlas, int index, float scaleX, float scaleY)
		{
			RectInt cell = atlas.CellPixelRect(index);

			// CellPixelRect is bottom-up, the way the texture is stored.
			float top = atlas.Sheet.height - cell.y - cell.height;
			return new Rect(
				content.x + cell.x * scaleX,
				content.y + top * scaleY,
				cell.width * scaleX,
				cell.height * scaleY);
		}

		private Rect ContentRect(Rect area, PetAtlas atlas)
		{
			return new Rect(
				area.x + _pan.x,
				area.y + _pan.y,
				atlas.Sheet.width * _zoom,
				atlas.Sheet.height * _zoom);
		}

		private void FrameToFit(Rect area, PetAtlas atlas)
		{
			float fit = Mathf.Min(
				area.width / atlas.Sheet.width,
				area.height / atlas.Sheet.height);

			_zoom = Mathf.Clamp(fit, MinZoom, MaxZoom);
			_pan = new Vector2(
				(area.width - atlas.Sheet.width * _zoom) * 0.5f,
				(area.height - atlas.Sheet.height * _zoom) * 0.5f);
			_framed = true;
		}

		private void HandleInput(Rect area, Rect content, PetAtlas atlas)
		{
			Event e = Event.current;
			bool inside = area.Contains(e.mousePosition);

			int previousHover = _hoveredCell;
			_hoveredCell = inside ? CellAt(content, atlas, e.mousePosition) : -1;
			if (_hoveredCell != previousHover)
			{
				// Only the outline changed, but IMGUI has no finer unit than a repaint.
				GUI.changed = true;
			}

			switch (e.type)
			{
				case EventType.ScrollWheel:
					if (!inside)
					{
						break;
					}

					ZoomAt(area, e.mousePosition, -e.delta.y);
					e.Use();
					break;

				case EventType.MouseDrag:
					// Middle button, or left with alt held, which is what every other Unity view uses for panning.
					if (e.button == 2 || (e.button == 0 && e.alt))
					{
						_pan += e.delta;
						e.Use();
					}

					break;

				case EventType.MouseDown:
					if (!inside || e.button != 0 || e.alt)
					{
						break;
					}

					int cell = CellAt(content, atlas, e.mousePosition);
					if (cell < 0)
					{
						break;
					}

					// Ctrl or Cmd removes rather than appends, so a misclick is undone without leaving the grid.
					if (e.control || e.command)
					{
						CellRemoveRequested?.Invoke(cell);
					}
					else
					{
						CellClicked?.Invoke(cell);
					}

					e.Use();
					break;
			}
		}

		private void ZoomAt(Rect area, Vector2 mouse, float notches)
		{
			float before = _zoom;
			float after = Mathf.Clamp(before * (1f + notches * ZoomPerNotch), MinZoom, MaxZoom);
			if (Mathf.Approximately(before, after))
			{
				return;
			}

			// Keep whatever is under the cursor under the cursor.
			Vector2 origin = area.position + _pan;
			Vector2 pointInSheet = (mouse - origin) / before;
			_pan = mouse - pointInSheet * after - area.position;
			_zoom = after;
		}

		// Which cell the cursor is over, by testing the rectangles.
		private int CellAt(Rect content, PetAtlas atlas, Vector2 position)
		{
			if (!content.Contains(position)) return -1;

			float scaleX = content.width / atlas.Sheet.width;
			float scaleY = content.height / atlas.Sheet.height;
			int found = -1;

			for (int index = 0; index < atlas.CellCount; index++)
			{
				if (CellRect(content, atlas, index, scaleX, scaleY).Contains(position)) found = index;
			}

			return found;
		}

	}
}
