using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

// UnityEngine has a CompressionLevel of its own, for asset bundles, and it is not this one.
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace FynnsTools.UnityPetFramework
{
	// Writes a .fynnpet, and proves the repacked sheet still shows the same pictures before it does.
	internal static class PetPackageWriter
	{
		internal sealed class Report
		{
			public string Path;
			public int CellsBefore;
			public int CellsAfter;
			public long PixelsBefore;
			public long PixelsAfter;
			public long FileBytes;

			public float PixelSaving => PixelsBefore <= 0
				? 0f
				: 1f - PixelsAfter / (float)PixelsBefore;
		}

		public static bool TryWrite(
			string destinationPath,
			PetAtlas atlas,
			PetDefinition definition,
			string petFolder,
			out Report report,
			out string error)
		{
			report = null;

			// Canonical before anything reads it.
			definition.Normalise();

			// Measured again on the way out rather than trusted.
			if (!PetContentBounds.Recalculate(definition, atlas, out error))
			{
				return false;
			}

			if (!PetAtlasRepacker.TryRepack(atlas, definition, out PetAtlasRepacker.Result packed, out error))
			{
				return false;
			}

			// The index remap is the one thing here that fails quietly.
			if (!VerifyRoundTrip(atlas, definition, packed, out error))
			{
				return false;
			}

			try
			{
				WriteArchive(destinationPath, packed, definition, petFolder, PetThumbnailRenderer.Render(atlas, definition));
			}
			catch (Exception exception)
			{
				error = "The pet package could not be written: " + exception.Message;
				return false;
			}

			report = new Report
			{
				Path = destinationPath,
				CellsBefore = packed.CellsBefore,
				CellsAfter = packed.CellsAfter,
				PixelsBefore = packed.PixelsBefore,
				PixelsAfter = packed.PixelsAfter,
				FileBytes = new FileInfo(destinationPath).Length
			};

			error = null;
			return true;
		}

		// Every frame of every animation.
		internal static bool VerifyRoundTrip(
			PetAtlas source,
			PetDefinition sourceDefinition,
			PetAtlasRepacker.Result packed,
			out string error)
		{
			PetAtlas check = PetAtlas.FromBytes(packed.AtlasPng, packed.Definition, out error);
			if (check == null)
			{
				error = "The packed sheet could not be read back: " + error;
				return false;
			}

			try
			{
				if (sourceDefinition.HasRectangles)
				{
					foreach (var original in sourceDefinition.AllSequences())
					{
						var rewritten = packed.Definition.FindClip(original.name);
						if (!SameSettings(original, rewritten, out string difference))
						{
							error = "Packing changed " + difference + " on " + original.name + ".";
							return false;
						}
						for (int i = 0; i < original.FrameCount; i++)
						{
							var a = sourceDefinition.spriteFrames[original.frames[i].cell];
							var b = packed.Definition.spriteFrames[rewritten.frames[i].cell];
							if (a.width != b.width || a.height != b.height || !Near(a.pivotX, b.pivotX) || !Near(a.pivotY, b.pivotY) || original.frames[i].flip != rewritten.frames[i].flip ||
								!SameFrame(source, original.frames[i].cell, new RectInt(0, 0, a.width, a.height), check, rewritten.frames[i].cell))
							{ error = "Packing changed pixels or anchors in " + original.name + "."; return false; }
						}
					}
					error = null;
					return true;
				}
				RectInt content = new RectInt(
					sourceDefinition.contentLeft,
					sourceDefinition.contentTop,
					packed.Definition.cellWidth,
					packed.Definition.cellHeight);

				for (int clipIndex = 0; clipIndex < sourceDefinition.clips.Count; clipIndex++)
				{
					PetClipDefinition original = sourceDefinition.clips[clipIndex];
					PetClipDefinition rewritten = packed.Definition.clips[clipIndex];

					if (original.FrameCount != rewritten.FrameCount)
					{
						error = "The packed pet lost frames from '" + original.name + "'.";
						return false;
					}

					for (int frame = 0; frame < original.FrameCount; frame++)
					{
						if (SameFrame(source, original.frames[frame].cell, content, check, rewritten.frames[frame].cell))
						{
							continue;
						}

						error = "The packed sheet does not match the original at frame " + frame +
							" of '" + original.name + "'. Nothing has been written.";
						return false;
					}
				}
			}
			finally
			{
				check.Dispose();
			}

			error = null;
			return true;
		}

		// Sampled on a grid rather than every pixel.
		private static bool SameFrame(PetAtlas source, int sourceCell, RectInt content, PetAtlas packed, int packedCell)
		{
			const int step = 1;

			for (int y = 0; y < content.height; y += step)
			{
				for (int x = 0; x < content.width; x += step)
				{
					Color32 a = source.GetPixel(sourceCell, content.x + x, content.y + y);
					Color32 b = packed.GetPixel(packedCell, x, y);

					if (a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a)
					{
						return false;
					}
				}
			}

			return true;
		}

		// Everything about an animation except its frames, which are checked pixel by pixel afterwards.
		private static bool SameSettings(PetClipDefinition original, PetClipDefinition rewritten, out string difference)
		{
			difference = null;

			if (rewritten == null)
			{
				difference = "an animation, which went missing";
				return false;
			}

			if (original.FrameCount != rewritten.FrameCount) difference = "the frame count";
			else if (!Near(original.fps, rewritten.fps)) difference = "the frame rate";
			else if (original.loop != rewritten.loop) difference = "the loop setting";
			else if (original.allowFlip != rewritten.allowFlip) difference = "the mirroring setting";
			else if (original.drawnFacing != rewritten.drawnFacing) difference = "the drawn facing";
			else if (!Near(original.holdSeconds, rewritten.holdSeconds)) difference = "the hold";
			else if (Sound(original) != Sound(rewritten)) difference = "the sound";

			return difference == null;
		}

		// Numbers that only ever pass straight through the JSON copy are compared with a tolerance.
		private static bool Near(float a, float b)
		{
			return Mathf.Abs(a - b) < .01f;
		}

		private static string Sound(PetClipDefinition clip)
		{
			return string.IsNullOrEmpty(clip.sound) ? "" : clip.sound;
		}

		private static void WriteArchive(string destinationPath, PetAtlasRepacker.Result packed, PetDefinition original, string petFolder, byte[] iconThumbnail)
		{
			string folder = Path.GetDirectoryName(destinationPath);
			if (!string.IsNullOrEmpty(folder))
			{
				Directory.CreateDirectory(folder);
			}

			if (File.Exists(destinationPath))
			{
				File.Delete(destinationPath);
			}

			packed.Definition.atlas = PetPackageFormat.AtlasEntry;
			packed.Definition.frameworkVersion = UnityPetFrameworkInfo.Version;
			packed.Definition.schemaVersion = UnityPetFrameworkInfo.PetSchemaVersion;

			Dictionary<string, byte[]> sounds = CollectSounds(packed.Definition, petFolder);
			// The icon's first frame.
			byte[] thumbnail = iconThumbnail ?? ReadThumbnail(original, petFolder);
			packed.Definition.thumbnail = thumbnail != null ? PetPackageFormat.ThumbnailEntry : "";

			using (FileStream stream = new FileStream(destinationPath, FileMode.CreateNew))
			using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
			{
				WriteEntry(archive, PetPackageFormat.DefinitionEntry,
					System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(packed.Definition, true)),
					CompressionLevel.Optimal);

				// PNG is already deflate compressed inside.
				WriteEntry(archive, PetPackageFormat.AtlasEntry, packed.AtlasPng, CompressionLevel.NoCompression);

				if (thumbnail != null)
				{
					WriteEntry(archive, PetPackageFormat.ThumbnailEntry, thumbnail, CompressionLevel.NoCompression);
				}

				foreach (KeyValuePair<string, byte[]> sound in sounds)
				{
					WriteEntry(archive, sound.Key, sound.Value, CompressionLevel.Optimal);
				}
			}
		}

		private static void WriteEntry(ZipArchive archive, string name, byte[] bytes, CompressionLevel level)
		{
			ZipArchiveEntry entry = archive.CreateEntry(name, level);
			using (Stream stream = entry.Open())
			{
				stream.Write(bytes, 0, bytes.Length);
			}
		}

		// Sounds are named by the clips, and each one is re-read from beside the pet.
		private static Dictionary<string, byte[]> CollectSounds(PetDefinition definition, string petFolder)
		{
			Dictionary<string, byte[]> sounds = new Dictionary<string, byte[]>();

			foreach (PetClipDefinition clip in definition.AllSequences())
			{
				if (string.IsNullOrEmpty(clip.sound) || sounds.ContainsKey(clip.sound))
				{
					continue;
				}

				string path = SafeSoundPath(petFolder, clip.sound);
				if (path == null || !File.Exists(path))
				{
					clip.sound = "";
					continue;
				}

				sounds[clip.sound] = File.ReadAllBytes(path);
			}

			return sounds;
		}

		private static string SafeSoundPath(string petFolder, string relative)
		{
			if (string.IsNullOrEmpty(petFolder) || !PetPackageFormat.IsSafeEntryName(relative))
			{
				return null;
			}

			string combined = Path.GetFullPath(Path.Combine(petFolder, relative.Replace('/', Path.DirectorySeparatorChar)));
			string root = Path.GetFullPath(petFolder);

			return combined.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? combined : null;
		}

		private static byte[] ReadThumbnail(PetDefinition definition, string petFolder)
		{
			if (string.IsNullOrEmpty(definition.thumbnail) || string.IsNullOrEmpty(petFolder))
			{
				return null;
			}

			string path = PetDefinitionSerializer.SafeCombine(petFolder, definition.thumbnail);
			return path != null && File.Exists(path) ? File.ReadAllBytes(path) : null;
		}
	}
}
