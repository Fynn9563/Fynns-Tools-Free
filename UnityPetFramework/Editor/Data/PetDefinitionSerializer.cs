using System;
using System.IO;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Reads and writes pet.json, and refuses anything that would not work.
	internal static class PetDefinitionSerializer
	{
		public const string DefinitionFileName = "pet.json";

		// Big enough for any sane pet.
		private const long MaxDefinitionBytes = 4 * 1024 * 1024;

		// Loads a pet folder: its definition, and the sheet named by it.
		public static bool TryLoad(string definitionPath, out PetDefinition definition, out PetAtlas atlas, out string error)
		{
			definition = null;
			atlas = null;

			if (!TryReadDefinition(definitionPath, out definition, out error))
			{
				return false;
			}

			string folder = Path.GetDirectoryName(definitionPath);
			string atlasPath = SafeCombine(folder, definition.atlas);
			if (atlasPath == null)
			{
				definition = null;
				error = "This pet points at an image outside its own folder, which is not allowed.";
				return false;
			}

			atlas = PetAtlas.FromFile(atlasPath, definition, out error);
			if (atlas == null)
			{
				definition = null;
				return false;
			}

			if (!PetDefinitionValidator.Validate(definition, atlas.Sheet.width, atlas.Sheet.height, out error))
			{
				atlas.Dispose();
				atlas = null;
				definition = null;
				return false;
			}

			error = null;
			return true;
		}

		// Reads and validates the definition without touching the sheet.
		public static bool TryReadDefinition(string definitionPath, out PetDefinition definition, out string error)
		{
			definition = null;

			if (string.IsNullOrEmpty(definitionPath) || !File.Exists(definitionPath))
			{
				error = "There is no pet file at " + (definitionPath ?? "<null>") + ".";
				return false;
			}

			try
			{
				FileInfo info = new FileInfo(definitionPath);
				if (info.Length > MaxDefinitionBytes)
				{
					error = "This pet file is far larger than a pet file should be, so it has not been read.";
					return false;
				}

				string json = File.ReadAllText(definitionPath);

				// JsonUtility is monomorphic.
				definition = JsonUtility.FromJson<PetDefinition>(json)?.Normalise();
			}
			catch (Exception exception)
			{
				error = "This pet file could not be read: " + exception.Message;
				return false;
			}

			if (definition == null)
			{
				error = "This pet file is not in the expected format.";
				return false;
			}

			// Sheet size is unknown here, so the grid is checked for sanity but not against the image.
			if (!PetDefinitionValidator.Validate(definition, 0, 0, out error))
			{
				definition = null;
				return false;
			}

			return true;
		}

		public static bool TrySave(string definitionPath, PetDefinition definition, out string error)
		{
			// Validated on the way out as well as on the way in.
			if (!PetDefinitionValidator.Validate(definition, 0, 0, out error))
			{
				return false;
			}

			try
			{
				string folder = Path.GetDirectoryName(definitionPath);
				if (!string.IsNullOrEmpty(folder))
				{
					Directory.CreateDirectory(folder);
				}

				definition.frameworkVersion = UnityPetFrameworkInfo.Version;
				definition.schemaVersion = UnityPetFrameworkInfo.PetSchemaVersion;

				File.WriteAllText(definitionPath, JsonUtility.ToJson(definition, true));
			}
			catch (Exception exception)
			{
				error = "This pet could not be saved: " + exception.Message;
				return false;
			}

			error = null;
			return true;
		}

		// Like SafeCombine, but for a path with folders in it such as "sounds/step.wav".
		public static string SafeCombineRelative(string folder, string relativePath)
		{
			if (string.IsNullOrEmpty(folder) || !PetPackageFormat.IsSafeEntryName(relativePath))
			{
				return null;
			}

			string root = Path.GetFullPath(folder);
			string combined;
			try
			{
				combined = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
			}
			catch (Exception)
			{
				return null;
			}

			string rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
				? root
				: root + Path.DirectorySeparatorChar;

			return combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) ? combined : null;
		}

		// Joins a file name onto a folder and returns null if the result escapes it.
		public static string SafeCombine(string folder, string fileName)
		{
			if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName))
			{
				return null;
			}

			if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				return null;
			}

			string root = Path.GetFullPath(folder);
			string combined;
			try
			{
				combined = Path.GetFullPath(Path.Combine(root, fileName));
			}
			catch (Exception)
			{
				return null;
			}

			string rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
				? root
				: root + Path.DirectorySeparatorChar;

			return combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) ? combined : null;
		}
	}
}
