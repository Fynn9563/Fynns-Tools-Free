using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HierarchyPlus
{
    internal sealed class CollectionUndoState : ScriptableObject
    {
        public string json;
    }

    [InitializeOnLoad]
    public static class CollectionsHierarchy
    {
        private const string PatchId = "fynn.hierarchy.collections.working-prototype";
        private const string SessionKey = "Fynn.Hierarchy.CollectionsPrototype";
        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static Harmony harmony;
        private static object target;
        private static EditorWindow window;
        private static CollectionTree tree;
        private static CollectionsNativeHierarchy native;
        private static SmartCollection displayFilter;
        private static bool alphabetical;
        private static int searchMode;
        private static CollectionUndoState undoState;
        private static string lastJson;
        private static bool selectionChanging;
        public static bool Enabled { get; private set; }
        internal static bool DrawingVirtualObjectRow { get; private set; }
        public static Rect LastTreeRect { get; private set; }
        public static int DrawCount { get; private set; }

        static CollectionsHierarchy()
        {
            var hierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow", true);
            new Harmony(PatchId + ".tab-menu").Patch(hierarchyType.GetMethod("AddItemsToMenu", Flags),
                postfix: new HarmonyMethod(typeof(CollectionsHierarchy).GetMethod(nameof(AddTabMenu), Flags)));
            EditorApplication.delayCall += () => { if (SessionState.GetBool(SessionKey, false)) Enable(); };
            Undo.undoRedoPerformed += OnUndo;
        }

        [MenuItem("Tools/Fynn's Tools/Hierarchy+/Enhanced Mode", false, 97)]
        public static void Toggle() { if (Enabled) Disable(); else Enable(); }

        private static void AddTabMenu(EditorWindow __instance, GenericMenu __0)
        {
            bool virtualView = Enabled && window == __instance;
            __0.AddSeparator("");
            __0.AddItem(new GUIContent("Enhanced Mode"), virtualView,
                () => { if (Enabled && window == __instance) Disable(); else EnableWindow(__instance); });
        }

        public static void Enable() => EnableWindow(null);

        private static void EnableWindow(EditorWindow selectedWindow)
        {
            if (Enabled && (selectedWindow == null || window == selectedWindow)) return;
            if (Enabled) Disable();
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow", true);
            window = selectedWindow != null ? selectedWindow : Resources.FindObjectsOfTypeAll(type).OfType<EditorWindow>().FirstOrDefault();
            if (window == null) throw new InvalidOperationException("Open Unity's Hierarchy first.");
            target = type.GetField("m_SceneHierarchy", Flags).GetValue(window);
            native = new CollectionsNativeHierarchy(target);
            tree = new CollectionTree(new TreeViewState());
            harmony = new Harmony(PatchId);
            harmony.Patch(target.GetType().GetMethod("OnGUI", Flags, null, new[] { typeof(Rect) }, null),
                prefix: new HarmonyMethod(typeof(CollectionsHierarchy).GetMethod(nameof(DrawPrefix), Flags)));
            harmony.Patch(target.GetType().GetMethod("CollapseAll", Flags), prefix: new HarmonyMethod(typeof(CollectionsHierarchy).GetMethod(nameof(CollapsePrefix), Flags)));
            harmony.Patch(target.GetType().GetMethod("ExpandAll", Flags), prefix: new HarmonyMethod(typeof(CollectionsHierarchy).GetMethod(nameof(ExpandPrefix), Flags)));
            Enabled = true;
            SessionState.SetBool(SessionKey, true);
            EditorApplication.hierarchyChanged += Reload;
            Selection.selectionChanged += OnSelectionChanged;
            SceneVisibilityManager.visibilityChanged += Repaint;
            SceneVisibilityManager.pickingChanged += Repaint;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            EditorApplication.quitting += BeforeReload;
            EditorApplication.playModeStateChanged += OnPlayMode;
            EnsureUndo();
            Reload();
        }

        public static void Disable()
        {
            SessionState.SetBool(SessionKey, false);
            BeforeReload();
        }

        private static void BeforeReload()
        {
            Enabled = false;
            EditorApplication.hierarchyChanged -= Reload;
            Selection.selectionChanged -= OnSelectionChanged;
            SceneVisibilityManager.visibilityChanged -= Repaint;
            SceneVisibilityManager.pickingChanged -= Repaint;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            EditorApplication.quitting -= BeforeReload;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            FocusCollection(null);
            harmony?.UnpatchAll(PatchId);
            harmony = null;
            tree = null;
            native = null;
            target?.GetType().GetMethod("ReloadData", Flags)?.Invoke(target, null);
            window?.Repaint();
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) FocusCollection(null);
            if (state == PlayModeStateChange.EnteredEditMode) Reload();
        }

        public static void Reload()
        {
            native?.Clear();
            tree?.Reload();
            SyncSelection();
            window?.Repaint();
        }

        private static void Repaint() { window?.Repaint(); }

        private static void SyncSelection()
        {
            if (!selectionChanging) tree?.SyncSelection();
            window?.Repaint();
        }

        private static void OnSelectionChanged()
        {
            SyncSelection();
            if (!selectionChanging && Selection.activeGameObject != null && !native.IsLocked)
                tree?.FrameObject(Selection.activeGameObject.GetInstanceID(), false);
        }

        private static void EnsureUndo()
        {
            if (undoState == null)
            {
                undoState = Resources.FindObjectsOfTypeAll<CollectionUndoState>().FirstOrDefault();
                if (undoState == null)
                {
                    undoState = ScriptableObject.CreateInstance<CollectionUndoState>();
                    undoState.hideFlags = HideFlags.HideAndDontSave;
                }
                undoState.json = lastJson = Collections.CaptureState();
            }
        }

        internal static void Change(string label, Action action)
        {
            EnsureUndo();
            undoState.json = Collections.CaptureState();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(label);
            Undo.RegisterCompleteObjectUndo(undoState, label);
            string focusId = Collections.activeFilter?.id;
            action();
            Collections.Save();
            undoState.json = lastJson = Collections.CaptureState();
            EditorUtility.SetDirty(undoState);
            Undo.FlushUndoRecordObjects();
            FocusCollection(focusId);
            Reload();
        }

        private static void OnUndo()
        {
            if (undoState != null && undoState.json != lastJson)
            {
                string focusId = Collections.activeFilter?.id;
                FocusCollection(null);
                Collections.RestoreState(undoState.json);
                lastJson = undoState.json;
                FocusCollection(focusId);
            }
            foreach (var c in Collections.collections) c.InvalidateIDs();
            if (Collections.activeFilter != null) FocusCollection(Collections.activeFilter.id);
            Reload();
        }

        public static string CreateCollection(string name)
        {
            var collection = new SmartCollection(string.IsNullOrWhiteSpace(name) ? "New Collection" : name.Trim());
            Change("Create Collection", () => Collections.collections.Add(collection));
            tree?.ExpandFolder(collection.id);
            return collection.id;
        }

        public static void RenameCollection(string id, string name)
        {
            var c = Find(id);
            if (c != null && !string.IsNullOrWhiteSpace(name)) Change("Rename Collection", () => c.name = name.Trim());
        }

        public static void DeleteCollection(string id)
        {
            var c = Find(id);
            if (c != null) Change("Delete Collection", () => Collections.collections.Remove(c));
        }

        public static void SetShared(string id, bool value)
        {
            var c = Find(id);
            if (c != null) Change("Change Shared Collection", () => c.alwaysVisible = value);
        }

        public static void SetColor(string id, Color value)
        {
            var c = Find(id);
            if (c != null) Change("Change Collection Color", () => c.color = value);
        }

        public static void SetMembership(string id, GameObject[] objects, bool add)
        {
            var c = Find(id);
            var valid = objects.Where(go => go != null && go.scene.IsValid() && !EditorUtility.IsPersistent(go))
                .Distinct().Where(go => c != null && c.Contains(go) != add).ToArray();
            if (c == null || valid.Length == 0) return;
            Change(add ? "Add to Collection" : "Remove from Collection", () =>
            {
                foreach (var go in valid) { if (add) c.Add(go); else c.Remove(go); }
            });
            tree?.ExpandFolder(id);
        }

        private static SmartCollection Find(string id) => Collections.collections.FirstOrDefault(c => c.id == id);

        public static void FocusCollection(string id)
        {
            Collections.SuppressVisibilityUndo = true;
            try
            {
                var c = Find(id);
                if (c == null) Collections.ClearFilter();
                else Collections.SetActiveFilter(c);
            }
            finally { Collections.SuppressVisibilityUndo = false; }
            tree?.Reload();
            window?.Repaint();
        }

        public static void DeleteSelectedObjects()
        {
            var selected = Selection.gameObjects.Where(go => go.scene.IsValid() && !EditorUtility.IsPersistent(go)).ToArray();
            var roots = selected.Where(go => !selected.Any(other => other != go && go.transform.IsChildOf(other.transform))).ToArray();
            if (roots.Length == 0) return;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Delete GameObjects");
            NativeCommand("DeleteGO");
            Reload();
        }

        private static void NativeCommand(string method)
        {
            target.GetType().GetMethod(method, Flags)?.Invoke(target, null);
            Reload();
        }

        private static void ClipboardCommand(string method)
        {
            typeof(Editor).Assembly.GetType("UnityEditor.ClipboardUtility", true).GetMethod(method, Flags, null, Type.EmptyTypes, null).Invoke(null, null);
        }

        private static bool DrawPrefix(object __instance, Rect rect)
        {
            if (!UseVirtual(__instance)) return true;
            try
            {
                DrawCount++;
                if (displayFilter != Collections.activeFilter || alphabetical != native.IsAlphabetical || searchMode != native.SearchMode)
                {
                    displayFilter = Collections.activeFilter;
                    alphabetical = native.IsAlphabetical;
                    searchMode = native.SearchMode;
                    tree.Reload();
                }
                int frameId = (int)target.GetType().GetField("m_FrameRequestID", Flags).GetValue(target);
                if (frameId != 0)
                {
                    bool ping = (bool)target.GetType().GetField("m_FrameRequestPing", Flags).GetValue(target);
                    target.GetType().GetField("m_FrameRequestID", Flags).SetValue(target, 0);
                    if (!tree.FrameObject(frameId, ping) && Collections.activeFilter != null)
                    { FocusCollection(null); tree.FrameObject(frameId, ping); }
                }
                var bar = new Rect(rect.x, rect.y, rect.width, 24);
                GUI.Box(bar, GUIContent.none, EditorStyles.toolbar);
                if (GUI.Button(new Rect(bar.x + 3, bar.y + 2, 45, 20), "New", EditorStyles.toolbarButton))
                {
                    string id = CreateCollection("New Collection");
                    EditorApplication.delayCall += () => tree?.RenameFolder(id);
                }
                bool showApply = bar.width >= 440;
                if (GUI.Button(new Rect(bar.x + 51, bar.y + 2, Mathf.Max(65, bar.width - (showApply ? 296 : 204)), 20),
                    "View: " + (Collections.activeFilter?.name ?? "All Collections"), EditorStyles.toolbarDropDown)) ShowFocusMenu();
                using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(tree.searchString) || native.IsAlphabetical))
                    if (showApply && GUI.Button(new Rect(bar.xMax - 240, bar.y + 2, 89, 20),
                        new GUIContent("Apply Order", "Apply collection display order to Unity's sibling order. Parents stay unchanged. Shared objects use their first position."), EditorStyles.toolbarButton))
                        EditorApplication.delayCall += ApplyOrderToUnity;
                if (GUI.Button(new Rect(bar.xMax - 148, bar.y + 2, 67, 20), "Options", EditorStyles.toolbarDropDown)) ShowOptions();
                if (GUI.Button(new Rect(bar.xMax - 78, bar.y + 2, 75, 20), "Restore", EditorStyles.toolbarButton)) EditorApplication.delayCall += Disable;
                tree.searchString = (string)target.GetType().GetField("m_SearchFilter", Flags).GetValue(target) ?? "";
                LastTreeRect = new Rect(rect.x, rect.y + 24, rect.width, Mathf.Max(0, rect.height - 24));
                tree.HandleCommands(Event.current);
                tree.OnGUI(LastTreeRect);
                if (Event.current.rawType != EventType.Layout && Event.current.rawType != EventType.Repaint) window.Repaint();
                return false;
            }
            catch (ExitGUIException) { throw; }
            catch (Exception ex)
            {
                Debug.LogError("[Hierarchy+ collections] " + ex);
                EditorApplication.delayCall += Disable;
                return false;
            }
        }

        private static bool UseVirtual(object instance) => Enabled && ReferenceEquals(instance, target) && !EditorApplication.isPlaying && PrefabStageUtility.GetCurrentPrefabStage() == null && !native.IsLocked;

        private static bool CollapsePrefix(object __instance)
        {
            if (!UseVirtual(__instance)) return true;
            tree.CollapseAll(); window.Repaint(); return false;
        }

        private static bool ExpandPrefix(object __instance)
        {
            if (!UseVirtual(__instance)) return true;
            tree.ExpandAll(); window.Repaint(); return false;
        }

        private static void ShowFocusMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("All Collections"), Collections.activeFilter == null, () => FocusCollection(null));
            foreach (var c in Collections.collections)
            {
                string id = c.id;
                menu.AddItem(new GUIContent(c.name), c == Collections.activeFilter, () => FocusCollection(id));
            }
            menu.ShowAsContext();
        }

        private static void ShowOptions()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Show Collection Icons in Normal Mode"), SavedSettings.settings.showCollectionsInNormalMode, () =>
            {
                SavedSettings.settings.showCollectionsInNormalMode.value = !SavedSettings.settings.showCollectionsInNormalMode;
                SavedSettings.Save();
                EditorApplication.RepaintHierarchyWindow();
            });
            if (tree != null && string.IsNullOrEmpty(tree.searchString) && !native.IsAlphabetical)
                menu.AddItem(new GUIContent("Apply Order to Unity"), false, ApplyOrderToUnity);
            else menu.AddDisabledItem(new GUIContent("Apply Order to Unity"));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Hide Other Objects in Scene"), Collections.HideFilteredInScene, () =>
            {
                Collections.HideFilteredInScene = !Collections.HideFilteredInScene;
                FocusCollection(Collections.activeFilter?.id);
            });
            menu.AddItem(new GUIContent("Scene Hiding/Hide in Editor"), Collections.UseEditorVisibility, () =>
            {
                Collections.UseEditorVisibility = true; FocusCollection(Collections.activeFilter?.id);
            });
            menu.AddItem(new GUIContent("Scene Hiding/Turn Objects Off"), !Collections.UseEditorVisibility, () =>
            {
                Collections.UseEditorVisibility = false; FocusCollection(Collections.activeFilter?.id);
            });
            menu.AddSeparator("");
            menu.AddDisabledItem(new GUIContent("Created by Fynn9563"));
            menu.AddItem(new GUIContent("GitHub"), false, () => Application.OpenURL("https://github.com/Fynn9563"));
            menu.ShowAsContext();
        }

        public static object[] DescribeRows() => tree?.DescribeRows() ?? new object[0];
        public static void ApplyOrderToUnity() { tree?.ApplyOrderToUnity(); }
        public static bool SelectRow(int id) => tree != null && tree.SelectRow(id);
        public static bool SetFolderExpanded(string name, bool expanded) => tree != null && tree.SetFolderExpanded(name, expanded);

        private sealed class CollectionTree : TreeView
        {
            private readonly Dictionary<string, int> stableIds = new Dictionary<string, int>();
            private readonly Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
            private readonly Dictionary<int, SmartCollection> folders = new Dictionary<int, SmartCollection>();
            private readonly Dictionary<int, Scene> scenes = new Dictionary<int, Scene>();
            private readonly Dictionary<int, string> groups = new Dictionary<int, string>();
            private int nextId = 1;
            private bool initialBuild = true;
            private bool folderSelection;
            private const string FolderDragKey = "Fynn.Hierarchy.FolderDrag";
            private const string ObjectDragKey = "Fynn.Hierarchy.ObjectDrag";
            private string searchQuery;
            private HashSet<int> searchMatches = new HashSet<int>();

            public CollectionTree(TreeViewState state) : base(state)
            {
                rowHeight = 18;
                baseIndent = 32;
                enableItemHovering = true;
                var controller = typeof(TreeView).GetField("m_TreeView", Flags).GetValue(this);
                controller.GetType().GetProperty("useExpansionAnimation", Flags).SetValue(controller, false, null);
                Reload();
            }

            private int Id(string key)
            {
                if (!stableIds.TryGetValue(key, out var id)) stableIds[key] = id = nextId++;
                return id;
            }

            protected override TreeViewItem BuildRoot()
            {
                objects.Clear(); folders.Clear(); groups.Clear(); scenes.Clear();
                searchQuery = null;
                var root = new TreeViewItem(0, -1, "Root");
                var assigned = new HashSet<int>();
                var membersByCollection = new Dictionary<SmartCollection, List<GameObject>>();
                foreach (var c in Collections.collections)
                {
                    c.RebuildIDsIfNeeded();
                    var members = c.objectIDs.Select(id => EditorUtility.InstanceIDToObject(id) as GameObject)
                        .Where(go => go != null && go.scene.IsValid() && !EditorSceneManager.IsPreviewScene(go.scene)).Distinct().ToList();
                    foreach (var go in members) assigned.Add(go.GetInstanceID());
                    membersByCollection[c] = members;
                }
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (EditorSceneManager.IsPreviewScene(scene) || !native.IsVisibleScene(scene)) continue;
                    var header = new TreeViewItem(Id("Scene:" + scene.handle), 0, scene.name);
                    scenes[header.id] = scene;
                    root.AddChild(header);
                    if (!scene.isLoaded) continue;
                    foreach (var c in Collections.collections)
                    {
                        if (Collections.activeFilter != null && c != Collections.activeFilter && !c.alwaysVisible) continue;
                        var members = membersByCollection[c].Where(go => go.scene == scene).ToList();
                        if (members.Count == 0 && (membersByCollection[c].Count > 0 || scene != SceneManager.GetActiveScene())) continue;
                        var folder = Folder(header, scene.handle + ":" + c.id, c.name, c);
                        var topMembers = members.Where(go => !members.Any(other => other != go && go.transform.IsChildOf(other.transform)));
                        topMembers = OrderMembers(topMembers, c.displayOrder);
                        foreach (var go in topMembers) AddObject(folder, go, c.id, null);
                    }
                    if (Collections.activeFilter == null)
                    {
                        var unassigned = Folder(header, scene.handle + ":Unassigned", "Unassigned", null);
                        IEnumerable<GameObject> roots = scene.GetRootGameObjects();
                        roots = OrderMembers(roots, Collections.unassignedOrder);
                        foreach (var go in roots) AddObject(unassigned, go, "Unassigned", assigned);
                    }
                }
                if (root.children == null) root.children = new List<TreeViewItem>();
                SetupDepthsFromParentsAndChildren(root);
                if (initialBuild) { state.expandedIDs.AddRange(folders.Keys.Concat(scenes.Keys)); state.expandedIDs.Sort(); initialBuild = false; }
                return root;
            }

            private TreeViewItem Folder(TreeViewItem root, string key, string name, SmartCollection c)
            {
                var item = new TreeViewItem(Id("Folder:" + key), root.depth + 1, name) { icon = EditorGUIUtility.FindTexture("Folder Icon") };
                folders[item.id] = c;
                root.AddChild(item);
                return item;
            }

            private IEnumerable<GameObject> OrderMembers(IEnumerable<GameObject> members, List<string> order)
            {
                if (native.IsAlphabetical) return members.OrderBy(go => go.name, Comparer<string>.Create(EditorUtility.NaturalCompare));
                if (order == null || order.Count == 0) return members;
                var ranks = order.Select((key, index) => new { key, index }).GroupBy(p => p.key).ToDictionary(g => g.Key, g => g.First().index);
                return members.OrderBy(go => ranks.TryGetValue(SmartCollection.GetGameObjectPath(go), out var rank) ? rank : int.MaxValue);
            }

            private void AddObject(TreeViewItem parent, GameObject go, string group, HashSet<int> excluded)
            {
                if ((go.hideFlags & HideFlags.HideInHierarchy) != 0 || (excluded != null && excluded.Contains(go.GetInstanceID()))) return;
                var item = new TreeViewItem(Id(group + ":" + go.GetInstanceID()), parent.depth + 1, go.name) { icon = AssetPreview.GetMiniThumbnail(go) };
                objects[item.id] = go; groups[item.id] = group;
                parent.AddChild(item);
                var children = go.transform.Cast<Transform>();
                if (native.IsAlphabetical) children = children.OrderBy(child => child.name, Comparer<string>.Create(EditorUtility.NaturalCompare));
                foreach (Transform child in children) AddObject(item, child.gameObject, group, excluded);
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                native.DrawGutter(args.rowRect);
                if (scenes.TryGetValue(args.item.id, out var scene))
                {
                    var blank = args; blank.label = "";
                    base.RowGUI(blank);
                    var label = args.rowRect; label.xMin += GetContentIndent(args.item);
                    native.DrawItem(native.SceneItem(scene, args.item.depth), args.rowRect, label, args.row, args.selected, args.focused, false);
                }
                else if (folders.TryGetValue(args.item.id, out var c))
                {
                    if (c != null) EditorGUI.DrawRect(new Rect(args.rowRect.x + 32, args.rowRect.y, 3, args.rowRect.height), c.color);
                    base.RowGUI(args);
                    if (c != null) DrawFolderButtons(args, c);
                }
                else
                {
                    if (objects.TryGetValue(args.item.id, out var go) && go != null)
                    {
                        var blank = args;
                        blank.label = args.isRenaming ? args.label : "";
                        var icon = args.item.icon;
                        args.item.icon = args.isRenaming ? icon : null;
                        try { base.RowGUI(blank); }
                        finally { args.item.icon = icon; }
                        var row = args.rowRect;
                        row.xMin += GetContentIndent(args.item);
                        native.DrawObject(go, args.item, args.rowRect, row, args.row, args.selected, args.focused, args.isRenaming);
                        row.xMax = native.DecorationRightEdge(args.rowRect);
                        DrawingVirtualObjectRow = true;
                        try { EditorApplication.hierarchyWindowItemOnGUI?.Invoke(go.GetInstanceID(), row); }
                        finally { DrawingVirtualObjectRow = false; }
                    }
                }
                DrawBranchLines(args);
                if (groups.TryGetValue(args.item.id, out var group))
                {
                    var collection = Find(group);
                    if (collection != null)
                        EditorGUI.DrawRect(new Rect(args.rowRect.x + 32, args.rowRect.y, 3, args.rowRect.height), collection.color);
                }
            }

            private static float FolderButtonAlpha(float buttonLeft, float labelRight) =>
                Mathf.InverseLerp(labelRight + 8, labelRight + 24, buttonLeft);

            private void DrawFolderButtons(RowGUIArgs args, SmartCollection collection)
            {
                var style = EditorStyles.miniButton;
                var sharedLabel = new GUIContent("Shared", "Keep this collection visible when focusing another.");
                var focusLabel = new GUIContent(collection == Collections.activeFilter ? "All" : "Focus", "Focus this collection, or return to all collections.");
                float focusWidth = Mathf.Max(56, Mathf.Ceil(style.CalcSize(new GUIContent("Focus")).x) + 8);
                float sharedWidth = Mathf.Max(60, Mathf.Ceil(style.CalcSize(sharedLabel).x) + 8);
                float right = Mathf.Min(args.rowRect.xMax, LastTreeRect.width) - 18;
                var focus = new Rect(right - focusWidth, args.rowRect.y, focusWidth, args.rowRect.height);
                var shared = new Rect(focus.x - sharedWidth - 2, args.rowRect.y, sharedWidth, args.rowRect.height);
                float labelRight = args.rowRect.x + GetContentIndent(args.item) + 18 + EditorStyles.label.CalcSize(new GUIContent(args.label)).x;
                var originalColor = GUI.color;
                var originalEnabled = GUI.enabled;
                try
                {
                    float alpha = FolderButtonAlpha(shared.x, labelRight);
                    GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * alpha);
                    GUI.enabled = originalEnabled && alpha > 0.5f;
                    if (alpha > 0)
                    {
                        bool value = GUI.Toggle(shared, collection.alwaysVisible, sharedLabel, style);
                        if (value != collection.alwaysVisible) { string id = collection.id; EditorApplication.delayCall += () => SetShared(id, value); }
                    }
                    alpha = FolderButtonAlpha(focus.x, labelRight);
                    GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * alpha);
                    GUI.enabled = originalEnabled && alpha > 0.5f;
                    if (alpha > 0 && GUI.Button(focus, focusLabel, style))
                    { string id = collection == Collections.activeFilter ? null : collection.id; EditorApplication.delayCall += () => FocusCollection(id); }
                }
                finally { GUI.color = originalColor; GUI.enabled = originalEnabled; }
            }

            private void DrawBranchLines(RowGUIArgs args)
            {
                var settings = SavedSettings.settings;
                if (Event.current.type != EventType.Repaint || !string.IsNullOrEmpty(searchString) ||
                    !settings.GetColorsEnabled() || !settings.guideLinesEnabled || args.item.depth < 1) return;
                var rect = args.rowRect;
                float indent = rect.x + GetContentIndent(args.item);
                bool Last(TreeViewItem item) => item.parent?.children != null && item.parent.children.Last() == item;
                void Line(float x, float top, float bottom) =>
                    EditorGUI.DrawRect(new Rect(x, top, 1, Mathf.Max(0, bottom - top)), settings.guideLinesColor);
                if (args.item.depth >= 2)
                {
                    float x = indent - depthIndentWidth - 7;
                    Line(x, rect.y, Last(args.item) ? rect.center.y : rect.yMax);
                    float end = indent - (args.item.hasChildren ? 14 : 2);
                    EditorGUI.DrawRect(new Rect(x, rect.center.y, Mathf.Max(0, end - x), 1), settings.guideLinesColor);
                    for (var ancestor = args.item.parent; ancestor != null && ancestor.depth >= 2; ancestor = ancestor.parent)
                        if (!Last(ancestor)) Line(rect.x + GetContentIndent(ancestor) - depthIndentWidth - 7, rect.y, rect.yMax);
                }
                if (args.item.hasChildren && IsExpanded(args.item.id))
                    Line(indent - 7, rect.center.y + 7, rect.yMax);
            }

            protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
            {
                if (searchQuery != search)
                {
                    searchQuery = search;
                    searchMatches = native.Search(search);
                }
                if (objects.TryGetValue(item.id, out var go)) return go != null && searchMatches.Contains(go.GetInstanceID());
                return base.DoesItemMatchSearch(item, search);
            }

            protected override void SelectionChanged(IList<int> ids)
            {
                folderSelection = ids.Any(id => folders.ContainsKey(id) || scenes.ContainsKey(id));
                selectionChanging = true;
                try
                {
                    Selection.objects = ids.Where(objects.ContainsKey).Select(id => (Object)objects[id]).Distinct().ToArray();
                    native.Select(ids.Where(id => scenes.ContainsKey(id) || objects.ContainsKey(id)).Select(id => scenes.ContainsKey(id) ? scenes[id].handle : objects[id].GetInstanceID()).ToArray());
                }
                finally { selectionChanging = false; }
            }

            public void SyncSelection()
            {
                if (folderSelection && Selection.gameObjects.Length == 0) return;
                folderSelection = false;
                var selected = new HashSet<int>(Selection.gameObjects.Select(go => go.GetInstanceID()));
                SetSelection(objects.Where(p => p.Value != null && selected.Contains(p.Value.GetInstanceID())).Select(p => p.Key).ToList());
            }

            public bool FrameObject(int instanceId, bool ping)
            {
                var component = EditorUtility.InstanceIDToObject(instanceId) as Component;
                if (component != null) instanceId = component.gameObject.GetInstanceID();
                var pair = objects.FirstOrDefault(p => p.Value != null && p.Value.GetInstanceID() == instanceId);
                if (pair.Key == 0) return false;
                FrameItem(pair.Key);
                var controller = typeof(TreeView).GetField("m_TreeView", Flags).GetValue(this);
                controller.GetType().GetMethod("Frame", Flags, null, new[] { typeof(int), typeof(bool), typeof(bool), typeof(bool) }, null)
                    .Invoke(controller, new object[] { pair.Key, true, ping, false });
                return true;
            }

            protected override bool CanRename(TreeViewItem item) => (objects.TryGetValue(item.id, out var go) && go != null && (go.hideFlags & HideFlags.NotEditable) == 0) || (folders.TryGetValue(item.id, out var c) && c != null);
            protected override void RenameEnded(RenameEndedArgs args)
            {
                if (!args.acceptedRename || string.IsNullOrWhiteSpace(args.newName)) return;
                if (folders.TryGetValue(args.itemID, out var c) && c != null) RenameCollection(c.id, args.newName);
                else if (objects.TryGetValue(args.itemID, out var go) && go != null)
                {
                    Undo.IncrementCurrentGroup();
                    Undo.RecordObject(go, "Rename GameObject"); go.name = args.newName;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(go);
                    Undo.FlushUndoRecordObjects(); Reload();
                }
            }

            protected override bool CanStartDrag(CanStartDragArgs args) => args.draggedItemIDs.Any(id => objects.ContainsKey(id) || scenes.ContainsKey(id) || (folders.TryGetValue(id, out var c) && c != null));
            protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
            {
                var sceneIds = args.draggedItemIDs.Where(scenes.ContainsKey).ToList();
                if (sceneIds.Count > 0)
                {
                    native.StartSceneDrag(scenes[sceneIds[0]], sceneIds.Select(id => scenes[id].handle).ToList());
                    return;
                }
                DragAndDrop.PrepareStartDrag();
                var collectionIds = args.draggedItemIDs.Where(id => folders.TryGetValue(id, out var c) && c != null).Select(id => folders[id].id).Distinct().ToArray();
                if (collectionIds.Length > 0)
                {
                    DragAndDrop.SetGenericData(FolderDragKey, collectionIds);
                    DragAndDrop.objectReferences = new Object[0];
                    DragAndDrop.StartDrag("Collections");
                    return;
                }
                var origins = args.draggedItemIDs.Where(objects.ContainsKey).GroupBy(id => objects[id].GetInstanceID()).ToDictionary(g => g.Key, g => groups[g.First()]);
                DragAndDrop.SetGenericData(ObjectDragKey, origins);
                DragAndDrop.objectReferences = args.draggedItemIDs.Where(objects.ContainsKey).Select(id => (Object)objects[id]).Distinct().ToArray();
                DragAndDrop.StartDrag("Collection Objects");
            }

            protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
            {
                var collectionIds = DragAndDrop.GetGenericData(FolderDragKey) as string[];
                if (collectionIds != null && collectionIds.Length > 0) return DropCollections(args, collectionIds);
                if (!string.IsNullOrEmpty(searchString) && args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
                    return DragAndDropVisualMode.Rejected;
                if (args.parentItem != null && folders.ContainsKey(args.parentItem.id)) return DropMembers(args);
                if (args.parentItem == null)
                    return native.DropOnScene(SceneManager.GetActiveScene(), args.performDrop);
                if (scenes.TryGetValue(args.parentItem.id, out var scene) && args.dragAndDropPosition == DragAndDropPosition.UponItem)
                    return native.DropOnScene(scene, args.performDrop);
                if (args.dragAndDropPosition == DragAndDropPosition.BetweenItems && args.parentItem.children != null && args.parentItem.children.Count > 0)
                {
                    int index = Mathf.Clamp(args.insertAtIndex, 0, args.parentItem.children.Count);
                    bool above = index < args.parentItem.children.Count;
                    var relative = args.parentItem.children[above ? index : index - 1];
                    if (scenes.TryGetValue(relative.id, out var relativeScene)) return native.DropBesideScene(relativeScene, above, args.performDrop);
                    if (objects.TryGetValue(relative.id, out var sibling) && sibling != null)
                        return native.DropBesideObject(sibling, above, args.performDrop);
                }
                if (args.parentItem != null && objects.TryGetValue(args.parentItem.id, out var parent) && parent != null && args.dragAndDropPosition == DragAndDropPosition.UponItem)
                {
                    var mode = native.DropOnObject(parent, args.parentItem.depth, args.performDrop);
                    if (args.performDrop && mode != DragAndDropVisualMode.Rejected && mode != DragAndDropVisualMode.None) EditorApplication.delayCall += CollectionsHierarchy.Reload;
                    return mode;
                }
                return DragAndDropVisualMode.Rejected;
            }

            private DragAndDropVisualMode DropCollections(DragAndDropArgs args, string[] ids)
            {
                if (!string.IsNullOrEmpty(searchString) || args.parentItem == null || !scenes.ContainsKey(args.parentItem.id) ||
                    args.dragAndDropPosition != DragAndDropPosition.BetweenItems) return DragAndDropVisualMode.Rejected;
                var moving = Collections.collections.Where(c => ids.Contains(c.id)).ToList();
                if (moving.Count == 0) return DragAndDropVisualMode.Rejected;
                var siblings = args.parentItem.children ?? new List<TreeViewItem>();
                var anchor = siblings.Skip(Mathf.Clamp(args.insertAtIndex, 0, siblings.Count))
                    .Where(r => folders.TryGetValue(r.id, out var c) && c != null && !ids.Contains(c.id))
                    .Select(r => folders[r.id]).FirstOrDefault();
                if (args.performDrop)
                {
                    DragAndDrop.AcceptDrag();
                    Change("Reorder Collections", () =>
                    {
                        foreach (var c in moving) Collections.collections.Remove(c);
                        int index = anchor == null ? Collections.collections.Count : Collections.collections.IndexOf(anchor);
                        Collections.collections.InsertRange(index, moving);
                    });
                }
                return DragAndDropVisualMode.Move;
            }

            private DragAndDropVisualMode DropMembers(DragAndDropArgs args)
            {
                bool between = args.dragAndDropPosition == DragAndDropPosition.BetweenItems;
                if ((!between && args.dragAndDropPosition != DragAndDropPosition.UponItem) || (between && native.IsAlphabetical))
                    return DragAndDropVisualMode.Rejected;
                var destination = folders[args.parentItem.id];
                var targetScene = scenes[args.parentItem.parent.id];
                var moving = DragAndDrop.objectReferences.OfType<GameObject>().Where(go => go.scene == targetScene && !EditorUtility.IsPersistent(go)).Distinct().ToList();
                if (moving.Count == 0 || (destination == null && moving.Any(go => go.transform.parent != null))) return DragAndDropVisualMode.Rejected;
                var origins = DragAndDrop.GetGenericData(ObjectDragKey) as Dictionary<int, string>;
                var siblings = args.parentItem.children ?? new List<TreeViewItem>();
                var ordered = siblings.Where(r => objects.ContainsKey(r.id)).Select(r => objects[r.id]).ToList();
                if (destination != null && moving.Any(go => ordered.Any(other => other != go && !moving.Contains(other) && go.transform.IsChildOf(other.transform))))
                    return DragAndDropVisualMode.Rejected;
                int insert = between ? Mathf.Clamp(args.insertAtIndex, 0, siblings.Count) : siblings.Count;
                var anchor = siblings.Skip(insert).Where(r => objects.ContainsKey(r.id) && !moving.Contains(objects[r.id])).Select(r => objects[r.id]).FirstOrDefault();
                ordered.RemoveAll(moving.Contains);
                ordered.InsertRange(anchor == null ? ordered.Count : ordered.IndexOf(anchor), moving);
                if (args.performDrop)
                {
                    DragAndDrop.AcceptDrag();
                    Change("Move Collection Items", () =>
                    {
                        foreach (var go in moving)
                        {
                            if (destination == null) { foreach (var c in Collections.collections) c.Remove(go); }
                            else
                            {
                                destination.Add(go);
                                if (origins != null && origins.TryGetValue(go.GetInstanceID(), out var source) && source != destination.id) Find(source)?.Remove(go);
                            }
                        }
                        var order = ordered.Select(SmartCollection.GetGameObjectPath).Distinct().ToList();
                        if (destination == null) Collections.unassignedOrder = order;
                        else destination.displayOrder = order;
                    });
                    SetExpanded(args.parentItem.id, true);
                }
                return origins == null ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Move;
            }

            public void ApplyOrderToUnity()
            {
                if (!string.IsNullOrEmpty(searchString) || native.IsAlphabetical) return;
                var displayed = rootItem.children.SelectMany(scene => scene.children ?? new List<TreeViewItem>())
                    .Where(folder => folders.ContainsKey(folder.id))
                    .SelectMany(folder => folder.children ?? new List<TreeViewItem>())
                    .Where(item => objects.ContainsKey(item.id)).Select(item => objects[item.id]).Where(go => go != null).Distinct().ToList();
                ApplySiblingOrder(displayed);
                CollectionsHierarchy.Reload();
            }

            private static void ApplySiblingOrder(List<GameObject> displayed)
            {
                var changes = displayed.GroupBy(go => go.scene.handle + ":" + (go.transform.parent == null ? 0 : go.transform.parent.GetInstanceID()))
                    .Select(group => group.ToList()).Where(group => !group.SequenceEqual(group.OrderBy(go => go.transform.GetSiblingIndex()))).ToList();
                if (changes.Count == 0) return;
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Apply Hierarchy Order");
                foreach (var group in changes)
                {
                    var parent = group[0].transform.parent;
                    if (parent != null) Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Apply Hierarchy Order");
                    else foreach (var go in group) Undo.RegisterFullObjectHierarchyUndo(go, "Apply Hierarchy Order");
                }
                foreach (var group in changes)
                {
                    var slots = group.Select(go => go.transform.GetSiblingIndex()).OrderBy(index => index).ToArray();
                    for (int i = 0; i < group.Count; i++) group[i].transform.SetSiblingIndex(slots[i]);
                    EditorSceneManager.MarkSceneDirty(group[0].scene);
                }
                Undo.FlushUndoRecordObjects();
            }

            protected override void DoubleClickedItem(int id)
            {
                if (folders.ContainsKey(id) || scenes.ContainsKey(id)) SetExpanded(id, !IsExpanded(id));
                else if (objects.ContainsKey(id)) SceneView.lastActiveSceneView?.FrameSelected();
            }

            protected override void ContextClickedItem(int id)
            {
                var menu = new GenericMenu();
                if (scenes.TryGetValue(id, out var scene))
                {
                    native.AddSceneMenu(menu, scene);
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("New Collection"), false, () => CreateCollection("New Collection"));
                }
                else if (folders.TryGetValue(id, out var c))
                {
                    menu.AddItem(new GUIContent("New Collection"), false, () => CreateCollection("New Collection"));
                    if (c != null)
                    {
                        string key = c.id;
                        menu.AddItem(new GUIContent("Rename"), false, () => RenameFolder(key));
                        menu.AddItem(new GUIContent("Always Visible (Shared)"), c.alwaysVisible, () => SetShared(key, !c.alwaysVisible));
                        menu.AddItem(new GUIContent("Focus"), c == Collections.activeFilter, () => FocusCollection(key));
                        menu.AddItem(new GUIContent("Color"), false, () => CollectionColorPopup.Open(key, c.color));
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Delete Collection (Keep Objects)"), false, () => DeleteCollection(key));
                    }
                }
                else if (objects.TryGetValue(id, out var go) && go != null)
                {
                    if (!GetSelection().Contains(id)) SetSelection(new[] { id }, TreeViewSelectionOptions.FireSelectionChanged);
                    var selected = Selection.gameObjects;
                    native.AddObjectMenu(menu, go, () => BeginRename(FindItem(id, rootItem)));
                    menu.AddSeparator("");
                    menu.AddSeparator("");
                    foreach (var collection in Collections.collections)
                    {
                        string key = collection.id;
                        menu.AddItem(new GUIContent("Add to Collection/" + collection.name), collection.Contains(go), () => SetMembership(key, selected, true));
                        if (selected.Any(collection.Contains)) menu.AddItem(new GUIContent("Remove from Collection/" + collection.name), false, () => SetMembership(key, selected, false));
                    }
                }
                menu.ShowAsContext();
            }

            protected override void ContextClicked()
            {
                var menu = new GenericMenu();
                native.AddEmptyMenu(menu);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("New Collection"), false, () => CreateCollection("New Collection"));
                menu.ShowAsContext();
            }

            public void HandleCommands(Event e)
            {
                if (GUIUtility.keyboardControl != 0 && EditorGUIUtility.editingTextField) return;
                if (e.type == EventType.KeyDown && (e.control || e.command) && (e.keyCode == KeyCode.Z || e.keyCode == KeyCode.Y))
                {
                    if (e.keyCode == KeyCode.Y || e.shift) Undo.PerformRedo();
                    else Undo.PerformUndo();
                    e.Use(); return;
                }
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F2)
                { var item = FindItem(GetSelection().FirstOrDefault(), rootItem); if (item != null && CanRename(item)) BeginRename(item); e.Use(); return; }
                bool delete = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace);
                string command = delete ? "Delete" : e.commandName;
                bool isCommand = e.type == EventType.ExecuteCommand || e.type == EventType.ValidateCommand;
                if (!delete && !isCommand) return;
                bool execute = delete || e.type == EventType.ExecuteCommand;
                if (execute)
                {
                    switch (command)
                    {
                        case "Delete": case "SoftDelete":
                            var selectedFolders = GetSelection().Where(folders.ContainsKey).Select(id => folders[id]).Where(c => c != null).ToArray();
                            if (selectedFolders.Length > 0) Change("Delete Collections", () => { foreach (var c in selectedFolders) Collections.collections.Remove(c); });
                            else if (!folderSelection) DeleteSelectedObjects();
                            break;
                        case "Duplicate": if (!folderSelection) NativeCommand("DuplicateGO"); break;
                        case "Copy": if (!folderSelection) ClipboardCommand("CopyGO"); break;
                        case "Cut": if (!folderSelection) ClipboardCommand("CutGO"); break;
                        case "Paste":
                            var folder = GetSelection().Where(folders.ContainsKey).Select(id => folders[id]).FirstOrDefault(c => c != null);
                            Undo.IncrementCurrentGroup();
                            int group = Undo.GetCurrentGroup();
                            NativeCommand("PasteGO");
                            if (folder != null) { SetMembership(folder.id, Selection.gameObjects, true); Undo.CollapseUndoOperations(group); }
                            break;
                        case "UndoRedoPerformed": Reload(); break;
                        case "Undo": Undo.PerformUndo(); break;
                        case "Redo": Undo.PerformRedo(); break;
                        case "FrameSelected": SceneView.lastActiveSceneView?.FrameSelected(); break;
                        case "SelectAll": SetSelection(GetRows().Where(r => objects.ContainsKey(r.id)).Select(r => r.id).ToList(), TreeViewSelectionOptions.FireSelectionChanged); break;
                        case "DeselectAll": SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged); break;
                        case "InvertSelection": SetSelection(GetRows().Where(r => objects.ContainsKey(r.id) && !GetSelection().Contains(r.id)).Select(r => r.id).ToList(), TreeViewSelectionOptions.FireSelectionChanged); break;
                        case "Find": return;
                    }
                }
                e.Use();
            }

            public void ExpandFolder(string id)
            {
                var match = folders.FirstOrDefault(p => p.Value != null && p.Value.id == id);
                if (match.Key != 0) SetExpanded(match.Key, true);
            }

            public void RenameFolder(string id)
            {
                var match = folders.FirstOrDefault(p => p.Value != null && p.Value.id == id);
                if (match.Key == 0) return;
                SetSelection(new[] { match.Key }, TreeViewSelectionOptions.FireSelectionChanged);
                BeginRename(FindItem(match.Key, rootItem));
            }

            public bool SetFolderExpanded(string name, bool expanded)
            {
                var pair = folders.FirstOrDefault(p => p.Value?.name == name || (p.Value == null && name == "Unassigned"));
                if (pair.Key == 0) return false;
                SetExpanded(pair.Key, expanded); return true;
            }

            public bool SelectRow(int id)
            {
                if (!objects.ContainsKey(id) && !folders.ContainsKey(id) && !scenes.ContainsKey(id)) return false;
                SetSelection(new[] { id }, TreeViewSelectionOptions.FireSelectionChanged); return true;
            }

            public object[] DescribeRows() => GetRows().Select(row => (object)new
            {
                id = row.id, name = row.displayName, depth = row.depth, folder = folders.ContainsKey(row.id),
                objectId = objects.TryGetValue(row.id, out var go) && go != null ? go.GetInstanceID() : 0,
                collectionId = folders.TryGetValue(row.id, out var c) ? c?.id : groups.TryGetValue(row.id, out var group) ? group : null,
                scene = scenes.ContainsKey(row.id), expanded = IsExpanded(row.id)
            }).ToArray();
        }

        private sealed class CollectionColorPopup : EditorWindow
        {
            private string collectionId;
            private Color color;
            public static void Open(string id, Color color)
            {
                var w = CreateInstance<CollectionColorPopup>();
                w.collectionId = id; w.color = color; w.titleContent = new GUIContent("Collection Color");
                w.minSize = w.maxSize = new Vector2(250, 70); w.ShowUtility();
            }
            private void OnGUI()
            {
                color = EditorGUILayout.ColorField("Color", color);
                if (GUILayout.Button("Apply")) { SetColor(collectionId, color); Close(); }
            }
        }
    }
}

