using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.AnimationHierarchy
{
    public enum PathEditMode { Exact, Text, Regex }

    public static class AnimationPathEditor
    {
        private sealed class CurveMove
        {
            internal EditorCurveBinding source;
            internal EditorCurveBinding destination;
            internal AnimationCurve curve;
            internal ObjectReferenceKeyframe[] references;
        }

        private sealed class ClipEdit
        {
            internal AnimationClip clip;
            internal readonly List<CurveMove> moves = new List<CurveMove>();
        }

        public static bool TryReplacePaths(IEnumerable<AnimationClip> clips, string oldPath, string newPath,
            PathEditMode mode, out int changedBindings, out string error)
        {
            changedBindings = 0;
            error = null;
            if (clips == null || oldPath == null || newPath == null)
            {
                error = "Choose a clip and enter both paths.";
                return false;
            }
            if (mode != PathEditMode.Exact && string.IsNullOrEmpty(oldPath))
            {
                error = "Enter text or a regex pattern to replace.";
                return false;
            }

            try
            {
                if (!Enum.IsDefined(typeof(PathEditMode), mode)) throw new ArgumentException("Choose a valid replacement mode.");
                var regex = mode == PathEditMode.Regex ? new Regex(oldPath, RegexOptions.None, TimeSpan.FromMilliseconds(250)) : null;
                return TryTransform(clips, path => mode == PathEditMode.Exact ? (path == oldPath ? newPath : path)
                    : regex != null ? regex.Replace(path, newPath) : path.Replace(oldPath, newPath), true, out changedBindings, out error);
            }
            catch (Exception e) { error = e.Message; return false; }
        }

        internal static bool TryMapPaths(IEnumerable<AnimationClip> clips, IReadOnlyDictionary<string, string> map,
            bool apply, out int changedBindings, out string error)
        {
            return TryTransform(clips, path => map.TryGetValue(path, out var next) ? next : path, apply, out changedBindings, out error);
        }

        private static bool TryTransform(IEnumerable<AnimationClip> clips, Func<string, string> transform,
            bool apply, out int changedBindings, out string error)
        {
            changedBindings = 0; error = null;
            var edits = new List<ClipEdit>();
            try
            {
                var selectedClips = clips.Where(c => c).Distinct().ToArray();
                if (selectedClips.Length == 0)
                    throw new InvalidOperationException("Choose at least one Animation Clip.");
                foreach (var clip in selectedClips)
                {
                    var edit = new ClipEdit { clip = clip };
                    var bindings = AnimationUtility.GetCurveBindings(clip)
                        .Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip)).ToArray();
                    var destinations = new HashSet<(string, Type, string, bool)>();
                    foreach (var binding in bindings)
                    {
                        string path = transform(binding.path);
                        if (path == null) throw new InvalidOperationException("A replacement path cannot be null.");
                        var destination = binding;
                        destination.path = path;
                        if (!destinations.Add((path, binding.type, binding.propertyName, binding.isPPtrCurve)))
                            throw new InvalidOperationException($"'{clip.name}' would have two curves at '{DisplayPath(path)}' for {binding.type.Name}.{binding.propertyName}. No clips were changed.");
                        if (path == binding.path) continue;
                        edit.moves.Add(new CurveMove
                        {
                            source = binding,
                            destination = destination,
                            curve = binding.isPPtrCurve ? null : AnimationUtility.GetEditorCurve(clip, binding),
                            references = binding.isPPtrCurve ? AnimationUtility.GetObjectReferenceCurve(clip, binding) : null
                        });
                    }
                    if (edit.moves.Count == 0) continue;
                    var assetPath = AssetDatabase.GetAssetPath(clip);
                    if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                        !assetPath.EndsWith(".anim", StringComparison.OrdinalIgnoreCase) ||
                        AssetDatabase.LoadMainAssetAtPath(assetPath) != clip || !AssetDatabase.IsOpenForEdit(clip) ||
                        (System.IO.File.GetAttributes(assetPath) & System.IO.FileAttributes.ReadOnly) != 0)
                        throw new InvalidOperationException($"'{clip.name}' must be an editable .anim asset under Assets. Duplicate imported or package clips into Assets first. No clips were changed.");
                    edits.Add(edit);
                }
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }

            if (!apply) { changedBindings = edits.Sum(e => e.moves.Count); return true; }
            if (edits.Count == 0) return true;
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Edit Animation Paths");
            Undo.RegisterCompleteObjectUndo(edits.Select(e => (UnityEngine.Object)e.clip).ToArray(), "Edit Animation Paths");
            try
            {
                foreach (var edit in edits)
                {
                    // Clear sources first so bindings can reuse vacated paths.
                    foreach (var move in edit.moves)
                        SetCurve(edit.clip, move.source, move, true);
                    foreach (var move in edit.moves)
                        SetCurve(edit.clip, move.destination, move, false);
                    EditorUtility.SetDirty(edit.clip);
                    changedBindings += edit.moves.Count;
                }
                Undo.CollapseUndoOperations(undoGroup);
                return true;
            }
            catch (Exception exception)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                changedBindings = 0;
                error = "The edit was reverted: " + exception.Message;
                return false;
            }
        }

        private static void SetCurve(AnimationClip clip, EditorCurveBinding binding, CurveMove move, bool clear)
        {
            if (binding.isPPtrCurve)
                AnimationUtility.SetObjectReferenceCurve(clip, binding, clear ? null : move.references);
            else
                AnimationUtility.SetEditorCurve(clip, binding, clear ? null : move.curve);
        }

        internal static string DisplayPath(string path) => string.IsNullOrEmpty(path) ? "(Animator root)" : path;
    }
}
