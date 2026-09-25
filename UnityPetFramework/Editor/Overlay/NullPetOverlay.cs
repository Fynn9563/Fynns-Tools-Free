using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The overlay on a platform that does not have one.
	internal sealed class NullPetOverlay : IPetOverlay
	{
		private readonly string _reason;

		public NullPetOverlay(string reason)
		{
			_reason = reason;
		}

		public bool IsAvailable => false;

		public string UnavailableReason => _reason;

		public bool IsVisible
		{
			get => false;
			set { }
		}

		public Vector2Int FrameSize => Vector2Int.zero;

		public double DoubleClickSeconds => 0.5d;

		public void Present(byte[] premultipliedBgra, Vector2Int desktopTopLeft)
		{
		}

		public PetPointerState SamplePointer()
		{
			return new PetPointerState(Vector2Int.zero, false, false);
		}

		public void SetClickThrough(bool clickThrough)
		{
		}

		public void Pump()
		{
		}

		public void Dispose()
		{
		}
	}
}
