using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.AnimationHierarchy
{
    [Serializable] internal sealed class AnimationRepathingPanel
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Object controller;
        [SerializeField] private bool review = true;
        [SerializeField] private Vector2 scroll;
        [NonSerialized] private bool tracking;
        private HierarchyPathSnapshot snapshot;
        private Dictionary<string, string> pending = new Dictionary<string, string>();
        private List<BrokenBinding> broken;
        private Dictionary<string, string> repairs = new Dictionary<string, string>();
        private Dictionary<string, string> preview;
        private string fingerprint, message;
        private AnimationClip[] previewClips;
        private int previewCount;
        private MessageType messageType;
        internal bool Tracking => tracking;
        internal GameObject RepairObject(string source)
        {
            if (!root || !repairs.TryGetValue(source, out var path)) return null;
            return path.Length == 0 ? root : root.transform.Find(path)?.gameObject;
        }
        internal bool AssignRepairObject(string source, GameObject target)
        {
            if (target && (!root || !target.transform.IsChildOf(root.transform)))
            {
                Report("Choose the animation root or one of its children.", MessageType.Warning);
                return false;
            }
            repairs[source] = target ? AnimationUtility.CalculateTransformPath(target.transform, root.transform) : source;
            preview = null; message = null;
            return true;
        }

        internal void Stop() { tracking = false; snapshot = null; pending.Clear(); preview = null; }
        internal void ResetAfterUndo()
        {
            preview = null; broken = null;
            if (tracking) Rebase();
        }
        private void Rebase()
        {
            try { snapshot = new HierarchyPathSnapshot(root); pending.Clear(); }
            catch (Exception e) { Stop(); Report(e.Message, MessageType.Warning); }
        }
        internal void HierarchyChanged()
        {
            if (!tracking || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!root) { Stop(); Report("The animation root was removed.", MessageType.Warning); return; }
            try
            {
                var changes = snapshot.Changes();
                bool different = pending.Count != changes.Count || changes.Any(p => !pending.TryGetValue(p.Key, out var value) || value != p.Value);
                if (!different) return;
                pending = changes; preview = null; broken = null;
                if (pending.Count == 0) return;
                if (!review) { BuildPreview(pending); ApplyPreview(); }
            }
            catch (Exception e) { if (!root) Stop(); preview = null; Report(e.Message, MessageType.Warning); }
        }
        private void Report(string text, MessageType type = MessageType.Info) { message = text; messageType = type; }
        private void BuildPreview(Dictionary<string, string> map)
        {
            preview = null;
            if (!map.Any(p => p.Key != p.Value)) throw new InvalidOperationException("Choose a different replacement object or path first.");
            var clips = AnimationRepathing.Clips(root, controller);
            if (clips.Length == 0) throw new InvalidOperationException("No animation clips were found in these controllers.");
            AnimationRepathing.ValidateRepair(root, clips, map);
            if (!AnimationPathEditor.TryMapPaths(clips, map, false, out previewCount, out var error)) throw new InvalidOperationException(error);
            previewClips = clips; fingerprint = AnimationRepathing.Fingerprint(clips);
            preview = new Dictionary<string, string>(map); message = null;
        }
        private void ApplyPreview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before editing paths.");
            if (preview == null) throw new InvalidOperationException("Preview the changes first.");
            var clips = AnimationRepathing.Clips(root, controller);
            if (!new HashSet<AnimationClip>(clips).SetEquals(previewClips) || AnimationRepathing.Fingerprint(clips) != fingerprint)
                throw new InvalidOperationException("The controllers or bindings changed. Preview again.");
            if (tracking)
            {
                var current = snapshot.Changes();
                if (pending.Count != current.Count || pending.Any(p => !current.TryGetValue(p.Key, out var value) || value != p.Value))
                    throw new InvalidOperationException("The hierarchy changed. Preview again.");
            }
            AnimationRepathing.ValidateRepair(root, clips, preview);
            if (!AnimationPathEditor.TryMapPaths(clips, preview, true, out var count, out var error)) throw new InvalidOperationException(error);
            preview = null; repairs.Clear();
            broken = AnimationRepathing.Scan(root, clips);
            if (tracking) Rebase();
            Report("Updated " + count + " binding(s). " + (broken.Count == 0 ? "No broken bindings remain." : broken.Count + " broken binding(s) remain below.") + " Undo is available with Ctrl+Z.");
        }
        private void Run(Action action)
        {
            try { action(); }
            catch (Exception e) { preview = null; Report(e.Message, MessageType.Warning); }
        }
        internal void Draw()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                root = (GameObject)EditorGUILayout.ObjectField("Animation Root", root, typeof(GameObject), true);
                controller = EditorGUILayout.ObjectField(new GUIContent("Controller Override", "Optional: limit the scan to a specific controller."), controller, typeof(RuntimeAnimatorController), false);
                if (EditorGUI.EndChangeCheck()) { Stop(); broken = null; repairs.Clear(); message = null; }
                if (GUILayout.Button("Use Selected Root"))
                {
                    Stop(); root = Selection.activeGameObject; broken = null; repairs.Clear(); message = null;
                }
            }
            using (new EditorGUI.DisabledScope(!root || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var next = GUILayout.Toggle(tracking, "Auto Repath", "Button", GUILayout.Height(26), GUILayout.Width(130));
                    if (next != tracking)
                    {
                        if (!next) Stop();
                        else Run(() => { snapshot = new HierarchyPathSnapshot(root); tracking = true; message = null; });
                    }
                    review = EditorGUILayout.ToggleLeft("Review before applying", review);
                    if (GUILayout.Button("Scan Broken Bindings", GUILayout.Width(160))) Run(() =>
                    {
                        broken = AnimationRepathing.Scan(root, AnimationRepathing.Clips(root, controller));
                        repairs.Clear(); preview = null;
                        Report(broken.Count == 0 ? "No broken bindings found." : broken.Count + " broken binding(s) found.");
                    });
                }
            }
            if (tracking) EditorGUILayout.HelpBox(review ? "Tracking renames and moves while this window is open. Review pending changes below."
                : "Tracking renames and moves while this window is open. Matching clips are updated automatically; shared clips affect every controller that uses them.", MessageType.Info);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (pending.Count > 0)
            {
                EditorGUILayout.LabelField("Hierarchy Changes", EditorStyles.boldLabel);
                foreach (var p in pending) EditorGUILayout.LabelField(AnimationPathEditor.DisplayPath(p.Key) + "  →  " + AnimationPathEditor.DisplayPath(p.Value));
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Preview Repath")) Run(() => BuildPreview(pending));
                    if (GUILayout.Button("Discard Changes")) { Rebase(); preview = null; }
                }
            }
            if (broken != null && broken.Count > 0)
            {
                EditorGUILayout.LabelField("Broken Bindings", EditorStyles.boldLabel);
                foreach (var group in broken.GroupBy(b => b.binding.path))
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(AnimationPathEditor.DisplayPath(group.Key), EditorStyles.boldLabel);
                        foreach (var b in group) EditorGUILayout.LabelField(b.clip.name + " · " + b.binding.type.Name + "." + b.binding.propertyName + " · " + b.reason, EditorStyles.wordWrappedMiniLabel);
                        if (group.Any(b => b.reason == "Missing or unsupported property"))
                            EditorGUILayout.HelpBox("The target must contain the animated property. If the property name is invalid, correct it in the Animation window; changing the object path cannot create it.", MessageType.Info);
                        if (!repairs.ContainsKey(group.Key)) repairs[group.Key] = group.Key;
                        EditorGUI.BeginChangeCheck();
                        repairs[group.Key] = EditorGUILayout.TextField("Replacement Path", repairs[group.Key]);
                        if (EditorGUI.EndChangeCheck()) preview = null;
                        EditorGUI.BeginChangeCheck();
                        var target = (GameObject)EditorGUILayout.ObjectField("Pick Object", RepairObject(group.Key), typeof(GameObject), true);
                        if (EditorGUI.EndChangeCheck()) AssignRepairObject(group.Key, target);
                    }
                }
                if (GUILayout.Button("Preview Repairs")) Run(() => BuildPreview(repairs.Where(p => p.Key != p.Value).ToDictionary(p => p.Key, p => p.Value)));
            }
            if (preview != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Preview · " + previewCount + " binding(s)", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Path edits include all bindings at each listed source path. Shared clips change for all their users.", MessageType.Warning);
                foreach (var pair in preview) EditorGUILayout.LabelField(AnimationPathEditor.DisplayPath(pair.Key) + "  →  " + AnimationPathEditor.DisplayPath(pair.Value));
                foreach (var clip in previewClips.Where(c => AnimationRepathing.Bindings(c).Any(b => preview.TryGetValue(b.path, out var next) && next != b.path)))
                    EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(clip), EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, messageType);
            using (new EditorGUI.DisabledScope(preview == null || previewCount == 0 || EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("Apply Path Changes", GUILayout.Height(30))) Run(ApplyPreview);
        }
    }
}
