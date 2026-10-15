using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        private static Node movingTreeNode;
        private static BlendTree movingTreeParent;
        private static int movingTreeIndex;
        private static void RegisterNativeBlendTreeTools()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            void Patch(Type type, string method, string prefix, string postfix = null)
            {
                var target = AccessTools.Method(type, method);
                if (target != null) EditorPatchRegistry.RateReg(target, prefix == null ? null : typeof(ControllerEditor).GetMethod(prefix, flags), postfix == null ? null : typeof(ControllerEditor).GetMethod(postfix, flags));
            }
            Patch(typeof(Node).Assembly.GetType("UnityEditor.Graphs.AnimationStateMachine.GraphGUI"), "AddStateFromNewBlendTreeCallback", nameof(NewExternalTreeStatePrefix));
            Patch(typeof(Node).Assembly.GetType("UnityEditor.Graphs.AnimationBlendTree.GraphGUI"), "CreateBlendTreeCallback", nameof(NewNativeTreeChildPrefix));
            Patch(typeof(Editor).Assembly.GetType("UnityEditor.BlendTreeInspector"), "AddBlendTreeCallback", nameof(NewInspectorTreeChildPrefix));
            Patch(typeof(UnityEditor.Graphs.GraphGUI), "Slot", nameof(TreeInputSlotPrefix));
            Patch(typeof(Node).Assembly.GetType("UnityEditor.Graphs.AnimationBlendTree.GraphGUI"), "OnGraphGUI", null, nameof(TreeReparentGraphPost));
        }
        private static BlendTree NewExternalRoot(AnimatorController controller)
        {
            string folder = ControllerBlendTreeTools.EnsureFolder(ControllerEditorExtraSettings.Current.blendTreeFolder);
            var tree = new BlendTree { name = "Blend Tree", blendType = BlendTreeType.Simple1D, blendParameter = controller.parameters.FirstOrDefault(parameter => parameter.type == AnimatorControllerParameterType.Float)?.name ?? "" };
            AssetDatabase.CreateAsset(tree, AssetDatabase.GenerateUniqueAssetPath(folder + "/Blend Tree.asset"));
            Undo.RegisterCreatedObjectUndo(tree, "Create External BlendTree"); return tree;
        }
        private static bool NewExternalTreeStatePrefix(object __instance, object __0)
        {
            if (!ControllerEditorExtraSettings.Current.externalBlendTrees) return true;
            var machine = AccessTools.Property(__instance.GetType(), "activeStateMachine")?.GetValue(__instance, null) as AnimatorStateMachine;
            var host = AccessTools.Field(__instance.GetType(), "m_Host")?.GetValue(__instance);
            var controller = host == null ? null : AccessTools.Property(host.GetType(), "animatorController")?.GetValue(host, null) as AnimatorController;
            if (!machine || !controller || !AssetDatabase.IsOpenForEdit(controller)) return true;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                Undo.RegisterCompleteObjectUndo(machine, "Create BlendTree State");
                var state = machine.AddState("Blend Tree", __0 is Vector2 position ? (Vector3)position : Vector3.zero); QueryAlgo(state);
                state.motion = NewExternalRoot(controller); Undo.RegisterCreatedObjectUndo(state, "Create BlendTree State");
                Undo.CollapseUndoOperations(group); PatchAlgo(); return false;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        private static bool NewNativeTreeChildPrefix(object __0)
        {
            if (!ControllerEditorExtraSettings.Current.externalBlendTrees || !(__0 is BlendTree parent)) return true;
            var tree = ControllerBlendTreeTools.AddChild(parent, BlendTreeType.Simple1D, LogoutMapper());
            tree.blendParameter = parent.blendParameter; EditorUtility.SetDirty(tree); return false;
        }
        private static bool NewInspectorTreeChildPrefix(Editor __instance)
        {
            if (!ControllerEditorExtraSettings.Current.externalBlendTrees || !(__instance.target is BlendTree parent)) return true;
            var controller = AccessTools.Property(__instance.GetType(), "currentController")?.GetValue(__instance, null) as AnimatorController;
            var tree = ControllerBlendTreeTools.AddChild(parent, BlendTreeType.Simple1D, controller);
            tree.blendParameter = parent.blendParameter; EditorUtility.SetDirty(tree); __instance.serializedObject.Update(); return false;
        }
        internal static bool MoveTreeChild(BlendTree source, int index, BlendTree destination, AnimatorController controller)
        {
            if (!source || !destination || source == destination || !AssetDatabase.IsOpenForEdit(source) || !AssetDatabase.IsOpenForEdit(destination) || index < 0 || index >= source.children.Length) return false;
            var child = source.children[index];
            if (!child.motion || child.motion == destination || (child.motion is BlendTree tree && ControllerBlendTreeTools.Contains(tree, destination))) return false;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                Undo.RegisterCompleteObjectUndo(new Object[] { source, destination }, "Move BlendTree Child");
                if (destination.blendType == BlendTreeType.Direct && (string.IsNullOrEmpty(child.directBlendParameter) || (controller && !controller.parameters.Any(parameter => parameter.name == child.directBlendParameter && parameter.type == AnimatorControllerParameterType.Float))))
                    child.directBlendParameter = ControllerBlendTreeTools.EnsureWeight(controller);
                source.children = source.children.Where((value, childIndex) => childIndex != index).ToArray();
                destination.children = destination.children.Concat(new[] { child }).ToArray();
                EditorUtility.SetDirty(source); EditorUtility.SetDirty(destination); Undo.CollapseUndoOperations(group); return true;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        private static void TreeInputSlotPrefix(object __instance, Rect __0, Slot __2)
        {
            var current = Event.current;
            if (!ControllerEditorExtraSettings.Current.reparentTrees || __instance.GetType() != m_InterpreterVisitor || __2 == null || __2.type != SlotType.InputSlot || current.type != EventType.MouseDown || current.button != 0 || !__0.Contains(current.mousePosition)) return;
            var node = __2.node; var parentNode = _InvocationVisitor.GetValue(node) as Node;
            var parent = parentNode ? codeVisitor.GetValue(parentNode) as BlendTree : null;
            int index = parentNode ? ((IList)m_DicVisitor.GetValue(parentNode)).IndexOf(node) : -1;
            if (!parent || index < 0 || index >= parent.children.Length) return;
            movingTreeNode = node; movingTreeParent = parent; movingTreeIndex = index;
            current.Use(); QueryAnnotation()?.Repaint();
        }
        private static void HandleTreeReparent(Node node, Rect rect)
        {
            var current = Event.current;
            if (!movingTreeNode || current.type != EventType.MouseUp || !rect.Contains(current.mousePosition)) return;
            var destination = codeVisitor.GetValue(node) as BlendTree;
            var source = movingTreeParent; int index = movingTreeIndex; var controller = LogoutMapper();
            movingTreeNode = null; movingTreeParent = null;
            if (!destination) return;
            EditorApplication.delayCall += () => { if (MoveTreeChild(source, index, destination, controller)) PatchAlgo(); };
            current.Use();
        }
        private static void TreeReparentGraphPost()
        {
            if (!movingTreeNode) return;
            var current = Event.current;
            if (current.rawType == EventType.MouseUp || (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape))
            { movingTreeNode = null; movingTreeParent = null; return; }
            var old = Handles.color;
            try { Handles.color = Color.white; Handles.DrawLine(movingTreeNode.position.position, current.mousePosition); }
            finally { Handles.color = old; }
            if (current.type == EventType.MouseDrag) QueryAnnotation()?.Repaint();
        }
    }
}


