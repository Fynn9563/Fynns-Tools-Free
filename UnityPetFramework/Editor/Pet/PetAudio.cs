using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Plays a pet's sounds.
	internal static class PetAudio
	{
		private sealed class Entry
		{
			public AudioClip Clip;
			public float[] Samples;
			public int Channels;
			public int SampleRate;
			public float AppliedVolume = -1f;
		}

		private static readonly Dictionary<string, Entry> _clips = new Dictionary<string, Entry>();

		private static bool _probed;
		private static MethodInfo _play;
		private static MethodInfo _stop;
		private static int _playArgumentCount;

		public static bool IsAvailable
		{
			get
			{
				Probe();
				return _play != null;
			}
		}

		// Loaded from bytes.
		public static void Register(string key, byte[] wavBytes)
		{
			if (string.IsNullOrEmpty(key) || wavBytes == null)
			{
				return;
			}

			if (!WavReader.TryRead(wavBytes, key, out AudioClip clip, out float[] samples, out int channels, out int sampleRate, out string error))
			{
				Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix + "A pet sound could not be loaded (" + key + "): " + error);
				return;
			}

			Release(key);
			_clips[key] = new Entry
			{
				Clip = clip,
				Samples = samples,
				Channels = channels,
				SampleRate = sampleRate
			};
		}

		public static void Play(string key)
		{
			if (!UnityPetFrameworkSettings.SoundEnabled || string.IsNullOrEmpty(key))
			{
				return;
			}

			float volume = UnityPetFrameworkSettings.SoundVolume;
			if (volume <= 0f)
			{
				// Nought per cent means silent.
				return;
			}

			Probe();
			if (_play == null || !_clips.TryGetValue(key, out Entry entry) || entry.Clip == null)
			{
				return;
			}

			ApplyVolume(entry, volume);
			AudioClip clip = entry.Clip;

			try
			{
				// Built from the signature that was actually found, rather than assuming one.
				object[] arguments;
				switch (_playArgumentCount)
				{
					case 1: arguments = new object[] { clip }; break;
					case 3: arguments = new object[] { clip, 0, false }; break;
					default: arguments = new object[] { clip, 0, false, false }; break;
				}

				_play.Invoke(null, arguments);
			}
			catch (Exception)
			{
				// A signature that no longer matches.
				_play = null;
			}
		}

		public static void StopAll()
		{
			Probe();

			try
			{
				_stop?.Invoke(null, null);
			}
			catch (Exception)
			{
				_stop = null;
			}
		}

		public static void ReleaseAll()
		{
			foreach (KeyValuePair<string, Entry> entry in _clips)
			{
				if (entry.Value?.Clip != null)
				{
					UnityEngine.Object.DestroyImmediate(entry.Value.Clip, true);
				}
			}

			_clips.Clear();
		}

		// Rewrites the clip's samples at the requested level.
		private static void ApplyVolume(Entry entry, float volume)
		{
			if (Mathf.Approximately(entry.AppliedVolume, volume) || entry.Samples == null)
			{
				return;
			}

			float[] scaled = new float[entry.Samples.Length];
			for (int i = 0; i < scaled.Length; i++)
			{
				scaled[i] = entry.Samples[i] * volume;
			}

			try
			{
				entry.Clip.SetData(scaled, 0);
				entry.AppliedVolume = volume;
			}
			catch (Exception)
			{
				// A clip Unity will not let us write to.
				entry.AppliedVolume = volume;
			}
		}

		private static void Release(string key)
		{
			if (_clips.TryGetValue(key, out Entry existing) && existing?.Clip != null)
			{
				UnityEngine.Object.DestroyImmediate(existing.Clip, true);
			}

			_clips.Remove(key);
		}

		private static void Probe()
		{
			if (_probed)
			{
				return;
			}

			_probed = true;

			Type audioUtil = typeof(EditorWindow).Assembly.GetType("UnityEditor.AudioUtil");
			if (audioUtil == null)
			{
				return;
			}

			const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			// 2020 and later call it PlayPreviewClip and take an extra loop flag.
			_play = audioUtil.GetMethod("PlayPreviewClip", flags, null,
				new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);

			if (_play == null)
			{
				_play = audioUtil.GetMethod("PlayClip", flags, null,
					new[] { typeof(AudioClip), typeof(int), typeof(bool), typeof(bool) }, null)
					?? audioUtil.GetMethod("PlayClip", flags, null, new[] { typeof(AudioClip) }, null);
			}

			_playArgumentCount = _play?.GetParameters().Length ?? 0;

			_stop = audioUtil.GetMethod("StopAllPreviewClips", flags)
				?? audioUtil.GetMethod("StopAllClips", flags);
		}
	}
}
