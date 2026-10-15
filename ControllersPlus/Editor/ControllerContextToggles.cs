using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        internal static BlendTree CreateContextToggle(Object[] targets, bool grouped)
        {
            var controller = LogoutMapper(); var machine = RevertMapper();
            if (!controller || !machine || !AssetDatabase.IsOpenForEdit(controller)) throw new ArgumentException("Open an editable Animator controller first.");
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                var parent = m_AlgoAnnotation.Where(state => state && AssetDatabase.GetAssetPath(state) == AssetDatabase.GetAssetPath(controller)).Select(state => state.motion as BlendTree).FirstOrDefault(tree => tree);
                if (!parent)
                {
                    Undo.RegisterCompleteObjectUndo(machine, "Create Toggle State");
                    var state = machine.AddState("Toggles", new Vector3(300, 300)); QueryAlgo(state);
                    parent = new BlendTree { name = "Toggles", blendType = BlendTreeType.Direct, hideFlags = HideFlags.HideInHierarchy };
                    AssetDatabase.AddObjectToAsset(parent, controller); state.motion = parent;
                    Undo.RegisterCreatedObjectUndo(parent, "Create Toggle State"); Undo.RegisterCreatedObjectUndo(state, "Create Toggle State");
                }
                var root = ControllerEditorSettingsWindow.m_AdvisorMapper ? ControllerEditorSettingsWindow.m_AdvisorMapper.transform : null;
                if (grouped) ControllerBlendTreeToggles.CreateGroup(parent, targets, controller, root);
                else foreach (var target in targets) ControllerBlendTreeToggles.Create(parent, target, controller, root);
                Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group); PatchAlgo(); return parent;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        [MenuItem("CONTEXT/Component/Fynn's Tools/Create BlendTree Toggle", true)]
        private static bool CanComponentToggle(MenuCommand command)
        { return command.context is Component component && component.GetType().GetProperty("enabled")?.PropertyType == typeof(bool) && LogoutMapper(); }
        [MenuItem("CONTEXT/Component/Fynn's Tools/Create BlendTree Toggle")]
        private static void ComponentToggle(MenuCommand command) { CreateContextToggle(new[] { command.context }, false); }
        [MenuItem("GameObject/Fynn's Tools/Create BlendTree Toggle", false, 52)]
        private static void ObjectToggles() { CreateContextToggle(Selection.gameObjects.Cast<Object>().ToArray(), false); }
        [MenuItem("GameObject/Fynn's Tools/Create BlendTree Toggle", true)]
        private static bool CanObjectToggles() { return Selection.gameObjects.Length > 0 && LogoutMapper(); }
        [MenuItem("GameObject/Fynn's Tools/Create Group BlendTree Toggle", false, 53)]
        private static void GroupObjectToggle() { CreateContextToggle(Selection.gameObjects.Cast<Object>().ToArray(), true); }
        [MenuItem("GameObject/Fynn's Tools/Create Group BlendTree Toggle", true)]
        private static bool CanGroupObjectToggle() { return Selection.gameObjects.Length > 1 && LogoutMapper(); }
    }
}
