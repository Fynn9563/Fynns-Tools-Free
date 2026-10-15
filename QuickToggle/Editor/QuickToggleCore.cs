#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsToolkit.QuickToggle
{
    internal enum SetupMode { ClipsOnly, VRChat, VRCFury, ModularAvatar }
    internal enum SplitMode { Together, PerObject, PerBlendshape }
    [Serializable] internal sealed class ToggleTarget
    {
        public Object target;
        public bool on = true;
    }
    [Serializable] internal sealed class ShapeTarget
    {
        public SkinnedMeshRenderer renderer;
        public string shape;
        public float on = 100;
        public bool fromTargets;
    }
    internal sealed class ToggleRequest
    {
        public GameObject root;
        public string name = "Toggle", folder = "Assets/Quick Toggles", menuPath = "", parameter = "Toggle";
        public SetupMode setup;
        public SplitMode split;
        public bool blendshapes, opposite = true, loop, defaultOn, saved = true, writeDefaults;
        public int frames = 1;
        public Object targetMenu;
        public bool fx = true, parameters = true, menu = true;
        public readonly List<ToggleTarget> targets = new List<ToggleTarget>();
        public readonly List<ShapeTarget> shapes = new List<ShapeTarget>();
    }
    internal sealed class TogglePlan
    {
        public string name, parameter;
        public AnimationClip on, off;
    }
    internal sealed class ToggleResult
    {
        public readonly List<string> assets = new List<string>();
        public readonly List<GameObject> objects = new List<GameObject>();
        public int toggles;
    }
    internal static class QuickToggleCore
    {
        internal static string Validate(ToggleRequest r)
        {
            try
            {
                var plans = Plan(r);
                try { QuickToggleIntegrations.Validate(r, plans); }
                finally { DestroyPlans(plans); }
                return null;
            }
            catch (Exception e) { return e.GetBaseException().Message; }
        }

        internal static List<TogglePlan> Plan(ToggleRequest r)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before creating toggles.");
            if (!Enum.IsDefined(typeof(SetupMode), r.setup) || !Enum.IsDefined(typeof(SplitMode), r.split)) throw new InvalidOperationException("Choose a supported setup and grouping option.");
            if (r.root == null || EditorUtility.IsPersistent(r.root) || !r.root.scene.IsValid())
                throw new InvalidOperationException("Choose a scene root or an avatar open in Prefab Mode.");
            if ((r.root.hideFlags & HideFlags.NotEditable) != 0) throw new InvalidOperationException("The selected root is not editable.");
            CheckName(r.name);
            if (r.frames < 1 || r.frames > 36000) throw new InvalidOperationException("Clip length must be between 1 and 36000 frames.");
            if (r.setup != SetupMode.VRCFury || (!r.blendshapes && r.targets.Any(t => t != null && t.target is Component))) CheckFolder(r.folder);
            var groups = new Dictionary<string, List<Tuple<EditorCurveBinding, float, float>>>();
            var labels = new Dictionary<string, string>();
            var seen = new HashSet<string>();
            Action<Object, Type, string, float, float, string> add = (obj, type, property, on, off, label) =>
            {
                var transform = obj is GameObject ? ((GameObject)obj).transform : ((Component)obj).transform;
                if (!(obj is GameObject) && transform.GetComponents(type).Length > 1)
                    throw new InvalidOperationException("Multiple " + type.Name + " components on " + transform.name + " cannot be addressed separately by an animation.");
                if (transform != r.root.transform && !transform.IsChildOf(r.root.transform))
                    throw new InvalidOperationException(obj.name + " is outside the selected root.");
                var path = BindingPath(transform, r.root.transform);
                var identity = path + "|" + type.FullName + "|" + property;
                if (!seen.Add(identity)) throw new InvalidOperationException("The same animation property is selected more than once: " + label);
                var key = r.split == SplitMode.Together ? "all" : r.split == SplitMode.PerObject ? transform.GetInstanceID().ToString() : identity;
                if (!groups.ContainsKey(key)) { groups[key] = new List<Tuple<EditorCurveBinding, float, float>>(); labels[key] = label; }
                groups[key].Add(Tuple.Create(EditorCurveBinding.FloatCurve(path, type, property), on, off));
            };
            if (r.blendshapes)
            {
                foreach (var row in r.shapes)
                {
                    if (row == null || row.renderer == null || row.renderer.sharedMesh == null)
                        throw new InvalidOperationException("Each blendshape row needs a renderer with a mesh.");
                    var index = row.renderer.sharedMesh.GetBlendShapeIndex(row.shape ?? "");
                    if (index < 0) throw new InvalidOperationException("Choose an existing blendshape on " + row.renderer.name + ".");
                    if (float.IsNaN(row.on) || float.IsInfinity(row.on)) throw new InvalidOperationException("Blendshape values must be finite.");
                    add(row.renderer, typeof(SkinnedMeshRenderer), "blendShape." + row.shape, row.on,
                        row.renderer.GetBlendShapeWeight(index), r.split == SplitMode.PerObject ? row.renderer.name : row.shape);
                }
            }
            else
            {
                if (r.split == SplitMode.PerBlendshape) throw new InvalidOperationException("Choose Together or Per Object for object toggles.");
                foreach (var row in r.targets)
                {
                    if (row == null || row.target == null) throw new InvalidOperationException("Each target row needs an object or an enabled component.");
                    var go = row.target as GameObject;
                    var comp = row.target as Component;
                    if (go != null)
                    {
                        if (go == r.root) throw new InvalidOperationException("Toggle a child object rather than disabling the root.");
                        add(go, typeof(GameObject), "m_IsActive", row.on ? 1 : 0, row.on ? 0 : 1, go.name);
                    }
                    else if (comp != null && !(comp is Transform))
                    {
                        var enabled = new SerializedObject(comp).FindProperty("m_Enabled");
                        if (enabled == null || enabled.propertyType != SerializedPropertyType.Boolean)
                            throw new InvalidOperationException(comp.GetType().Name + " has no enabled property.");
                        if (comp.gameObject.GetComponents(comp.GetType()).Length > 1)
                            throw new InvalidOperationException("Multiple " + comp.GetType().Name + " components on " + comp.name + " cannot be addressed separately by an animation.");
                        add(comp, comp.GetType(), "m_Enabled", row.on ? 1 : 0, row.on ? 0 : 1, comp.name);
                    }
                    else throw new InvalidOperationException("Choose a GameObject or a component with an enabled checkbox.");
                }
            }
            if (groups.Count == 0) throw new InvalidOperationException(r.blendshapes ? "Add at least one blendshape." : "Add at least one target.");
            var plans = new List<TogglePlan>();
            try
            {
                foreach (var group in groups)
                {
                    var name = r.split == SplitMode.Together ? r.name.Trim() : r.name.Trim() + " " + labels[group.Key];
                    var plan = new TogglePlan { name = name, on = new AnimationClip { name = name + " On", frameRate = 60 }, off = new AnimationClip { name = name + " Off", frameRate = 60 } };
                    plans.Add(plan);
                    foreach (var value in group.Value)
                    {
                        AnimationUtility.SetEditorCurve(plan.on, value.Item1, AnimationCurve.Constant(0, r.frames / 60f, value.Item2));
                        AnimationUtility.SetEditorCurve(plan.off, value.Item1, AnimationCurve.Constant(0, r.frames / 60f, value.Item3));
                    }
                    foreach (var clip in new[] { plan.on, plan.off })
                    {
                        var settings = AnimationUtility.GetAnimationClipSettings(clip);
                        settings.loopTime = r.loop;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                    }
                }
                return plans;
            }
            catch { DestroyPlans(plans); throw; }
        }

        internal static string BindingPath(Transform target, Transform root)
        {
            for (var t = target; t != root; t = t.parent)
            {
                if (t == null) throw new InvalidOperationException("Target is outside the selected root.");
                if (t.name.Contains("/")) throw new InvalidOperationException("Rename " + t.name + ": slashes make animation paths ambiguous.");
                if (t.parent.Cast<Transform>().Count(s => s.name == t.name) > 1)
                    throw new InvalidOperationException("Rename duplicate siblings named " + t.name + " before creating animations.");
            }
            return AnimationUtility.CalculateTransformPath(target, root);
        }

        internal static ToggleResult Create(ToggleRequest r)
        {
            var plans = Plan(r);
            var result = new ToggleResult();
            var folders = new List<string>();
            int group = -1;
            try
            {
                QuickToggleIntegrations.Validate(r, plans);
                Undo.IncrementCurrentGroup(); group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Create Quick Toggles");
                if (r.setup != SetupMode.VRCFury || plans.Any(p => AnimationUtility.GetCurveBindings(p.on).Any(b => b.propertyName == "m_Enabled"))) EnsureFolder(r.folder, folders);
                foreach (var p in plans.Where(p => r.setup != SetupMode.VRCFury))
                {
                    SaveAsset(p.on, r.folder, result);
                    if (r.opposite || (r.setup != SetupMode.ClipsOnly && r.setup != SetupMode.VRCFury)) SaveAsset(p.off, r.folder, result);
                }
                QuickToggleIntegrations.Create(r, plans, result);
                foreach (var path in result.assets) AssetDatabase.SaveAssetIfDirty(AssetDatabase.LoadMainAssetAtPath(path));
                result.toggles = plans.Count;
                Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group);
                return result;
            }
            catch
            {
                if (group >= 0) Undo.RevertAllDownToGroup(group);
                foreach (var path in result.assets.AsEnumerable().Reverse()) AssetDatabase.DeleteAsset(path);
                foreach (var path in folders.AsEnumerable().Reverse())
                    if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any()) AssetDatabase.DeleteAsset(path);
                throw;
            }
            finally { DestroyPlans(plans); }
        }

        internal static void SaveAsset(Object asset, string folder, ToggleResult result)
        {
            var extension = asset is AnimationClip ? ".anim" : asset is AnimatorController ? ".controller" : ".asset";
            var safe = new string(asset.name.Select(c => Path.GetInvalidFileNameChars().Contains(c) || c == '/' || c == '\\' ? '_' : c).ToArray()).Trim();
            if (string.IsNullOrWhiteSpace(safe)) safe = "Toggle";
            var path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + safe + extension);
            AssetDatabase.CreateAsset(asset, path);
            result.assets.Add(path);
        }
        private static void CheckName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Any(char.IsControl)) throw new InvalidOperationException("Enter a toggle name without control characters.");
        }
        private static void CheckFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !(folder == "Assets" || folder.StartsWith("Assets/", StringComparison.Ordinal)) ||
                folder.Split('/').Any(p => p == "." || p == ".." || string.IsNullOrWhiteSpace(p) || p.Any(c => Path.GetInvalidFileNameChars().Contains(c))) ||
                folder.Split('/').Any(p => p.EndsWith(".") || p.EndsWith(" ")))
                throw new InvalidOperationException("Choose an output folder inside Assets.");
            var current = "Assets";
            foreach (var part in folder.Split('/').Skip(1))
            { current += "/" + part; if (File.Exists(current)) throw new InvalidOperationException("The output path contains a file: " + current); }
        }
        private static void EnsureFolder(string folder, List<string> made)
        {
            var current = "Assets";
            foreach (var part in folder.Split('/').Skip(1))
            {
                var next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next))
                {
                    if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(current, part))) throw new IOException("Could not create " + next);
                    made.Add(next);
                }
                current = next;
            }
        }
        private static void DestroyPlans(IEnumerable<TogglePlan> plans)
        {
            foreach (var p in plans)
                foreach (var clip in new[] { p.on, p.off })
                    if (clip != null && !EditorUtility.IsPersistent(clip)) Object.DestroyImmediate(clip);
        }
    }
}
#endif
