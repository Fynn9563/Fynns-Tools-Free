using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.UnityPetFramework
{
	// Sharing pets: writing a .fynnpet out, and taking one in.
	internal sealed class PetImportExportPage
	{
		// Matches the pet selector, so a pet looks the same in both lists.
		private const float IconSize = 32f;

		private IMGUIContainer _container;
		private List<PetLibrary.Entry> _pets;
		private int _listedVersion = -1;
		private Vector2 _scroll;

		private readonly PetDeferredActions _deferred = new PetDeferredActions();
		private string _status;
		private MessageType _statusType = MessageType.None;

		public VisualElement Build()
		{
			_container = new IMGUIContainer(OnGUI);
			_container.style.flexGrow = 1f;
			return _container;
		}

		private void OnGUI()
		{
			DrawPage();

			// Import and export both open a modal picker, which cannot happen inside a layout scope.
			if (_deferred.Run()) _container?.MarkDirtyRepaint();
		}

		private void DrawPage()
		{
			// Re-listed whenever anything has written a pet to disk.
			if (_pets == null || _listedVersion != PetLibrary.Version)
			{
				_pets = PetLibrary.Discover();
				_listedVersion = PetLibrary.Version;
			}

			PetIconRenderer.BeginFrame();

			using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(_scroll))
			{
				_scroll = scroll.scrollPosition;

				DrawImport();
				EditorGUILayout.Space(8);
				DrawExport();

				if (!string.IsNullOrEmpty(_status))
				{
					EditorGUILayout.Space(6);
					EditorGUILayout.HelpBox(_status, _statusType);
				}
			}

			if (PetIconRenderer.AnyAnimated)
			{
				_container?.MarkDirtyRepaint();
			}
		}

		private void DrawImport()
		{
			EditorGUILayout.LabelField("Import", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(
					"A pet package holds pictures, sounds and a description of the animations, and nothing else. " +
					"It cannot contain code, it is never added to your project, and nothing in it is ever run. " +
					"Anything unexpected inside one and the whole package is refused.",
					EditorStyles.wordWrappedMiniLabel);

				EditorGUILayout.Space(4);

				if (GUILayout.Button("Import a pet", GUILayout.Height(26f)))
				{
					_deferred.Defer(Import);
				}

				EditorGUILayout.LabelField(
					"Imported pets are kept outside your project, in your own app data folder.",
					EditorStyles.miniLabel);
			}
		}

		private void DrawExport()
		{
			EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField(
					"Exporting packs the sheet down to only the frames the pet actually uses, cropped to the drawn area.",
					EditorStyles.wordWrappedMiniLabel);

				EditorGUILayout.Space(4);

				if (_pets.Count == 0)
				{
					EditorGUILayout.LabelField("No pets to export.", EditorStyles.miniLabel);
					return;
				}

				foreach (PetLibrary.Entry pet in _pets)
				{
					using (new EditorGUILayout.HorizontalScope(GUILayout.Height(IconSize + 4f)))
					{
						Rect icon = GUILayoutUtility.GetRect(IconSize, IconSize, GUILayout.Width(IconSize), GUILayout.Height(IconSize));
						if (!PetIconRenderer.Draw(icon, pet))
						{
							Texture2D thumbnail = PetThumbnails.Get(pet.ThumbnailPath);
							if (thumbnail != null)
							{
								GUI.DrawTexture(icon, thumbnail, ScaleMode.ScaleToFit, true);
							}
							else
							{
								EditorGUI.DrawRect(icon, new Color(0f, 0f, 0f, 0.15f));
							}
						}

						EditorGUILayout.LabelField(pet.DisplayName, GUILayout.ExpandWidth(true));

						if (GUILayout.Button("Export", EditorStyles.miniButton, GUILayout.Width(70f)))
						{
							_deferred.Defer(() => Export(pet));
						}
					}
				}
			}
		}

		private void Import()
		{
			string picked = EditorUtility.OpenFilePanel("Import a pet", "", PetPackageFormat.Extension);
			if (string.IsNullOrEmpty(picked))
			{
				return;
			}

			if (!PetPackageReader.TryRead(picked, out PetPackage package, out string error))
			{
				SetStatus(error, MessageType.Warning);
				return;
			}

			using (package)
			{
				if (!PetLibrary.Install(package, out string writeError))
				{
					SetStatus(writeError, MessageType.Warning);
					return;
				}
			}

			_pets = null;
			PetLibrary.NotifyChanged();
			PetThumbnails.Clear();
			PetIconRenderer.Clear();
			SetStatus("Imported. It is now in the list on the Pet page.", MessageType.Info);
		}

		private void Export(PetLibrary.Entry pet)
		{
			if (!PetDefinitionSerializer.TryLoad(pet.DefinitionPath, out PetDefinition definition, out PetAtlas atlas, out string error))
			{
				SetStatus(error, MessageType.Warning);
				return;
			}

			using (atlas)
			{
				string suggested = PetLibrary.SanitiseId(pet.Id) + "." + PetPackageFormat.Extension;
				string destination = EditorUtility.SaveFilePanel(
					"Export pet", "", suggested, PetPackageFormat.Extension);

				if (string.IsNullOrEmpty(destination))
				{
					return;
				}

				if (!PetPackageWriter.TryWrite(
						destination,
						atlas,
						definition,
						Path.GetDirectoryName(pet.DefinitionPath),
						out PetPackageWriter.Report report,
						out error))
				{
					SetStatus(error, MessageType.Warning);
					return;
				}

				SetStatus(string.Format(
					"Exported {0}.\n{1} frames kept of {2}, sheet {3:P0} smaller, file {4} KB.",
					Path.GetFileName(report.Path),
					report.CellsAfter,
					report.CellsBefore,
					report.PixelSaving,
					report.FileBytes / 1024),
					MessageType.Info);
			}
		}

		private void SetStatus(string message, MessageType type)
		{
			_status = message;
			_statusType = type;
			_container?.MarkDirtyRepaint();
		}
	}
}
