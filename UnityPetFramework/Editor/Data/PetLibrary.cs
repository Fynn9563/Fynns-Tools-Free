using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	internal static class PetLibrary
	{
		public static int Version { get; private set; }

		private static readonly HashSet<string> _bundledIds = new HashSet<string>(StringComparer.Ordinal);

		private static bool _bundledChecked;

		public static void NotifyChanged()
		{
			Version++;
		}

		internal sealed class Entry
		{
			public Entry(string id, string displayName, string author, string definitionPath, bool bundled, string thumbnailPath)
			{
				Id = id;
				DisplayName = displayName;
				Author = author;
				DefinitionPath = definitionPath;
				Bundled = bundled;
				ThumbnailPath = thumbnailPath;
			}

			public string Id { get; }

			public string DisplayName { get; }

			public string Author { get; }

			public string DefinitionPath { get; }

			public bool Bundled { get; }

			public string ThumbnailPath { get; }
		}

		public static List<Entry> Discover()
		{
			InstallBundled();

			List<Entry> entries = new List<Entry>();

			CollectFrom(UnityPetFrameworkPaths.ToAbsolute(UnityPetFrameworkPaths.BundledPetsFolder), true, entries);
			CollectFrom(UnityPetFrameworkPaths.ImportedPetsFolder, false, entries);

			return entries;
		}

		private static void InstallBundled()
		{
			if (_bundledChecked) return;

			_bundledChecked = true;
			_bundledIds.Clear();

			string root = UnityPetFrameworkPaths.ToAbsolute(UnityPetFrameworkPaths.BundledPetsFolder);
			if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;

			foreach (string path in Directory.GetFiles(root, "*." + PetPackageFormat.Extension))
			{
				if (!PetPackageReader.TryRead(path, out PetPackage package, out string error))
				{
					Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix +
						"The pet in " + Path.GetFileName(path) + " could not be read: " + error);
					continue;
				}

				using (package)
				{
					_bundledIds.Add(package.Definition.id);

					if (!NeedsInstall(package.Definition.id, path)) continue;

					if (!Install(package, out error))
					{
						Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix +
							"The pet in " + Path.GetFileName(path) + " could not be unpacked: " + error);
					}
				}
			}
		}

		private static bool NeedsInstall(string petId, string packagePath)
		{
			string definitionPath = Path.Combine(FolderForImport(petId), PetDefinitionSerializer.DefinitionFileName);

			try
			{
				if (!File.Exists(definitionPath)) return true;

				return File.GetLastWriteTimeUtc(packagePath) > File.GetLastWriteTimeUtc(definitionPath);
			}
			catch (Exception)
			{
				return true;
			}
		}

		public static string ResolveSelectedDefinitionPath()
		{
			List<Entry> entries = Discover();
			if (entries.Count == 0)
			{
				return null;
			}

			string selected = UnityPetFrameworkSettings.SelectedPetId;
			if (!string.IsNullOrEmpty(selected))
			{
				foreach (Entry entry in entries)
				{
					if (entry.Id == selected)
					{
						return entry.DefinitionPath;
					}
				}
			}

			return entries[0].DefinitionPath;
		}

		public static bool Delete(Entry entry, out string error)
		{
			if (entry == null)
			{
				error = "There is no pet to delete.";
				return false;
			}

			if (entry.Bundled)
			{
				error = "The pet that comes with the tool cannot be deleted. Remove the tool folder from your project instead.";
				return false;
			}

			string folder;
			string root;

			try
			{
				root = Path.GetFullPath(UnityPetFrameworkPaths.ImportedPetsFolder);
				folder = Path.GetFullPath(Path.GetDirectoryName(entry.DefinitionPath) ?? string.Empty);
			}
			catch (Exception exception)
			{
				error = "That pet is not somewhere this can delete from: " + exception.Message;
				return false;
			}

			string prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			if (folder.Length <= prefix.Length || !folder.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				error = "That pet is not in your imported pets folder, so it will not be deleted.";
				return false;
			}

			try
			{
				Directory.Delete(folder, true);
			}
			catch (Exception exception)
			{
				error = "The pet could not be deleted: " + exception.Message;
				return false;
			}

			NotifyChanged();
			error = null;
			return true;
		}

		public static string FolderForImport(string petId)
		{
			// Validate imported IDs before using them as paths.
			string safe = SanitiseId(petId);
			return Path.Combine(UnityPetFrameworkPaths.ImportedPetsFolder, safe);
		}

		public static bool Install(PetPackage package, out string error)
		{
			string folder = FolderForImport(package.Definition.id);

			try
			{
				Directory.CreateDirectory(folder);

				package.Definition.atlas = PetPackageFormat.AtlasEntry;
				File.WriteAllBytes(Path.Combine(folder, PetPackageFormat.AtlasEntry), package.AtlasPng);

				if (package.ThumbnailPng != null)
				{
					package.Definition.thumbnail = PetPackageFormat.ThumbnailEntry;
					File.WriteAllBytes(Path.Combine(folder, PetPackageFormat.ThumbnailEntry), package.ThumbnailPng);
				}

				foreach (KeyValuePair<string, byte[]> sound in package.Sounds)
				{
					string path = PetDefinitionSerializer.SafeCombineRelative(folder, sound.Key);
					if (path == null) continue;

					Directory.CreateDirectory(Path.GetDirectoryName(path));
					File.WriteAllBytes(path, sound.Value);
				}

				return PetDefinitionSerializer.TrySave(
					Path.Combine(folder, PetDefinitionSerializer.DefinitionFileName),
					package.Definition,
					out error);
			}
			catch (Exception exception)
			{
				error = "The pet could not be saved: " + exception.Message;
				return false;
			}
		}

		public static string SanitiseId(string petId)
		{
			if (string.IsNullOrEmpty(petId))
			{
				return "pet";
			}

			char[] characters = petId.ToCharArray();
			for (int i = 0; i < characters.Length; i++)
			{
				char character = characters[i];
				bool allowed = (character >= 'a' && character <= 'z') ||
					(character >= 'A' && character <= 'Z') ||
					(character >= '0' && character <= '9') ||
					character == '.' || character == '-' || character == '_';

				if (!allowed)
				{
					characters[i] = '_';
				}
			}

			string result = new string(characters).Trim('.', ' ');
			return string.IsNullOrEmpty(result) ? "pet" : result;
		}

		private static void CollectFrom(string root, bool bundled, List<Entry> entries)
		{
			if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
			{
				return;
			}

			foreach (string folder in Directory.GetDirectories(root))
			{
				string definitionPath = Path.Combine(folder, PetDefinitionSerializer.DefinitionFileName);
				if (!File.Exists(definitionPath))
				{
					continue;
				}

				if (!PetDefinitionSerializer.TryReadDefinition(definitionPath, out PetDefinition definition, out string error))
				{
					Debug.LogWarning(UnityPetFrameworkInfo.LogPrefix +
						"Skipped the pet in " + folder + ": " + error);
					continue;
				}

				string thumbnail = string.IsNullOrEmpty(definition.thumbnail)
					? null
					: PetDefinitionSerializer.SafeCombine(folder, definition.thumbnail);

				entries.Add(new Entry(
					definition.id,
					definition.displayName,
					definition.author,
					definitionPath,
					bundled || _bundledIds.Contains(definition.id),
					thumbnail != null && File.Exists(thumbnail) ? thumbnail : null));
			}
		}
	}
}
