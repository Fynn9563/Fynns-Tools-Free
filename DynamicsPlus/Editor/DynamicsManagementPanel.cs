#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    [Serializable]
    internal sealed class DynamicsManagementPanel
    {
        [Serializable]
        private sealed class Row
        {
            public Component component;
            public bool organize;
            public bool remove;
            public string folder = "";
            [NonSerialized] public string blocked;
            [NonSerialized] public string removalBlocked;
        }
        [SerializeField] private GameObject scannedRoot;
        [SerializeField] private List<Row> rows = new List<Row>();
        [SerializeField] private Transform scope;
        [SerializeField] private List<Transform> excluded = new List<Transform>();
        [SerializeField] private string search = "";
        [SerializeField] private int kind;
        [SerializeField] private string bonePrefix = "";
        [SerializeField] private string colliderPrefix = "";
        [SerializeField] private bool sort = true;
        [SerializeField] private bool showScope;
        [SerializeField] private bool showSuggestions;
        [SerializeField] private bool highlight = true;
        [SerializeField] private float weightThreshold;
        [NonSerialized] private HashSet<Component> suggestions = new HashSet<Component>();
        [NonSerialized] private bool dirty = true;
        [NonSerialized] private string message;
        [NonSerialized] private bool error;
        private Vector2 scroll;

        internal void Invalidate() { dirty = true; }

        internal void Draw(GameObject root, bool removing)
        {
            if (root != scannedRoot)
            {
                scannedRoot = root; rows.Clear(); scope = null; excluded.Clear(); suggestions.Clear();
                message = null; dirty = true;
            }
            if (root == null) { EditorGUILayout.HelpBox("Choose a root to see its PhysBones and colliders.", MessageType.Info); return; }
            try { DynamicsManagement.RequireRoot(root); }
            catch (Exception exception) { EditorGUILayout.HelpBox(exception.Message, MessageType.Info); return; }
            if (dirty) Refresh(root);
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    search = EditorGUILayout.TextField("Search", search);
                    kind = EditorGUILayout.Popup(kind, new[] { "All", "PhysBones", "Colliders" }, GUILayout.Width(115));
                }
                showScope = EditorGUILayout.Foldout(showScope, "Scope", true);
                if (showScope)
                {
                    scope = (Transform)EditorGUILayout.ObjectField("Only this branch", scope, typeof(Transform), true);
                    for (int i = 0; i < excluded.Count; i++)
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            excluded[i] = (Transform)EditorGUILayout.ObjectField("Exclude branch", excluded[i], typeof(Transform), true);
                            if (GUILayout.Button("Remove", GUILayout.Width(65))) { excluded.RemoveAt(i); break; }
                        }
                    if (GUILayout.Button("Add excluded branch", GUILayout.Width(160))) excluded.Add(null);
                }
                if (change.changed) { ClearMarks(); message = null; }
            }
            if (scope != null && scope != root.transform && !scope.IsChildOf(root.transform) ||
                excluded.Any(t => t != null && t != root.transform && !t.IsChildOf(root.transform)))
            {
                EditorGUILayout.HelpBox("Scope and excluded branches must be inside the chosen root.", MessageType.Warning);
                return;
            }
            if (!removing)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bonePrefix = EditorGUILayout.TextField("PhysBone prefix", bonePrefix);
                    colliderPrefix = EditorGUILayout.TextField("Collider prefix", colliderPrefix);
                }
                sort = EditorGUILayout.ToggleLeft("Sort alphabetically", sort);
            }
            else
            {
                showSuggestions = EditorGUILayout.Foldout(showSuggestions, "Find candidates", true);
                if (showSuggestions)
                {
                    EditorGUILayout.HelpBox("These are suggestions, not proof that a component is unused. Props can work without mesh bones; colliders can use global collisions or scripts.", MessageType.Info);
                    weightThreshold = EditorGUILayout.Slider("Minimum mesh weight", weightThreshold, 0f, 0.5f);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("No weighted mesh bones")) Run(() => suggestions = DynamicsManagement.NoWeightedMeshBones(root, weightThreshold));
                        if (GUILayout.Button("Unreferenced colliders")) Run(() => suggestions = DynamicsManagement.UnreferencedColliders(root));
                        if (GUILayout.Button("Clear", GUILayout.Width(55))) suggestions.Clear();
                    }
                    EditorGUILayout.LabelField(suggestions.Count + " candidate(s). Select the ones you want to remove.", EditorStyles.miniLabel);
                }
            }
            var visible = rows.Where(r => Visible(root, r)).ToArray();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(visible.Length + " components");
                if (GUILayout.Button("Select visible", EditorStyles.toolbarButton))
                    foreach (var row in visible) SetMarked(row, removing, removing ? row.removalBlocked == null : row.blocked == null && !DynamicsManagement.Organized(root, row.component));
                if (GUILayout.Button("Clear selection", EditorStyles.toolbarButton)) ClearMarks();
                highlight = GUILayout.Toggle(highlight, "Scene highlights", EditorStyles.toolbarButton);
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var row in visible)
            {
                var c = row.component;
                bool organized = DynamicsManagement.Organized(root, c);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(removing ? row.removalBlocked != null : row.blocked != null || organized))
                            SetMarked(row, removing, EditorGUILayout.Toggle(Marked(row, removing), GUILayout.Width(18)));
                        using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField(c, typeof(Component), true);
                        GUILayout.Label(c is VRCPhysBone ? "PhysBone" : "Collider", EditorStyles.miniLabel, GUILayout.Width(60));
                        if (GUILayout.Button("Show", GUILayout.Width(48))) { Selection.activeGameObject = c.gameObject; EditorGUIUtility.PingObject(c); }
                    }
                    string path = AnimationUtility.CalculateTransformPath(c.transform, root.transform);
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(path) ? "(Root)" : path, EditorStyles.miniLabel);
                    if (removing)
                    {
                        if (row.removalBlocked != null) EditorGUILayout.LabelField(row.removalBlocked, EditorStyles.wordWrappedMiniLabel);
                        if (suggestions.Contains(c)) EditorGUILayout.LabelField("Candidate — review before selecting", EditorStyles.miniLabel);
                    }
                    else if (row.blocked != null || organized)
                        EditorGUILayout.LabelField(row.blocked ?? "Already organized", EditorStyles.wordWrappedMiniLabel);
                    else
                    {
                        row.folder = EditorGUILayout.TextField("Subfolder", row.folder);
                        EditorGUILayout.LabelField("PhysBones/" + (c is VRCPhysBone ? "Bones/" : "Colliders/") +
                            (row.folder == "" ? "" : row.folder + "/") + (c is VRCPhysBone ? bonePrefix : colliderPrefix) + c.name, EditorStyles.miniLabel);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            var chosen = visible.Where(r => Marked(r, removing)).ToArray();
            if (message != null) EditorGUILayout.HelpBox(message, error ? MessageType.Error : MessageType.Info);
            using (new EditorGUI.DisabledScope(chosen.Length == 0))
                if (GUILayout.Button((removing ? "Remove selected" : "Organize selected") + " (" + chosen.Length + ")", GUILayout.Height(28)))
                    Run(() =>
                    {
                        var targets = chosen.Select(r => r.component).ToArray();
                        DynamicsManagement.RequireTargets(root, targets);
                        if (removing)
                        {
                            int references = DynamicsManagement.ReferenceCount(root, targets);
                            if (!EditorUtility.DisplayDialog("Remove dynamics components",
                                "Remove " + targets.Length + " selected component(s)?\n" + references + " scene reference(s) will be cleared. GameObjects and bones are kept.\n\nUndo restores this operation.", "Remove", "Cancel")) return;
                            DynamicsManagement.Remove(root, targets);
                        }
                        else
                        {
                            if (!EditorUtility.DisplayDialog("Organize dynamics components",
                                "Move " + targets.Length + " selected component(s) into PhysBones folders?\nEffective roots and serialized scene references are preserved.\n\nScripts that control the original host's activation or find components by location may need updating. Undo restores this operation.", "Organize", "Cancel")) return;
                            DynamicsManagement.Organize(root, chosen.Select(r => new DynamicsManagement.Move
                            {
                                source = r.component, folder = r.folder, name = (r.component is VRCPhysBone ? bonePrefix : colliderPrefix) + r.component.name
                            }).ToArray(), sort);
                        }
                        message = (removing ? "Removed " : "Organized ") + targets.Length + " component(s).";
                        rows.Clear(); dirty = true; suggestions.Clear(); SceneView.RepaintAll();
                    });
            if (GUI.changed) SceneView.RepaintAll();
        }

        private void Refresh(GameObject root)
        {
            var previous = rows.Where(r => r.component != null).ToDictionary(r => r.component);
            var animations = DynamicsManagement.AnimationSources(DynamicsManagement.SceneComponents(root));
            rows = DynamicsManagement.Scan(root).Select(c => previous.TryGetValue(c, out var row) ? row : new Row { component = c }).ToList();
            foreach (var row in rows)
            {
                row.blocked = DynamicsManagement.AnimationBlock(row.component, animations);
                row.removalBlocked = null;
                if (PrefabUtility.IsPartOfPrefabInstance(row.component) && !PrefabUtility.IsAddedComponentOverride(row.component))
                    row.blocked = row.removalBlocked = "Open its source prefab in Prefab Mode, or unpack it first.";
                if (row.blocked != null) row.organize = false;
            }
            dirty = false;
        }

        private bool Visible(GameObject root, Row row)
        {
            var c = row.component;
            if (c == null || kind == 1 && !(c is VRCPhysBone) || kind == 2 && !(c is VRCPhysBoneCollider)) return false;
            var effective = DynamicsOperations.Root(c);
            if (scope != null && effective != scope && !effective.IsChildOf(scope)) return false;
            if (excluded.Any(t => t != null && (effective == t || effective.IsChildOf(t)))) return false;
            return (c.name + " " + AnimationUtility.CalculateTransformPath(c.transform, root.transform) + " " + c.GetType().Name)
                .IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool Marked(Row row, bool removing) { return removing ? row.remove : row.organize; }
        private static void SetMarked(Row row, bool removing, bool value) { if (removing) row.remove = value; else row.organize = value; }
        private void ClearMarks() { foreach (var row in rows) { row.remove = false; row.organize = false; } SceneView.RepaintAll(); }
        private void Run(Action action)
        {
            error = false;
            try { action(); }
            catch (ExitGUIException) { throw; }
            catch (Exception exception) { message = exception.Message; error = true; }
        }

        internal void DrawHighlights(GameObject root, bool removing)
        {
            if (!highlight || root == null || DynamicsPreview.Active != null || SceneView.currentDrawingSceneView == null || SceneView.currentDrawingSceneView.camera == null) return;
            var previousColor = Handles.color;
            foreach (var row in rows.Where(r => Marked(r, removing) && Visible(root, r)))
            {
                var position = DynamicsOperations.Root(row.component).position;
                Handles.color = removing ? new Color(1f, 0.35f, 0.3f) : new Color(0.3f, 0.8f, 1f);
                Handles.DrawWireDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward, HandleUtility.GetHandleSize(position) * 0.04f);
                Handles.Label(position, row.component.name);
            }
            Handles.color = previousColor;
        }
    }
}
#endif
