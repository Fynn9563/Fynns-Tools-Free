using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.AssetOrganization
{
    public sealed class AssetOrganizerWindow : EditorWindow
    {
        private const string SettingsKey = "FynnsTools.AssetOrganizer.Settings";
        private const string RecoveryKey = "FynnsTools.AssetOrganizer.LastMove";
        [SerializeField] private Object mainAsset;
        [SerializeField] private string destination = "";
        [SerializeField] private OrganizerSettings settings;
        [SerializeField] private List<AssetEntry> entries = new List<AssetEntry>();
        [SerializeField] private int tab;
        [SerializeField] private int optionsTab;
        private List<AssetMove> plan = new List<AssetMove>();
        private string planError;
        private string status;
        private MessageType statusType;
        private MoveReport lastMove;
        private Vector2 scroll;
        private GUIStyle creditStyle;

        [MenuItem("Tools/Fynn's Tools/Asset Organizer", false, 70)]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetOrganizerWindow>("Asset Organizer");
            window.minSize = new Vector2(740, 400);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(740, 400);
            if (settings == null)
            {
                try { settings = JsonUtility.FromJson<OrganizerSettings>(EditorPrefs.GetString(SettingsKey, "")); }
                catch (ArgumentException) { settings = null; }
            }
            settings = settings ?? OrganizerSettings.Defaults();
            settings.Validate();
            entries = entries ?? new List<AssetEntry>();
            if (mainAsset is GameObject && (string.IsNullOrWhiteSpace(destination) || destination.TrimEnd('/', '\\') == "Assets"))
                destination = DefaultDestination(mainAsset);
            try { lastMove = JsonUtility.FromJson<MoveReport>(SessionState.GetString(RecoveryKey, "")); }
            catch (ArgumentException) { lastMove = null; }
            RefreshPlan(true);
        }

        private void OnDisable() => SaveSettings();
        private void SaveSettings() { if (settings != null) EditorPrefs.SetString(SettingsKey, JsonUtility.ToJson(settings)); }

        private void OnGUI()
        {
            tab = GUILayout.Toolbar(tab, new[] { "Organizer", "Options" });
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (tab == 0) DrawOrganizer(); else DrawOptions();
            EditorGUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, statusType);
            DrawCredit();
        }

        private void DrawOrganizer()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    mainAsset = EditorGUILayout.ObjectField("Avatar / Main Asset", mainAsset, typeof(Object), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetSource(mainAsset);
                    }
                    using (new EditorGUI.DisabledScope(!mainAsset))
                        if (GUILayout.Button("Get Assets", GUILayout.Width(90))) Scan();
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    destination = EditorGUILayout.TextField("Destination Folder", destination);
                    if (EditorGUI.EndChangeCheck()) RefreshPlan(true);
                    if (GUILayout.Button("...", GUILayout.Width(28)))
                    {
                        string selected = EditorUtility.OpenFolderPanel("Destination Folder", "Assets", "");
                        if (!string.IsNullOrEmpty(selected))
                        {
                            string path = FileUtil.GetProjectRelativePath(selected);
                            if (AssetOrganizerOperations.TryFolder(path, out path, out string error)) { destination = path; RefreshPlan(true); }
                            else SetStatus(error, MessageType.Warning);
                        }
                    }
                }
            }

            if (entries.Count == 0) EditorGUILayout.HelpBox("Drag an avatar from the Hierarchy or Project window to gather its used assets, or choose a folder to scan its contents. Then press Get Assets. Moves also affect other avatars sharing those assets; their references are preserved.", MessageType.Info);
            else
            {
                EditorGUILayout.LabelField($"{entries.Count} assets · {plan.Count} moves", EditorStyles.boldLabel);
                foreach (var entry in entries)
                {
                    bool alreadyOrganized = IsAlreadyOrganized(entry);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(new GUIContent(entry.path, entry.typeName), EditorStyles.label))
                                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(entry.path));
                            GUILayout.Label(entry.category.ToString(), GUILayout.Width(75));
                            using (new EditorGUI.DisabledScope(entry.blockedReason != null))
                            {
                                EditorGUI.BeginChangeCheck();
                                var action = (OrganizeAction)EditorGUILayout.EnumPopup(entry.action, GUILayout.Width(65));
                                if (EditorGUI.EndChangeCheck()) { entry.action = action; entry.automaticallySkipped = false; RefreshPlan(); }
                            }
                        }
                        var move = plan.FirstOrDefault(m => m.guid == entry.guid);
                        if (entry.blockedReason != null || !alreadyOrganized)
                            EditorGUILayout.LabelField(entry.blockedReason ?? (move != null ? "→ " + move.destination : entry.action == OrganizeAction.Skip ? "Skip" : "Already in its destination"), EditorStyles.miniLabel);
                    }
                }
            }
            if (!string.IsNullOrEmpty(planError)) EditorGUILayout.HelpBox(planError, MessageType.Warning);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(plan.Count == 0 || planError != null))
                    if (GUILayout.Button("Organize Assets")) Organize();
                using (new EditorGUI.DisabledScope(lastMove == null || lastMove.moved.Count == 0))
                    if (GUILayout.Button("Revert Last Move")) RevertLast();
            }
        }

        private void DrawOptions()
        {
            optionsTab = GUILayout.Toolbar(optionsTab, new[] { "Folders", "Types" });
            EditorGUI.BeginChangeCheck();
            if (optionsTab == 0)
            {
                EditorGUILayout.HelpBox("The first enabled folder-name rule overrides the type rule. These rules match folders anywhere in an asset's path.", MessageType.None);
                for (int i = 0; i < settings.folders.Count; i++)
                {
                    var rule = settings.folders[i];
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        var color = GUI.backgroundColor;
                        GUI.backgroundColor = rule.enabled ? Color.green : Color.gray;
                        rule.enabled = GUILayout.Toggle(rule.enabled, rule.enabled ? "Enabled" : "Disabled", "Button", GUILayout.Width(80));
                        GUI.backgroundColor = color;
                        rule.name = EditorGUILayout.TextField(rule.name ?? "");
                        rule.action = (OrganizeAction)EditorGUILayout.EnumPopup(rule.action, GUILayout.Width(65));
                        if (GUILayout.Button("X", GUILayout.Width(22))) { settings.folders.RemoveAt(i); GUI.changed = true; break; }
                    }
                }
                if (GUILayout.Button("Add")) { settings.folders.Add(new FolderRule("")); GUI.changed = true; }
            }
            else
                foreach (AssetCategory category in Enum.GetValues(typeof(AssetCategory)))
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(category.ToString());
                        settings.types[(int)category] = (OrganizeAction)EditorGUILayout.EnumPopup(settings.types[(int)category], GUILayout.Width(65));
                    }
            EditorGUILayout.Space(8);
            settings.deleteEmptyFolders = EditorGUILayout.Toggle("Delete Empty Folders", settings.deleteEmptyFolders);
            settings.sort = (AssetSort)EditorGUILayout.EnumPopup("Sort Search By", settings.sort);
            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings();
                if (entries.Count > 0 && mainAsset) Scan();
            }
        }

        private void SetSource(Object source)
        {
            mainAsset = source;
            entries.Clear();
            destination = DefaultDestination(source);
            RefreshPlan();
        }

        private static string DefaultDestination(Object source)
        {
            if (!source) return "";
            if (source is GameObject)
            {
                string name = source.name;
                foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
                name = name.Trim().TrimEnd(' ', '.');
                if (string.IsNullOrWhiteSpace(name)) name = "Avatar";
                string stem = name.Split('.')[0].ToUpperInvariant();
                if (stem == "CON" || stem == "PRN" || stem == "AUX" || stem == "NUL" ||
                    Enumerable.Range(1, 9).Any(i => stem == "COM" + i || stem == "LPT" + i)) name = "_" + name;
                return "Assets/" + name;
            }
            string path = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(path) || !(path == "Assets" || path.StartsWith("Assets/", StringComparison.Ordinal))) return "";
            return AssetDatabase.IsValidFolder(path) ? path : Path.GetDirectoryName(path).Replace('\\', '/');
        }

        private void Scan()
        {
            try
            {
                entries = AssetOrganizerOperations.Scan(mainAsset, settings);
                status = null;
                RefreshPlan(true);
            }
            catch (Exception exception) { entries.Clear(); RefreshPlan(); SetStatus(exception.Message, MessageType.Warning); }
        }

        private bool IsAlreadyOrganized(AssetEntry entry)
        {
            return AssetOrganizerOperations.TryFolder(destination, out string folder, out _) &&
                string.Equals(entry.path, folder + "/" + entry.category + "/" + Path.GetFileName(entry.path), StringComparison.OrdinalIgnoreCase);
        }

        private void RefreshPlan(bool applyDefaults = false)
        {
            foreach (var entry in entries)
            {
                bool organized = IsAlreadyOrganized(entry);
                if (entry.automaticallySkipped && !organized)
                {
                    entry.action = OrganizeAction.Move;
                    entry.automaticallySkipped = false;
                }
                if (applyDefaults && organized && entry.action == OrganizeAction.Move)
                {
                    entry.action = OrganizeAction.Skip;
                    entry.automaticallySkipped = true;
                }
            }
            AssetOrganizerOperations.TryPlan(entries, destination, out plan, out planError);
        }

        private void Organize()
        {
            var report = AssetOrganizerOperations.Execute(entries, destination, settings.deleteEmptyFolders);
            if (report.moved.Count > 0)
            {
                lastMove = report;
                SessionState.SetString(RecoveryKey, JsonUtility.ToJson(lastMove));
            }
            entries.Clear();
            RefreshPlan();
            SetStatus($"Moved {report.moved.Count}; skipped {report.skipped}; failed {report.errors.Count}." + (report.errors.Count == 0 ? "" : "\n" + string.Join("\n", report.errors)), report.errors.Count == 0 ? MessageType.Info : MessageType.Warning);
            GUIUtility.ExitGUI();
        }

        private void RevertLast()
        {
            var result = AssetOrganizerOperations.Revert(lastMove);
            if (result.moved.Count > 0)
            {
                var restored = new HashSet<string>(result.moved.Select(m => m.guid));
                lastMove.moved.RemoveAll(m => restored.Contains(m.guid));
                SessionState.SetString(RecoveryKey, JsonUtility.ToJson(lastMove));
            }
            entries.Clear();
            RefreshPlan();
            SetStatus($"Restored {result.moved.Count}; failed {result.errors.Count}." + (result.errors.Count == 0 ? "" : "\n" + string.Join("\n", result.errors)), result.errors.Count == 0 ? MessageType.Info : MessageType.Warning);
            GUIUtility.ExitGUI();
        }

        private void SetStatus(string message, MessageType type) { status = message; statusType = type; }

        private void DrawCredit()
        {
            if (creditStyle == null)
            {
                creditStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, alignment = TextAnchor.MiddleRight };
                var color = creditStyle.normal.textColor; color.a = 0.6f; creditStyle.normal.textColor = color;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Created by Fynn9563", "https://github.com/Fynn9563"), creditStyle, GUILayout.Width(120), GUILayout.Height(18)))
                    Application.OpenURL("https://github.com/Fynn9563");
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            }
        }
    }
}
