using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HierarchyPlus
{
    internal sealed class CollectionsNativeHierarchy
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private readonly object hierarchy;
        private readonly object gui;
        private readonly object controller;
        private readonly Type itemType;
        private readonly MethodInfo initialize;
        private readonly MethodInfo additional;
        private readonly MethodInfo content;
        private readonly MethodInfo visibility;
        private readonly MethodInfo background;
        private readonly Dictionary<int, TreeViewItem> items = new Dictionary<int, TreeViewItem>();
        private readonly HierarchyProperty properties = new HierarchyProperty(HierarchyType.GameObjects);

        internal CollectionsNativeHierarchy(object hierarchy)
        {
            this.hierarchy = hierarchy;
            controller = hierarchy.GetType().GetField("m_TreeView", Flags).GetValue(hierarchy);
            gui = controller.GetType().GetProperty("gui", Flags).GetValue(controller);
            var assembly = typeof(Editor).Assembly;
            itemType = assembly.GetType("UnityEditor.GameObjectTreeViewItem", true);
            initialize = gui.GetType().GetMethod("EnsureLazyInitialization", Flags);
            additional = gui.GetType().GetMethod("OnAdditionalGUI", Flags);
            content = gui.GetType().GetMethod("OnContentGUI", Flags);
            var visibilityType = assembly.GetType("UnityEditor.SceneVisibilityHierarchyGUI", true);
            visibility = visibilityType.GetMethod("DoItemGUI", Flags);
            background = visibilityType.GetMethod("DrawBackground", Flags);
        }

        internal void Clear() { items.Clear(); }

        internal bool IsVisibleScene(Scene scene)
        {
            var data = controller.GetType().GetProperty("data", Flags).GetValue(controller);
            var property = (HierarchyProperty)data.GetType().GetMethod("CreateHierarchyProperty", Flags).Invoke(data, null);
            return property.Find(scene.handle, null);
        }

        internal TreeViewItem Item(GameObject go, int depth)
        {
            int id = go.GetInstanceID();
            if (!items.TryGetValue(id, out var item))
            {
                item = (TreeViewItem)Activator.CreateInstance(itemType, new object[] { id, depth, null, go.name });
                itemType.GetProperty("objectPPTR", Flags).SetValue(item, go);
                itemType.GetProperty("scene", Flags).SetValue(item, go.scene);
                if (properties.Find(id, null)) itemType.GetProperty("colorCode", Flags).SetValue(item, properties.colorCode);
                items.Add(id, item);
            }
            item.depth = depth;
            item.displayName = go.name;
            return item;
        }

        internal TreeViewItem SceneItem(Scene scene, int depth)
        {
            var item = (TreeViewItem)Activator.CreateInstance(itemType, new object[] { scene.handle, depth, null, scene.name });
            itemType.GetProperty("scene", Flags).SetValue(item, scene);
            itemType.GetProperty("isSceneHeader", Flags).SetValue(item, true);
            return item;
        }

        internal void DrawGutter(Rect rect) { background.Invoke(null, new object[] { rect }); }

        internal void DrawObject(GameObject go, TreeViewItem projection, Rect rect, Rect labelRect, int row, bool selected, bool focused, bool renaming)
        {
            var item = Item(go, projection.depth);
            DrawItem(item, rect, labelRect, row, selected, focused, renaming);
        }

        internal void DrawItem(TreeViewItem item, Rect rect, Rect labelRect, int row, bool selected, bool focused, bool renaming)
        {
            initialize.Invoke(gui, new object[] { item });
            additional.Invoke(gui, new object[] { rect, row, item, selected, focused });
            if (!renaming)
                content.Invoke(gui, new object[] { labelRect, row, item, item.displayName, selected, focused, (bool)itemType.GetProperty("isSceneHeader", Flags).GetValue(item), true });
            bool hovering = rect.Contains(Event.current.mousePosition);
            bool dragging = Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform;
            visibility.Invoke(null, new object[] { rect, item, selected && !renaming, hovering, focused, dragging });
        }

        internal float DecorationRightEdge(Rect row)
        {
            var styles = gui.GetType().GetNestedType("GameObjectStyles", Flags);
            var arrow = (GUIStyle)styles.GetField("rightArrow", Flags).GetValue(null);
            return row.xMax - arrow.fixedWidth - arrow.margin.horizontal;
        }

        internal void AddObjectMenu(GenericMenu menu, GameObject go, Action rename)
        {
            Select(Selection.instanceIDs);
            hierarchy.GetType().GetMethod("CreateGameObjectContextClick", Flags).Invoke(hierarchy, new object[] { menu, go.GetInstanceID() });
            foreach (var entry in (System.Collections.IEnumerable)typeof(GenericMenu).GetProperty("menuItems", Flags).GetValue(menu))
            {
                var type = entry.GetType();
                var label = (GUIContent)type.GetField("content", Flags).GetValue(entry);
                if (label != null && label.text == "Rename")
                {
                    type.GetField("func", Flags).SetValue(entry, new GenericMenu.MenuFunction(rename.Invoke));
                    type.GetField("func2", Flags).SetValue(entry, null);
                }
            }
        }

        internal void AddSceneMenu(GenericMenu menu, Scene scene)
        {
            Select(new[] { scene.handle });
            hierarchy.GetType().GetMethod("CreateSceneHeaderContextClick", Flags).Invoke(hierarchy, new object[] { menu, scene });
        }

        internal void AddEmptyMenu(GenericMenu menu)
        {
            Selection.objects = new UnityEngine.Object[0];
            Select(new int[0]);
            hierarchy.GetType().GetMethod("CreateGameObjectContextClick", Flags).Invoke(hierarchy, new object[] { menu, 0 });
        }

        internal void Select(int[] ids)
        {
            controller.GetType().GetMethod("SetSelection", Flags, null, new[] { typeof(int[]), typeof(bool) }, null).Invoke(controller, new object[] { ids, false });
        }

        internal DragAndDropVisualMode DropOnObject(GameObject go, int depth, bool perform)
        {
            return Drop(Item(go, depth), Item(go, depth), "Upon", perform);
        }

        internal DragAndDropVisualMode DropOnScene(Scene scene, bool perform)
        {
            var item = SceneItem(scene, 0);
            return Drop(item, item, "Upon", perform);
        }

        internal DragAndDropVisualMode DropBesideScene(Scene scene, bool above, bool perform)
        {
            return Drop(null, SceneItem(scene, 0), above ? "Above" : "Below", perform);
        }

        internal void StartSceneDrag(Scene scene, List<int> ids)
        {
            var dragging = controller.GetType().GetProperty("dragging", Flags).GetValue(controller);
            dragging.GetType().GetMethod("StartDrag", Flags).Invoke(dragging, new object[] { SceneItem(scene, 0), ids });
        }

        internal DragAndDropVisualMode DropBesideObject(GameObject go, bool above, bool perform)
        {
            var target = Item(go, 0);
            var parent = go.transform.parent;
            var parentItem = parent != null ? Item(parent.gameObject, 0) : SceneItem(go.scene, 0);
            target.parent = parentItem;
            parentItem.children = new List<TreeViewItem>();
            if (parent != null && parent.childCount > 0) parentItem.children.Add(Item(parent.GetChild(0).gameObject, 0));
            else if (parent == null && go.scene.rootCount > 0) parentItem.children.Add(Item(go.scene.GetRootGameObjects()[0], 0));
            return Drop(parentItem, target, above ? "Above" : "Below", perform);
        }

        private DragAndDropVisualMode Drop(TreeViewItem parent, TreeViewItem target, string position, bool perform)
        {
            var dragging = controller.GetType().GetProperty("dragging", Flags).GetValue(controller);
            var method = dragging.GetType().GetMethod("DoDrag", Flags);
            var positionType = method.GetParameters()[3].ParameterType;
            return (DragAndDropVisualMode)method.Invoke(dragging, new object[] { parent, target, perform, Enum.Parse(positionType, position) });
        }

        internal bool IsAlphabetical => (string)hierarchy.GetType().GetField("m_CurrentSortingName", Flags).GetValue(hierarchy) == "AlphabeticalSorting";

        internal bool IsLocked
        {
            get
            {
                var tracker = hierarchy.GetType().GetField("m_LockTracker", Flags).GetValue(hierarchy);
                return (bool)tracker.GetType().GetProperty("isLocked", Flags).GetValue(tracker);
            }
        }

        internal int SearchMode => Convert.ToInt32(hierarchy.GetType().GetField("m_SearchMode", Flags).GetValue(hierarchy));

        internal HashSet<int> Search(string query)
        {
            var found = new HashSet<int>();
            var property = new HierarchyProperty(HierarchyType.GameObjects);
            property.SetSearchFilter(query, SearchMode);
            while (property.Next(null)) found.Add(property.instanceID);
            return found;
        }
    }
}
