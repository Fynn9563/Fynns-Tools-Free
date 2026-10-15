using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.AnimationHierarchy
{
    [Serializable] internal sealed class MotionReplacementPanel
    {
        [SerializeField] private Object target;
        [SerializeField] private List<MotionRule> rules = new List<MotionRule>();
        [SerializeField] private Vector2 scroll;
        private List<MotionSlot> preview;
        private List<MotionSlot> scanned;
        private string message;
        private MessageType messageType;
        internal void Invalidate() { preview = null; scanned = null; message = null; }
        private void Refresh()
        {
            preview = null; message = null;
            try
            {
                scanned = MotionReplacement.Scan(MotionReplacement.Controllers(target));
                rules = scanned.Select(s => s.before).Distinct().Select(m => rules.FirstOrDefault(r => r.source == m) ?? new MotionRule { source = m }).ToList();
            }
            catch (Exception e) { scanned = null; rules.Clear(); message = e.Message; messageType = MessageType.Info; }
        }
        internal void Draw()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                var next = EditorGUILayout.ObjectField("Controller / Avatar", target, typeof(Object), true);
                if (EditorGUI.EndChangeCheck()) { target = next; rules.Clear(); Refresh(); }
                if (GUILayout.Button("Use Selection", GUILayout.Width(105))) { target = Selection.activeObject; rules.Clear(); Refresh(); }
                if (GUILayout.Button("Refresh", GUILayout.Width(65))) Refresh();
            }
            if (!target) EditorGUILayout.HelpBox("Choose an Animator Controller, Override Controller, or an avatar/Animator object.", MessageType.Info);
            else if (scanned == null && message == null) Refresh();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (scanned != null)
            {
                EditorGUILayout.LabelField(MotionReplacement.Controllers(target).Length + " controller(s) · " + scanned.Count + " motion slots", EditorStyles.miniLabel);
                foreach (var rule in rules)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        rule.enabled = EditorGUILayout.Toggle(rule.enabled, GUILayout.Width(20));
                        using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField(rule.source, typeof(Motion), false);
                        if (!rule.source) GUILayout.Label("Empty", EditorStyles.miniLabel, GUILayout.Width(38));
                        GUILayout.Label("to", GUILayout.Width(18));
                        using (new EditorGUI.DisabledScope(!rule.enabled)) rule.replacement = (Motion)EditorGUILayout.ObjectField(rule.replacement, typeof(Motion), false);
                        if (EditorGUI.EndChangeCheck()) { preview = null; message = null; }
                    }
                }
                if (rules.Any(r => r.enabled && r.source && !r.replacement))
                    EditorGUILayout.HelpBox("Checked rows with an empty replacement will clear those motion slots.", MessageType.Warning);
                if (preview != null)
                {
                    EditorGUILayout.Space(8);
                    EditorGUILayout.LabelField("Replacement Preview", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(preview.Count + " slot(s) in " + preview.Select(s => AssetDatabase.GetAssetPath(s.owner)).Distinct().Count() + " asset(s)");
                    if (preview.Any(s => s.owner is UnityEditor.Animations.BlendTree))
                        EditorGUILayout.HelpBox("Blend Tree edits affect every controller that shares that tree. The affected assets are shown below.", MessageType.Warning);
                    foreach (var edit in preview)
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField(edit.location, EditorStyles.boldLabel);
                            EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(edit.owner), EditorStyles.miniLabel);
                            using (new EditorGUILayout.HorizontalScope())
                            using (new EditorGUI.DisabledScope(true))
                            {
                                EditorGUILayout.ObjectField(edit.before, typeof(Motion), false);
                                GUILayout.Label("to", GUILayout.Width(18));
                                EditorGUILayout.ObjectField(edit.after, typeof(Motion), false);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, messageType);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!target || !rules.Any(r => r.enabled && r.source != r.replacement) || EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("Preview Changes", GUILayout.Height(30)))
                    {
                        try { preview = MotionReplacement.Preview(MotionReplacement.Controllers(target), rules); message = preview.Count == 0 ? "No matching motion slots would change." : null; messageType = MessageType.Info; }
                        catch (Exception e) { preview = null; message = e.Message; messageType = MessageType.Warning; }
                    }
                using (new EditorGUI.DisabledScope(preview == null || preview.Count == 0 || EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("Apply Replacements", GUILayout.Height(30)))
                    {
                        try
                        {
                            var current = MotionReplacement.Preview(MotionReplacement.Controllers(target), rules);
                            if (current.Count != preview.Count || current.Where((e, i) => e.owner != preview[i].owner || e.location != preview[i].location || e.before != preview[i].before || e.after != preview[i].after).Any())
                                throw new InvalidOperationException("The controller changed since the preview. Preview the replacements again.");
                            var count = MotionReplacement.Apply(current); Refresh(); message = "Replaced " + count + " motion slot(s). Undo is available with Ctrl+Z."; messageType = MessageType.Info;
                        }
                        catch (Exception e) { preview = null; message = e.Message; messageType = MessageType.Warning; }
                    }
            }
        }
    }
}
