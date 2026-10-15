using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal sealed class PetAnimator
	{
		private const int MaxFramesPerStep = 240;

		private readonly Queue<string> _queue = new Queue<string>();

		private PetDefinition _definition;
		private PetClipDefinition _clip;

		private int _frame;
		private float _time;
		private bool _held;

		private bool _reverse;

		public event Action<string> ClipFinished;

		public float Speed { get; set; } = 1f;

		public string CurrentClipName => _clip?.name;

		public bool IsPlaying => _clip != null && !_held;

		public int QueuedCount => _queue.Count;

		public IEnumerable<string> QueuedNames => _queue;

		public int CurrentFrameIndex => _clip == null ? -1 : _frame;

		public int CurrentFrameCount => _clip?.FrameCount ?? 0;

		public float CurrentFps => _clip?.fps ?? 0f;

		public PetClipDefinition CurrentClip => _clip;

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
					_time = 0f;
					break;
				}
			}

			if (advanced >= MaxFramesPerStep)
			{
				_time = 0f;
			}
		}

		private bool Advance()
		{
			_frame += _reverse ? -1 : 1;

			if (_reverse ? _frame >= 0 : _frame < _clip.FrameCount)
			{
				return true;
			}

			PetClipDefinition finished = _clip;

			ClipFinished?.Invoke(finished.name);

			if (!ReferenceEquals(finished, _clip))
			{
				return _clip != null && !_held;
			}

			if (_queue.Count > 0)
			{
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

			// Hold the last frame for one-shot clips.
			_frame = _reverse ? 0 : finished.FrameCount - 1;
			_held = true;
			return false;
		}
	}
}
