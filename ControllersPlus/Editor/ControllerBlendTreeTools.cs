using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerBlendTreeTools
    {
        internal static BlendTree CloneData(BlendTree source)
        {
            var copy = new BlendTree();
            EditorUtility.CopySerialized(source, copy);
            copy.name = source.name;
            return copy;
        }
        internal static string EnsureFolder(string folder)
        {
            folder = (folder ?? "").Replace('\\', '/').TrimEnd('/');
            if (Path.IsPathRooted(folder)) folder = FileUtil.GetProjectRelativePath(folder);
            if (folder != "Assets" && !folder.StartsWith("Assets/", StringComparison.Ordinal))
                throw new ArgumentException("Choose a folder inside Assets.");
            var parts = folder.Split('/');
            if (parts.Any(part => string.IsNullOrWhiteSpace(part) || part == "." || part == "..")) throw new ArgumentException("Choose a valid Assets folder.");
            string path = "Assets";
            foreach (string part in parts.Skip(1))
            {
                if (!AssetDatabase.IsValidFolder(path + "/" + part)) AssetDatabase.CreateFolder(path, part);
                path += "/" + part;
            }
            return path;
        }
        private static void Attach(BlendTree tree, Object owner)
        {
            string path = AssetDatabase.GetAssetPath(owner);
            var main = AssetDatabase.LoadMainAssetAtPath(path);
            bool externalOwner = main is BlendTree;
            if (!externalOwner && ControllerEditorExtraSettings.Current.externalBlendTrees)
            {
                string folder = EnsureFolder(ControllerEditorExtraSettings.Current.blendTreeFolder);
                AssetDatabase.CreateAsset(tree, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + ControllerEditor.MotionAssetOperations.AwakeReg(tree.name) + ".asset"));
            }
            else
            {
                if (!main || !AssetDatabase.IsOpenForEdit(main)) throw new InvalidOperationException("Save an editable controller or BlendTree first.");
                tree.hideFlags |= HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(tree, main);
            }
        }
        internal static string EnsureWeight(AnimatorController controller)
        {
            string name = string.IsNullOrWhiteSpace(ControllerEditorExtraSettings.Current.directWeightParameter) ? "Weight" : ControllerEditorExtraSettings.Current.directWeightParameter;
            if (!controller) return name;
            var existing = controller.parameters.FirstOrDefault(parameter => parameter.name == name);
            if (existing != null && existing.type == AnimatorControllerParameterType.Float) return name;
            if (existing != null) name = controller.MakeUniqueParameterName(name);
            Undo.RecordObject(controller, "Add Direct Blend Weight");
            controller.AddParameter(new AnimatorControllerParameter { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = 1 });
            EditorUtility.SetDirty(controller); return name;
        }
        internal static BlendTree AddChild(BlendTree parent, BlendTreeType type, AnimatorController controller = null)
        {
            if (!parent) return null;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            var tree = new BlendTree { name = "BlendTree", blendType = type, blendParameter = "", blendParameterY = "" };
            try
            {
                Attach(tree, parent);
                Undo.RegisterCompleteObjectUndo(parent, "Add BlendTree");
                var children = parent.children;
                string directWeight = parent.blendType == BlendTreeType.Direct ? EnsureWeight(controller ? controller : AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(parent)) as AnimatorController) : ControllerEditorExtraSettings.Current.directWeightParameter;
                parent.children = children.Concat(new[] { new ChildMotion { motion = tree, timeScale = 1,
                    threshold = children.Length == 0 ? 0 : children.Max(child => child.threshold) + 1,
                    directBlendParameter = directWeight } }).ToArray();
                EditorUtility.SetDirty(parent); EditorUtility.SetDirty(tree);
                Undo.RegisterCreatedObjectUndo(tree, "Add BlendTree"); Undo.CollapseUndoOperations(group);
                return tree;
            }
            catch { Undo.RevertAllDownToGroup(group); if (tree && !AssetDatabase.Contains(tree)) Object.DestroyImmediate(tree); throw; }
        }
        internal static BlendTree WrapContents(BlendTree parent)
        {
            if (!parent) return null;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            var child = CloneData(parent); child.name = "BlendTree";
            try
            {
                Attach(child, parent);
                Undo.RegisterCompleteObjectUndo(parent, "Add Root Tree");
                string directWeight = parent.blendType == BlendTreeType.Direct ? EnsureWeight(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(parent)) as AnimatorController) : ControllerEditorExtraSettings.Current.directWeightParameter;
                parent.children = new[] { new ChildMotion { motion = child, timeScale = 1, directBlendParameter = directWeight } };
                EditorUtility.SetDirty(parent); EditorUtility.SetDirty(child);
                Undo.RegisterCreatedObjectUndo(child, "Add Root Tree"); Undo.CollapseUndoOperations(group);
                return child;
            }
            catch { Undo.RevertAllDownToGroup(group); if (child && !AssetDatabase.Contains(child)) Object.DestroyImmediate(child); throw; }
        }
        internal static BlendTree CopyTree(BlendTree source, Object owner, bool external)
        {
            if (!source || !owner) throw new ArgumentException("Choose a saved BlendTree and its destination.");
            string sourcePath = AssetDatabase.GetAssetPath(source);
            var copies = new Dictionary<BlendTree, BlendTree>();
            Object destination = owner;
            BlendTree Clone(BlendTree input, bool root)
            {
                if (copies.TryGetValue(input, out var known)) return known;
                var copy = CloneData(input); copy.name = input.name; copy.hideFlags &= ~(HideFlags.DontSave | HideFlags.NotEditable); copies.Add(input, copy);
                if (root && external)
                {
                    string folder = EnsureFolder(ControllerEditorExtraSettings.Current.blendTreeFolder);
                    copy.hideFlags &= ~HideFlags.HideInHierarchy;
                    AssetDatabase.CreateAsset(copy, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + ControllerEditor.MotionAssetOperations.AwakeReg(copy.name) + ".asset"));
                    destination = copy;
                }
                else
                {
                    if (!AssetDatabase.Contains(destination) || !AssetDatabase.IsOpenForEdit(destination)) throw new InvalidOperationException("Choose a saved editable destination.");
                    copy.hideFlags |= HideFlags.HideInHierarchy; AssetDatabase.AddObjectToAsset(copy, destination);
                }
                var children = copy.children;
                for (int index = 0; index < children.Length; index++)
                    if (children[index].motion is BlendTree tree && (AssetDatabase.GetAssetPath(tree) == sourcePath && !AssetDatabase.IsMainAsset(tree)))
                        children[index].motion = Clone(tree, false);
                copy.children = children; EditorUtility.SetDirty(copy);
                return copy;
            }
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                var result = Clone(source, true);
                foreach (var copy in copies.Values) Undo.RegisterCreatedObjectUndo(copy, "Copy BlendTree");
                Undo.CollapseUndoOperations(group); return result;
            }
            catch
            {
                Undo.RevertAllDownToGroup(group);
                foreach (var copy in copies.Values) if (copy && !AssetDatabase.Contains(copy)) Object.DestroyImmediate(copy);
                throw;
            }
        }
        internal static int ReplaceReferences(AnimatorController controller, Motion before, Motion after)
        {
            if (!controller || !before || before == after) return 0;
            int changed = 0;
            var visited = new HashSet<Motion>(); var machines = new HashSet<AnimatorStateMachine>();
            Action<Motion> motion = null;
            motion = value =>
            {
                if (!value || !visited.Add(value) || !(value is BlendTree tree)) return;
                var children = tree.children; bool dirty = false;
                for (int index = 0; index < children.Length; index++)
                {
                    if (children[index].motion == before) { children[index].motion = after; dirty = true; changed++; }
                    else motion(children[index].motion);
                }
                if (dirty) { Undo.RecordObject(tree, "Replace BlendTree"); tree.children = children; EditorUtility.SetDirty(tree); }
            };
            Action<AnimatorStateMachine> visit = null;
            visit = machine =>
            {
                if (!machine || !machines.Add(machine)) return;
                foreach (var child in machine.states)
                {
                    if (child.state.motion == before) { Undo.RecordObject(child.state, "Replace BlendTree"); child.state.motion = after; EditorUtility.SetDirty(child.state); changed++; }
                    else motion(child.state.motion);
                }
                foreach (var child in machine.stateMachines) visit(child.stateMachine);
            };
            foreach (var layer in controller.layers) visit(layer.stateMachine);
            return changed;
        }
        internal static bool Contains(BlendTree root, Motion target)
        {
            var seen = new HashSet<BlendTree>();
            bool Visit(BlendTree tree)
            {
                if (!tree || !seen.Add(tree)) return false;
                return tree == target || tree.children.Any(child => child.motion == target || (child.motion is BlendTree nested && Visit(nested)));
            }
            return Visit(root);
        }
        internal static bool ReplaceParent(AnimatorController controller, BlendTree parent, BlendTree child)
        {
            if (!controller || !parent || !child || parent == child || !parent.children.Any(value => value.motion == child) || Contains(child, parent)) return false;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            int count = ReplaceReferences(controller, parent, child); Undo.CollapseUndoOperations(group);
            return count > 0;
        }
        internal static AnimationClip ConstantClip(string parameter, float value, string path)
        {
            if (string.IsNullOrWhiteSpace(parameter) || float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentException("Enter a parameter name and a finite value.");
            path = path.Replace('\\', '/');
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Save an animation inside Assets.");
            EnsureFolder(Path.GetDirectoryName(path));
            var clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path), frameRate = 60 };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), parameter), AnimationCurve.Constant(0, 1f / 60f, value));
            AssetDatabase.CreateAsset(clip, AssetDatabase.GenerateUniqueAssetPath(path));
            Undo.RegisterCreatedObjectUndo(clip, "Create Parameter Animation"); return clip;
        }
    }

    internal sealed class ConstantParameterPopup : PopupWindowContent
    {
        private string parameter; private float value;
        private readonly Action<AnimationClip> created;
        private readonly AnimatorController controller;
        internal ConstantParameterPopup(string parameter, float value, Action<AnimationClip> created = null, AnimatorController controller = null) { this.parameter = parameter; this.value = value; this.created = created; this.controller = controller; }
        public override Vector2 GetWindowSize() { return new Vector2(330, 135); }
        public override void OnGUI(Rect rect)
        {
            parameter = EditorGUILayout.TextField("Parameter", parameter); value = EditorGUILayout.FloatField("Value", value);
            var existing = controller ? controller.parameters.FirstOrDefault(item => item.name == parameter) : null;
            bool invalidType = existing != null && existing.type != AnimatorControllerParameterType.Float;
            if (invalidType) EditorGUILayout.LabelField("Choose a Float parameter.", EditorStyles.miniLabel);
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(parameter) || invalidType || float.IsNaN(value) || float.IsInfinity(value)))
                if (GUILayout.Button("Create Parameter Animation"))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Parameter Animation", ControllerEditor.MotionAssetOperations.AwakeReg(parameter), "anim", "Save the constant parameter animation");
                    if (string.IsNullOrEmpty(path)) return;
                    Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
                    var clip = ControllerBlendTreeTools.ConstantClip(parameter, value, path);
                    if (controller && existing == null)
                    {
                        Undo.RecordObject(controller, "Add Animated Parameter");
                        controller.AddParameter(new AnimatorControllerParameter { name = parameter, type = AnimatorControllerParameterType.Float, defaultFloat = value });
                        EditorUtility.SetDirty(controller);
                    }
                    created?.Invoke(clip); Undo.CollapseUndoOperations(group); EditorGUIUtility.PingObject(clip); editorWindow.Close();
                }
        }
    }
}
