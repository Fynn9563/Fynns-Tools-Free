using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Moves the pet along the ground and keeps it inside where it is allowed.
	internal sealed class PetMotor
	{
		private const float WalkPixelsPerSecond = 48f;

		// How far a jump carries the pet, as a multiple of its own width.
		public const float JumpDistanceInBodyWidths = 1.25f;

		private Vector2Int _feet;
		private bool _freePlacement;
		public void PlaceDropped(Vector2Int feet) { _feet = feet; _freePlacement = true; }

		public Vector2Int Feet => _feet;

		// The window position the overlay needs, worked back from the feet.
		public Vector2Int TopLeftFor(Vector2Int frameSize)
		{
			return new Vector2Int(_feet.x - frameSize.x / 2, _feet.y - frameSize.y);
		}

		public void Place(Vector2Int feet)
		{
			_feet = feet;
			_freePlacement = false;
		}

		// Walks, and reports whether the pet ran into the edge of where it is allowed to be.
		public bool Walk(float deltaTime, PetFacing facing, float scale, PetSafeArea area, Vector2Int frameSize)
		{
			float speed = WalkPixelsPerSecond * Mathf.Max(0.2f, scale);
			int step = Mathf.RoundToInt(speed * deltaTime * (facing == PetFacing.Left ? -1f : 1f));
			if (step == 0)
			{
				step = facing == PetFacing.Left ? -1 : 1;
			}

			return MoveBy(step, area, frameSize);
		}

		// Carries the pet forwards through a jump.
		public bool JumpStep(float progressDelta, PetFacing facing, float scale, PetSafeArea area, Vector2Int frameSize)
		{
			if (progressDelta <= 0f)
			{
				return true;
			}

			float distance = frameSize.x * JumpDistanceInBodyWidths;
			int step = Mathf.RoundToInt(distance * progressDelta * (facing == PetFacing.Left ? -1f : 1f));
			if (step == 0)
			{
				return true;
			}

			return !MoveBy(step, area, frameSize);
		}

		// Puts the pet back somewhere sensible after a layout change.
		public void Settle(PetSafeArea area, Vector2Int frameSize)
		{
			_feet = _freePlacement ? area.ClampPlacement(_feet, frameSize) : area.ClampFeet(_feet, frameSize);
		}

		// Returns true when the move was cut short by a boundary.
		private bool MoveBy(int step, PetSafeArea area, Vector2Int frameSize)
		{
			Vector2Int wanted = new Vector2Int(_feet.x + step, _feet.y);
			Vector2Int allowed = _freePlacement ? area.ClampPlacement(wanted, frameSize) : area.ClampFeet(wanted, frameSize);

			bool blocked = allowed.x != wanted.x;
			_feet = allowed;
			return blocked;
		}
	}
}
