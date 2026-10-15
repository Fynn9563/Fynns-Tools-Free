using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
using VRC.SDK3.Avatars.Components;
#endif
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerBlendTreeToggles
    {
        internal static BlendTree Create(BlendTree parent, Object target, AnimatorController controller, Transform assignedRoot = null)
        {
            return CreateGroup(parent, new[] { target }, controller, assignedRoot);
        }
        internal static BlendTree CreateGroup(BlendTree parent, Object[] targets, AnimatorController controller, Transform assignedRoot = null)
        {
            targets = targets.Where(value => value).Distinct().ToArray();
            if (!parent || !AssetDatabase.IsOpenForEdit(parent) || targets.Length == 0) throw new ArgumentException("Choose an editable BlendTree and scene objects.");
            var component = targets[0] as Component;
            var gameObject = targets[0] as GameObject ?? component?.gameObject;
            var root = ToggleRoot(targets[0], assignedRoot);
            foreach (var target in targets) ToggleBinding(target, root);
            string parameter = ControllerEditor.MotionAssetOperations.AwakeReg(targets.Length == 1 ? gameObject.name : "Group") + "_Enabled";
            if (controller)
            {
                string baseName = parameter; int suffix = 1;
                while (controller.parameters.Any(value => value.name == parameter)) parameter = baseName + "_" + suffix++;
            }
            string folder = ControllerBlendTreeTools.EnsureFolder("Assets/Fynn's Tools/Generated/Controller Editor/Toggles");
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                AnimationClip Clip(bool enabled)
                {
                    var clip = new AnimationClip { name = parameter + (enabled ? " On" : " Off"), frameRate = 60 };
                    foreach (var target in targets) AnimationUtility.SetEditorCurve(clip, ToggleBinding(target, root), AnimationCurve.Constant(0, 1f / 60f, enabled ? 1 : 0));
                    AssetDatabase.CreateAsset(clip, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + clip.name + ".anim"));
                    Undo.RegisterCreatedObjectUndo(clip, "Create BlendTree Toggle"); return clip;
                }
                var off = Clip(false); var on = Clip(true);
                var tree = ControllerBlendTreeTools.AddChild(parent, BlendTreeType.Simple1D, controller);
                Undo.RegisterCompleteObjectUndo(tree, "Create BlendTree Toggle");
                tree.name = parameter; tree.blendParameter = parameter; tree.useAutomaticThresholds = false;
                tree.children = new[] { new ChildMotion { motion = off, threshold = 0, timeScale = 1 }, new ChildMotion { motion = on, threshold = 1, timeScale = 1 } };
                if (controller)
                {
                    Undo.RecordObject(controller, "Create BlendTree Toggle");
                    controller.AddParameter(new AnimatorControllerParameter { name = parameter, type = AnimatorControllerParameterType.Float,
                        defaultFloat = component ? ((bool)component.GetType().GetProperty("enabled").GetValue(component, null) ? 1 : 0) : gameObject.activeSelf ? 1 : 0 });
                    if (parent.blendType == BlendTreeType.Direct && !controller.parameters.Any(value => value.name == ControllerEditorExtraSettings.Current.directWeightParameter))
                        controller.AddParameter(new AnimatorControllerParameter { name = ControllerEditorExtraSettings.Current.directWeightParameter, type = AnimatorControllerParameterType.Float, defaultFloat = 1 });
                    EditorUtility.SetDirty(controller);
                }
                EditorUtility.SetDirty(tree); Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group); ControllerParameterTools.ClearCache(); return tree;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        private static Transform ToggleRoot(Object target, Transform assignedRoot)
        {
            var gameObject = target as GameObject ?? (target as Component)?.gameObject;
            if (!gameObject || !gameObject.scene.IsValid()) throw new ArgumentException("Choose a scene object.");
            var animator = gameObject.GetComponentInParent<Animator>();
            var root = assignedRoot ? assignedRoot : animator ? animator.transform : gameObject.transform.root;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            var avatar = gameObject.GetComponentInParent<VRCAvatarDescriptor>();
            if (!assignedRoot && avatar) root = avatar.transform;
#endif
            return root;
        }
        private static EditorCurveBinding ToggleBinding(Object target, Transform root)
        {
            var component = target as Component;
            var gameObject = target as GameObject ?? component?.gameObject;
            if (!gameObject || !root || !gameObject.scene.IsValid() || (gameObject.transform != root && !gameObject.transform.IsChildOf(root))) throw new ArgumentException("All targets must belong to the chosen avatar.");
            if (component && component.GetType().GetProperty("enabled")?.PropertyType != typeof(bool)) throw new ArgumentException("This component has no Enabled property.");
            return EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(gameObject.transform, root), component ? component.GetType() : typeof(GameObject), component ? "m_Enabled" : "m_IsActive");
        }
        internal static AnimationClip AddAnimation(BlendTree parent, Object[] targets, bool enabled, AnimatorController controller, Transform assignedRoot = null)
        {
            if (targets == null || targets.Length == 0 || !parent || !AssetDatabase.IsOpenForEdit(parent)) throw new ArgumentException("Choose a BlendTree and scene objects.");
            var root = ToggleRoot(targets[0], assignedRoot);
            var bindings = targets.Select(target => ToggleBinding(target, root)).ToArray();
            string folder = ControllerBlendTreeTools.EnsureFolder("Assets/Fynn's Tools/Generated/Controller Editor/Toggles");
            var clip = new AnimationClip { name = ControllerEditor.MotionAssetOperations.AwakeReg(targets[0].name) + (enabled ? " On" : " Off"), frameRate = 60 };
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                foreach (var binding in bindings) AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0, 1f / 60f, enabled ? 1 : 0));
                AssetDatabase.CreateAsset(clip, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + clip.name + ".anim"));
                Undo.RegisterCreatedObjectUndo(clip, "Create Toggle Animation"); ControllerBlendTreeCreation.AddMotion(parent, clip, controller); Undo.CollapseUndoOperations(group); return clip;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        internal static void HandleDrop(Rect rect, BlendTree tree, AnimatorController controller, Transform root)
        {
            var current = Event.current;
            if (!tree || !rect.Contains(current.mousePosition) || (current.type != EventType.DragUpdated && current.type != EventType.DragPerform)) return;
            var targets = DragAndDrop.objectReferences.Where(value => value is GameObject || (value is Component component && component.GetType().GetProperty("enabled")?.PropertyType == typeof(bool))).Distinct().ToArray();
            if (targets.Length == 0) return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (current.type == EventType.DragPerform)
            {
                Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
                try
                {
                    if (targets.Length == 1 && !current.alt) Create(tree, targets[0], controller, root);
                    else
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Create Separate Toggles"), false, () => { Undo.IncrementCurrentGroup(); int undo = Undo.GetCurrentGroup(); try { foreach (var target in targets) Create(tree, target, controller, root); Undo.CollapseUndoOperations(undo); } catch { Undo.RevertAllDownToGroup(undo); throw; } });
                        menu.AddItem(new GUIContent("Create Group Toggle"), false, () => CreateGroup(tree, targets, controller, root));
                        menu.AddItem(new GUIContent("Add On Animation"), false, () => AddAnimation(tree, targets, true, controller, root));
                        menu.AddItem(new GUIContent("Add Off Animation"), false, () => AddAnimation(tree, targets, false, controller, root));
                        menu.ShowAsContext();
                    }
                    Undo.CollapseUndoOperations(group); DragAndDrop.AcceptDrag();
                }
                catch (Exception exception) { Undo.RevertAllDownToGroup(group); Debug.LogWarning(exception.Message, tree); }
            }
            current.Use();
        }
    }
}
