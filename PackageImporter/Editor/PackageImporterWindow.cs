using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.PackageImporter
{
    internal sealed class PackageImporterWindow : EditorWindow
    {
        private Vector2 scroll;

        [MenuItem("Tools/Fynn's Tools/Package Importer", false, 72)]
        internal static void Open()
        {
            var window = GetWindow<PackageImporterWindow>();
            window.titleContent = new GUIContent("Package Importer");
            window.minSize = new Vector2(560, 360);
        }

        private void OnEnable() { PackageImportQueue.Changed += Repaint; }
        private void OnDisable() { PackageImportQueue.Changed -= Repaint; }

        private void OnGUI()
        {
            var state = PackageImportQueue.State;
            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Package Importer", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(state.Busy))
            {
                if (GUILayout.Button("Add Folder", GUILayout.Width(95))) AddFolder();
                if (GUILayout.Button("Add Package", GUILayout.Width(105))) AddPackage();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);
            HandleDrop(state.Busy);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUI.skin.box);
            if (state.items.Count == 0)
            {
                GUILayout.Space(24);
                GUILayout.Label("Drop .unitypackage files here or add a folder.", EditorStyles.centeredGreyMiniLabel);
            }
            for (int index = 0; index < state.items.Count; index++)
            {
                var item = state.items[index];
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(state.Busy))
                {
                    bool selected = EditorGUILayout.Toggle(item.selected, GUILayout.Width(18));
                    if (selected != item.selected) { item.selected = selected; PackageImportQueue.Save(); }
                }
                GUILayout.Label(new GUIContent(Path.GetFileName(item.path), item.path), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(StatusLabel(item.status), EditorStyles.miniLabel, GUILayout.Width(85));
                using (new EditorGUI.DisabledScope(state.Busy))
                {
                    if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60)))
                    {
                        state.items.RemoveAt(index);
                        PackageImportQueue.Save();
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(item.detail)) GUILayout.Label(item.detail, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(6);
            using (new EditorGUI.DisabledScope(state.Busy))
            {
                bool review = EditorGUILayout.ToggleLeft(new GUIContent("Review every package before importing",
                    "Existing assets always open Unity's review dialog. Enable this to review new assets too."), state.reviewAll);
                if (review != state.reviewAll) { state.reviewAll = review; PackageImportQueue.Save(); }
            }
            if (!string.IsNullOrEmpty(state.message))
                EditorGUILayout.HelpBox(state.message, state.uncertain ? MessageType.Warning : MessageType.Info);
            if (state.Current != null && state.uncertain)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Mark Imported")) Resolve(ImportStatus.Imported);
                if (GUILayout.Button("Mark Skipped")) Resolve(ImportStatus.Skipped);
                if (GUILayout.Button("Mark Failed")) Resolve(ImportStatus.Failed);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            if (state.Busy)
            {
                using (new EditorGUI.DisabledScope(state.stopRequested || state.uncertain))
                    if (GUILayout.Button("Stop Queue", GUILayout.Height(28))) PackageImportQueue.Stop();
                if (state.Current != null && !state.uncertain && GUILayout.Button("Check Result", GUILayout.Height(28)))
                    PackageImportQueue.CheckResult();
            }
            else
            {
                using (new EditorGUI.DisabledScope(!state.items.Any(i => i.selected && i.status == ImportStatus.Pending)
                    || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating || EditorApplication.isCompiling || PackageImportQueue.ExternalImport))
                    if (GUILayout.Button("Import Selected", GUILayout.Height(28))) PackageImportQueue.Start();
                using (new EditorGUI.DisabledScope(!state.items.Any(i => i.selected && i.status != ImportStatus.Pending)))
                    if (GUILayout.Button("Reset Selected", GUILayout.Height(28))) PackageImportQueue.RetrySelected();
                using (new EditorGUI.DisabledScope(state.items.Count == 0))
                    if (GUILayout.Button("Clear", GUILayout.Width(60), GUILayout.Height(28)))
                    { state.items.Clear(); state.message = ""; PackageImportQueue.Save(); }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(state.items.Count(i => i.status == ImportStatus.Imported) + " imported   "
                + state.items.Count(i => i.status == ImportStatus.Skipped) + " skipped   "
                + state.items.Count(i => i.status == ImportStatus.Failed) + " failed", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Created by Fynn9563", EditorStyles.linkLabel)) Application.OpenURL("https://github.com/Fynn9563");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        private static string StatusLabel(ImportStatus status)
        {
            return status == ImportStatus.NeedsReview ? "Check Result" : status.ToString();
        }

        private static void Resolve(ImportStatus status)
        {
            if (EditorUtility.DisplayDialog("Record Package Result",
                "Finish or close Unity's import dialog and check the assets first. Record this package as "
                + status.ToString().ToLowerInvariant() + "?", "Record Result", "Back"))
                PackageImportQueue.Resolve(status);
        }

        private static void AddFolder()
        {
            var state = PackageImportQueue.State;
            string path = EditorUtility.OpenFolderPanel("Add Unity Packages", state.folder, "");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                state.folder = path;
                PackageImportQueue.Add(Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase)));
            }
            catch (Exception e) { state.message = e.Message; PackageImportQueue.Save(); }
        }

        private static void AddPackage()
        {
            string path = EditorUtility.OpenFilePanel("Add Unity Package", PackageImportQueue.State.folder, "unitypackage");
            if (!string.IsNullOrEmpty(path)) PackageImportQueue.Add(new[] { path });
        }

        private static void HandleDrop(bool busy)
        {
            var current = Event.current;
            if (busy || (current.type != EventType.DragUpdated && current.type != EventType.DragPerform)) return;
            var paths = DragAndDrop.paths.Where(p => p.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (paths.Length == 0) return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (current.type == EventType.DragPerform) { DragAndDrop.AcceptDrag(); PackageImportQueue.Add(paths); }
            current.Use();
        }
    }
}
