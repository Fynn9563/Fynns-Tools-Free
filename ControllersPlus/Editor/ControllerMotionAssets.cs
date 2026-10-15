using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerMotionAssets
    {
        internal static Motion Embed(AnimatorState state)
        {
            string controllerPath = RequireEditableState(state);
            return Copy(state, controllerPath, true);
        }

        internal static Motion Extract(AnimatorState state)
        {
            string controllerPath = RequireEditableState(state);
            string extension = state.motion is AnimationClip ? ".anim" : ".asset";
            string name = ControllerEditor.MotionAssetOperations.AwakeReg(state.motion.name);
            string path = AssetDatabase.GenerateUniqueAssetPath(Path.GetDirectoryName(controllerPath).Replace('\\', '/') + "/" + name + extension);
            return Copy(state, path, false);
        }

        private static string RequireEditableState(AnimatorState state)
        {
            if (!state || !state.motion)
                throw new ArgumentException("Choose a state with a motion.");
            string path = AssetDatabase.GetAssetPath(state);
            if (string.IsNullOrEmpty(path) || !(AssetDatabase.LoadMainAssetAtPath(path) is AnimatorController))
                throw new InvalidOperationException("Save the state's controller before changing motion assets.");
            if (!AssetDatabase.IsOpenForEdit(state))
                throw new InvalidOperationException("The state's controller is read-only.");
            return path;
        }

        private static Motion Copy(AnimatorState state, string path, bool embedded)
        {
            string sourcePath = AssetDatabase.GetAssetPath(state.motion);
            var copies = new Dictionary<Motion, Motion>();
            UnityEngine.Object owner = embedded ? AssetDatabase.LoadMainAssetAtPath(path) : null;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            string action = embedded ? "Embed Motion Copy" : "Extract Motion Copy";
            Undo.SetCurrentGroupName(action);
            Undo.RegisterCompleteObjectUndo(state, action);
            Motion Clone(Motion source, bool root)
            {
                if (!source)
                    return null;
                if (copies.TryGetValue(source, out var existing))
                    return existing;
                if (!root && (AssetDatabase.GetAssetPath(source) != sourcePath || AssetDatabase.IsMainAsset(source)))
                    return source;
                var copy = source is BlendTree sourceTree ? (Motion)ControllerBlendTreeTools.CloneData(sourceTree) : UnityEngine.Object.Instantiate(source);
                copy.name = source.name;
                copies.Add(source, copy);
                if (root && !embedded)
                {
                    copy.hideFlags &= ~HideFlags.HideInHierarchy;
                    AssetDatabase.CreateAsset(copy, path);
                    owner = copy;
                }
                else
                {
                    copy.hideFlags |= HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(copy, owner);
                }

                if (copy is BlendTree tree)
                {
                    var children = tree.children;
                    for (int index = 0; index < children.Length; index++)
                        children[index].motion = Clone(children[index].motion, false);
                    tree.children = children;
                }

                EditorUtility.SetDirty(copy);
                return copy;
            }

            try
            {
                var motion = Clone(state.motion, true);
                state.motion = motion;
                EditorUtility.SetDirty(state);
                foreach (var copy in copies.Values)
                    Undo.RegisterCreatedObjectUndo(copy, action);
                Undo.CollapseUndoOperations(group);
                EditorUtility.SetDirty(owner);
                AssetDatabase.SaveAssetIfDirty(owner);
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(state)));
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                return motion;
            }
            catch
            {
                Undo.RevertAllDownToGroup(group);
                foreach (var copy in copies.Values)
                    if (copy && !AssetDatabase.Contains(copy))
                        UnityEngine.Object.DestroyImmediate(copy);
                throw;
            }
        }
    }
}
