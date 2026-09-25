using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Where the pet is allowed to be, in desktop pixels.
	internal sealed class PetSafeArea
	{
		// Enough to clear a tab strip, so the pet stands inside a view rather than on the tabs above it.
		private const int TopInsetPixels = 22;

		// A region narrower than this cannot be walked across meaningfully.
		private const int MinUsableWidth = 64;

		// How often the views are re-measured when nothing else has prompted it.
		private const double RefreshIntervalSeconds = 0.35d;

		private readonly List<Rect> _viewPoints = new List<Rect>();
		private readonly List<RectInt> _regions = new List<RectInt>();
		private readonly List<RectInt> _candidate = new List<RectInt>();

		private double _lastRefresh = double.NegativeInfinity;
		private PetMovementMode _lastMode = PetMovementMode.FreeRoam;
		private bool _hasMode;

		public IReadOnlyList<RectInt> Regions => _regions;

		// True on the tick the regions actually changed.
		public bool ChangedThisTick { get; private set; }

		// True when there is nowhere valid.
		public bool IsEmpty => _regions.Count == 0;

		// Re-measures when something has probably moved, and on a slow poll otherwise.
		public void Tick(IPetHost host, PetMovementMode mode, double now, bool force)
		{
			bool modeChanged = !_hasMode || mode != _lastMode;
			_hasMode = true;
			_lastMode = mode;

			if (!force && !modeChanged && now - _lastRefresh < RefreshIntervalSeconds)
			{
				ChangedThisTick = false;
				return;
			}

			_lastRefresh = now;
			Refresh(host, mode);
		}

		public void Refresh(IPetHost host, PetMovementMode mode)
		{
			_candidate.Clear();
			Collect(host, mode, _candidate);

			ChangedThisTick = !SameAs(_candidate);
			if (!ChangedThisTick)
			{
				return;
			}

			_regions.Clear();
			_regions.AddRange(_candidate);
		}

		private void Collect(IPetHost host, PetMovementMode mode, List<RectInt> results)
		{
			RectInt hostBounds = host.HostBoundsPx;
			if (hostBounds.width <= 0 || hostBounds.height <= 0)
			{
				return;
			}

			if (mode == PetMovementMode.FreeRoam)
			{
				results.Add(hostBounds);
				return;
			}

			EditorViewLocator.CollectVisibleViews(_viewPoints);

			foreach (Rect viewPoints in _viewPoints)
			{
				RectInt pixels = host.PointToPixel(viewPoints);
				pixels = new RectInt(pixels.x, pixels.y + TopInsetPixels, pixels.width, pixels.height - TopInsetPixels);

				if (pixels.width < MinUsableWidth || pixels.height <= 0)
				{
					continue;
				}

				// A floating view is a separate top-level window of its own.
				if (!Encloses(hostBounds, pixels))
				{
					continue;
				}

				results.Add(pixels);
			}
		}

		private bool SameAs(List<RectInt> other)
		{
			if (_regions.Count != other.Count)
			{
				return false;
			}

			for (int i = 0; i < _regions.Count; i++)
			{
				if (!_regions[i].Equals(other[i]))
				{
					return false;
				}
			}

			return true;
		}

		// Keeps the pet's feet inside a region.
		public Vector2Int ClampFeet(Vector2Int feet, Vector2Int frameSize)
		{
			if (_regions.Count == 0)
			{
				return feet;
			}

			RectInt region = RegionFor(feet);
			int halfWidth = frameSize.x / 2;

			int minX = region.xMin + halfWidth;
			int maxX = Mathf.Max(minX, region.xMax - halfWidth);
			int x = Mathf.Clamp(feet.x, minX, maxX);

			// The feet go on the bottom edge of the region, always.
			return new Vector2Int(x, region.yMax);
		}

		// Dropping keeps both coordinates.
		public Vector2Int ClampPlacement(Vector2Int feet, Vector2Int frameSize)
		{
			if (_regions.Count == 0) return feet;
			RectInt region = RegionFor(feet);
			int half = Mathf.Min(frameSize.x / 2, region.width / 2);
			int top = region.y + Mathf.Min(frameSize.y, region.height);
			return new Vector2Int(Mathf.Clamp(feet.x, region.x + half, region.xMax - half), Mathf.Clamp(feet.y, top, region.yMax));
		}

		// Somewhere else to stand that the pet can actually reach.
		public bool TryPickDestination(Vector2Int from, Vector2Int frameSize, out PetDestination destination)
		{
			destination = default;

			if (_regions.Count == 0)
			{
				return false;
			}

			RectInt current = RegionFor(from);

			// Somewhere the pet is not already, preferably.
			List<RectInt> elsewhere = new List<RectInt>();
			foreach (RectInt region in _regions)
			{
				if (!region.Equals(current) && region.width >= frameSize.x)
				{
					elsewhere.Add(region);
				}
			}

			RectInt target = elsewhere.Count > 0
				? elsewhere[Random.Range(0, elsewhere.Count)]
				: current;

			int halfWidth = frameSize.x / 2;
			int minX = target.xMin + halfWidth;
			int maxX = Mathf.Max(minX, target.xMax - halfWidth);
			int x = Random.Range(minX, maxX + 1);

			// Staying in the same region.
			if (elsewhere.Count == 0 && maxX - minX > frameSize.x)
			{
				int minimumTravel = (maxX - minX) / 3;
				if (Mathf.Abs(x - from.x) < minimumTravel)
				{
					x = from.x < (minX + maxX) / 2 ? maxX : minX;
				}
			}

			Vector2Int feet = ClampFeet(new Vector2Int(x, target.yMax), frameSize);
			destination = new PetDestination(feet, target);
			return true;
		}

		// Whether a destination still makes sense.
		public bool IsDestinationValid(PetDestination destination, Vector2Int frameSize)
		{
			if (!destination.IsSet)
			{
				return false;
			}

			foreach (RectInt region in _regions)
			{
				if (region.Equals(destination.Region))
				{
					return destination.Feet.x >= region.xMin && destination.Feet.x <= region.xMax;
				}
			}

			return false;
		}

		// A destination in the direction the pet is being pushed, for Run From Mouse.
		public bool TryPickAwayFrom(Vector2Int from, int awayFromX, Vector2Int frameSize, out PetDestination destination)
		{
			destination = default;

			if (_regions.Count == 0)
			{
				return false;
			}

			RectInt region = RegionFor(from);
			int halfWidth = frameSize.x / 2;
			int minX = region.xMin + halfWidth;
			int maxX = Mathf.Max(minX, region.xMax - halfWidth);

			int direction = awayFromX < from.x ? 1 : -1;
			int wanted = from.x + direction * frameSize.x * 2;
			int x = Mathf.Clamp(wanted, minX, maxX);

			// Already as far away as this region goes.
			if (Mathf.Abs(x - from.x) < 4)
			{
				return false;
			}

			destination = new PetDestination(ClampFeet(new Vector2Int(x, region.yMax), frameSize), region);
			return true;
		}

		public bool Contains(Vector2Int feet)
		{
			foreach (RectInt region in _regions)
			{
				if (feet.x >= region.xMin && feet.x <= region.xMax &&
					feet.y >= region.yMin && feet.y <= region.yMax)
				{
					return true;
				}
			}

			return false;
		}

		private RectInt RegionFor(Vector2Int feet)
		{
			RectInt best = _regions[0];
			float bestDistance = float.MaxValue;

			foreach (RectInt region in _regions)
			{
				if (feet.x >= region.xMin && feet.x <= region.xMax &&
					feet.y >= region.yMin && feet.y <= region.yMax)
				{
					return region;
				}

				Vector2 centre = new Vector2(region.center.x, region.center.y);
				float distance = Vector2.SqrMagnitude(centre - new Vector2(feet.x, feet.y));
				if (distance < bestDistance)
				{
					bestDistance = distance;
					best = region;
				}
			}

			return best;
		}

		private static bool Encloses(RectInt outer, RectInt inner)
		{
			return inner.xMin >= outer.xMin && inner.yMin >= outer.yMin &&
				inner.xMax <= outer.xMax && inner.yMax <= outer.yMax;
		}
	}
}
