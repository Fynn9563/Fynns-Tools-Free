using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed class MotionNamePopup : PopupWindowContent
    {
        private readonly Motion motion; private string name; private bool focus = true;
        internal MotionNamePopup(Motion motion) { this.motion = motion; name = motion.name; }
        public override Vector2 GetWindowSize() { return new Vector2(280, 70); }
        public override void OnGUI(Rect rect)
        {
            GUI.SetNextControlName("MotionName"); name = EditorGUILayout.TextField(name);
            var current = Event.current;
            bool enter = current.type == EventType.KeyDown && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter);
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape) { editorWindow.Close(); current.Use(); return; }
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(name) || !AssetDatabase.IsOpenForEdit(motion)))
                if ((enter || GUILayout.Button("Rename")) && !string.IsNullOrWhiteSpace(name) && AssetDatabase.IsOpenForEdit(motion))
                { Undo.RecordObject(motion, "Rename Motion"); motion.name = name.Trim(); EditorUtility.SetDirty(motion); editorWindow.Close(); }
            if (focus && current.type == EventType.Repaint) { focus = false; GUI.FocusControl("MotionName"); }
        }
    }
    internal sealed partial class ControllerEditor
    {
        private static void DrawTreeMotionExtras(Node node, Rect rect)
        {
            HandleTreeReparent(node, rect);
            var motion = codeVisitor.GetValue(node) as Motion;
            var parentNode = _InvocationVisitor.GetValue(node);
            var parent = parentNode == null ? null : codeVisitor.GetValue(parentNode) as BlendTree;
            int index = parentNode == null ? -1 : ((IList)m_DicVisitor.GetValue(parentNode)).IndexOf(node);
            var current = Event.current;
            if (motion && current.type == EventType.KeyDown && current.keyCode == KeyCode.F2 && !EditorGUIUtility.editingTextField &&
                (UnityAnimatorBridge.RefTests.CalcPolicy()?.Any(selected => selected.FillPolicy() == node) ?? false))
            { PopupWindow.Show(new Rect(rect.x, rect.y, rect.width, 1), new MotionNamePopup(motion)); current.Use(); }
            if (!ControllerEditorExtraSettings.Current.blendTreeDetails || !motion) return;
            string description = motion is AnimationClip clip ? clip.length.ToString("0.##") + "s" + (clip.isLooping ? " · Loop" : "") : "";
            if (parent && index >= 0 && index < parent.children.Length)
            {
                var child = parent.children[index];
                string weight = parent.blendType == BlendTreeType.Direct ? child.directBlendParameter : child.threshold.ToString("0.###");
                if (!(motion is BlendTree)) description += " · " + weight;
            }
            if (description.Length > 0)
                GUI.Label(new Rect(rect.x + 4, rect.yMax - 17, Mathf.Max(1, rect.width - 8), 16), new GUIContent(description, description), new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.LowerCenter });
        }
        private static void AddTreeMotionSearch(GenericMenu menu, object node)
        {
            var parentNode = _InvocationVisitor.GetValue(node);
            var parent = parentNode == null ? null : codeVisitor.GetValue(parentNode) as BlendTree;
            if (!parent || !ControllerEditorExtraSettings.Current.motionSearch) return;
            int index = ((IList)m_DicVisitor.GetValue(parentNode)).IndexOf(node);
            if (index < 0 || index >= parent.children.Length) return;
            menu.AddItem(new GUIContent("Search Motion…"), false, () => PopupWindow.Show(new Rect(Event.current?.mousePosition ?? Vector2.zero, Vector2.zero), new ControllerMotionPicker((motion, record) =>
            {
                if (motion is BlendTree tree && ControllerBlendTreeTools.Contains(tree, parent)) return;
                Undo.RecordObject(parent, "Set Child Motion");
                var children = parent.children; children[index].motion = motion; parent.children = children; EditorUtility.SetDirty(parent);
                if (record && motion is AnimationClip clip) { var window = EditorWindow.GetWindow<AnimationWindow>(); window.animationClip = clip; if (window.canRecord) window.recording = true; }
            })));
        }
    }
}
