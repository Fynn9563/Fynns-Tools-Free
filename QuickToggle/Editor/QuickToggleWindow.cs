#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsToolkit.QuickToggle
{
    internal sealed class QuickToggleWindow : EditorWindow
    {
        [SerializeField] private GameObject root;
        [SerializeField] private List<ToggleTarget> targets = new List<ToggleTarget>();
        [SerializeField] private List<ShapeTarget> shapes = new List<ShapeTarget>();
        [SerializeField] private int tab;
        [SerializeField] private SetupMode setup;
        [SerializeField] private SplitMode split;
        [SerializeField] private string toggleName = "Toggle", parameter = "Toggle", menuPath = "", folder = "Assets/Quick Toggles";
        [SerializeField] private bool opposite = true, loop, saved = true, defaultOn, writeDefaults, fx = true, parameters = true, menu = true;
        [SerializeField] private Object targetMenu;
        [SerializeField] private int frames = 1;
        private Vector2 scroll;
        private string error, result;
        private bool check = true;
        private HashSet<SkinnedMeshRenderer> syncedRenderers;
        private readonly bool[] available = new bool[4];
        private static readonly string[] SetupNames = { "Animation Clips", "VRChat (direct)", "VRCFury", "Modular Avatar" };

        [MenuItem("Tools/Fynn's Tools/Quick Toggle", false, 12)]
        internal static void Open()
        {
            var w = GetWindow<QuickToggleWindow>("Quick Toggle");
            w.minSize = new Vector2(620, 520);
            if (w.root == null) w.UseSelection();
            w.check = true;
        }
        [MenuItem("GameObject/Fynn's Tools/Quick Toggle", false, 20)]
        private static void OpenSelection()
        {
            Open();
            var w = GetWindow<QuickToggleWindow>(); w.UseSelection();
        }
        private void OnEnable()
        {
            minSize = new Vector2(620, 520);
            for (var i = 0; i < available.Length; i++) available[i] = QuickToggleIntegrations.Available((SetupMode)i);
            EditorApplication.hierarchyChanged += Invalidate;
            Undo.undoRedoPerformed += Invalidate;
            check = true;
        }
        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= Invalidate;
            Undo.undoRedoPerformed -= Invalidate;
        }
        private void Invalidate() { check = true; Repaint(); }
        private void UseSelection()
        {
            var objects = Selection.gameObjects.Where(g => g.scene.IsValid() && !EditorUtility.IsPersistent(g)).ToArray();
            if (objects.Length == 0) return;
            var first = objects[0].transform;
            var avatar = first.GetComponentsInParent<Component>(true).FirstOrDefault(c => c != null && c.GetType().FullName == "VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
            root = avatar != null ? avatar.gameObject : first.root.gameObject;
            targets = objects.Where(g => g != root).Select(g => new ToggleTarget { target = g }).ToList();
            SyncTargetShapes();
            if (objects.Length == 1 && objects[0] != root) toggleName = parameter = objects[0].name;
            check = true;
        }
        internal void SyncTargetShapes()
        {
            var renderers = new HashSet<SkinnedMeshRenderer>();
            foreach (var target in targets)
            {
                if (target == null || target.target == null) continue;
                var go = target.target as GameObject ?? (target.target as Component)?.gameObject;
                if (go == null) continue;
                foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    if (renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0) renderers.Add(renderer);
            }
            if (syncedRenderers != null && syncedRenderers.SetEquals(renderers)) return;
            syncedRenderers = renderers;
            if (shapes.RemoveAll(s => s.fromTargets && (s.renderer == null || !renderers.Contains(s.renderer))) > 0) check = true;
            foreach (var renderer in renderers)
            {
                if (shapes.Any(s => s.renderer == renderer)) continue;
                shapes.Add(new ShapeTarget { renderer = renderer, shape = renderer.sharedMesh.GetBlendShapeName(0), fromTargets = true });
                check = true;
            }
        }
        private ToggleRequest Request()
        {
            var r = new ToggleRequest { root = root, name = toggleName, parameter = parameter, menuPath = menuPath,
                folder = folder, setup = setup, split = split, blendshapes = tab == 1, opposite = opposite, loop = loop,
                frames = frames, saved = saved, defaultOn = defaultOn, writeDefaults = writeDefaults,
                targetMenu = targetMenu, fx = fx, parameters = parameters, menu = menu };
            r.targets.AddRange(targets); r.shapes.AddRange(shapes); return r;
        }
        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Quick Toggle", EditorStyles.boldLabel);
            SyncTargetShapes();
            EditorGUI.BeginChangeCheck();
            var nextTab = GUILayout.Toolbar(tab, new[] { "Objects & Components", "Blendshapes", "Settings" }, GUILayout.Height(28));
            if (nextTab != tab) { tab = nextTab; if (tab == 0 && split == SplitMode.PerBlendshape) split = SplitMode.Together; check = true; result = null; }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (tab == 2) DrawSettings();
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    root = (GameObject)EditorGUILayout.ObjectField("Root / Avatar", root, typeof(GameObject), true);
                    if (GUILayout.Button("Use Selection", GUILayout.Width(110))) UseSelection();
                }
                toggleName = EditorGUILayout.TextField("Toggle Name", toggleName);
                DrawSetup();
                EditorGUILayout.Space(8);
                if (tab == 0) DrawObjects(); else DrawShapes();
                EditorGUILayout.Space(8);
                if (tab == 0) split = (SplitMode)EditorGUILayout.Popup("Create", (int)split, new[] { "One toggle together", "One toggle per object" });
                else split = (SplitMode)EditorGUILayout.Popup("Create", (int)split, new[] { "One toggle together", "One toggle per renderer", "One toggle per blendshape" });
                if (setup == SetupMode.ClipsOnly)
                    opposite = EditorGUILayout.Toggle("Create Off Clip", opposite);
                if (setup == SetupMode.VRCFury && tab == 0 && targets.Any(t => t != null && t.target is Component))
                    EditorGUILayout.HelpBox("Component enable/disable needs an animation clip. Objects and blendshapes use native VRCFury actions.", MessageType.None);
            }
            if (EditorGUI.EndChangeCheck()) { check = true; result = null; }
            EditorGUILayout.EndScrollView();
            if (tab != 2)
            {
                if (check) { error = QuickToggleCore.Validate(Request()); check = false; }
                if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Info);
                if (!string.IsNullOrEmpty(result)) EditorGUILayout.HelpBox(result, MessageType.Info);
                using (new EditorGUI.DisabledScope(error != null || EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    if (GUILayout.Button(setup == SetupMode.ClipsOnly ? "Create Animation Clips" : "Create " + SetupNames[(int)setup] + " Toggles", GUILayout.Height(32)))
                    {
                        try
                        {
                            var created = QuickToggleCore.Create(Request());
                            result = "Created " + created.toggles + (created.toggles == 1 ? " toggle" : " toggles") + " and " + created.assets.Count + " assets. " +
                                (setup == SetupMode.ClipsOnly ? "" : "Undo restores the setup; generated assets stay available.");
                            if (created.objects.Count > 0) EditorGUIUtility.PingObject(created.objects[0]);
                            else if (created.assets.Count > 0) EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(created.assets[0]));
                        }
                        catch (Exception e) { error = e.GetBaseException().Message; result = null; }
                    }
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Created by Fynn9563", EditorStyles.miniLabel, GUILayout.Width(125))) Application.OpenURL("https://github.com/Fynn9563");
            }
        }
        private void DrawSetup()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Create With");
                if (GUILayout.Button(SetupNames[(int)setup], EditorStyles.popup))
                {
                    var choices = new GenericMenu();
                    for (var i = 0; i < SetupNames.Length; i++)
                    {
                        var value = (SetupMode)i; var label = new GUIContent(SetupNames[i] + (available[i] ? "" : " (not installed / unsupported)"));
                        if (available[i]) choices.AddItem(label, setup == value, () => { setup = value; check = true; result = null; Repaint(); });
                        else choices.AddDisabledItem(label);
                    }
                    choices.ShowAsContext();
                }
            }
            if (setup == SetupMode.ClipsOnly) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                defaultOn = EditorGUILayout.Toggle("Default On", defaultOn);
                saved = EditorGUILayout.Toggle("Saved", saved);
            }
            if (setup == SetupMode.VRCFury) menuPath = EditorGUILayout.TextField("Submenu Path", menuPath);
            else
            {
                if (setup == SetupMode.VRChat)
                {
                    parameter = EditorGUILayout.TextField(new GUIContent("Parameter", "A unique name is generated if this name is already used."), parameter);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        fx = EditorGUILayout.ToggleLeft("FX Layer", fx); parameters = EditorGUILayout.ToggleLeft("Expression Parameters", parameters); menu = EditorGUILayout.ToggleLeft("Menu Item", menu);
                    }
                }
                using (new EditorGUI.DisabledScope(setup == SetupMode.VRChat && !menu))
                    targetMenu = EditorGUILayout.ObjectField(new GUIContent("Target Menu", "Leave empty to use the avatar's main menu."), targetMenu, QuickToggleIntegrations.Find("VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu") ?? typeof(ScriptableObject), false);
            }
        }
        private void DrawObjects()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);
                if (GUILayout.Button("Invert All", GUILayout.Width(80))) { foreach (var t in targets) t.on = !t.on; check = true; }
            }
            var remove = -1;
            for (var i = 0; i < targets.Count; i++)
            {
                var row = targets[i];
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    row.target = EditorGUILayout.ObjectField(row.target, typeof(Object), true, GUILayout.MinWidth(160));
                    var go = row.target as GameObject ?? (row.target as Component)?.gameObject;
                    if (go != null)
                    {
                        var choices = new List<Object> { go };
                        choices.AddRange(go.GetComponents<Component>().Where(c => c != null && !(c is Transform) && new SerializedObject(c).FindProperty("m_Enabled")?.propertyType == SerializedPropertyType.Boolean));
                        var selected = Mathf.Max(0, choices.IndexOf(row.target));
                        var next = EditorGUILayout.Popup(selected, choices.Select(c => c is GameObject ? "GameObject" : c.GetType().Name).ToArray(), GUILayout.Width(130));
                        if (next != selected) row.target = choices[next];
                    }
                    row.on = EditorGUILayout.ToggleLeft("On", row.on, GUILayout.Width(42));
                    if (GUILayout.Button("X", GUILayout.Width(24))) remove = i;
                }
            }
            if (remove >= 0) { targets.RemoveAt(remove); check = true; }
            if (GUILayout.Button("Add Target")) { targets.Add(new ToggleTarget()); check = true; }
        }
        private void DrawShapes()
        {
            EditorGUILayout.LabelField("Blendshapes", EditorStyles.boldLabel);
            var remove = -1;
            for (var i = 0; i < shapes.Count; i++)
            {
                var row = shapes[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var renderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(row.renderer, typeof(SkinnedMeshRenderer), true);
                        if (renderer != row.renderer) { row.renderer = renderer; row.shape = null; row.fromTargets = false; }
                        if (GUILayout.Button("X", GUILayout.Width(24))) remove = i;
                    }
                    var mesh = row.renderer != null ? row.renderer.sharedMesh : null;
                    if (mesh != null && mesh.blendShapeCount > 0)
                    {
                        var names = Enumerable.Range(0, mesh.blendShapeCount).Select(mesh.GetBlendShapeName).ToArray();
                        var index = Mathf.Max(0, Array.IndexOf(names, row.shape));
                        row.shape = names[EditorGUILayout.Popup("Shape", index, names)];
                        row.on = EditorGUILayout.Slider("On Value", row.on, 0, 100);
                        EditorGUILayout.LabelField("Off Value", row.renderer.GetBlendShapeWeight(mesh.GetBlendShapeIndex(row.shape)).ToString("0.##") + " (current)");
                    }
                    else EditorGUILayout.LabelField("Choose a renderer with blendshapes.", EditorStyles.miniLabel);
                }
            }
            if (remove >= 0) { shapes.RemoveAt(remove); check = true; }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Blendshape")) { shapes.Add(new ShapeTarget()); check = true; }
                if (GUILayout.Button("Add All From Selected Renderers"))
                    foreach (var renderer in Selection.gameObjects.Select(g => g.GetComponent<SkinnedMeshRenderer>()).Where(r => r != null && r.sharedMesh != null))
                        for (var i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                        {
                            var name = renderer.sharedMesh.GetBlendShapeName(i);
                            if (!shapes.Any(s => s.renderer == renderer && s.shape == name)) { shapes.Add(new ShapeTarget { renderer = renderer, shape = name, on = renderer.GetBlendShapeWeight(i) }); check = true; }
                        }
            }
        }
        private void DrawSettings()
        {
            folder = EditorGUILayout.TextField("Output Folder", folder);
            frames = EditorGUILayout.IntSlider("Clip Length (60 fps)", frames, 1, 600);
            loop = EditorGUILayout.Toggle("Loop Clips", loop);
            writeDefaults = EditorGUILayout.Toggle(new GUIContent("Write Defaults", "Applies to direct VRChat controllers. Modular Avatar matches the avatar's controller settings."), writeDefaults);
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Installed Integrations", EditorStyles.boldLabel);
            for (var i = 1; i < SetupNames.Length; i++) EditorGUILayout.LabelField(SetupNames[i], available[i] ? "Available" : "Not installed / unsupported");
        }
    }
}
#endif
