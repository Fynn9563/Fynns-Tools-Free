using System;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// What the mouse does to the pet.
	internal sealed class PetInteraction
	{
		// How far from where it started a press may be released and still count as a click.
		private const float ClickSlopPixels = 6f;
		private const float ClickTravelPixels = 16f;

		// How long the button has to be down before a press stops being a click and becomes petting.
		private const float PetHoldSeconds = .35f;

		// How long the second press of a double click has to be held before the pet is picked up.
		private const float PickupHoldSeconds = .25f;

		private bool _pressed;
		private bool _pressStartedOnPet;
		private Vector2Int _pressOrigin;
		private Vector2Int _lastPosition;
		private float _pressTravel;
		private bool _hasLastPosition;

		private double _pressTime;
		private double _lastClickTime;

		// Whether this press is the second half of a double click, and whether it has already petted.
		private bool _pressIsSecondClick;
		private bool _petted;

		public bool PickupEnabled { get; set; }

		public bool IsDragging { get; private set; }

		// True for as long as the pet is being petted.
		public bool IsPetting { get; private set; }

		public bool IsPressOnPet => _pressed && _pressStartedOnPet;

		public event Action Clicked;

		public event Action DoubleClicked;

		public void Reset()
		{
			_pressed = false;
			IsDragging = false;
			_pressStartedOnPet = false;
			_pressTravel = 0f;
			_hasLastPosition = false;
			_pressIsSecondClick = false;
			_petted = false;
			IsPetting = false;
		}

		public void Sample(PetPointerState pointer, double now, double doubleClickSeconds)
		{
			float moved = _hasLastPosition ? Vector2Int.Distance(_lastPosition, pointer.DesktopPosition) : 0f;

			_lastPosition = pointer.DesktopPosition;
			_hasLastPosition = true;

			if (pointer.LeftDown && !_pressed)
			{
				BeginPress(pointer, now, doubleClickSeconds);
			}
			else if (pointer.LeftDown)
			{
				ContinuePress(pointer, moved, now);
			}
			else if (_pressed)
			{
				EndPress(pointer, now);
			}
		}

		private void BeginPress(PetPointerState pointer, double now, double doubleClickSeconds)
		{
			_pressed = true;

			// The only place this is decided.
			_pressStartedOnPet = pointer.IsOverPet;
			_pressOrigin = pointer.DesktopPosition;
			_pressTravel = 0f;
			_pressTime = now;
			_petted = false;

			// Recorded as the button goes down.
			_pressIsSecondClick = now - _lastClickTime <= doubleClickSeconds;
		}

		private void ContinuePress(PetPointerState pointer, float moved, double now)
		{
			if (!_pressStartedOnPet)
			{
				return;
			}

			_pressTravel += moved;
			double held = now - _pressTime;

			// A pet with no pickup pose, or with pickup turned off, falls through to petting instead.
			if (_pressIsSecondClick && PickupEnabled && held >= PickupHoldSeconds)
			{
				IsDragging = true;
			}

			// Stops the moment the cursor leaves the pet and picks up again when it comes back.
			IsPetting = !IsDragging && pointer.IsOverPet && held >= PetHoldSeconds;
			if (IsPetting) _petted = true;
		}

		private void EndPress(PetPointerState pointer, double now)
		{
			bool wasDragging = IsDragging;

			IsDragging = false;
			IsPetting = false;
			_pressed = false;

			if (wasDragging || !_pressStartedOnPet)
			{
				return;
			}

			// A press that petted was a pet, not a click.
			if (_petted)
			{
				_lastClickTime = 0d;
				return;
			}

			bool stayedPut = Vector2Int.Distance(_pressOrigin, pointer.DesktopPosition) <= ClickSlopPixels &&
				_pressTravel <= ClickTravelPixels;

			if (!stayedPut)
			{
				return;
			}

			if (_pressIsSecondClick)
			{
				_lastClickTime = 0d;
				DoubleClicked?.Invoke();
				return;
			}

			_lastClickTime = now;
			Clicked?.Invoke();
		}
	}
}
