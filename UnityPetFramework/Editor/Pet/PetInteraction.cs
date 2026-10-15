using System;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal sealed class PetInteraction
	{
		private const float ClickSlopPixels = 6f;
		private const float ClickTravelPixels = 16f;

		private const float PetHoldSeconds = .35f;

		private const float PickupHoldSeconds = .25f;

		private bool _pressed;
		private bool _pressStartedOnPet;
		private Vector2Int _pressOrigin;
		private Vector2Int _lastPosition;
		private float _pressTravel;
		private bool _hasLastPosition;

		private double _pressTime;
		private double _lastClickTime;

		private bool _pressIsSecondClick;
		private bool _petted;

		public bool PickupEnabled { get; set; }

		public bool IsDragging { get; private set; }

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

			_pressStartedOnPet = pointer.IsOverPet;
			_pressOrigin = pointer.DesktopPosition;
			_pressTravel = 0f;
			_pressTime = now;
			_petted = false;

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

			if (_pressIsSecondClick && PickupEnabled && held >= PickupHoldSeconds)
			{
				IsDragging = true;
			}

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
