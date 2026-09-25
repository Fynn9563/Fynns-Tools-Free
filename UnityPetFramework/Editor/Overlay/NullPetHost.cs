using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// The editor's geometry on a platform with no overlay.
	internal sealed class NullPetHost : IPetHost
	{
		public bool IsReady => false;

		public bool ShouldShowPet => false;

		public RectInt HostBoundsPx => new RectInt(0, 0, 0, 0);

		public bool BoundsChangedThisTick => false;

		public Vector2Int PointToPixel(Vector2 point)
		{
			return new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
		}

		public RectInt PointToPixel(Rect pointRect)
		{
			return new RectInt(
				Mathf.RoundToInt(pointRect.x),
				Mathf.RoundToInt(pointRect.y),
				Mathf.RoundToInt(pointRect.width),
				Mathf.RoundToInt(pointRect.height));
		}

		public void Poll()
		{
		}
	}
}
