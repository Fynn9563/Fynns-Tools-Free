#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    internal static class DynamicsManagement
    {
        internal sealed class Move
        {
            internal Component source;
            internal string folder = "";
            internal string name;
        }

        internal static Component[] Scan(GameObject root)
        {
            if (root == null) return Array.Empty<Component>();
            return root.GetComponentsInChildren<Component>(true)
                .Where(c => c is VRCPhysBone || c is VRCPhysBoneCollider)
                .OrderBy(c => AnimationUtility.CalculateTransformPath(c.transform, root.transform), StringComparer.Ordinal)
                .ThenBy(c => c.GetType().Name).ToArray();
        }

        internal static Component[] SceneComponents(GameObject root)
        {
            return Resources.FindObjectsOfTypeAll<Component>().Where(c => c != null &&
                !EditorUtility.IsPersistent(c) && c.gameObject.scene.IsValid() && c.gameObject.scene.isLoaded &&
                (!EditorSceneManager.IsPreviewScene(c.gameObject.scene) || c.gameObject.scene == root.scene)).ToArray();
        }

        internal static void RequireRoot(GameObject root)
        {
            if (root == null || EditorUtility.IsPersistent(root) || !root.scene.IsValid())
                throw new InvalidOperationException("Choose a scene root or open the prefab in Prefab Mode.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before organizing or removing components.");
            if (DynamicsPreview.Active != null)
                throw new InvalidOperationException("Stop the PhysBone test before organizing or removing components.");
        }

        internal static void RequireTargets(GameObject root, Component[] targets)
        {
            RequireRoot(root);
            if (targets.Length == 0) throw new InvalidOperationException("Select at least one component.");
            foreach (var c in targets)
            {
                if (c == null || !(c is VRCPhysBone || c is VRCPhysBoneCollider) ||
                    !(c.transform == root.transform || c.transform.IsChildOf(root.transform)))
                    throw new InvalidOperationException("The selection changed. Refresh and select components again.");
                DynamicsOperations.RequireEditable(c);
                if (PrefabUtility.IsPartOfPrefabInstance(c) && !PrefabUtility.IsAddedComponentOverride(c))
                    throw new InvalidOperationException(c.name + ": open its source prefab in Prefab Mode, or unpack it first.");
            }
        }

        internal static bool ValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name == name.Trim() && name != "." && name != ".." &&
                name.IndexOfAny(new[] { '/', '\\', '\n', '\r', '\t' }) < 0;
        }

        internal static bool Organized(GameObject root, Component c)
        {
            var container = root.transform.Find("PhysBones");
            if (container == null || c.transform.parent == null) return false;
            var branch = container.Find(c is VRCPhysBone ? "Bones" : "Colliders");
            return branch != null && c.transform.IsChildOf(branch);
        }

        internal sealed class AnimationSource
        {
            internal Component owner;
            internal AnimationClip[] clips;
        }

        internal static AnimationSource[] AnimationSources(Component[] scene)
        {
            var sources = new List<AnimationSource>();
            foreach (var owner in scene)
            {
                var clips = new HashSet<AnimationClip>();
                if (owner is Animator animator && animator.runtimeAnimatorController != null)
                    clips.UnionWith(animator.runtimeAnimatorController.animationClips);
                if (owner is Animation animation)
                    clips.UnionWith(AnimationUtility.GetAnimationClips(animation.gameObject));
                var it = new SerializedObject(owner).GetIterator();
                while (it.Next(true))
                {
                    if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                    if (it.objectReferenceValue is RuntimeAnimatorController controller) clips.UnionWith(controller.animationClips);
                    if (it.objectReferenceValue is AnimationClip clip) clips.Add(clip);
                }
                if (clips.Count > 0) sources.Add(new AnimationSource { owner = owner, clips = clips.Where(c => c != null).ToArray() });
            }
            return sources.ToArray();
        }

        internal static string AnimationBlock(Component target, AnimationSource[] sources)
        {
            foreach (var source in sources)
            {
                var owner = source.owner;
                foreach (var clip in source.clips)
                {
                    foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                        if (AnimationUtility.GetObjectReferenceCurve(clip, binding).Any(k => k.value == target))
                            return "Referenced by animation " + clip.name + ". Keep this component in place.";
                    if (!(target.transform == owner.transform || target.transform.IsChildOf(owner.transform))) continue;
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip).Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip)))
                    {
                        var bound = string.IsNullOrEmpty(binding.path) ? owner.transform : owner.transform.Find(binding.path);
                        if (bound == null) continue;
                        if (bound == target.transform && binding.type != null && binding.type.IsAssignableFrom(target.GetType()))
                            return "Animated by " + clip.name + ". Keep this component in place.";
                        if (binding.type == typeof(GameObject) && binding.propertyName == "m_IsActive" &&
                            (target.transform == bound || target.transform.IsChildOf(bound)))
                            return "Activation animated by " + clip.name + ". Keep this component in place.";
                    }
                }
            }
            return null;
        }

        internal static Dictionary<Component, Component> Organize(GameObject root, Move[] moves, bool sort)
        {
            RequireTargets(root, moves.Select(m => m.source).ToArray());
            if (moves.Select(m => m.source).Distinct().Count() != moves.Length)
                throw new InvalidOperationException("A component cannot have two destinations.");
            var scene = SceneComponents(root);
            var animations = AnimationSources(scene);
            foreach (var move in moves)
            {
                if (!ValidName(move.name) || move.folder != "" && !ValidName(move.folder))
                    throw new InvalidOperationException("Use a non-empty object name and a single folder name without slashes.");
                var block = AnimationBlock(move.source, animations);
                if (block != null) throw new InvalidOperationException(move.source.name + ": " + block);
            }
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            const string label = "Organize PhysBones and colliders";
            Undo.SetCurrentGroupName(label);
            var map = new Dictionary<Component, Component>();
            var originalTransforms = new HashSet<Transform>(root.GetComponentsInChildren<Transform>(true));
            try
            {
                var container = Folder(root.transform, "PhysBones", label);
                var groups = moves.GroupBy(m => new { host = m.source.gameObject, bone = m.source is VRCPhysBone, m.folder, m.name });
                if (sort) groups = groups.OrderBy(g => g.Key.name, StringComparer.OrdinalIgnoreCase);
                foreach (var batch in groups)
                {
                    var branch = Folder(container, batch.Key.bone ? "Bones" : "Colliders", label);
                    if (batch.Key.folder != "") branch = Folder(branch, batch.Key.folder, label);
                    var host = new GameObject(GameObjectUtility.GetUniqueNameForSibling(branch, batch.Key.name));
                    Undo.RegisterCreatedObjectUndo(host, label);
                    host.transform.SetParent(branch, false);
                    host.layer = batch.Key.host.layer;
                    bool active = true;
                    for (var t = batch.Key.host.transform; t != null && t != root.transform; t = t.parent) active &= t.gameObject.activeSelf;
                    host.SetActive(active);
                    foreach (var move in batch)
                    {
                        var copy = Undo.AddComponent(host, move.source.GetType());
                        Undo.RegisterCompleteObjectUndo(copy, label);
                        EditorUtility.CopySerialized(move.source, copy);
                        if (copy is VRCPhysBone bone) bone.rootTransform = ((VRCPhysBone)move.source).GetRootTransform();
                        if (copy is VRCPhysBoneCollider collider) collider.rootTransform = ((VRCPhysBoneCollider)move.source).GetRootTransform();
                        EditorUtility.SetDirty(copy);
                        map.Add(move.source, copy);
                    }
                }
                // Repair collider references on replacement components too.
                Remap(scene.Concat(map.Values).Distinct(), map.ToDictionary(p => (UnityEngine.Object)p.Key, p => (UnityEngine.Object)p.Value), label);
                foreach (var move in moves) Undo.DestroyObjectImmediate(move.source);
                // Keep organizer objects outside simulated chains.
                var newBranches = root.GetComponentsInChildren<Transform>(true).Where(t => !originalTransforms.Contains(t) &&
                    t.parent != null && originalTransforms.Contains(t.parent)).ToArray();
                foreach (var bone in scene.Concat(map.Values).OfType<VRCPhysBoneBase>().Where(b => b != null).Distinct())
                {
                    var affected = new HashSet<Transform>(DynamicsOperations.AffectedTransforms(bone));
                    var ignores = newBranches.Where(affected.Contains).ToArray();
                    if (ignores.Length == 0) continue;
                    Undo.RecordObject(bone, label);
                    bone.ignoreTransforms = new List<Transform>(bone.ignoreTransforms ?? new List<Transform>());
                    bone.ignoreTransforms.AddRange(ignores);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(bone);
                    EditorUtility.SetDirty(bone);
                }
                if (sort)
                    foreach (var parent in map.Values.Select(c => c.transform.parent).Distinct())
                    {
                        var children = parent.Cast<Transform>().OrderBy(t => t.name, StringComparer.OrdinalIgnoreCase).ToArray();
                        Undo.RegisterCompleteObjectUndo(children, label);
                        for (int i = 0; i < children.Length; i++) children[i].SetSiblingIndex(i);
                    }
                Undo.FlushUndoRecordObjects();
                Undo.CollapseUndoOperations(group);
                return map;
            }
            catch { Undo.FlushUndoRecordObjects(); Undo.RevertAllDownToGroup(group); throw; }
        }

        private static Transform Folder(Transform parent, string name, string label)
        {
            var matches = parent.Cast<Transform>().Where(t => t.name == name).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException("More than one folder is named " + name + ". Rename the duplicates first.");
            if (matches.Length == 1)
            {
                var folder = matches[0];
                if (folder.GetComponents<Component>().Length != 1 || !folder.gameObject.activeSelf ||
                    PrefabUtility.IsPartOfPrefabInstance(folder.gameObject))
                    throw new InvalidOperationException(name + " must be an active empty folder outside a prefab instance.");
                return folder;
            }
            var host = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(host, label);
            host.transform.SetParent(parent, false);
            return host.transform;
        }

        internal static int ReferenceCount(GameObject root, IEnumerable<Component> targets)
        {
            var set = new HashSet<UnityEngine.Object>(targets);
            int count = 0;
            foreach (var c in SceneComponents(root).Where(c => !set.Contains(c)))
            {
                var it = new SerializedObject(c).GetIterator();
                while (it.Next(true))
                    if (it.propertyType == SerializedPropertyType.ObjectReference && set.Contains(it.objectReferenceValue)) count++;
            }
            return count;
        }

        internal static void Remove(GameObject root, Component[] targets)
        {
            RequireTargets(root, targets);
            targets = targets.Distinct().ToArray();
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            const string label = "Remove PhysBones and colliders";
            Undo.SetCurrentGroupName(label);
            try
            {
                Remap(SceneComponents(root).Except(targets), targets.ToDictionary(c => (UnityEngine.Object)c, c => (UnityEngine.Object)null), label);
                foreach (var c in targets) Undo.DestroyObjectImmediate(c);
                Undo.FlushUndoRecordObjects();
                Undo.CollapseUndoOperations(group);
            }
            catch { Undo.FlushUndoRecordObjects(); Undo.RevertAllDownToGroup(group); throw; }
        }

        private static void Remap(IEnumerable<Component> components, Dictionary<UnityEngine.Object, UnityEngine.Object> map, string label)
        {
            foreach (var c in components)
            {
                if (c == null) continue;
                var serialized = new SerializedObject(c);
                var it = serialized.GetIterator();
                bool changed = false;
                while (it.Next(true))
                {
                    if (it.propertyType != SerializedPropertyType.ObjectReference || it.objectReferenceValue == null ||
                        !map.TryGetValue(it.objectReferenceValue, out var replacement)) continue;
                    if (!changed) Undo.RecordObject(c, label);
                    it.objectReferenceValue = replacement;
                    changed = true;
                }
                if (!changed) continue;
                serialized.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
                EditorUtility.SetDirty(c);
            }
        }

        // Unskinned props and animations can still use these bones.
        internal static HashSet<Component> NoWeightedMeshBones(GameObject root, float threshold)
        {
            var used = new HashSet<Transform>();
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var mesh = renderer.sharedMesh;
                if (mesh == null) continue;
                var bones = renderer.bones;
                using (var weights = mesh.GetAllBoneWeights())
                    foreach (var weight in weights)
                        if (weight.weight > threshold && weight.boneIndex >= 0 && weight.boneIndex < bones.Length && bones[weight.boneIndex] != null)
                            used.Add(bones[weight.boneIndex]);
            }
            return new HashSet<Component>(Scan(root).OfType<VRCPhysBone>().Where(b =>
                !DynamicsOperations.AffectedTransforms(b).Any(used.Contains)).Cast<Component>());
        }

        internal static HashSet<Component> UnreferencedColliders(GameObject root)
        {
            var candidates = Scan(root).OfType<VRCPhysBoneCollider>().Cast<Component>().ToArray();
            var referenced = new HashSet<UnityEngine.Object>();
            foreach (var c in SceneComponents(root))
            {
                var it = new SerializedObject(c).GetIterator();
                while (it.Next(true))
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                        referenced.Add(it.objectReferenceValue);
            }
            return new HashSet<Component>(candidates.Where(c => !referenced.Contains(c)));
        }
    }
}
#endif
