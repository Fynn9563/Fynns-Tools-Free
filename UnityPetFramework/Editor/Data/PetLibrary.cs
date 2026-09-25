using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Finds the pets available to show.
	internal static class PetLibrary
	{
		// Bumped whenever the set of pets on disk changes.
		public static int Version { get; private set; }

		// Ids that came out of a package shipped with the tool.
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

			// Absolute path, or null when the pet has no picture.
			public string ThumbnailPath { get; }
		}

		// Every pet that loads, bundled first.
		public static List<Entry> Discover()
		{
			InstallBundled();

			List<Entry> entries = new List<Entry>();

			// The tool folder is still scanned for pets stored as a folder.
			CollectFrom(UnityPetFrameworkPaths.ToAbsolute(UnityPetFrameworkPaths.BundledPetsFolder), true, entries);
			CollectFrom(UnityPetFrameworkPaths.ImportedPetsFolder, false, entries);

			return entries;
		}

		// Unpacks the packages that ship with the tool into the machine-local pet folder.
		private static void InstallBundled()
		{
			if (_bundledChecked) return;

			_bundledChecked = true;
			_bundledIds.Clear();

			string root = UnityPetFrameworkPaths.ToAbsolute(UnityPetFrameworkPaths.BundledPetsFolder);
			if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;

			foreach (string path in Directory.GetFiles(root, "*." + PetPackageFormat.Extension))
			{
				// Through the same hardened reader as any other package.
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

		// Missing, or the shipped package is newer than what was unpacked from it.
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

		// The file the overlay should load.
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

		// Removes an imported pet from disk, folder and all.
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

			// A separator on the end of the root.
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
			// The id comes from a pet file, so it is not ours to trust as a path.
			string safe = SanitiseId(petId);
			return Path.Combine(UnityPetFrameworkPaths.ImportedPetsFolder, safe);
		}

		// Anything that is not a plain identifier character becomes an underscore.
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

				// Read without the sheet.
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
