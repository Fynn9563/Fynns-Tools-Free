#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FynnsTools.UnityPetFramework
{
	// A transparent, click-through-where-empty window that sits above Unity.
	internal sealed class LayeredPetOverlay : IPetOverlay
	{
		// Shared by every instance in the process and never unregistered.
		public const string WindowClassName = "FynnsToolsUnityPetOverlay";

		private const int MaxMessagesPerPump = 64;
		private const int MaxRecreateAttempts = 3;

		private static bool _classRegistered;

		private readonly int _mainThreadId;
		private readonly IntPtr _owner;

		private IntPtr _hwnd;
		private IntPtr _screenDc;
		private IntPtr _memDc;
		private IntPtr _dib;
		private IntPtr _oldBitmap;
		private IntPtr _dibBits;

		private int _byteCount;
		private bool _visible;
		private bool _clickThrough;
		private bool _disposed;
		private int _recreateAttempts;
		private string _unavailableReason;

		private LayeredPetOverlay(IntPtr owner, Vector2Int frameSize)
		{
			_mainThreadId = Thread.CurrentThread.ManagedThreadId;
			_owner = owner;
			FrameSize = frameSize;
			_byteCount = frameSize.x * frameSize.y * 4;
		}

		public bool IsAvailable => !_disposed && _hwnd != IntPtr.Zero && _unavailableReason == null;

		public string UnavailableReason => _unavailableReason;

		public Vector2Int FrameSize { get; }

		public double DoubleClickSeconds => Win32.GetDoubleClickTime() / 1000d;

		public bool IsVisible
		{
			get => _visible;
			set
			{
				if (_visible == value || !IsAvailable)
				{
					return;
				}

				_visible = value;
				Win32.ShowWindow(_hwnd, value ? Win32.SW_SHOWNOACTIVATE : Win32.SW_HIDE);
			}
		}

		// Returns null rather than throwing when the window cannot be made.
		public static LayeredPetOverlay TryCreate(IntPtr owner, Vector2Int frameSize, out string error)
		{
			if (frameSize.x <= 0 || frameSize.y <= 0)
			{
				error = "The pet has no size to draw at.";
				return null;
			}

			if (!EnsureClassRegistered(out error))
			{
				return null;
			}

			LayeredPetOverlay overlay = new LayeredPetOverlay(owner, frameSize);
			if (!overlay.CreateWindowAndSurface(out error))
			{
				overlay.Dispose();
				return null;
			}

			return overlay;
		}

		// Destroys any window of our class left behind by a previous domain.
		public static int SweepOrphans()
		{
			uint ourProcess = (uint)Process.GetCurrentProcess().Id;
			int destroyed = 0;

			IntPtr found = IntPtr.Zero;
			while (true)
			{
				found = Win32.FindWindowExW(IntPtr.Zero, found, WindowClassName, null);
				if (found == IntPtr.Zero)
				{
					break;
				}

				Win32.GetWindowThreadProcessId(found, out uint owningProcess);
				if (owningProcess != ourProcess)
				{
					// Another Unity instance's pet.
					continue;
				}

				IntPtr doomed = found;

				// Step the search on before destroying.
				found = Win32.FindWindowExW(IntPtr.Zero, doomed, WindowClassName, null);
				if (Win32.DestroyWindow(doomed))
				{
					destroyed++;
				}
			}

			return destroyed;
		}

		public void Present(byte[] premultipliedBgra, Vector2Int desktopTopLeft)
		{
			AssertMainThread();

			if (!IsAvailable || premultipliedBgra == null || premultipliedBgra.Length < _byteCount)
			{
				return;
			}

			Marshal.Copy(premultipliedBgra, 0, _dibBits, _byteCount);

			Win32.POINT destination = new Win32.POINT { x = desktopTopLeft.x, y = desktopTopLeft.y };
			Win32.POINT source = new Win32.POINT { x = 0, y = 0 };
			Win32.SIZE size = new Win32.SIZE { cx = FrameSize.x, cy = FrameSize.y };

			// AC_SRC_ALPHA means the colour channels are expected to be premultiplied by alpha already.
			Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION
			{
				BlendOp = Win32.AC_SRC_OVER,
				BlendFlags = 0,
				SourceConstantAlpha = 255,
				AlphaFormat = Win32.AC_SRC_ALPHA
			};

			// Position and pixels in one call, so moving the pet is not a second syscall per frame.
			if (Win32.UpdateLayeredWindow(_hwnd, IntPtr.Zero, ref destination, ref size,
					_memDc, ref source, 0, ref blend, Win32.ULW_ALPHA))
			{
				return;
			}

			HandlePresentFailure();
		}

		public PetPointerState SamplePointer()
		{
			if (!IsAvailable || !Win32.GetCursorPos(out Win32.POINT cursor))
			{
				return new PetPointerState(Vector2Int.zero, false, false);
			}

			// WindowFromPoint honours a layered window's per-pixel alpha.
			bool overPet = !_clickThrough && _visible && Win32.WindowFromPoint(cursor) == _hwnd;
			bool leftDown = (Win32.GetAsyncKeyState(Win32.VK_LBUTTON) & 0x8000) != 0;

			return new PetPointerState(new Vector2Int(cursor.x, cursor.y), overPet, leftDown);
		}

		public void SetClickThrough(bool clickThrough)
		{
			AssertMainThread();

			if (_clickThrough == clickThrough || !IsAvailable)
			{
				return;
			}

			_clickThrough = clickThrough;

			int style = Win32.GetWindowLongW(_hwnd, Win32.GWL_EXSTYLE);
			style = clickThrough
				? style | unchecked((int)Win32.WS_EX_TRANSPARENT)
				: style & ~unchecked((int)Win32.WS_EX_TRANSPARENT);

			Win32.SetWindowLongW(_hwnd, Win32.GWL_EXSTYLE, style);
		}

		// Bounded, and every message goes straight to DefWindowProcW, which is native.
		public void Pump()
		{
			if (!IsAvailable)
			{
				return;
			}

			for (int i = 0; i < MaxMessagesPerPump; i++)
			{
				if (!Win32.PeekMessageW(out Win32.MSG message, _hwnd, 0, 0, Win32.PM_REMOVE))
				{
					return;
				}

				Win32.DispatchMessageW(ref message);
			}
		}
		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			// The window goes first.
			try
			{
				if (_hwnd != IntPtr.Zero)
				{
					Win32.DestroyWindow(_hwnd);
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "The pet window could not be closed cleanly: " + exception.Message);
			}
			finally
			{
				_hwnd = IntPtr.Zero;
			}

			// Reselecting the original bitmap before deleting ours is not optional.
			try
			{
				if (_memDc != IntPtr.Zero && _oldBitmap != IntPtr.Zero)
				{
					Win32.SelectObject(_memDc, _oldBitmap);
				}

				if (_dib != IntPtr.Zero)
				{
					Win32.DeleteObject(_dib);
				}

				if (_memDc != IntPtr.Zero)
				{
					Win32.DeleteDC(_memDc);
				}

				if (_screenDc != IntPtr.Zero)
				{
					Win32.ReleaseDC(IntPtr.Zero, _screenDc);
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "The pet's drawing surface could not be released cleanly: " + exception.Message);
			}
			finally
			{
				_oldBitmap = IntPtr.Zero;
				_dib = IntPtr.Zero;
				_memDc = IntPtr.Zero;
				_screenDc = IntPtr.Zero;
				_dibBits = IntPtr.Zero;
			}
		}

		private static bool EnsureClassRegistered(out string error)
		{
			error = null;
			if (_classRegistered)
			{
				return true;
			}

			IntPtr user32 = Win32.GetModuleHandleW("user32.dll");
			IntPtr defWindowProc = user32 == IntPtr.Zero
				? IntPtr.Zero
				: Win32.GetProcAddress(user32, "DefWindowProcW");

			if (defWindowProc == IntPtr.Zero)
			{
				error = "Windows would not hand over its default window handler, so the pet cannot be shown.";
				return false;
			}

			Win32.WNDCLASSEX windowClass = new Win32.WNDCLASSEX
			{
				cbSize = (uint)Marshal.SizeOf(typeof(Win32.WNDCLASSEX)),
				style = Win32.CS_HREDRAW | Win32.CS_VREDRAW,

				// The whole point.
				lpfnWndProc = defWindowProc,
				hInstance = Win32.GetModuleHandleW(null),
				lpszClassName = WindowClassName
			};

			if (Win32.RegisterClassExW(ref windowClass) == 0)
			{
				int lastError = Marshal.GetLastWin32Error();

				// Already registered by a previous domain in this same process.
				if (lastError != Win32.ERROR_CLASS_ALREADY_EXISTS)
				{
					error = "The pet's window could not be registered with Windows (error " + lastError + ").";
					return false;
				}
			}

			_classRegistered = true;
			return true;
		}

		private bool CreateWindowAndSurface(out string error)
		{
			// WS_EX_LAYERED gives per-pixel alpha.
			uint exStyle = Win32.WS_EX_LAYERED | Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW;

			_hwnd = Win32.CreateWindowExW(
				exStyle,
				WindowClassName,
				"Fynn's Unity Pet",
				Win32.WS_POPUP,
				0, 0, FrameSize.x, FrameSize.y,
				_owner, IntPtr.Zero, Win32.GetModuleHandleW(null), IntPtr.Zero);

			if (_hwnd == IntPtr.Zero)
			{
				error = "The pet's window could not be created (error " + Marshal.GetLastWin32Error() + ").";
				return false;
			}

			return CreateSurface(out error);
		}

		private bool CreateSurface(out string error)
		{
			_screenDc = Win32.GetDC(IntPtr.Zero);
			_memDc = Win32.CreateCompatibleDC(_screenDc);

			Win32.BITMAPINFO info = new Win32.BITMAPINFO
			{
				bmiHeader = new Win32.BITMAPINFOHEADER
				{
					biSize = (uint)Marshal.SizeOf(typeof(Win32.BITMAPINFOHEADER)),
					biWidth = FrameSize.x,

					// Positive height means bottom-up rows.
					biHeight = FrameSize.y,
					biPlanes = 1,
					biBitCount = 32,
					biCompression = Win32.BI_RGB
				}
			};

			_dib = Win32.CreateDIBSection(_screenDc, ref info, Win32.DIB_RGB_COLORS, out _dibBits, IntPtr.Zero, 0);
			if (_dib == IntPtr.Zero || _dibBits == IntPtr.Zero)
			{
				error = "The pet's drawing surface could not be created (error " + Marshal.GetLastWin32Error() + ").";
				return false;
			}

			_oldBitmap = Win32.SelectObject(_memDc, _dib);
			_byteCount = FrameSize.x * FrameSize.y * 4;

			error = null;
			return true;
		}

		// UpdateLayeredWindow failing usually means the window is gone.
		private void HandlePresentFailure()
		{
			if (Win32.IsWindow(_hwnd))
			{
				return;
			}

			if (++_recreateAttempts > MaxRecreateAttempts)
			{
				_unavailableReason = "The pet's window kept closing, so it has been switched off. Reopening the Unity Pet Framework window will try again.";
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + _unavailableReason);
				return;
			}

			_hwnd = IntPtr.Zero;
			if (!CreateWindowAndSurface(out string error))
			{
				_unavailableReason = error;
				return;
			}

			if (_visible)
			{
				Win32.ShowWindow(_hwnd, Win32.SW_SHOWNOACTIVATE);
			}
		}

		private void AssertMainThread()
		{
			if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
			{
				return;
			}

			// Not an exception.
			Debug.LogError(UnityPetFrameworkInfo.LogPrefix +
				"The pet's window was touched from a background thread, which Windows does not allow. The call was ignored.");
		}
	}
}
#endif
