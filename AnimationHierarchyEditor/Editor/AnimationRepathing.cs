using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.AnimationHierarchy
{
    internal sealed class BrokenBinding
    {
        internal AnimationClip clip;
        internal EditorCurveBinding binding;
        internal string reason;
    }

    internal static class AnimationRepathing
    {
        internal static AnimationClip[] Clips(GameObject root, Object controller)
        {
            if (!root) throw new InvalidOperationException("Choose the animation root object.");
            return MotionReplacement.Controllers(controller ? controller : root)
                .SelectMany(c => c.animationClips).Where(c => c).Distinct().ToArray();
        }

        internal static string Problem(GameObject root, EditorCurveBinding binding)
        {
            if (!root) return "Missing animation root";
            var found = binding.path.Length == 0 ? root.transform : root.transform.Find(binding.path);
            if (!found) return "Missing object";
            if (!AnimationUtility.GetAnimatedObject(root, binding)) return "Missing component";
            float number; Object reference;
            bool valid = binding.isPPtrCurve ? AnimationUtility.GetObjectReferenceValue(root, binding, out reference)
                : AnimationUtility.GetFloatValue(root, binding, out number);
            if (!valid) valid = AnimationUtility.GetAnimatableBindings(found.gameObject, root)
                .Any(b => b.path == binding.path && b.type == binding.type && b.propertyName == binding.propertyName && b.isPPtrCurve == binding.isPPtrCurve);
            return valid ? null : "Missing or unsupported property";
        }

        internal static List<BrokenBinding> Scan(GameObject root, IEnumerable<AnimationClip> clips)
        {
            if (!root) throw new InvalidOperationException("Choose the animation root object.");
            return clips.Where(c => c).Distinct().SelectMany(c => Bindings(c).Select(b => new BrokenBinding
                { clip = c, binding = b, reason = Problem(root, b) })).Where(b => b.reason != null).ToList();
        }

        internal static IEnumerable<EditorCurveBinding> Bindings(AnimationClip clip) =>
            AnimationUtility.GetCurveBindings(clip).Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip));

        internal static string Fingerprint(IEnumerable<AnimationClip> clips) => string.Join("\n", clips.OrderBy(c => c.GetInstanceID())
            .SelectMany(c => Bindings(c).Select(b => c.GetInstanceID() + "|" + b.path.Length + ":" + b.path
                + "|" + b.type.AssemblyQualifiedName + "|" + b.propertyName + "|" + b.isPPtrCurve)).OrderBy(s => s, StringComparer.Ordinal));

        internal static void ValidateRepair(GameObject root, IEnumerable<AnimationClip> clips, IReadOnlyDictionary<string, string> map)
        {
            foreach (var binding in clips.SelectMany(Bindings))
            {
                if (!map.TryGetValue(binding.path, out var next) || next == binding.path) continue;
                var edited = binding; edited.path = next;
                var problem = Problem(root, edited);
                if (problem != null) throw new InvalidOperationException(AnimationPathEditor.DisplayPath(next) + ": " + problem
                    + " for " + binding.type.Name + "." + binding.propertyName + ".");
            }
        }
    }

    internal sealed class HierarchyPathSnapshot
    {
        private readonly GameObject root;
        private readonly Dictionary<Transform, string> paths;
        internal HierarchyPathSnapshot(GameObject root)
        {
            this.root = root;
            paths = Capture(root);
        }
        private static Dictionary<Transform, string> Capture(GameObject root)
        {
            if (!root) throw new InvalidOperationException("The animation root was removed.");
            var result = new Dictionary<Transform, string>();
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != root.transform && (t.name.Length == 0 || t.name.Contains("/")))
                    throw new InvalidOperationException("Use non-empty object names without '/' before tracking paths.");
                var path = AnimationUtility.CalculateTransformPath(t, root.transform);
                if (!unique.Add(path)) throw new InvalidOperationException("Ambiguous hierarchy path: " + path + ". Give sibling objects unique names.");
                result.Add(t, path);
            }
            return result;
        }
        internal Dictionary<string, string> Changes()
        {
            var current = Capture(root);
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in paths)
                if (pair.Key && current.TryGetValue(pair.Key, out var next) && next != pair.Value)
                    result.Add(pair.Value, next);
            foreach (var pair in current)
                if (!paths.ContainsKey(pair.Key)) paths.Add(pair.Key, pair.Value);
            return result;
        }
    }
}
