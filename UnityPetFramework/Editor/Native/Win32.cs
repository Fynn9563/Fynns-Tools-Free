#if UNITY_EDITOR_WIN
using System;
using System.Runtime.InteropServices;

namespace FynnsTools.UnityPetFramework
{
	// Declarations only.
	internal static class Win32
	{
		// Window styles.
		public const uint WS_POPUP = 0x80000000;

		public const uint WS_EX_LAYERED = 0x00080000;
		public const uint WS_EX_TRANSPARENT = 0x00000020;
		public const uint WS_EX_TOOLWINDOW = 0x00000080;
		public const uint WS_EX_NOACTIVATE = 0x08000000;

		public const int GWL_EXSTYLE = -20;

		// Class styles.
		public const uint CS_HREDRAW = 0x0002;
		public const uint CS_VREDRAW = 0x0001;

		public const int ERROR_CLASS_ALREADY_EXISTS = 1410;

		// UpdateLayeredWindow.
		public const int ULW_ALPHA = 0x00000002;
		public const byte AC_SRC_OVER = 0x00;
		public const byte AC_SRC_ALPHA = 0x01;

		// CreateDIBSection.
		public const uint BI_RGB = 0;
		public const uint DIB_RGB_COLORS = 0;

		// ShowWindow.
		public const int SW_HIDE = 0;
		public const int SW_SHOWNOACTIVATE = 4;

		// SetWindowPos.
		public const uint SWP_NOSIZE = 0x0001;
		public const uint SWP_NOMOVE = 0x0002;
		public const uint SWP_NOACTIVATE = 0x0010;
		public const uint SWP_NOOWNERZORDER = 0x0200;
		public static readonly IntPtr HWND_TOP = IntPtr.Zero;

		// PeekMessage.
		public const uint PM_REMOVE = 0x0001;

		// GetAsyncKeyState.
		public const int VK_LBUTTON = 0x01;

		// GetAncestor.
		public const uint GA_ROOTOWNER = 3;

		// GetWindow.
		public const uint GW_OWNER = 4;

		// DwmGetWindowAttribute.
		public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE
		{
			public int cx;
			public int cy;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MSG
		{
			public IntPtr hwnd;
			public uint message;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public POINT pt;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BLENDFUNCTION
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFOHEADER
		{
			public uint biSize;
			public int biWidth;

			// Positive means the rows run bottom to top.
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFO
		{
			public BITMAPINFOHEADER bmiHeader;

			// One entry is enough.
			public uint bmiColors;
		}

		// lpfnWndProc is an IntPtr rather than a delegate on purpose.
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WNDCLASSEX
		{
			public uint cbSize;
			public uint style;
			public IntPtr lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			[MarshalAs(UnmanagedType.LPWStr)] public string lpszMenuName;
			[MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
			public IntPtr hIconSm;
		}

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern ushort RegisterClassExW(ref WNDCLASSEX lpwcx);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr CreateWindowExW(
			uint dwExStyle,
			[MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
			[MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
			uint dwStyle,
			int x, int y, int nWidth, int nHeight,
			IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindowExW(IntPtr hWndParent, IntPtr hWndChildAfter,
			[MarshalAs(UnmanagedType.LPWStr)] string lpszClass,
			[MarshalAs(UnmanagedType.LPWStr)] string lpszWindow);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowLongW(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int SetWindowLongW(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
			int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out POINT lpPoint);

		// Respects the per-pixel alpha of a layered window.
		[DllImport("user32.dll")]
		public static extern IntPtr WindowFromPoint(POINT point);

		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(int vKey);

		[DllImport("user32.dll")]
		public static extern uint GetDoubleClickTime();

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PeekMessageW(out MSG lpMsg, IntPtr hWnd,
			uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr DispatchMessageW(ref MSG lpMsg);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
			ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc,
			uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi,
			uint usage, out IntPtr ppvBits, IntPtr hSection, uint offset);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		[DllImport("gdi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject(IntPtr hObject);

		[DllImport("dwmapi.dll")]
		public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute,
			out RECT pvAttribute, int cbAttribute);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
	}
}
#endif
