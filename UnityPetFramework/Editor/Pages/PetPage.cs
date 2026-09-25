using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.UnityPetFramework
{
	// Switching the pet on, choosing which one, and saying why it is not there when it is not.
	internal sealed class PetPage
	{
		// Big enough to recognise the pet.
		private const float ThumbnailSize = 32f;

		private IMGUIContainer _container;
		private readonly PetDeferredActions _deferred = new PetDeferredActions();
		private List<PetLibrary.Entry> _pets;
		private string _status;
		private MessageType _statusType = MessageType.None;
		private int _listedVersion = -1;
		private Vector2 _scroll;

		public VisualElement Build()
		{
			_container = new IMGUIContainer(OnGUI);
			_container.style.flexGrow = 1f;
			return _container;
		}

		public void Refresh()
		{
			_pets = null;
			_container?.MarkDirtyRepaint();
		}

		private void OnGUI()
		{
			DrawPage();

			// Deleting asks for confirmation.
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

				DrawEnableRow();
				EditorGUILayout.Space(6);
				DrawStatus();
				EditorGUILayout.Space(6);
				DrawPetList();
			}

			// Only while an icon on screen actually has more than one frame.
			if (PetIconRenderer.AnyAnimated)
			{
				_container?.MarkDirtyRepaint();
			}
		}

		private void DrawEnableRow()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
			{
				DrawSelectedIcon();

				using (new EditorGUI.DisabledScope(!PetOverlayFactory.IsSupportedPlatform))
				{
					EditorGUI.BeginChangeCheck();
					bool enabled = EditorGUILayout.ToggleLeft(
						new GUIContent("Show the pet", "Puts the pet on screen, above the Unity window."),
						UnityPetFrameworkSettings.Enabled,
						GUILayout.Width(140f));

					if (EditorGUI.EndChangeCheck())
					{
						PetOverlayHost.SetEnabled(enabled);
					}
				}

				GUILayout.FlexibleSpace();

				if (UnityPetFrameworkSettings.Enabled)
				{
					EditorGUILayout.LabelField(
						string.Format("Energy {0:P0}", PetOverlayHost.Energy),
						EditorStyles.miniLabel,
						GUILayout.Width(90f));

					// Energy only moves on the editor's update loop.
					_container?.MarkDirtyRepaint();
				}
			}
		}

		private void DrawStatus()
		{
			string status = PetOverlayHost.StatusMessage;
			if (string.IsNullOrEmpty(status))
			{
				EditorGUILayout.HelpBox("The pet is on screen.", MessageType.Info);
				return;
			}

			// Not being on Windows is a fact rather than a fault, so it is not a warning.
			MessageType type = PetOverlayFactory.IsSupportedPlatform && UnityPetFrameworkSettings.Enabled
				? MessageType.Warning
				: MessageType.Info;

			EditorGUILayout.HelpBox(status, type);
		}

		private void DrawPetList()
		{
			EditorGUILayout.LabelField("Pets", EditorStyles.boldLabel);

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (_pets.Count == 0)
				{
					EditorGUILayout.HelpBox(
						"No pets were found. The one that ships with the tool lives in its Pets folder, so this usually means part of the folder was moved.",
						MessageType.Warning);
					return;
				}

				string selected = UnityPetFrameworkSettings.SelectedPetId;
				foreach (PetLibrary.Entry pet in _pets)
				{
					DrawPetRow(pet, pet.Id == selected || string.IsNullOrEmpty(selected) && pet.Bundled);
				}

				EditorGUILayout.Space(2);
				if (GUILayout.Button("Rescan", EditorStyles.miniButton, GUILayout.Width(80f)))
				{
					Refresh();
				}
			}
		}

		private void DrawPetRow(PetLibrary.Entry pet, bool selected)
		{
			using (new EditorGUILayout.HorizontalScope(GUILayout.Height(ThumbnailSize + 4f)))
			{
				DrawThumbnail(pet);

				using (new EditorGUILayout.VerticalScope())
				{
					GUILayout.FlexibleSpace();

					if (GUILayout.Button(
						new GUIContent((selected ? "> " : "   ") + pet.DisplayName, pet.DefinitionPath),
						selected ? EditorStyles.boldLabel : EditorStyles.label,
						GUILayout.ExpandWidth(true)))
					{
						UnityPetFrameworkSettings.SelectedPetId = pet.Id;

						// A different pet is a different frame size.
						PetOverlayHost.Restart();
					}

					GUILayout.FlexibleSpace();
				}

				EditorGUILayout.LabelField(
					pet.Bundled ? "included" : pet.Author ?? "",
					EditorStyles.miniLabel,
					GUILayout.Width(80f));

				// Absent rather than disabled for the bundled pet.
				if (pet.Bundled)
				{
					GUILayout.Space(56f);
					return;
				}

				if (GUILayout.Button(
					new GUIContent("Delete", "Removes this pet from your computer. The package file you imported it from is not touched."),
					EditorStyles.miniButton,
					GUILayout.Width(52f)))
				{
					_deferred.Defer(() => ConfirmDelete(pet));
				}
			}
		}

		private void ConfirmDelete(PetLibrary.Entry pet)
		{
			if (!EditorUtility.DisplayDialog(
				"Delete " + pet.DisplayName,
				"Delete " + pet.DisplayName + " from your computer?" + Break +
				"Its pictures, sounds and animations are removed from " + Path.GetDirectoryName(pet.DefinitionPath) + "." + Break +
				"Any .fynnpet file you imported it from is left alone, so you can bring it back by importing it again.",
				"Delete",
				"Cancel"))
			{
				return;
			}

			if (!PetLibrary.Delete(pet, out string error))
			{
				SetStatus(error, MessageType.Warning);
				return;
			}

			_pets = null;
			PetThumbnails.Clear();
			PetIconRenderer.Clear();

			// The pet on screen may be the one whose files have just gone.
			if (UnityPetFrameworkSettings.SelectedPetId == pet.Id)
			{
				List<PetLibrary.Entry> remaining = PetLibrary.Discover();
				UnityPetFrameworkSettings.SelectedPetId = remaining.Count > 0 ? remaining[0].Id : "";
			}

			PetOverlayHost.Restart();
			SetStatus("Deleted " + pet.DisplayName + ".", MessageType.Info);
		}

		private void SetStatus(string message, MessageType type)
		{
			_status = message;
			_statusType = type;
		}

		// A blank line inside a dialog, kept as a constant so the escape is not repeated in prose.
		private static readonly string Break = "\n\n";

		// The selected pet.
		private void DrawSelectedIcon()
		{
			if (_pets == null || _pets.Count == 0)
			{
				return;
			}

			string selected = UnityPetFrameworkSettings.SelectedPetId;
			PetLibrary.Entry pet = _pets.Find(p => p.Id == selected) ?? _pets[0];

			Rect box = GUILayoutUtility.GetRect(ThumbnailSize, ThumbnailSize, GUILayout.Width(ThumbnailSize), GUILayout.Height(ThumbnailSize));
			if (!PetIconRenderer.Draw(box, pet))
			{
				Texture2D thumbnail = PetThumbnails.Get(pet.ThumbnailPath);
				if (thumbnail != null)
				{
					GUI.DrawTexture(box, thumbnail, ScaleMode.ScaleToFit, true);
				}
			}
		}

		// The pet's own icon animation first.
		private static void DrawThumbnail(PetLibrary.Entry pet)
		{
			Rect box = GUILayoutUtility.GetRect(ThumbnailSize, ThumbnailSize, GUILayout.Width(ThumbnailSize), GUILayout.Height(ThumbnailSize));

			if (PetIconRenderer.Draw(box, pet))
			{
				return;
			}

			Texture2D thumbnail = PetThumbnails.Get(pet.ThumbnailPath);
			if (thumbnail != null)
			{
				GUI.DrawTexture(box, thumbnail, ScaleMode.ScaleToFit, true);
				return;
			}

			EditorGUI.DrawRect(box, new Color(0f, 0f, 0f, 0.15f));
		}
	}
}
