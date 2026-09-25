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

	// Decides what the pet does next.
	internal sealed class PetBrain
	{
		private const float MinIdleSeconds = 1.5f;
		private const float MaxIdleSeconds = 6f;
		private const float MinWalkSeconds = 1.2f;
		private const float MaxWalkSeconds = 5f;
		private const float JumpChance = 0.12f;
		private const float ThinkChance = 0.25f;

		// How often a pause is spent sitting, and how much longer a sit lasts than a stand.
		private const string RestBehaviour = "Rest";
		private const float RestChance = 0.18f;
		private const float AlternateIdleChance = 0.25f;
		private const float RestLengthening = 2.5f;

		// How often a sleeping pet rolls over.
		private const float MinSleepTurnSeconds = 12f;
		private const float MaxSleepTurnSeconds = 40f;

		private readonly PetAnimator _animator;
		private PetDefinition _definition;
		private string _reactionBehaviour;

		// Whether the current reaction is being held open by something still going on.
		private bool _sustained;
		private string _idleBehaviour = "Idle";
		public bool Suspended { get; set; }
		public void SetDefinition(PetDefinition definition) { _definition = definition; Suspended = false; }
		// Starts a reaction and keeps it running until released.
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

		// Lets the current cycle finish rather than cutting it off part way.
		public void ReleaseReaction()
		{
			_sustained = false;
		}

		// Starts a reaction, unless the pet is already in the middle of that same one.
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

		// Where the turn currently playing is taking us.
		private PetFacing _turnTarget = PetFacing.Right;

		// Posture, not state.
		private bool _sitting;

		private float _stateTimer;
		private float _sleepTurnTimer;

		// Whether this pet filled in the sitting turn slot.
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

		// Walking and jumping cost energy; standing about does not.
		public bool IsActive => _state == PetState.Walk || _state == PetState.Jump || _state == PetState.Celebrate;

		// True while the pet is doing something that movement must not cut across.
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

			// A one-shot state ends when its animation ends.
			if (_stateTimer > 0f || !CanInterrupt(_state))
			{
				return;
			}

			ChooseIdleBehaviour();
		}

		// Asks to face the other way.
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

			// A pet with no turn animation simply faces the other way.
			if (!_definition.HasClip(TurnAnimation()))
			{
				_facing = facing;
				return true;
			}
			_turnTarget = facing;
			Enter(PetState.Turn);
			return true;
		}

		// Used by clicks and editor events.
		public bool TryReact(PetState reaction)
		{
			if (Suspended || !CanInterrupt(_state))
			{
				return false;
			}

			// Already doing it.
			if (_state == reaction)
			{
				return false;
			}

			Enter(reaction);
			return true;
		}

		// The animation for the state the pet is in, and whether it plays backwards.
		public string ClipForCurrentState(out bool reverse)
		{
			reverse = false;

			switch (_state)
			{
				case PetState.Turn:
					// Forwards turns left to right, backwards turns right to left.
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

		// Whichever turn suits the posture the pet is currently in.
		private string TurnAnimation()
		{
			return _definition.ResolveBehaviour(_sitting ? "Turn sitting" : "Turn");
		}

		// A sleeping pet turns over now and again.
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

		// Turn and Jump run to completion.
		private static bool CanInterrupt(PetState state)
		{
			return state != PetState.Turn && state != PetState.Jump;
		}

		private void ChooseIdleBehaviour()
		{
			float idleBias = UnityPetFrameworkSettings.IdleFrequency;
			// Optional reactions are reachable without requiring them on every pet.
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

		// Which pose a pause is spent in.
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

			// Posture changes here and nowhere else.
			switch (state)
			{
				case PetState.Sleep:
					_sitting = true;
					_sleepTurnTimer = Random.Range(MinSleepTurnSeconds, MaxSleepTurnSeconds);
					break;

				case PetState.Turn:
					break;

				case PetState.Idle:
					// Resting is a sitting idle rather than a state of its own.
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
					// A rest lasts longer than a pause.
					_stateTimer = Random.Range(MinIdleSeconds, MaxIdleSeconds) *
						(_idleBehaviour == RestBehaviour ? RestLengthening : 1f);
					break;

				default:
					// One-shot, or ended by something other than a clock.
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
					// The one place facing actually changes, and only once the sprite has finished turning.
					_facing = _turnTarget;

					// Back to whatever the pet was doing before it turned.
					Enter(_sitting ? PetState.Sleep : PetState.Idle);
					_stateTimer = 0f;
					break;

				case PetState.Jump:
					// Never held.
					Enter(PetState.Idle);
					break;

				case PetState.Celebrate:
				case PetState.Alert:
				case PetState.Happy:
					// Still being petted, so round again.
					if (_sustained && _reactionBehaviour != null &&
						_animator.Play(_definition.ResolveBehaviour(_reactionBehaviour), true)) return;

					PetClipDefinition finished = _definition.FindClip(clipName);

					// A looping clip fires this every cycle.
					if (finished != null && !finished.loop && finished.holdSeconds > 0f)
					{
						// Stays in this state with the animator resting on its last frame.
						_stateTimer = finished.holdSeconds;
						return;
					}

					Enter(PetState.Idle);
					break;
			}
		}
	}
}
