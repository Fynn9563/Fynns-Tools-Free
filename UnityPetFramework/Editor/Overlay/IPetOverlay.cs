using System;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The line between the pet and the operating system.
	internal interface IPetOverlay : IDisposable
	{
		// False on anything but Windows.
		bool IsAvailable { get; }

		// Why not, in words meant for the settings window rather than a log file.
		string UnavailableReason { get; }

		bool IsVisible { get; set; }

		// Constant while a pet is loaded at a given scale.
		Vector2Int FrameSize { get; }
		void Present(byte[] premultipliedBgra, Vector2Int desktopTopLeft);

		// Where the cursor is and what it is doing, read fresh.
		PetPointerState SamplePointer();

		// When true the pet stops receiving clicks and they fall through to the editor underneath.
		void SetClickThrough(bool clickThrough);

		// How long the system counts as a double click.
		double DoubleClickSeconds { get; }

		// Drains any messages the window has been sent.
		void Pump();
	}

	// The editor as geometry.
	internal interface IPetHost
	{
		// False until Unity's own window has been found.
		bool IsReady { get; }

		// False when Unity is minimised or another application is in front.
		bool ShouldShowPet { get; }

		// Unity's main window in desktop pixels.
		RectInt HostBoundsPx { get; }

		// True on the tick the window moved or resized.
		bool BoundsChangedThisTick { get; }

		// Unity reports positions in points, which are not pixels once display scaling is involved.
		Vector2Int PointToPixel(Vector2 point);

		RectInt PointToPixel(Rect pointRect);

		void Poll();
	}

	// The cursor, as the pet needs to see it.
	internal readonly struct PetPointerState
	{
		public PetPointerState(Vector2Int desktopPosition, bool isOverPet, bool leftDown)
		{
			DesktopPosition = desktopPosition;
			IsOverPet = isOverPet;
			LeftDown = leftDown;
		}

		public Vector2Int DesktopPosition { get; }

		// True only over pixels the pet actually draws on.
		public bool IsOverPet { get; }

		public bool LeftDown { get; }
	}
}
