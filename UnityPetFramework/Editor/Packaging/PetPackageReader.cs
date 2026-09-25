using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Opens a .fynnpet, and refuses anything it is not completely happy with.
	internal static class PetPackageReader
	{
		public static bool TryRead(string packagePath, out PetPackage package, out string error)
		{
			package = null;

			if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
			{
				error = "There is no pet package at that path.";
				return false;
			}

			try
			{
				FileInfo info = new FileInfo(packagePath);
				if (info.Length > PetPackageFormat.MaxTotalBytes)
				{
					error = "That pet package is larger than pet packages are allowed to be.";
					return false;
				}

				using (ZipArchive archive = ZipFile.OpenRead(packagePath))
				{
					return TryReadArchive(archive, out package, out error);
				}
			}
			catch (InvalidDataException)
			{
				error = "That file is not a pet package.";
				return false;
			}
			catch (Exception exception)
			{
				error = "That pet package could not be opened: " + exception.Message;
				return false;
			}
		}

		private static bool TryReadArchive(ZipArchive archive, out PetPackage package, out string error)
		{
			package = null;

			if (archive.Entries.Count > PetPackageFormat.MaxEntries)
			{
				error = "That pet package contains more files than a pet needs.";
				return false;
			}

			// Every name is inspected before a single byte is read out of any of them.
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (entry.FullName.EndsWith("/", StringComparison.Ordinal))
				{
					continue;
				}

				if (!PetPackageFormat.IsSafeEntryName(entry.FullName))
				{
					error = "That pet package contains a file with an unsafe name (" + entry.FullName + "), so none of it has been read.";
					return false;
				}

				if (!PetPackageFormat.IsAllowedExtension(entry.FullName))
				{
					error = "That pet package contains " + entry.FullName + ", which is not something a pet is allowed to carry. Nothing has been read from it.";
					return false;
				}
			}

			PetPackage built = new PetPackage();
			long total = 0;

			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (entry.FullName.EndsWith("/", StringComparison.Ordinal))
				{
					continue;
				}

				if (!TryReadEntry(entry, ref total, out byte[] bytes, out error))
				{
					built.Dispose();
					return false;
				}

				Store(built, entry.FullName, bytes);
			}

			if (!Finish(built, out error))
			{
				built.Dispose();
				return false;
			}

			package = built;
			error = null;
			return true;
		}

		// Reads one entry, counting the bytes as they arrive.
		private static bool TryReadEntry(ZipArchiveEntry entry, ref long total, out byte[] bytes, out string error)
		{
			bytes = null;

			if (entry.Length > PetPackageFormat.MaxEntryBytes)
			{
				error = "A file inside that pet package (" + entry.FullName + ") is too large.";
				return false;
			}

			long remaining = PetPackageFormat.MaxTotalBytes - total;
			using (Stream stream = entry.Open())
			using (MemoryStream buffer = new MemoryStream())
			{
				byte[] chunk = new byte[64 * 1024];
				int read;

				while ((read = stream.Read(chunk, 0, chunk.Length)) > 0)
				{
					remaining -= read;
					if (remaining < 0 || buffer.Length + read > PetPackageFormat.MaxEntryBytes)
					{
						error = "That pet package expands to far more than it claims, so it has been rejected.";
						return false;
					}

					buffer.Write(chunk, 0, read);
				}

				bytes = buffer.ToArray();
			}

			total += bytes.Length;
			error = null;
			return true;
		}

		private static void Store(PetPackage package, string entryName, byte[] bytes)
		{
			if (entryName == PetPackageFormat.DefinitionEntry)
			{
				package.Definition = JsonUtility.FromJson<PetDefinition>(
					System.Text.Encoding.UTF8.GetString(bytes))?.Normalise();
				return;
			}

			if (entryName == PetPackageFormat.AtlasEntry)
			{
				package.AtlasPng = bytes;
				return;
			}

			if (entryName == PetPackageFormat.ThumbnailEntry)
			{
				package.ThumbnailPng = bytes;
				return;
			}

			if (entryName.StartsWith(PetPackageFormat.SoundsFolder, StringComparison.Ordinal))
			{
				package.Sounds[entryName] = bytes;
			}
		}

		// Everything that has to be true before the pet is usable.
		private static bool Finish(PetPackage package, out string error)
		{
			if (package.Definition == null)
			{
				error = "That pet package has no pet.json in it.";
				return false;
			}

			if (package.AtlasPng == null)
			{
				error = "That pet package has no atlas.png in it.";
				return false;
			}

			package.Atlas = PetAtlas.FromBytes(
				package.AtlasPng,
				package.Definition,
				out error);

			if (package.Atlas == null)
			{
				return false;
			}

			// The same validator the bundled pet goes through.
			if (!PetDefinitionValidator.Validate(
					package.Definition,
					package.Atlas.Sheet.width,
					package.Atlas.Sheet.height,
					out error))
			{
				return false;
			}

			foreach (PetClipDefinition clip in package.Definition.AllSequences())
			{
				if (string.IsNullOrEmpty(clip.sound) || package.Sounds.ContainsKey(clip.sound))
				{
					continue;
				}

				error = "The animation '" + clip.name + "' asks for a sound (" + clip.sound + ") that is not in the package.";
				return false;
			}

			error = null;
			return true;
		}
	}
}
