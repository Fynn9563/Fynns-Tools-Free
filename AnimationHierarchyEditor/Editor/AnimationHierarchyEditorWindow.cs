using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.AnimationHierarchy
{
    public sealed class AnimationHierarchyEditorWindow : EditorWindow
    {
        [SerializeField] private AnimationClip[] clips = Array.Empty<AnimationClip>();
        [SerializeField] private Animator rootAnimator;
        [SerializeField] private string oldText = "Root";
        [SerializeField] private string newText = "SomeNewObject/Root";
        [SerializeField] private bool useRegex;
        private readonly Dictionary<string, int> pathCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, string> pendingPaths = new Dictionary<string, string>();
        private string[] paths = Array.Empty<string>();
        private Vector2 scroll;
        private string status;
        private MessageType statusType;
        private GUIStyle creditStyle;
        [SerializeField] private int tab;
        [SerializeField] private MotionReplacementPanel motions = new MotionReplacementPanel();
        [SerializeField] private AnimationRepathingPanel repathing = new AnimationRepathingPanel();
        private double nextHierarchyCheck;

        [MenuItem("Tools/Fynn's Tools/Animation Tools", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimationHierarchyEditorWindow>("Animation Tools");
            window.titleContent.image = EditorGUIUtility.IconContent("AnimationClip Icon").image;
            window.minSize = new Vector2(680, 320);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(680, 320);
            titleContent = new GUIContent("Animation Tools", EditorGUIUtility.IconContent("AnimationClip Icon").image);
            if (motions == null) motions = new MotionReplacementPanel();
            if (repathing == null) repathing = new AnimationRepathingPanel();
            repathing.Stop();
            motions.Invalidate();
            Undo.undoRedoPerformed += RefreshPaths;
            Undo.undoRedoPerformed += OnPathUndo;
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.update += CheckTrackedHierarchy;
            if (tab == 0) ReadSelectedClips();
            RefreshPaths();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= RefreshPaths;
            Undo.undoRedoPerformed -= OnPathUndo;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.update -= CheckTrackedHierarchy;
            repathing?.Stop();
        }
        private void OnPathUndo() { repathing?.ResetAfterUndo(); Repaint(); }
        private void OnPlayMode(PlayModeStateChange state) { repathing?.Stop(); Repaint(); }
        private void OnHierarchyChange() { repathing?.HierarchyChanged(); Repaint(); }
        private void CheckTrackedHierarchy()
        {
            if (repathing == null || !repathing.Tracking || EditorApplication.timeSinceStartup < nextHierarchyCheck) return;
            nextHierarchyCheck = EditorApplication.timeSinceStartup + 0.25;
            repathing.HierarchyChanged(); Repaint();
        }

        private void OnSelectionChange()
        {
            if (tab == 0) ReadSelectedClips();
            Repaint();
        }

        private void ReadSelectedClips()
        {
            var selected = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets).Where(c => c).Distinct().ToArray();
            if (selected.Length == 0) return;
            clips = selected;
            status = null;
            RefreshPaths();
        }

        private void RefreshPaths()
        {
            motions?.Invalidate();
            pathCounts.Clear();
            pendingPaths.Clear();
            clips = (clips ?? Array.Empty<AnimationClip>()).Where(c => c).Distinct().ToArray();
            foreach (var clip in clips)
                foreach (var binding in AnimationUtility.GetCurveBindings(clip).Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip)))
                {
                    pathCounts.TryGetValue(binding.path, out int count);
                    pathCounts[binding.path] = count + 1;
                }
            paths = pathCounts.Keys.OrderBy(p => p, StringComparer.Ordinal).ToArray();
            foreach (var path in paths) pendingPaths[path] = path;
            Repaint();
        }

        private void OnGUI()
        {
            tab = GUILayout.Toolbar(tab, new[] { "Binding Paths", "Replace Motions", "Repath & Repair" }, GUILayout.Height(28));
            if (tab == 2)
            {
                repathing.Draw();
                DrawCredit();
                return;
            }
            if (tab == 1)
            {
                motions.Draw();
                DrawCredit();
                return;
            }
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                rootAnimator = (Animator)EditorGUILayout.ObjectField("Root Animator", rootAnimator, typeof(Animator), true);
                if (clips.Length <= 1)
                {
                    EditorGUI.BeginChangeCheck();
                    var clip = (AnimationClip)EditorGUILayout.ObjectField("Target Clip", clips.FirstOrDefault(), typeof(AnimationClip), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        clips = clip ? new[] { clip } : Array.Empty<AnimationClip>();
                        status = null;
                        RefreshPaths();
                    }
                }
                else EditorGUILayout.LabelField("Target Clips", $"{clips.Length} selected Animation Clips");
            }

            if (clips.Length == 0)
                EditorGUILayout.HelpBox("Select Animation Clips in the Project window, or assign a Target Clip above.", MessageType.Info);
            else
            {
                EditorGUILayout.Space(8);
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    oldText = EditorGUILayout.TextField(new GUIContent("", "Text or regex pattern to find"), oldText);
                    newText = EditorGUILayout.TextField(new GUIContent("", "Replacement text; regex supports $1 captures"), newText);
                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(oldText)))
                        if (GUILayout.Button("Replace Path", GUILayout.Width(100)))
                            Apply(oldText, newText, useRegex ? PathEditMode.Regex : PathEditMode.Text);
                    useRegex = GUILayout.Toggle(useRegex, new GUIContent("@", "Use Regex"), "Button", GUILayout.Width(25));
                }
                if (paths.Length == 0)
                    EditorGUILayout.HelpBox("These clips have no animation bindings.", MessageType.Info);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                foreach (var path in paths)
                    DrawPath(path);
                EditorGUILayout.EndScrollView();
            }

            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, statusType);
            GUILayout.FlexibleSpace();
            DrawCredit();
        }

        private void DrawPath(string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(pendingPaths[path] == path))
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Refresh")) { tooltip = "Reset this path field" }, GUILayout.Width(24)))
                        pendingPaths[path] = path;
                pendingPaths[path] = EditorGUILayout.TextField(new GUIContent("", AnimationPathEditor.DisplayPath(path)), pendingPaths[path]);
                using (new EditorGUI.DisabledScope(pendingPaths[path] == path))
                    if (GUILayout.Button("Apply", GUILayout.Width(55)))
                        Apply(path, pendingPaths[path], PathEditMode.Exact);
                GUILayout.Label($"({pathCounts[path]})", GUILayout.Width(40));
                var resolved = ResolveObject(path);
                var color = GUI.color;
                try
                {
                    GUI.color = !rootAnimator ? color : resolved ? Color.green : Color.red;
                    using (new EditorGUI.DisabledScope(!rootAnimator))
                    {
                        EditorGUI.BeginChangeCheck();
                        var selected = (GameObject)EditorGUILayout.ObjectField(resolved, typeof(GameObject), true, GUILayout.Width(160));
                        if (EditorGUI.EndChangeCheck() && selected)
                        {
                            if (TryGetObjectPath(rootAnimator, selected, out string newPath)) Apply(path, newPath, PathEditMode.Exact);
                            else SetStatus("Choose the Animator root or one of its children.", MessageType.Warning);
                        }
                    }
                }
                finally { GUI.color = color; }
            }
        }

        internal GameObject ResolveObject(string path)
        {
            if (!rootAnimator) return null;
            return string.IsNullOrEmpty(path) ? rootAnimator.gameObject : rootAnimator.transform.Find(path)?.gameObject;
        }

        internal static bool TryGetObjectPath(Animator animator, GameObject target, out string path)
        {
            path = null;
            if (!animator || !target || !target.transform.IsChildOf(animator.transform)) return false;
            path = AnimationUtility.CalculateTransformPath(target.transform, animator.transform);
            return true;
        }

        private void Apply(string oldPath, string newPath, PathEditMode mode)
        {
            bool applied = AnimationPathEditor.TryReplacePaths(clips, oldPath, newPath, mode, out int count, out string error);
            if (applied)
            {
                RefreshPaths();
                SetStatus(count == 0 ? "No matching paths changed." : $"Updated {count} animation binding(s). Undo is available with Ctrl+Z.", MessageType.Info);
            }
            else SetStatus(error, MessageType.Warning);
            GUIUtility.ExitGUI();
        }

        private void SetStatus(string message, MessageType type) { status = message; statusType = type; }

        private void DrawCredit()
        {
            if (creditStyle == null)
            {
                creditStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, alignment = TextAnchor.MiddleRight };
                var color = creditStyle.normal.textColor;
                color.a = 0.6f;
                creditStyle.normal.textColor = color;
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
