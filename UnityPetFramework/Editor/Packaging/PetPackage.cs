using System;
using System.Collections.Generic;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// A pet as it arrives in a .fynnpet, before anything has been written to disk.
	internal sealed class PetPackage : IDisposable
	{
		public PetDefinition Definition;
		public byte[] AtlasPng;
		public byte[] ThumbnailPng;

		// Keyed by the path inside the package, e.g.
		public readonly Dictionary<string, byte[]> Sounds = new Dictionary<string, byte[]>();

		public PetAtlas Atlas;

		public void Dispose()
		{
			Atlas?.Dispose();
			Atlas = null;
		}
	}

	internal static class PetPackageFormat
	{
		public const string Extension = "fynnpet";
		public const string DefinitionEntry = "pet.json";
		public const string AtlasEntry = "atlas.png";
		public const string ThumbnailEntry = "thumbnail.png";
		public const string SoundsFolder = "sounds/";

		// Generous for a pet.
		public const int MaxEntries = 64;
		public const long MaxEntryBytes = 16L * 1024 * 1024;
		public const long MaxTotalBytes = 64L * 1024 * 1024;

		// Nothing else may be in a package.
		public static bool IsAllowedExtension(string entryName)
		{
			string lower = entryName.ToLowerInvariant();
			return lower.EndsWith(".json", StringComparison.Ordinal)
				|| lower.EndsWith(".png", StringComparison.Ordinal)
				|| lower.EndsWith(".wav", StringComparison.Ordinal);
		}

		// Checked on the name the archive carries, never on a path built from it.
		public static bool IsSafeEntryName(string entryName)
		{
			if (string.IsNullOrEmpty(entryName) || entryName.Length > 200)
			{
				return false;
			}

			if (entryName.IndexOf('\\') >= 0 || entryName.IndexOf(':') >= 0)
			{
				return false;
			}

			if (entryName[0] == '/' || entryName.Contains(".."))
			{
				return false;
			}

			foreach (char character in entryName)
			{
				bool allowed = char.IsLetterOrDigit(character)
					|| character == '.' || character == '-' || character == '_' || character == '/';

				if (!allowed)
				{
					return false;
				}
			}

			return true;
		}
	}
}
