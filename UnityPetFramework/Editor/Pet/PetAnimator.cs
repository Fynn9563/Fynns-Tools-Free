using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Plays a pet's clips, one at a time, with a queue behind them.
	internal sealed class PetAnimator
	{
		// A clip cannot advance more than this many frames in one step.
		private const int MaxFramesPerStep = 240;

		private readonly Queue<string> _queue = new Queue<string>();

		private PetDefinition _definition;
		private PetClipDefinition _clip;

		private int _frame;
		private float _time;
		private bool _held;

		// Plays the frames back to front.
		private bool _reverse;

		// Raised when a clip reaches its end, which for a looping clip means the end of one cycle.
		public event Action<string> ClipFinished;

		public float Speed { get; set; } = 1f;

		public string CurrentClipName => _clip?.name;

		public bool IsPlaying => _clip != null && !_held;

		public int QueuedCount => _queue.Count;

		public IEnumerable<string> QueuedNames => _queue;

		// Where we are within the clip, counting from 0.
		public int CurrentFrameIndex => _clip == null ? -1 : _frame;

		public int CurrentFrameCount => _clip?.FrameCount ?? 0;

		public float CurrentFps => _clip?.fps ?? 0f;

		// The animation playing right now.
		public PetClipDefinition CurrentClip => _clip;

		// The frame on screen right now: which cell, and whether the creator mirrored it by hand.
		public PetFrameRef CurrentFrame
		{
			get
			{
				if (_clip == null || _frame < 0 || _frame >= _clip.FrameCount)
				{
					return new PetFrameRef(-1, false);
				}

				return _clip.frames[_frame];
			}
		}

		public int CurrentCell => CurrentFrame.cell;

		public void SetDefinition(PetDefinition definition)
		{
			_definition = definition;
			Stop();
		}

		// Returns false when the pet has no clip by that name.
		public bool Play(string clipName, bool restartIfSame = false, bool reverse = false)
		{
			PetClipDefinition clip = _definition?.FindClip(clipName);
			if (clip == null || clip.FrameCount == 0)
			{
				return false;
			}

			if (!restartIfSame && ReferenceEquals(clip, _clip) && _reverse == reverse && !_held)
			{
				return true;
			}

			_clip = clip;
			_reverse = reverse;
			_frame = reverse ? clip.FrameCount - 1 : 0;
			_time = 0f;
			_held = false;
			return true;
		}

		public void Queue(string clipName)
		{
			if (_definition?.FindClip(clipName) == null)
			{
				return;
			}

			// Queuing with nothing playing starts immediately.
			if (_clip == null)
			{
				Play(clipName, true);
				return;
			}

			_queue.Enqueue(clipName);
		}

		public void ClearQueue()
		{
			_queue.Clear();
		}

		public void Stop()
		{
			_queue.Clear();
			_clip = null;
			_frame = 0;
			_time = 0f;
			_held = false;
		}

		public void Step(float deltaTime)
		{
			if (_clip == null || _held || deltaTime <= 0f)
			{
				return;
			}

			float fps = _clip.fps * Mathf.Max(0.01f, Speed);
			if (fps <= 0f)
			{
				return;
			}

			float frameDuration = 1f / fps;
			_time += deltaTime;

			int advanced = 0;
			while (_time >= frameDuration && advanced < MaxFramesPerStep)
			{
				_time -= frameDuration;
				advanced++;

				if (!Advance())
				{
					// The clip ended and nothing replaced it, so there is no more time to spend.
					_time = 0f;
					break;
				}
			}

			if (advanced >= MaxFramesPerStep)
			{
				_time = 0f;
			}
		}

		// Moves on one frame.
		private bool Advance()
		{
			_frame += _reverse ? -1 : 1;

			if (_reverse ? _frame >= 0 : _frame < _clip.FrameCount)
			{
				return true;
			}

			PetClipDefinition finished = _clip;

			// Fired before deciding what comes next.
			ClipFinished?.Invoke(finished.name);

			// A listener is allowed to take over completely by calling Play or Stop.
			if (!ReferenceEquals(finished, _clip))
			{
				return _clip != null && !_held;
			}

			if (_queue.Count > 0)
			{
				// Play only fails if the pet was swapped mid-chain.
				if (Play(_queue.Dequeue(), true))
				{
					return true;
				}

				_clip = null;
				return false;
			}

			if (finished.loop)
			{
				_frame = _reverse ? finished.FrameCount - 1 : 0;
				return true;
			}

			// A clip that plays once holds its last frame rather than blanking.
			_frame = _reverse ? 0 : finished.FrameCount - 1;
			_held = true;
			return false;
		}
	}
}
