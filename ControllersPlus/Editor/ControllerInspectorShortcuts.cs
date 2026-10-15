using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    [InitializeOnLoad]
    internal static class ControllerInspectorShortcuts
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly FieldInfo Handler = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly EditorApplication.CallbackFunction Callback = Handle;
        static ControllerInspectorShortcuts()
        {
            if (Handler == null) return;
            var current = (EditorApplication.CallbackFunction)Handler.GetValue(null);
            Handler.SetValue(null, (EditorApplication.CallbackFunction)Delegate.Combine(Delegate.Remove(current, Callback), Callback));
        }
        private static void Handle()
        {
            var window = EditorWindow.focusedWindow;
            var current = Event.current;
            if (!window || current == null || current.type != EventType.KeyDown || EditorGUIUtility.editingTextField || !ControllerEditorExtraSettings.Current.inspectorShortcuts) return;
            bool inspector = window.GetType().Name == "InspectorWindow";
            bool project = window.GetType().Name == "ProjectBrowser";
            bool modifier = current.control || current.command;
            if (!current.alt && current.keyCode == KeyCode.L && (inspector || project))
            {
                var locked = window.GetType().GetProperty("isLocked", Flags);
                if (locked == null || !locked.CanWrite) return;
                locked.SetValue(window, !(bool)locked.GetValue(window, null), null); window.Repaint(); current.Use();
            }
            else if (inspector && modifier && current.keyCode == KeyCode.W) { window.Close(); current.Use(); }
            else if (inspector && (current.keyCode == KeyCode.I || current.keyCode == KeyCode.D) && !current.alt && (!modifier || current.shift))
            {
                Duplicate(window, current.shift && modifier ? 0 : current.shift ? 2 : 1);
                current.Use();
            }
        }
        internal static EditorWindow Duplicate(EditorWindow original, int mode)
        {
            if (!original || original.GetType().Name != "InspectorWindow") return null;
            var duplicate = (EditorWindow)ScriptableObject.CreateInstance(original.GetType());
            duplicate.titleContent = new GUIContent("Inspector");
            var objects = new List<Object>();
            original.GetType().GetMethod("GetObjectsLocked", Flags)?.Invoke(original, new object[] { objects });
            if (objects.Count == 0) objects.AddRange(Selection.objects.Where(value => value));
            duplicate.GetType().GetMethod("SetObjectsLocked", Flags)?.Invoke(duplicate, new object[] { objects });
            duplicate.GetType().GetProperty("isLocked", Flags)?.SetValue(duplicate, true, null);
            var parentField = typeof(EditorWindow).GetField("m_Parent", Flags);
            var dock = parentField?.GetValue(original);
            var dockType = typeof(EditorWindow).Assembly.GetType("UnityEditor.DockArea");
            var viewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.View");
            var splitType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SplitView");
            if (dock == null || dockType == null || !dockType.IsInstanceOfType(dock)) { duplicate.position = original.position; duplicate.Show(); return duplicate; }
            if (mode == 0)
            {
                dockType.GetMethod("AddTab", Flags, null, new[] { typeof(EditorWindow), typeof(bool) }, null)?.Invoke(dock, new object[] { duplicate, true });
                return duplicate;
            }
            var parent = viewType.GetProperty("parent", Flags).GetValue(dock, null);
            if (parent == null) { duplicate.position = original.position; duplicate.Show(); return duplicate; }
            object reflow = parent;
            var sibling = ScriptableObject.CreateInstance(dockType);
            var add = viewType.GetMethod("AddChild", Flags, null, new[] { viewType, typeof(int) }, null);
            var position = viewType.GetProperty("position", Flags);
            bool vertical = mode == 2;
            int index = (int)viewType.GetMethod("IndexOfChild", Flags).Invoke(parent, new[] { dock });
            if (splitType.IsInstanceOfType(parent) && (bool)splitType.GetField("vertical", Flags).GetValue(parent) == vertical)
                add.Invoke(parent, new object[] { sibling, index + 1 });
            else
            {
                var split = ScriptableObject.CreateInstance(splitType);
                reflow = split;
                splitType.GetField("vertical", Flags).SetValue(split, vertical);
                var rect = position.GetValue(dock, null);
                viewType.GetMethod("RemoveChild", Flags, null, new[] { viewType }, null).Invoke(parent, new[] { dock });
                add.Invoke(parent, new object[] { split, index });
                add.Invoke(split, new object[] { dock, 0 }); add.Invoke(split, new object[] { sibling, 1 });
                position.SetValue(split, rect, null);
            }
            dockType.GetMethod("AddTab", Flags, null, new[] { typeof(EditorWindow), typeof(bool) }, null).Invoke(sibling, new object[] { duplicate, true });
            splitType.GetMethod("Reflow", Flags).Invoke(reflow, null);
            duplicate.Focus(); return duplicate;
        }
    }
}
