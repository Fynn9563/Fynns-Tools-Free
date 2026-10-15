using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal enum PetState
	{
		Idle,
		Walk,
		Turn,
		Jump,
		Celebrate,
		Think,
		Sleep,
		Alert,
		Happy,
		Failure
	}

	internal enum PetFacing
	{
		Left,
		Right
	}

	internal sealed class PetBrain
	{
		private const float MinIdleSeconds = 1.5f;
		private const float MaxIdleSeconds = 6f;
		private const float MinWalkSeconds = 1.2f;
		private const float MaxWalkSeconds = 5f;
		private const float JumpChance = 0.12f;
		private const float ThinkChance = 0.25f;

		private const string RestBehaviour = "Rest";
		private const float RestChance = 0.18f;
		private const float AlternateIdleChance = 0.25f;
		private const float RestLengthening = 2.5f;

		private const float MinSleepTurnSeconds = 12f;
		private const float MaxSleepTurnSeconds = 40f;

		private readonly PetAnimator _animator;
		private PetDefinition _definition;
		private string _reactionBehaviour;

		private bool _sustained;
		private string _idleBehaviour = "Idle";
		public bool Suspended { get; set; }
		public void SetDefinition(PetDefinition definition) { _definition = definition; Suspended = false; }
		public bool SustainReaction(string behaviour)
		{
			if (Suspended || !CanInterrupt(_state)) return false;

			if (_state == PetState.Happy && _reactionBehaviour == behaviour)
			{
				_sustained = true;
				return true;
			}

			Enter(PetState.Happy);
			_reactionBehaviour = behaviour;
			_sustained = true;
			return true;
		}

		// Finish the current animation cycle before changing state.
		public void ReleaseReaction()
		{
			_sustained = false;
		}

		public bool TryReactBehaviour(string behaviour)
		{
			if (Suspended || !CanInterrupt(_state)) return false;
			if (_state == PetState.Happy && _reactionBehaviour == behaviour) return false;

			Enter(PetState.Happy);
			_reactionBehaviour = behaviour;
			return true;
		}

		private PetState _state = PetState.Idle;
		private PetFacing _facing = PetFacing.Right;

		private PetFacing _turnTarget = PetFacing.Right;

		private bool _sitting;

		private float _stateTimer;
		private float _sleepTurnTimer;

		private bool _hasSittingTurn;

		public PetBrain(PetAnimator animator)
		{
			_animator = animator;
			_animator.ClipFinished += OnClipFinished;
		}

		public PetState State => _state;

		public PetFacing Facing => _facing;

		public bool IsSitting => _sitting;

		public bool IsWalking => _state == PetState.Walk;

		public bool IsAsleep => _state == PetState.Sleep;

		public bool IsJumping => _state == PetState.Jump;

		public bool IsActive => _state == PetState.Walk || _state == PetState.Jump || _state == PetState.Celebrate;

		public bool IsBusy => !CanInterrupt(_state);

		public void Detach()
		{
			_animator.ClipFinished -= OnClipFinished;
		}

		public void Reset(PetFacing facing, bool hasSittingTurn)
		{
			_hasSittingTurn = hasSittingTurn;
			_facing = facing;
			_turnTarget = facing;
			_sitting = false;
			Enter(PetState.Idle);
		}

		public void Step(float deltaTime, bool wantsSleep)
		{
			if (Suspended) return;
			_stateTimer -= deltaTime;

			if (wantsSleep && CanInterrupt(_state) && _state != PetState.Sleep)
			{
				Enter(PetState.Sleep);
				return;
			}

			if (!wantsSleep && _state == PetState.Sleep)
			{
				Enter(PetState.Idle);
				return;
			}

			if (_state == PetState.Sleep)
			{
				StepSleep(deltaTime);
				return;
			}

			if (_stateTimer > 0f || !CanInterrupt(_state))
			{
				return;
			}

			ChooseIdleBehaviour();
		}

		public bool RequestFacing(PetFacing facing)
		{
			if (_facing == facing || _state == PetState.Turn)
			{
				return false;
			}

			if (Suspended || !CanInterrupt(_state))
			{
				return false;
			}

			if (!_definition.HasClip(TurnAnimation()))
			{
				_facing = facing;
				return true;
			}
			_turnTarget = facing;
			Enter(PetState.Turn);
			return true;
		}

		public bool TryReact(PetState reaction)
		{
			if (Suspended || !CanInterrupt(_state))
			{
				return false;
			}

			if (_state == reaction)
			{
				return false;
			}

			Enter(reaction);
			return true;
		}

		public string ClipForCurrentState(out bool reverse)
		{
			reverse = false;

			switch (_state)
			{
				case PetState.Turn:
					reverse = _turnTarget == PetFacing.Left;
					return TurnAnimation();

				case PetState.Walk: return _definition.ResolveBehaviour("Move");
				case PetState.Jump: return _definition.ResolveBehaviour("Jump");
				case PetState.Celebrate: return _definition.ResolveBehaviour("Success");
				case PetState.Think: return _definition.ResolveBehaviour("Confused");
				case PetState.Sleep: return _definition.ResolveBehaviour("Sleep");
				case PetState.Alert: return _definition.ResolveBehaviour("Startled");
				case PetState.Happy: return _definition.ResolveBehaviour(_reactionBehaviour ?? "Click");
				default: return _definition.ResolveBehaviour(_idleBehaviour);
			}
		}

		private string TurnAnimation()
		{
			return _definition.ResolveBehaviour(_sitting ? "Turn sitting" : "Turn");
		}

		private void StepSleep(float deltaTime)
		{
			if (!_hasSittingTurn)
			{
				return;
			}

			_sleepTurnTimer -= deltaTime;
			if (_sleepTurnTimer > 0f)
			{
				return;
			}

			_sleepTurnTimer = Random.Range(MinSleepTurnSeconds, MaxSleepTurnSeconds);
			RequestFacing(_facing == PetFacing.Left ? PetFacing.Right : PetFacing.Left);
		}

		private static bool CanInterrupt(PetState state)
		{
			return state != PetState.Turn && state != PetState.Jump;
		}

		private void ChooseIdleBehaviour()
		{
			float idleBias = UnityPetFrameworkSettings.IdleFrequency;
			float rare = Random.value;
			string optional = rare < .01f ? "Special" : rare < .02f ? "Joke" : rare < .05f ? "Eat" : rare < .10f ? "Playful" : null;
			if (optional != null && _definition.HasClip(_definition.ResolveBehaviour(optional))) { TryReactBehaviour(optional); return; }

			if (Random.value < idleBias)
			{
				Enter(Random.value < ThinkChance ? PetState.Think : PetState.Idle);
				return;
			}

			if (Random.value < JumpChance && _definition.HasClip(_definition.ResolveBehaviour("Jump")))
			{
				Enter(PetState.Jump);
				return;
			}

			Enter(_definition.HasClip(_definition.ResolveBehaviour("Move")) ? PetState.Walk : PetState.Idle);
		}

		private string ChooseIdlePose()
		{
			float roll = Random.value;

			if (roll < RestChance && _definition.HasClip(_definition.ResolveBehaviour(RestBehaviour)))
			{
				return RestBehaviour;
			}

			if (roll < RestChance + AlternateIdleChance && _definition.HasClip(_definition.ResolveBehaviour("Alternate idle")))
			{
				return "Alternate idle";
			}

			return "Idle";
		}

		private void Enter(PetState state)
		{
			_state = state;
			_reactionBehaviour = null;
			_sustained = false;
			if (state == PetState.Idle)
			{
				_idleBehaviour = ChooseIdlePose();
			}

			switch (state)
			{
				case PetState.Sleep:
					_sitting = true;
					_sleepTurnTimer = Random.Range(MinSleepTurnSeconds, MaxSleepTurnSeconds);
					break;

				case PetState.Turn:
					break;

				case PetState.Idle:
					_sitting = _idleBehaviour == RestBehaviour;
					break;

				default:
					_sitting = false;
					break;
			}

			switch (state)
			{
				case PetState.Walk:
					_stateTimer = Random.Range(MinWalkSeconds, MaxWalkSeconds);
					break;

				case PetState.Idle:
				case PetState.Think:
					_stateTimer = Random.Range(MinIdleSeconds, MaxIdleSeconds) *
						(_idleBehaviour == RestBehaviour ? RestLengthening : 1f);
					break;

				default:
					_stateTimer = float.MaxValue;
					break;
			}
		}

		private void OnClipFinished(string clipName)
		{
			if (Suspended) return;
			switch (_state)
			{
				case PetState.Turn:
					// Change facing after the turn animation finishes.
					_facing = _turnTarget;

					Enter(_sitting ? PetState.Sleep : PetState.Idle);
					_stateTimer = 0f;
					break;

				case PetState.Jump:
					Enter(PetState.Idle);
					break;

				case PetState.Celebrate:
				case PetState.Alert:
				case PetState.Happy:
					if (_sustained && _reactionBehaviour != null &&
						_animator.Play(_definition.ResolveBehaviour(_reactionBehaviour), true)) return;

					PetClipDefinition finished = _definition.FindClip(clipName);

					if (finished != null && !finished.loop && finished.holdSeconds > 0f)
					{
						_stateTimer = finished.holdSeconds;
						return;
					}

					Enter(PetState.Idle);
					break;
			}
		}
	}
}
