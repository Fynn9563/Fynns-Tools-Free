using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.BuildSizes
{
    public sealed class BuildSizeViewerWindow : EditorWindow
    {
        [SerializeField] private string logPath;
        [SerializeField] private SizeReport report;
        [SerializeField] private string message;
        [SerializeField] private bool warning;
        [SerializeField] private string search = "";
        [SerializeField] private bool showDetails = true;
        [SerializeField] private float detailsHeight = 150;
        private Vector2 assetScroll;
        private Vector2 categoryScroll;
        private GUIStyle creditStyle;

        [MenuItem("Tools/Fynn's Tools/Build Size Viewer", false, 71)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildSizeViewerWindow>("Build Size Viewer");
            window.minSize = new Vector2(640, 420);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(640, 420);
            if (string.IsNullOrEmpty(logPath)) logPath = Application.consoleLogPath;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Build Size Viewer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Build an avatar or world, then read the log to see its size breakdown.", EditorStyles.wordWrappedMiniLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                logPath = EditorGUILayout.TextField("Editor Log", logPath);
                if (EditorGUI.EndChangeCheck()) ClearReport();
                if (GUILayout.Button("Browse", GUILayout.Width(65)))
                {
                    string selected = EditorUtility.OpenFilePanel("Open Editor Log", Path.GetDirectoryName(Application.consoleLogPath), "log");
                    if (!string.IsNullOrEmpty(selected)) { logPath = selected; ClearReport(); }
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Current Log", GUILayout.Width(100))) { logPath = Application.consoleLogPath; ClearReport(); }
                if (GUILayout.Button("Read Build Log")) ReadLog();
            }
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, warning ? MessageType.Warning : MessageType.Info);
            if (report != null) DrawReport();
            else GUILayout.FlexibleSpace();
            DrawCredit();
        }

        private void ClearReport() { report = null; message = null; assetScroll = Vector2.zero; categoryScroll = Vector2.zero; }

        private void ReadLog()
        {
            var result = BuildSizeLog.Read(logPath);
            report = result.report; message = result.message; warning = result.warning;
            assetScroll = Vector2.zero; categoryScroll = Vector2.zero;
            Repaint();
        }

        private void DrawReport()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent(report.context, report.context), EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Compressed Build Size", report.compressedSize ?? "Not recorded", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("The rows below show uncompressed sizes; they do not add up to the compressed download size.", EditorStyles.wordWrappedMiniLabel);
                if (report.categories.Count > 0)
                {
                    showDetails = EditorGUILayout.Foldout(showDetails, "Uncompressed Size by Category", true);
                    if (showDetails)
                    {
                        float maximum = Mathf.Max(30, Mathf.Min(position.height * 0.4f, position.height - 360));
                        detailsHeight = Mathf.Clamp(detailsHeight, 30, maximum);
                        categoryScroll = EditorGUILayout.BeginScrollView(categoryScroll, GUILayout.Height(detailsHeight));
                        foreach (string category in report.categories) EditorGUILayout.LabelField(category, EditorStyles.miniLabel);
                        EditorGUILayout.EndScrollView();
                        DrawResizeHandle(maximum);
                    }
                }
            }
            search = EditorGUILayout.TextField("Filter Assets", search);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("%", GUILayout.Width(60));
                GUILayout.Label("Size", GUILayout.Width(85));
                GUILayout.Label("Asset Path");
                GUILayout.Space(45);
            }
            var rows = report.assets.Where(a => string.IsNullOrEmpty(search) || a.path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            assetScroll = EditorGUILayout.BeginScrollView(assetScroll);
            foreach (var asset in rows)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    Color color = GUI.contentColor;
                    if (asset.percent > 10) GUI.contentColor = new Color(1, 0.7f, 0.2f);
                    GUILayout.Label(asset.percent.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%", GUILayout.Width(60));
                    GUI.contentColor = color;
                    GUILayout.Label(asset.size, GUILayout.Width(85));
                    GUILayout.Label(new GUIContent(asset.path, asset.path), GUILayout.MinWidth(40), GUILayout.ExpandWidth(true));
                    using (new EditorGUI.DisabledScope(asset.path.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase)))
                        if (GUILayout.Button("Go", GUILayout.Width(40))) SelectAsset(asset.path);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField(rows.Length + " of " + report.assets.Count + " assets", EditorStyles.miniLabel);
        }

        public static Object ResolveAsset(string path, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(path)) { error = "Asset path is empty."; return null; }
            path = path.Replace('\\', '/');
            if (path.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase)) { error = "This is a built-in Unity resource."; return null; }
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (!asset && !path.StartsWith("Assets/", StringComparison.Ordinal) && !path.StartsWith("Packages/", StringComparison.Ordinal))
                asset = AssetDatabase.LoadMainAssetAtPath("Assets/" + path);
            if (asset) return asset;
            string filename = Path.GetFileName(path);
            if (string.IsNullOrEmpty(filename)) { error = "Asset path has no filename."; return null; }
            var matches = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename))
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => string.Equals(Path.GetFileName(p), filename, StringComparison.OrdinalIgnoreCase)).Distinct().ToArray();
            if (matches.Length == 1) return AssetDatabase.LoadMainAssetAtPath(matches[0]);
            error = matches.Length > 1 ? "Several assets match " + filename + ". The log path is no longer exact; no asset was selected."
                : "Asset not found: " + path;
            return null;
        }

        private void SelectAsset(string path)
        {
            var asset = ResolveAsset(path, out string error);
            if (asset) { Selection.activeObject = asset; EditorGUIUtility.PingObject(asset); }
            else { message = error; warning = true; }
        }

        private void DrawResizeHandle(float maximum)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 8, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(new Rect(rect.x + 10, rect.y + 3, rect.width - 20, 2), new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);
            int control = GUIUtility.GetControlID(FocusType.Passive);
            var current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition)) { GUIUtility.hotControl = control; current.Use(); }
            if (GUIUtility.hotControl != control) return;
            if (current.type == EventType.MouseDrag) { detailsHeight = Mathf.Clamp(detailsHeight + current.delta.y, 30, maximum); Repaint(); current.Use(); }
            if (current.type == EventType.MouseUp) { GUIUtility.hotControl = 0; current.Use(); }
        }

        private void DrawCredit()
        {
            if (creditStyle == null) creditStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, alignment = TextAnchor.MiddleRight };
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Original by MunifiSense", "https://github.com/MunifiSense/VRChat-Build-Size-Viewer"), creditStyle))
                    Application.OpenURL("https://github.com/MunifiSense/VRChat-Build-Size-Viewer");
                if (GUILayout.Button(new GUIContent("Created by Fynn9563", "https://github.com/Fynn9563"), creditStyle)) Application.OpenURL("https://github.com/Fynn9563");
            }
        }
    }
}
