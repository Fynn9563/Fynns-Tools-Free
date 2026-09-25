using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.UnityPetFramework
{
	// The one window for the whole tool.
	public sealed class UnityPetFrameworkWindow : EditorWindow
	{
		private const string SelectedPageKey = "FynnsTools.UnityPetFramework.SelectedPage";

		// A page id is the suffix of both its tab button and its container in the layout.
		private static readonly string[] PageIds = { "pet", "settings", "creator", "io", "about" };

		private static readonly Vector2 MinWindowSize = new Vector2(760f, 520f);

		private string _selectedPage;

		// Owns a sprite sheet texture, so it has to be disposed rather than dropped.
		private PetCreatorPage _creatorPage;

		private PetPage _petPage;
		private PetSettingsPage _settingsPage;
		private PetAboutPage _aboutPage;
		private PetImportExportPage _importExportPage;

		public static void Open()
		{
			UnityPetFrameworkWindow window = GetWindow<UnityPetFrameworkWindow>(false, UnityPetFrameworkInfo.DisplayName, true);
			window.minSize = MinWindowSize;
			window.Show();
		}

		private void CreateGUI()
		{
			VisualElement root = rootVisualElement;
			root.Clear();

			string uxmlPath = UnityPetFrameworkPaths.WindowUxmlPath;
			VisualTreeAsset uxml = string.IsNullOrEmpty(uxmlPath)
				? null
				: AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

			if (uxml == null)
			{
				// The layout is the entire window, so there is nothing to degrade to.
				root.Add(new HelpBox(
					"The window layout could not be loaded. Was part of the Unity Pet Framework folder moved or deleted?",
					HelpBoxMessageType.Warning));
				return;
			}

			uxml.CloneTree(root);

			Label title = root.Q<Label>("title");
			if (title != null)
			{
				title.text = string.Format(title.text, UnityPetFrameworkInfo.Version);
			}

			foreach (string id in PageIds)
			{
				// Captured per iteration, or every tab would select the last page.
				string pageId = id;
				Button tab = root.Q<Button>("tab-" + pageId);
				if (tab != null)
				{
					tab.clicked += () => SelectPage(pageId);
				}
			}

			Label credit = root.Q<Label>("credit");
			if (credit != null)
			{
				credit.RegisterCallback<ClickEvent>(_ => Application.OpenURL(UnityPetFrameworkInfo.CreditUrl));
			}

			AttachPages(root);

			string remembered = EditorPrefs.GetString(SelectedPageKey, PageIds[0]);
			SelectPage(Array.IndexOf(PageIds, remembered) < 0 ? PageIds[0] : remembered);
		}

		// Each page builds its own content into the empty container the layout declares for it.
		private void AttachPages(VisualElement root)
		{
			VisualElement petContainer = root.Q<VisualElement>("page-pet");
			if (petContainer != null)
			{
				_petPage = new PetPage();
				petContainer.Add(_petPage.Build());
			}

			VisualElement settingsContainer = root.Q<VisualElement>("page-settings");
			if (settingsContainer != null)
			{
				_settingsPage = new PetSettingsPage();
				settingsContainer.Add(_settingsPage.Build());
			}

			VisualElement ioContainer = root.Q<VisualElement>("page-io");
			if (ioContainer != null)
			{
				_importExportPage = new PetImportExportPage();
				ioContainer.Add(_importExportPage.Build());
			}

			VisualElement aboutContainer = root.Q<VisualElement>("page-about");
			if (aboutContainer != null && _aboutPage == null)
			{
				_aboutPage = new PetAboutPage();
				aboutContainer.Add(_aboutPage.Build());
			}

			VisualElement creatorContainer = root.Q<VisualElement>("page-creator");
			if (creatorContainer != null)
			{
				DisposeCreatorPage();
				_creatorPage = new PetCreatorPage();
				creatorContainer.Add(_creatorPage.Build());
			}
		}

		private void OnDisable()
		{
			// Runs on window close and before every assembly reload.
			DisposeCreatorPage();
		}

		private void DisposeCreatorPage()
		{
			_creatorPage?.Dispose();
			_creatorPage = null;
		}

		private void SelectPage(string pageId)
		{
			_selectedPage = pageId;
			EditorPrefs.SetString(SelectedPageKey, pageId);

			foreach (string id in PageIds)
			{
				bool active = id == _selectedPage;

				// Opening the Pet page is the moment a newly imported or saved pet should appear in its list.
				if (active && id == "pet")
				{
					_petPage?.Refresh();
				}

				VisualElement page = rootVisualElement.Q<VisualElement>("page-" + id);
				if (page != null)
				{
					page.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
				}

				Button tab = rootVisualElement.Q<Button>("tab-" + id);
				if (tab != null)
				{
					// The active tab is marked with weight and opacity rather than a style class.
					tab.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
					tab.style.opacity = active ? 1f : 0.6f;
				}
			}
		}
	}
}
