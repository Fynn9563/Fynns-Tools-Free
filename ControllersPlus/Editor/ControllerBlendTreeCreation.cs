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
    internal static class ControllerBlendTreeCreation
    {
        internal static void AddMotion(BlendTree parent, Motion motion, AnimatorController controller)
        {
            if (!parent || !motion || !AssetDatabase.IsOpenForEdit(parent)) throw new ArgumentException("Choose an editable BlendTree and a motion.");
            if (motion is BlendTree nested && ControllerBlendTreeTools.Contains(nested, parent)) throw new ArgumentException("A BlendTree cannot contain itself.");
            Undo.RecordObject(parent, "Add Child Motion");
            var children = parent.children;
            parent.children = children.Concat(new[] { new ChildMotion { motion = motion, timeScale = 1,
                threshold = children.Length == 0 ? 0 : children.Max(child => child.threshold) + 1,
                directBlendParameter = parent.blendType == BlendTreeType.Direct ? ControllerBlendTreeTools.EnsureWeight(controller) : "" } }).ToArray();
            EditorUtility.SetDirty(parent);
        }
        internal static BlendTree Smooth(AnimatorController controller, BlendTree parent, string input, string output, string smoothing, float minimum, float maximum)
        {
            if (!controller || !parent || !AssetDatabase.IsOpenForEdit(controller) || !AssetDatabase.IsOpenForEdit(parent)) throw new ArgumentException("Choose an editable controller and BlendTree.");
            var names = new[] { input, output, smoothing };
            if (names.Any(string.IsNullOrWhiteSpace) || names.Distinct(StringComparer.Ordinal).Count() != 3) throw new ArgumentException("Use three different Float parameter names.");
            if (float.IsNaN(minimum) || float.IsInfinity(minimum) || float.IsNaN(maximum) || float.IsInfinity(maximum) || minimum >= maximum) throw new ArgumentException("Minimum must be below maximum.");
            foreach (string name in names)
                if (controller.parameters.Any(parameter => parameter.name == name && parameter.type != AnimatorControllerParameterType.Float)) throw new ArgumentException(name + " must be a Float parameter.");
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                Undo.RecordObject(controller, "Create Parameter Smoothing");
                foreach (string name in names)
                    if (!controller.parameters.Any(parameter => parameter.name == name)) controller.AddParameter(new AnimatorControllerParameter { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = name == smoothing ? 0.9f : Mathf.Clamp(0, minimum, maximum) });
                var tree = ControllerBlendTreeTools.AddChild(parent, BlendTreeType.Simple1D, controller);
                tree.name = "Smooth " + input + " to " + output; tree.blendParameter = smoothing; tree.useAutomaticThresholds = false;
                AnimationClip Clip(float value, string suffix)
                {
                    var clip = new AnimationClip { name = output + " " + suffix, frameRate = 60, hideFlags = HideFlags.HideInHierarchy };
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), output), AnimationCurve.Constant(0, 1f / 60f, value));
                    AssetDatabase.AddObjectToAsset(clip, AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(tree)));
                    Undo.RegisterCreatedObjectUndo(clip, "Create Parameter Smoothing"); return clip;
                }
                var low = Clip(minimum, "Minimum"); var high = Clip(maximum, "Maximum");
                var branches = new List<BlendTree>();
                foreach (string parameter in new[] { input, output })
                {
                    var branch = ControllerBlendTreeTools.AddChild(tree, BlendTreeType.Simple1D, controller);
                    Undo.RegisterCompleteObjectUndo(branch, "Create Parameter Smoothing");
                    branch.name = parameter == input ? "Input " + input : "Feedback " + output;
                    branch.blendParameter = parameter; branch.useAutomaticThresholds = false;
                    branch.children = new[] { new ChildMotion { motion = low, threshold = minimum, timeScale = 1 }, new ChildMotion { motion = high, threshold = maximum, timeScale = 1 } };
                    EditorUtility.SetDirty(branch); branches.Add(branch);
                }
                Undo.RegisterCompleteObjectUndo(tree, "Create Parameter Smoothing");
                tree.children = new[] { new ChildMotion { motion = branches[0], threshold = 0, timeScale = 1 }, new ChildMotion { motion = branches[1], threshold = 1, timeScale = 1 } };
                EditorUtility.SetDirty(controller); EditorUtility.SetDirty(tree); ControllerParameterTools.ClearCache();
                Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group); return tree;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        internal static AnimationClip NewClip(BlendTree parent, AnimatorController controller, string path)
        {
            if (!parent || !AssetDatabase.IsOpenForEdit(parent)) throw new ArgumentException("Choose an editable BlendTree.");
            string folder = ControllerBlendTreeTools.EnsureFolder(Path.GetDirectoryName(path));
            if (!path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Choose an animation path.");
            var clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path), frameRate = 60 };
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                AssetDatabase.CreateAsset(clip, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + Path.GetFileName(path)));
                Undo.RegisterCreatedObjectUndo(clip, "Create Child Animation"); AddMotion(parent, clip, controller); Undo.CollapseUndoOperations(group); return clip;
            }
            catch { Undo.RevertAllDownToGroup(group); if (clip && !AssetDatabase.Contains(clip)) Object.DestroyImmediate(clip); throw; }
        }
    }
    [InitializeOnLoad]
    internal static class ControllerBlendTreeClipboard
    {
        private static BlendTree snapshot;
        private static readonly List<BlendTree> owned = new List<BlendTree>();
        static ControllerBlendTreeClipboard() { AssemblyReloadEvents.beforeAssemblyReload += Clear; EditorApplication.quitting += Clear; }
        internal static bool HasTree => snapshot;
        internal static void Clear() { snapshot = null; foreach (var tree in owned) if (tree) Object.DestroyImmediate(tree); owned.Clear(); }
        internal static void Copy(BlendTree source)
        {
            Clear(); if (!source) return;
            var copies = new Dictionary<BlendTree, BlendTree>();
            BlendTree Clone(BlendTree tree)
            {
                if (copies.TryGetValue(tree, out var existing)) return existing;
                var copy = ControllerBlendTreeTools.CloneData(tree); copy.name = tree.name; copy.hideFlags = HideFlags.HideAndDontSave; copies.Add(tree, copy); owned.Add(copy);
                var children = copy.children;
                for (int i = 0; i < children.Length; i++) if (children[i].motion is BlendTree nested) children[i].motion = Clone(nested);
                copy.children = children; return copy;
            }
            snapshot = Clone(source);
        }
        internal static BlendTree Paste(BlendTree parent, AnimatorController controller)
        {
            if (!snapshot || !parent || !AssetDatabase.IsOpenForEdit(parent)) return null;
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                var copy = ControllerBlendTreeTools.CopyTree(snapshot, parent, ControllerEditorExtraSettings.Current.externalBlendTrees);
                ControllerBlendTreeCreation.AddMotion(parent, copy, controller); Undo.CollapseUndoOperations(group); return copy;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
    }
    internal sealed class ParameterSmoothingPopup : PopupWindowContent
    {
        private readonly AnimatorController controller; private readonly BlendTree parent;
        private string input = "Input", output = "Smoothed", smoothing = "Smoothing";
        private float minimum, maximum = 1; private string error;
        internal ParameterSmoothingPopup(AnimatorController controller, BlendTree parent) { this.controller = controller; this.parent = parent; }
        public override Vector2 GetWindowSize() { return new Vector2(340, 220); }
        public override void OnGUI(Rect rect)
        {
            input = EditorGUILayout.TextField("Input parameter", input);
            output = EditorGUILayout.TextField("Output parameter", output);
            smoothing = EditorGUILayout.TextField("Smoothing parameter", smoothing);
            minimum = EditorGUILayout.FloatField("Minimum", minimum); maximum = EditorGUILayout.FloatField("Maximum", maximum);
            EditorGUILayout.LabelField("Smoothing: 0 = immediate, 1 = hold previous value", EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Warning);
            if (GUILayout.Button("Create Smoothing Tree"))
                try { ControllerBlendTreeCreation.Smooth(controller, parent, input, output, smoothing, minimum, maximum); editorWindow.Close(); }
                catch (ArgumentException exception) { error = exception.Message; }
        }
    }
}
