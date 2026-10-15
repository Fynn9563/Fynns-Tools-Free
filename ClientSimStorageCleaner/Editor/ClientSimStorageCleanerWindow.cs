using UnityEditor;
using UnityEngine;

namespace FynnsTools.ClientSimStorageCleaner
{
	internal sealed class ClientSimStorageCleanerWindow : EditorWindow
	{
		private const string WindowTitle = "ClientSim Storage Cleaner";

		private static readonly Vector2 MinWindowSize = new Vector2(380, 160);

		public static void Open()
		{
			ClientSimStorageCleanerWindow window = GetWindow<ClientSimStorageCleanerWindow>(WindowTitle);
			window.minSize = MinWindowSize;
			window.Show();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				bool enabled = ClientSimStorageCleanerSettings.Enabled;
				bool wanted = EditorGUILayout.ToggleLeft("Delete ClientSimStorage after play mode", enabled, EditorStyles.boldLabel);
				if (wanted != enabled)
				{
					ClientSimStorageCleanerSettings.Enabled = wanted;
				}

				EditorGUILayout.LabelField(
					"ClientSim saves the player data and player objects of every test run in a ClientSimStorage folder next to Assets, and loads them again on the next run. With this on, the folder is deleted every time play mode ends, so each run starts as a new player would.",
					EditorStyles.wordWrappedMiniLabel);
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Folder", EditorStyles.boldLabel);
			EditorGUILayout.SelectableLabel(ClientSimStorage.FolderPath, EditorStyles.wordWrappedLabel, GUILayout.Height(30));
			EditorGUILayout.LabelField(ClientSimStorage.Exists ? "Present" : "Not there", EditorStyles.miniLabel);

			using (new EditorGUI.DisabledScope(!ClientSimStorage.Exists || EditorApplication.isPlayingOrWillChangePlaymode))
			{
				if (GUILayout.Button("Clear Now"))
				{
					if (!ClientSimStorage.Clear(out string error))
					{
						Debug.LogWarning("[ClientSim Storage Cleaner] Could not delete " + ClientSimStorage.FolderPath + ": " + error);
					}
				}
			}
		}

		private void OnFocus()
		{
			Repaint();
		}
	}
}
