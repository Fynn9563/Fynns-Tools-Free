#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    internal static class DynamicsOperations
    {
        internal static void RequireEditable(Component component)
        {
            if (component == null || EditorUtility.IsPersistent(component) || !component.gameObject.scene.IsValid())
                throw new InvalidOperationException("Choose a scene object or open the prefab in Prefab Mode.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before editing dynamics.");
        }

        internal static void Change(IEnumerable<Component> targets, string label, Action<Component> edit)
        {
            var components = targets.Where(c => c != null).Distinct().ToArray();
            if (components.Length == 0) return;
            foreach (var component in components) RequireEditable(component);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(label);
            try
            {
                Undo.RecordObjects(components, label);
                foreach (var component in components)
                {
                    edit(component);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                    EditorUtility.SetDirty(component);
                }
                if (DynamicsPreview.Active != null && components.Any(DynamicsPreview.Active.ContainsCopy)) DynamicsPreview.Active.RefreshConfiguration();
                Undo.CollapseUndoOperations(group);
            }
            catch { Undo.FlushUndoRecordObjects(); Undo.RevertAllDownToGroup(group); throw; }
        }

        internal static VRCPhysBoneCollider MoveCollider(VRCPhysBoneCollider original)
        {
            RequireEditable(original);
            if (DynamicsPreview.Active != null) throw new InvalidOperationException("Stop the PhysBone test before moving components.");
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Move collider to empty object");
            try
            {
                var host = new GameObject(original.gameObject.name + " Collider");
                Undo.RegisterCreatedObjectUndo(host, "Move collider to empty object");
                Undo.SetTransformParent(host.transform, original.transform, "Parent collider");
                host.transform.localPosition = Vector3.zero;
                host.transform.localRotation = Quaternion.identity;
                host.transform.localScale = Vector3.one;
                host.layer = original.gameObject.layer;
                var replacement = Undo.AddComponent<VRCPhysBoneCollider>(host);
                Undo.RegisterCompleteObjectUndo(replacement, "Copy collider settings");
                EditorUtility.CopySerialized(original, replacement);
                replacement.rootTransform = original.GetRootTransform();
                PrefabUtility.RecordPrefabInstancePropertyModifications(replacement);
                RemapReferences(original, replacement);
                Undo.DestroyObjectImmediate(original);
                Undo.CollapseUndoOperations(group);
                return replacement;
            }
            catch { Undo.FlushUndoRecordObjects(); Undo.RevertAllDownToGroup(group); throw; }
        }

        private static void RemapReferences(UnityEngine.Object original, UnityEngine.Object replacement)
        {
            foreach (var component in UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (component == null || component == original || EditorUtility.IsPersistent(component)) continue;
                var serialized = new SerializedObject(component);
                var property = serialized.GetIterator();
                bool changed = false;
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != original) continue;
                    if (!changed) Undo.RecordObject(component, "Repair moved collider references");
                    property.objectReferenceValue = replacement;
                    changed = true;
                }
                if (!changed) continue;
                serialized.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
        }

        internal static Transform Root(Component component)
        {
            if (component is VRCPhysBoneBase bone) return bone.GetRootTransform();
            if (component is VRCPhysBoneColliderBase collider) return collider.GetRootTransform();
            if (component is ContactBase contact) return contact.GetRootTransform();
            return component.transform;
        }

        internal static List<Transform> AffectedTransforms(VRCPhysBoneBase bone)
        {
            var result = new List<Transform>();
            var ignored = new HashSet<Transform>(bone.ignoreTransforms ?? new List<Transform>());
            var otherRoots = new HashSet<Transform>();
            if (bone.ignoreOtherPhysBones)
                foreach (var other in UnityEngine.Object.FindObjectsByType<VRCPhysBoneBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (other != bone) otherRoots.Add(other.GetRootTransform());
            Action<Transform> visit = null;
            visit = transform =>
            {
                if (ignored.Contains(transform) || transform != bone.GetRootTransform() && otherRoots.Contains(transform)) return;
                result.Add(transform);
                foreach (Transform child in transform) visit(child);
            };
            visit(bone.GetRootTransform());
            return result;
        }

        internal static void OptimizeIgnores(IEnumerable<VRCPhysBoneBase> bones)
        {
            Change(bones.Cast<Component>(), "Optimize ignored transforms", component =>
            {
                var bone = (VRCPhysBoneBase)component;
                var root = bone.GetRootTransform();
                var list = (bone.ignoreTransforms ?? new List<Transform>()).Where(t => t != null && (t == root || t.IsChildOf(root))).Distinct().ToList();
                bone.ignoreTransforms = list.Where(t => !list.Any(parent => parent != t && t.IsChildOf(parent))).ToList();
            });
        }

        internal static void ToggleIgnore(IEnumerable<VRCPhysBoneBase> bones, Transform transform)
        {
            var targets = bones.Where(b => transform == b.GetRootTransform() || transform.IsChildOf(b.GetRootTransform())).ToArray();
            bool remove = targets.Length > 0 && targets.All(b => b.ignoreTransforms != null && b.ignoreTransforms.Contains(transform));
            Change(targets.Cast<Component>(), "Edit ignored transforms", component =>
            {
                var bone = (VRCPhysBoneBase)component;
                if (bone.ignoreTransforms == null) bone.ignoreTransforms = new List<Transform>();
                if (remove) bone.ignoreTransforms.RemoveAll(t => t == transform);
                else if (!bone.ignoreTransforms.Contains(transform)) bone.ignoreTransforms.Add(transform);
            });
        }

        internal static void ToggleCollider(IEnumerable<VRCPhysBoneBase> bones, VRCPhysBoneColliderBase collider)
        {
            var targets = bones.ToArray();
            bool remove = targets.Length > 0 && targets.All(b => b.colliders != null && b.colliders.Contains(collider));
            Change(targets.Cast<Component>(), "Edit PhysBone colliders", component =>
            {
                var bone = (VRCPhysBoneBase)component;
                if (bone.colliders == null) bone.colliders = new List<VRCPhysBoneColliderBase>();
                if (remove) bone.colliders.RemoveAll(c => c == collider);
                else if (!bone.colliders.Contains(collider)) bone.colliders.Add(collider);
            });
        }

        internal static void CopyIgnores(VRCPhysBoneBase source, VRCPhysBoneBase target)
        {
            var mapped = new List<Transform>();
            var sourceRoot = source.GetRootTransform();
            var targetRoot = target.GetRootTransform();
            foreach (var original in source.ignoreTransforms ?? new List<Transform>())
            {
                if (original == null) continue;
                if (original != sourceRoot && !original.IsChildOf(sourceRoot))
                    throw new InvalidOperationException("An ignored transform is outside the source chain.");
                string path = AnimationUtility.CalculateTransformPath(original, sourceRoot);
                var match = path.Length == 0 ? targetRoot : targetRoot.Find(path);
                if (match == null) throw new InvalidOperationException("The target chain has no transform at: " + path);
                if (!mapped.Contains(match)) mapped.Add(match);
            }
            Change(new[] { target }, "Copy ignored transforms", component => ((VRCPhysBoneBase)component).ignoreTransforms = mapped);
        }

        internal static void CopyColliders(VRCPhysBoneBase source, VRCPhysBoneBase target)
        {
            Change(new[] { target }, "Copy collider list", component =>
                ((VRCPhysBoneBase)component).colliders = new List<VRCPhysBoneColliderBase>(source.colliders ?? new List<VRCPhysBoneColliderBase>()));
        }
    }
}
#endif
