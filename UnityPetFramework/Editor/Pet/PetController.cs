using System;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal sealed class PetController : IDisposable
	{
		private const float PettingEnergyPerSecond = 0.12f;

		private const double AnnoyanceMemorySeconds = 2d;
		private const int AnnoyanceClicks = 4;

		private const double AnnoyedLockSeconds = 3d;
		private const float CelebrationEnergy = 0.05f;

		private const int ArrivalSlopPixels = 4;

		private readonly PetAnimator _animator = new PetAnimator();
		private readonly PetFrameBaker _baker = new PetFrameBaker();
		private readonly PetSafeArea _safeArea = new PetSafeArea();
		private readonly PetMotor _motor = new PetMotor();
		private readonly PetEnergy _energy = new PetEnergy();
		private readonly PetInteraction _interaction = new PetInteraction();
		private readonly PetBrain _brain;

		private PetAtlas _atlas;
		private PetDefinition _definition;
		private string _currentClip;
		private bool _currentReverse;

		private PetDestination _destination;

		private bool _moveAfterReaction;

		private float _jumpProgress;
		private bool _jumpBlocked;
		private int _jumpLift;
		private bool _carrying, _lowEnergyNotified;
		private Vector2Int _grabOffset;
		private double _now, _lastClick;
		private int _annoyance;
		private double _annoyedUntil;

		public PetController()
		{
			_brain = new PetBrain(_animator);

			_interaction.Clicked += OnClicked;
			_interaction.DoubleClicked += OnDoubleClicked;
			PetEditorEvents.Reaction += OnEditorReaction;
		}

		public Vector2Int FrameSize => _baker.FrameSize;

		public byte[] CurrentFrame { get; private set; }

		public Vector2Int TopLeft => new Vector2Int(_motor.Feet.x - FrameSize.x / 2, _motor.Feet.y - _baker.GroundOffset - _jumpLift);

		public float Energy => _energy.Value;

		public bool IsReady => _baker.IsReady && _definition != null;

		public PetFacing Facing => _brain.Facing;

		public Vector2Int Feet => _motor.Feet;

		public void Dispose()
		{
			_interaction.Clicked -= OnClicked;
			_interaction.DoubleClicked -= OnDoubleClicked;
			PetEditorEvents.Reaction -= OnEditorReaction;
			_brain.Detach();

			_atlas?.Dispose();
			_atlas = null;
		}

		public void SetPet(PetAtlas atlas, PetDefinition definition, float scale)
		{
			if (!ReferenceEquals(_atlas, atlas))
			{
				_atlas?.Dispose();
			}

			_atlas = atlas;
			_definition = definition;

			_animator.SetDefinition(definition);
			_baker.SetPet(atlas, definition, scale);
			_energy.Load(UnityPetFrameworkSettings.Energy);
			_brain.SetDefinition(definition);
			_brain.Reset(PetFacing.Right, definition.HasClip(definition.ResolveBehaviour("Turn sitting")));
			_interaction.Reset(); _carrying = false; _jumpLift = 0;
			_lowEnergyNotified = false; _annoyance = 0; _annoyedUntil = 0; _lastClick = double.NegativeInfinity;

			_destination = default;
			_moveAfterReaction = false;
			_currentClip = null;
			CurrentFrame = null;
		}

		public void Place(Vector2Int feet)
		{
			_motor.Place(feet);
		}

		public void Tick(float deltaTime, PetPointerState pointer, IPetHost host, double now, double doubleClickSeconds)
		{
			if (!IsReady)
			{
				return;
			}

			_animator.Speed = UnityPetFrameworkSettings.AnimationSpeed;

			_now = now;
			_interaction.PickupEnabled = _definition.allowPickup && _definition.HasClip(_definition.ResolveBehaviour("Pickup"));
			if (pointer.LeftDown && !_interaction.IsPressOnPet && pointer.IsOverPet) _grabOffset = _motor.Feet - pointer.DesktopPosition;
			_interaction.Sample(pointer, now, doubleClickSeconds);

			_safeArea.Tick(host, UnityPetFrameworkSettings.MovementMode, now, host.BoundsChangedThisTick);

			if (_safeArea.ChangedThisTick)
			{
				_motor.Settle(_safeArea, _baker.FrameSize);

				if (!_safeArea.IsDestinationValid(_destination, _baker.FrameSize))
				{
					_destination = default;
				}
			}

			if (_interaction.IsDragging)
			{
				_carrying = true; _brain.Suspended = true; _jumpLift = 0;
				_destination = default; _moveAfterReaction = false;
				_motor.PlaceDropped(_safeArea.ClampPlacement(pointer.DesktopPosition + _grabOffset, FrameSize));
				_animator.Play(_definition.ResolveBehaviour("Pickup"));
				_animator.Step(deltaTime);
				CurrentFrame = _baker.Bake(_animator.CurrentFrame, _animator.CurrentClip, _brain.Facing);
				return;
			}
			if (_carrying)
			{
				_carrying = false; _brain.Suspended = false;
				_motor.PlaceDropped(_safeArea.ClampPlacement(pointer.DesktopPosition + _grabOffset, FrameSize));
				_brain.Reset(_brain.Facing, _definition.HasClip(_definition.ResolveBehaviour("Turn sitting"))); _currentClip = null;
			}
			StepPetting(deltaTime);
			_energy.Step(deltaTime, _brain.IsAsleep, _brain.IsActive);
			UnityPetFrameworkSettings.Energy = _energy.Value;

			if (UnityPetFrameworkSettings.RunAwayFromMouse && pointer.IsOverPet && !_interaction.IsPressOnPet)
			{
				FleeFrom(pointer.DesktopPosition);
			}

			if (!_interaction.IsPressOnPet)
			{
				if (_energy.Value < .25f && !_lowEnergyNotified && _definition.HasClip(_definition.ResolveBehaviour("Low energy")))
				{ _lowEnergyNotified = _brain.TryReactBehaviour("Low energy"); }
				else _brain.Step(deltaTime, _energy.WantsSleep);
				if (_energy.Value > .4f) _lowEnergyNotified = false;
			}

			if (!_safeArea.IsEmpty && !_interaction.IsPressOnPet)
			{
				StepMovement(deltaTime);
			}

			SyncAnimation();
			_animator.Step(deltaTime);

			CurrentFrame = _baker.Bake(_animator.CurrentFrame, _animator.CurrentClip, _brain.Facing);
		}

		public void ResetInteraction()
		{
			_interaction.Reset();
			_carrying = false; _brain.Suspended = false; _currentClip = null;
		}

		public void RefreshSafeArea(IPetHost host)
		{
			_safeArea.Refresh(host, UnityPetFrameworkSettings.MovementMode);
			_motor.Settle(_safeArea, _baker.FrameSize);
		}

		private void StepMovement(float deltaTime)
		{
			if (_brain.IsJumping)
			{
				StepJump(deltaTime);
				return;
			}

			if (_brain.IsBusy || _brain.State == PetState.Alert || _brain.State == PetState.Happy ||
				_brain.State == PetState.Celebrate || _brain.State == PetState.Sleep)
			{
				return;
			}

			if (_moveAfterReaction)
			{
				_moveAfterReaction = false;
				if (_safeArea.TryPickDestination(_motor.Feet, _baker.FrameSize, out PetDestination picked))
				{
					_destination = picked;
				}
			}

			if (_destination.IsSet)
			{
				WalkTowardsDestination(deltaTime);
				return;
			}

			if (!_brain.IsWalking || !_definition.HasClip(_definition.ResolveBehaviour("Move")))
			{
				return;
			}

			if (_motor.Walk(deltaTime, _brain.Facing, UnityPetFrameworkSettings.Scale, _safeArea, _baker.FrameSize))
			{
				_brain.RequestFacing(_brain.Facing == PetFacing.Left ? PetFacing.Right : PetFacing.Left);
			}
		}

		private void StepJump(float deltaTime)
		{
			int frames = Mathf.Max(1, _animator.CurrentFrameCount);
			float fps = Mathf.Max(0.01f, _animator.CurrentFps * _animator.Speed);
			float duration = frames / fps;

			float previous = _jumpProgress;
			_jumpProgress = Mathf.Clamp01(_jumpProgress + deltaTime / duration);
			_jumpLift = _definition.physicalJump ? Mathf.RoundToInt(Mathf.Sin(_jumpProgress * Mathf.PI) * FrameSize.y * .5f) : 0;

			if (_jumpBlocked)
			{
				return;
			}
			if (!_motor.JumpStep(_jumpProgress - previous, _brain.Facing, UnityPetFrameworkSettings.Scale, _safeArea, _baker.FrameSize))
			{
				_jumpBlocked = true;
			}
		}

		private void WalkTowardsDestination(float deltaTime)
		{
			if (!_definition.HasClip(_definition.ResolveBehaviour("Move"))) { _destination = default; return; }
			int distance = _destination.Feet.x - _motor.Feet.x;
			if (Mathf.Abs(distance) <= ArrivalSlopPixels)
			{
				_destination = default;
				return;
			}

			PetFacing needed = distance < 0 ? PetFacing.Left : PetFacing.Right;
			if (_brain.Facing != needed)
			{
				_brain.RequestFacing(needed);
				return;
			}

			if (_brain.State != PetState.Walk)
			{
				_brain.TryReact(PetState.Walk);
			}

			if (_motor.Walk(deltaTime, needed, UnityPetFrameworkSettings.Scale, _safeArea, _baker.FrameSize))
			{
				_destination = default;
			}
		}

		private void SyncAnimation()
		{
			string wanted = _brain.ClipForCurrentState(out bool reverse);
			if (!_brain.IsJumping) _jumpLift = 0;
			if (!_definition.HasClip(wanted))
			{
				wanted = _definition.ResolveBehaviour("Idle");
				reverse = false;
			}

			if (wanted == _currentClip && reverse == _currentReverse && _animator.CurrentClipName == wanted)
			{
				return;
			}

			if (!_animator.Play(wanted, true, reverse))
			{
				return;
			}

			_currentClip = wanted;
			_currentReverse = reverse;

			if (_brain.IsJumping)
			{
				_jumpProgress = 0f;
				_jumpBlocked = false;
			}

			PetClipDefinition clip = _definition.FindClip(wanted);
			if (clip != null && !string.IsNullOrEmpty(clip.sound))
			{
				PetAudio.Play(clip.sound);
			}
		}

		private void FleeFrom(Vector2Int cursor)
		{
			if (_destination.IsSet || _brain.IsBusy)
			{
				return;
			}

			if (_safeArea.TryPickAwayFrom(_motor.Feet, cursor.x, _baker.FrameSize, out PetDestination away))
			{
				_destination = away;
			}
		}

		private bool Prodded()
		{
			_annoyance = _now - _lastClick < AnnoyanceMemorySeconds ? _annoyance + 1 : 1;
			_lastClick = _now;

			if (_now < _annoyedUntil) return true;

			if (_annoyance < AnnoyanceClicks) return false;

			if (!_definition.HasClip(_definition.ResolveBehaviour("Annoyed"))) return false;

			if (_brain.TryReactBehaviour("Annoyed")) _annoyedUntil = _now + AnnoyedLockSeconds;

			return true;
		}

		private void OnClicked()
		{
			if (Prodded()) return;

			_brain.TryReactBehaviour("Click");
		}

		private void OnDoubleClicked()
		{
			if (Prodded()) return;

			if (_brain.TryReact(PetState.Alert))
			{
				_destination = default;
				_moveAfterReaction = true;
			}
		}

		private void StepPetting(float deltaTime)
		{
			if (!_interaction.IsPetting)
			{
				_brain.ReleaseReaction();
				return;
			}

			_energy.Add(PettingEnergyPerSecond * deltaTime);

			string petting = _definition.HasClip(_definition.ResolveBehaviour("Petting")) ? "Petting" : "Click";
			_brain.SustainReaction(petting);
		}

		private void OnEditorReaction(PetState state)
		{
			if (state == PetState.Celebrate)
			{
				_energy.Add(CelebrationEnergy);
			}

			_brain.TryReactBehaviour(state == PetState.Alert || state == PetState.Failure ? "Failure" : state == PetState.Think ? "Compile" : "Success");
		}
	}
}
