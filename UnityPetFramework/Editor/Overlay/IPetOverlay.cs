using System;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal interface IPetOverlay : IDisposable
	{
		bool IsAvailable { get; }

		string UnavailableReason { get; }

		bool IsVisible { get; set; }

		Vector2Int FrameSize { get; }
		void Present(byte[] premultipliedBgra, Vector2Int desktopTopLeft);

		PetPointerState SamplePointer();

		void SetClickThrough(bool clickThrough);

		double DoubleClickSeconds { get; }

		void Pump();
	}

	internal interface IPetHost
	{
		bool IsReady { get; }

		bool ShouldShowPet { get; }

		RectInt HostBoundsPx { get; }

		bool BoundsChangedThisTick { get; }

		Vector2Int PointToPixel(Vector2 point);

		RectInt PointToPixel(Rect pointRect);

		void Poll();
	}

	internal readonly struct PetPointerState
	{
		public PetPointerState(Vector2Int desktopPosition, bool isOverPet, bool leftDown)
		{
			DesktopPosition = desktopPosition;
			IsOverPet = isOverPet;
			LeftDown = leftDown;
		}

		public Vector2Int DesktopPosition { get; }

		public bool IsOverPet { get; }

		public bool LeftDown { get; }
	}
}
