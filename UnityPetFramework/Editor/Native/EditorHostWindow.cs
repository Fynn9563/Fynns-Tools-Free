#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Unity's own window.
	internal sealed class EditorHostWindow : IPetHost
	{
		// Unity's main window class on Windows.
		private const string UnityContainerClass = "UnityContainerWndClass";

		private IntPtr _owner;
		private RectInt _bounds;
		private bool _boundsChanged;

		// Points to pixels, fitted from the main window rather than assumed.
		private float _scaleX = 1f;
		private float _scaleY = 1f;
		private Rect _mainWindowPoints;

		public IntPtr Owner => _owner;

		public bool ShouldShowPet { get; private set; }

		public RectInt HostBoundsPx => _bounds;

		public bool BoundsChangedThisTick => _boundsChanged;

		public bool IsReady => _owner != IntPtr.Zero && Win32.IsWindow(_owner);

		public void Poll()
		{
			_boundsChanged = false;

			if (!IsReady)
			{
				_owner = Resolve();
				if (_owner == IntPtr.Zero)
				{
					ShouldShowPet = false;
					return;
				}
			}

			if (Win32.IsIconic(_owner))
			{
				ShouldShowPet = false;
				return;
			}

			// GA_ROOTOWNER walks up to the window that owns the foreground one.
			IntPtr foregroundRoot = Win32.GetAncestor(Win32.GetForegroundWindow(), Win32.GA_ROOTOWNER);
			ShouldShowPet = foregroundRoot == _owner;

			RectInt current = ReadBounds(_owner);
			if (current.width <= 0 || current.height <= 0)
			{
				ShouldShowPet = false;
				return;
			}

			if (!current.Equals(_bounds))
			{
				_bounds = current;
				_boundsChanged = true;
				Recalibrate();
			}
		}

		// Re-asserts the pet above any Unity window created since the last check.
		public void RaiseAbove(IntPtr petWindow)
		{
			if (petWindow == IntPtr.Zero || !IsReady)
			{
				return;
			}

			Win32.SetWindowPos(petWindow, Win32.HWND_TOP, 0, 0, 0, 0,
				Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE | Win32.SWP_NOOWNERZORDER);
		}

		public Vector2Int PointToPixel(Vector2 point)
		{
			return new Vector2Int(
				Mathf.RoundToInt(_bounds.xMin + (point.x - _mainWindowPoints.xMin) * _scaleX),
				Mathf.RoundToInt(_bounds.yMin + (point.y - _mainWindowPoints.yMin) * _scaleY));
		}

		public RectInt PointToPixel(Rect pointRect)
		{
			Vector2Int min = PointToPixel(pointRect.min);
			Vector2Int max = PointToPixel(pointRect.max);
			return new RectInt(min.x, min.y, Mathf.Max(0, max.x - min.x), Mathf.Max(0, max.y - min.y));
		}

		// Fits points to pixels from the one rectangle known in both.
		private void Recalibrate()
		{
			_mainWindowPoints = EditorGUIUtility.GetMainWindowPosition();

			if (_mainWindowPoints.width > 1f && _mainWindowPoints.height > 1f)
			{
				_scaleX = _bounds.width / _mainWindowPoints.width;
				_scaleY = _bounds.height / _mainWindowPoints.height;
				return;
			}

			_scaleX = 1f;
			_scaleY = 1f;
		}

		// The frame bounds rather than GetWindowRect.
		private static RectInt ReadBounds(IntPtr window)
		{
			if (Win32.DwmGetWindowAttribute(window, Win32.DWMWA_EXTENDED_FRAME_BOUNDS,
					out Win32.RECT frame, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32.RECT))) != 0)
			{
				if (!Win32.GetWindowRect(window, out frame))
				{
					return new RectInt(0, 0, 0, 0);
				}
			}

			return new RectInt(frame.left, frame.top, frame.right - frame.left, frame.bottom - frame.top);
		}

		// FindWindowEx rather than EnumWindows.
		private static IntPtr Resolve()
		{
			uint ourProcess = (uint)Process.GetCurrentProcess().Id;

			IntPtr byClass = FindTopLevel(UnityContainerClass, ourProcess);
			if (byClass != IntPtr.Zero)
			{
				return byClass;
			}

			return FindTopLevel(null, ourProcess);
		}

		private static IntPtr FindTopLevel(string className, uint processId)
		{
			IntPtr best = IntPtr.Zero;
			int bestArea = 0;

			IntPtr found = IntPtr.Zero;
			while (true)
			{
				found = Win32.FindWindowExW(IntPtr.Zero, found, className, null);
				if (found == IntPtr.Zero)
				{
					return best;
				}

				Win32.GetWindowThreadProcessId(found, out uint owningProcess);
				if (owningProcess != processId)
				{
					continue;
				}

				if (!Win32.IsWindowVisible(found) || Win32.GetWindow(found, Win32.GW_OWNER) != IntPtr.Zero)
				{
					continue;
				}

				int style = Win32.GetWindowLongW(found, Win32.GWL_EXSTYLE);
				if ((style & unchecked((int)Win32.WS_EX_TOOLWINDOW)) != 0)
				{
					continue;
				}

				// Largest wins.
				RectInt bounds = ReadBounds(found);
				int area = bounds.width * bounds.height;
				if (area > bestArea)
				{
					bestArea = area;
					best = found;
				}
			}
		}
	}
}
#endif
