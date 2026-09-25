using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.UnityPetFramework
{
	// Opening and saving a pet, and the folder its parts live in while it is being made.
	internal sealed partial class PetCreatorPage
	{
		// The pet being authored, on disk, while it is authored.
		private string WorkingFolder
		{
			get
			{
				string id = PetLibrary.SanitiseId(_definition?.id);
				return Path.Combine(UnityPetFrameworkPaths.DraftFolder, string.IsNullOrEmpty(id) ? "untitled" : id);
			}
		}

		private void ChooseSound(PetClipDefinition clip)
		{
			string picked = EditorUtility.OpenFilePanel("Choose a sound", "", "wav");
			if (string.IsNullOrEmpty(picked)) return;

			try
			{
				// Parsed here rather than at export.
				if (!WavReader.TryRead(File.ReadAllBytes(picked), "preview", out AudioClip preview, out string error))
				{
					SetStatus(error, MessageType.Warning);
					return;
				}

				UnityEngine.Object.DestroyImmediate(preview, true);

				string soundsFolder = Path.Combine(WorkingFolder, "sounds");
				Directory.CreateDirectory(soundsFolder);

				string fileName = PetLibrary.SanitiseId(Path.GetFileNameWithoutExtension(picked)) + ".wav";
				File.Copy(picked, Path.Combine(soundsFolder, fileName), true);

				clip.sound = PetPackageFormat.SoundsFolder + fileName;
				SaveWorkingPet();
				SetStatus(null, MessageType.None);
			}
			catch (Exception exception)
			{
				SetStatus("The sound could not be copied: " + exception.Message, MessageType.Warning);
			}
		}

		private void BrowseToSavePet()
		{
			string suggested = PetLibrary.SanitiseId(_definition.id);
			string picked = EditorUtility.SaveFilePanel(
				"Save pet package",
				string.IsNullOrEmpty(_petPath) ? "" : Path.GetDirectoryName(_petPath),
				string.IsNullOrEmpty(suggested) ? "pet" : suggested,
				PetPackageFormat.Extension);

			if (!string.IsNullOrEmpty(picked)) SavePet(picked);
		}
		// Writes the package first and installs it second.
		private void SavePet(string packagePath)
		{
			if (_atlas == null)
			{
				SetStatus("Choose a sprite sheet before saving.", MessageType.Warning);
				return;
			}

			if (_definition.UsedCells().Count == 0)
			{
				SetStatus("Put some frames into at least one animation before saving.", MessageType.Warning);
				return;
			}

			_definition.frameworkVersion = UnityPetFrameworkInfo.Version;
			_definition.schemaVersion = UnityPetFrameworkInfo.PetSchemaVersion;

			if (!PetPackageWriter.TryWrite(packagePath, _atlas, _definition, WorkingFolder,
				out PetPackageWriter.Report report, out string error))
			{
				SetStatus(error, MessageType.Warning);
				return;
			}

			_petPath = packagePath;
			EditorPrefs.SetString(PetPathKey, _petPath);
			SaveWorkingPet();

			if (!InstallSaved(packagePath, out string installError))
			{
				// The package is on disk and is the thing that was asked for.
				SetStatus(string.Format(
					"Saved {0}, but it could not be added to your pet list: {1}",
					Path.GetFileName(packagePath), installError),
					MessageType.Warning);
				return;
			}

			SetStatus(string.Format(
				"Saved {0} and added it to your pet list. {1} of {2} frames packed, {3:P0} fewer pixels than the source sheet, {4:N0} KB.",
				Path.GetFileName(packagePath), report.CellsAfter, report.CellsBefore,
				report.PixelSaving, report.FileBytes / 1024),
				MessageType.Info);
		}

		// Read back through the same hardened reader an imported package goes through.
		private bool InstallSaved(string packagePath, out string error)
		{
			if (!PetPackageReader.TryRead(packagePath, out PetPackage package, out error)) return false;

			using (package)
			{
				if (!PetLibrary.Install(package, out error)) return false;
			}

			PetLibrary.NotifyChanged();
			PetThumbnails.Clear();
			PetIconRenderer.Clear();
			return true;
		}

		private void BrowseForPet()
		{
			string start = string.IsNullOrEmpty(_petPath)
				? UnityPetFrameworkPaths.ToAbsolute(UnityPetFrameworkPaths.BundledPetsFolder) ?? Application.dataPath
				: Path.GetDirectoryName(_petPath);
			string picked = EditorUtility.OpenFilePanelWithFilters(
				"Open a pet",
				start,
				new[]
				{
					"Pet", PetPackageFormat.Extension + ",json",
					"Pet package", PetPackageFormat.Extension,
					"Pet definition", "json"
				});
			if (string.IsNullOrEmpty(picked)) return;

			if (picked.EndsWith("." + PetPackageFormat.Extension, StringComparison.OrdinalIgnoreCase))
			{
				OpenPackage(picked);
				return;
			}

			LoadPet(picked);
		}

		// Unpacked into the draft folder and opened from there.
		private void OpenPackage(string packagePath)
		{
			if (!PetPackageReader.TryRead(packagePath, out PetPackage package, out string error))
			{
				SetStatus(error, MessageType.Warning);
				return;
			}

			using (package)
			{
				string folder = Path.Combine(UnityPetFrameworkPaths.DraftFolder, PetLibrary.SanitiseId(package.Definition.id));
				string definitionPath = Path.Combine(folder, PetDefinitionSerializer.DefinitionFileName);

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
						// Checked once before the archive was read and again here.
						string path = PetDefinitionSerializer.SafeCombineRelative(folder, sound.Key);
						if (path == null) continue;

						Directory.CreateDirectory(Path.GetDirectoryName(path));
						File.WriteAllBytes(path, sound.Value);
					}

					if (!PetDefinitionSerializer.TrySave(definitionPath, package.Definition, out error))
					{
						SetStatus(error, MessageType.Warning);
						return;
					}
				}
				catch (Exception exception)
				{
					SetStatus("The package could not be unpacked for editing: " + exception.Message, MessageType.Warning);
					return;
				}

				LoadPet(definitionPath);
			}

			_petPath = packagePath;
			EditorPrefs.SetString(PetPathKey, _petPath);
		}

		private void LoadPet(string definitionPath)
		{
			if (!PetDefinitionSerializer.TryLoad(definitionPath, out PetDefinition loaded, out PetAtlas loadedAtlas, out string error))
			{
				SetStatus(error, MessageType.Warning);
				return;
			}

			// Swapped only once the new pet is known good.
			_atlas?.Dispose();
			_atlas = loadedAtlas;
			_atlasError = null;

			_definition = loaded;
			_petPath = definitionPath;
			_sheetPath = PetDefinitionSerializer.SafeCombine(Path.GetDirectoryName(definitionPath), loaded.atlas);

			AdoptLoadedSlicing(loaded);

			EditorPrefs.SetString(PetPathKey, _petPath);
			EditorPrefs.SetString(SheetPathKey, _sheetPath ?? "");

			_selectedSlot = FirstFilledSlot() ?? PetClipSlots.Idle;
			_undo.Clear();
			_grid.Reset();
			_preview.SetDefinition(_definition);
			SaveWorkingPet();

			SetStatus(string.Format("Opened {0}.", _definition.displayName), MessageType.Info);
		}
	}
}
