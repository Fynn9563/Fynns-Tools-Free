using System;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Reads a WAV file into an AudioClip.
	internal static class WavReader
	{
		private const int MaxChannels = 2;
		private const int MinSampleRate = 4000;
		private const int MaxSampleRate = 192000;
		private const int MaxSamplesPerChannel = 48000 * 30;

		public static bool TryRead(byte[] bytes, string clipName, out AudioClip clip, out string error)
		{
			return TryRead(bytes, clipName, out clip, out _, out _, out _, out error);
		}
		public static bool TryRead(byte[] bytes, string clipName, out AudioClip clip,
			out float[] samples, out int channelCount, out int rate, out string error)
		{
			clip = null;
			samples = null;
			channelCount = 0;
			rate = 0;

			if (bytes == null || bytes.Length < 44)
			{
				error = "That sound file is too short to be a WAV.";
				return false;
			}

			if (!Matches(bytes, 0, "RIFF") || !Matches(bytes, 8, "WAVE"))
			{
				error = "That sound file is not a WAV.";
				return false;
			}

			int channels = 0;
			int sampleRate = 0;
			int bitsPerSample = 0;
			int format = 0;
			int dataStart = -1;
			int dataLength = 0;

			// Chunks start after the 12 byte RIFF header.
			int position = 12;
			while (position + 8 <= bytes.Length)
			{
				string chunkId = System.Text.Encoding.ASCII.GetString(bytes, position, 4);
				long chunkSize = BitConverter.ToUInt32(bytes, position + 4);
				int payload = position + 8;

				// The file says how big this chunk is.
				if (chunkSize < 0 || payload + chunkSize > bytes.Length)
				{
					error = "That sound file is damaged: it claims more data than it contains.";
					return false;
				}

				if (chunkId == "fmt " && chunkSize >= 16)
				{
					format = BitConverter.ToUInt16(bytes, payload);
					channels = BitConverter.ToUInt16(bytes, payload + 2);
					sampleRate = BitConverter.ToInt32(bytes, payload + 4);
					bitsPerSample = BitConverter.ToUInt16(bytes, payload + 14);
				}
				else if (chunkId == "data")
				{
					dataStart = payload;
					dataLength = (int)chunkSize;
				}

				position = payload + (int)chunkSize;
				if ((chunkSize & 1) == 1)
				{
					position++;
				}
			}

			if (!Validate(format, channels, sampleRate, bitsPerSample, dataStart, dataLength, out error))
			{
				return false;
			}

			int bytesPerSample = bitsPerSample / 8;
			int totalSamples = dataLength / bytesPerSample;

			if (totalSamples <= 0 || totalSamples / channels > MaxSamplesPerChannel)
			{
				error = "That sound is either empty or longer than a pet sound should be.";
				return false;
			}

			float[] decoded = new float[totalSamples];
			for (int i = 0; i < totalSamples; i++)
			{
				decoded[i] = ReadSample(bytes, dataStart + i * bytesPerSample, bitsPerSample, format);
			}

			try
			{
				clip = AudioClip.Create(clipName, totalSamples / channels, channels, sampleRate, false);
				clip.SetData(decoded, 0);
			}
			catch (Exception exception)
			{
				error = "That sound could not be loaded: " + exception.Message;
				return false;
			}

			samples = decoded;
			channelCount = channels;
			rate = sampleRate;

			error = null;
			return true;
		}

		private static bool Validate(int format, int channels, int sampleRate, int bitsPerSample,
			int dataStart, int dataLength, out string error)
		{
			// 1 is integer PCM, 3 is 32 bit float PCM. Anything else is compressed.
			if (format != 1 && format != 3)
			{
				error = "That WAV is compressed. Pet sounds have to be plain PCM.";
				return false;
			}

			if (channels < 1 || channels > MaxChannels)
			{
				error = "That WAV has " + channels + " channels. Pet sounds can be mono or stereo.";
				return false;
			}

			if (sampleRate < MinSampleRate || sampleRate > MaxSampleRate)
			{
				error = "That WAV has an unusable sample rate of " + sampleRate + ".";
				return false;
			}

			if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24 && bitsPerSample != 32)
			{
				error = "That WAV stores " + bitsPerSample + " bits per sample, which is not supported.";
				return false;
			}

			if (format == 3 && bitsPerSample != 32)
			{
				error = "That WAV claims to be float but is not 32 bit.";
				return false;
			}

			if (dataStart < 0 || dataLength <= 0)
			{
				error = "That WAV has no audio in it.";
				return false;
			}

			error = null;
			return true;
		}

		private static float ReadSample(byte[] bytes, int offset, int bitsPerSample, int format)
		{
			switch (bitsPerSample)
			{
				case 8:
					// Eight bit WAV is unsigned, with 128 as silence.
					return (bytes[offset] - 128) / 128f;

				case 16:
					return BitConverter.ToInt16(bytes, offset) / 32768f;

				case 24:
					int value = bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16);
					if ((value & 0x800000) != 0)
					{
						value |= unchecked((int)0xFF000000);
					}

					return value / 8388608f;

				case 32:
					return format == 3
						? BitConverter.ToSingle(bytes, offset)
						: BitConverter.ToInt32(bytes, offset) / 2147483648f;

				default:
					return 0f;
			}
		}

		private static bool Matches(byte[] bytes, int offset, string text)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (bytes[offset + i] != text[i])
				{
					return false;
				}
			}

			return true;
		}
	}
}
