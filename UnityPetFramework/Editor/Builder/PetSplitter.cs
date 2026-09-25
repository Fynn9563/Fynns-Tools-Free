using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// A draggable divider between two columns, with the position remembered.
	internal sealed class PetSplitter
	{
		public const float HandleWidth = 6f;

		private readonly string _key;
		private readonly float _fallback;
		private readonly float _minLeft;
		private readonly float _minRight;

		private float _left = -1f;
		private float _lowerBound;
		private float _upperBound;
		private int _hot = -1;

		public PetSplitter(string key, float fallback, float minLeft, float minRight)
		{
			_key = key;
			_fallback = fallback;
			_minLeft = minLeft;
			_minRight = minRight;
			_lowerBound = minLeft;
			_upperBound = minLeft;
		}

		// Below this there is no room for both columns.
		public float MinimumForSplit => _minLeft + _minRight + HandleWidth;

		public bool IsDragging => _hot >= 0 && GUIUtility.hotControl == _hot;

		// The left column's width for this frame.
		public float Resolve(float available)
		{
			if (_left < 0f)
			{
				_left = EditorPrefs.GetFloat(_key, _fallback);
			}

			_lowerBound = _minLeft;
			_upperBound = Mathf.Max(_minLeft, available - _minRight - HandleWidth);
			return Mathf.Clamp(_left, _lowerBound, _upperBound);
		}

		// Call between the two columns, inside the horizontal scope.
		public void DrawHandle()
		{
			Rect area = GUILayoutUtility.GetRect(
				HandleWidth, HandleWidth, 0f, float.MaxValue,
				GUILayout.Width(HandleWidth), GUILayout.ExpandHeight(true));

			// The whole strip is grabbable.
			EditorGUI.DrawRect(new Rect(Mathf.Round(area.center.x), area.y, 1f, area.height), SeparatorColour);
			EditorGUIUtility.AddCursorRect(area, MouseCursor.ResizeHorizontal);

			int id = GUIUtility.GetControlID(FocusType.Passive);
			Event e = Event.current;

			switch (e.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (e.button == 0 && area.Contains(e.mousePosition))
					{
						_hot = id;
						GUIUtility.hotControl = id;
						e.Use();
					}

					break;

				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						// Clamped as it moves, not only when read.
						_left = Mathf.Clamp(_left + e.delta.x, _lowerBound, _upperBound);
						e.Use();
					}

					break;

				case EventType.MouseUp:
					if (GUIUtility.hotControl == id)
					{
						GUIUtility.hotControl = 0;
						_hot = -1;

						// Written on release rather than per drag event.
						EditorPrefs.SetFloat(_key, _left);
						e.Use();
					}

					break;
			}
		}

		private static Color SeparatorColour => EditorGUIUtility.isProSkin
			? new Color(0f, 0f, 0f, .5f)
			: new Color(0f, 0f, 0f, .25f);
	}
}
