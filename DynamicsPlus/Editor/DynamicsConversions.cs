#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    internal static class DynamicsConversions
    {
        internal enum ComponentKind { Sender, Receiver, Collider }

        internal static bool CanCreate(Component source, ComponentKind kind, out string reason)
        {
            reason = null;
            if (!Enum.IsDefined(typeof(ComponentKind), kind))
                reason = "Unknown component type.";
            else if (source == null || (source is ContactBase) == false && (source is VRCPhysBoneColliderBase) == false)
                reason = "Select a Contact or PhysBone collider.";
            else if (source is VRCContactSender && kind == ComponentKind.Sender ||
                     source is VRCContactReceiver && kind == ComponentKind.Receiver ||
                     source is VRCPhysBoneColliderBase && kind == ComponentKind.Collider)
                reason = "Choose a different component type.";
            else if (EditorApplication.isPlayingOrWillChangePlaymode)
                reason = "Exit Play Mode before creating components.";
            else if (DynamicsPreview.Active != null)
                reason = "Stop the PhysBone test before changing component types.";
            else if (EditorUtility.IsPersistent(source) || !source.gameObject.scene.IsValid())
                reason = "Use a scene object or open the prefab in Prefab Mode.";
            else if (source is ContactBase contact && kind == ComponentKind.Collider &&
                     contact.shapeType != ContactBase.ShapeType.Sphere && contact.shapeType != ContactBase.ShapeType.Capsule)
                reason = "This Contact shape cannot be represented by a PhysBone collider.";
            else if (source is VRCPhysBoneColliderBase collider && kind != ComponentKind.Collider &&
                     (collider.insideBounds || collider.shapeType != VRCPhysBoneColliderBase.ShapeType.Sphere &&
                      collider.shapeType != VRCPhysBoneColliderBase.ShapeType.Capsule))
                reason = "Planes and inside-bounds colliders cannot be represented by Contacts.";
            return reason == null;
        }

        internal static int CountReferences(Component source)
        {
            int count = 0;
            foreach (var component in UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (component == null || component == source || EditorUtility.IsPersistent(component)) continue;
                var property = new SerializedObject(component).GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == source) count++;
            }
            return count;
        }

        internal static Component Create(Component source, ComponentKind kind, bool replaceOriginal = false)
        {
            if (!CanCreate(source, kind, out var reason))
                throw new InvalidOperationException(reason);

            var shape = ShapeSnapshot.Capture(source);
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create dynamics component from shape");
            try
            {
                Component result;
                switch (kind)
                {
                    case ComponentKind.Sender: result = Undo.AddComponent<VRCContactSender>(source.gameObject); break;
                    case ComponentKind.Receiver: result = Undo.AddComponent<VRCContactReceiver>(source.gameObject); break;
                    case ComponentKind.Collider: result = Undo.AddComponent<VRCPhysBoneCollider>(source.gameObject); break;
                    default: throw new ArgumentOutOfRangeException(nameof(kind));
                }
                Undo.RecordObject(result, "Copy dynamics shape");
                shape.Apply(result);
                if (source is ContactBase original && result is ContactBase copy)
                {
                    copy.collisionTags = original.collisionTags == null
                        ? new List<string>() : new List<string>(original.collisionTags);
                    copy.localOnly = original.localOnly;
                    copy.contentTypes = original.contentTypes;
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(result);
                EditorUtility.SetDirty(result);
                if (replaceOriginal)
                {
                    foreach (var component in UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        if (component == null || component == source || EditorUtility.IsPersistent(component)) continue;
                        var serialized = new SerializedObject(component);
                        var property = serialized.GetIterator();
                        bool changed = false;
                        while (property.Next(true))
                        {
                            if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != source) continue;
                            if (!changed) Undo.RecordObject(component, "Clear converted component references");
                            property.objectReferenceValue = null;
                            changed = true;
                        }
                        if (!changed) continue;
                        serialized.ApplyModifiedProperties();
                        PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                    }
                    Undo.DestroyObjectImmediate(source);
                }
                Undo.CollapseUndoOperations(group);
                return result;
            }
            catch
            {
                Undo.FlushUndoRecordObjects();
                Undo.RevertAllDownToGroup(group);
                throw;
            }
        }

        private struct ShapeSnapshot
        {
            private Transform root;
            private Vector3 position;
            private Quaternion rotation;
            private float radius;
            private float height;
            private Vector3 size;
            private ContactBase.ShapeType contactShape;
            private VRCPhysBoneColliderBase.ShapeType colliderShape;
            private bool fromContact;

            internal static ShapeSnapshot Capture(Component source)
            {
                if (source is ContactBase contact)
                {
                    return new ShapeSnapshot
                    {
                        root = contact.GetRootTransform(), position = contact.position,
                        rotation = contact.rotation, radius = contact.radius, height = contact.height,
                        size = contact.size, contactShape = contact.shapeType, fromContact = true
                    };
                }
                var collider = (VRCPhysBoneColliderBase)source;
                return new ShapeSnapshot
                {
                    root = collider.GetRootTransform(), position = collider.position,
                    rotation = collider.rotation, radius = collider.radius, height = collider.height,
                    colliderShape = collider.shapeType
                };
            }

            internal void Apply(Component target)
            {
                Transform explicitRoot = root == target.transform ? null : root;
                if (target is ContactBase contact)
                {
                    contact.rootTransform = explicitRoot;
                    contact.position = position;
                    contact.rotation = rotation;
                    contact.radius = radius;
                    contact.height = height;
                    contact.shapeType = fromContact ? contactShape :
                        colliderShape == VRCPhysBoneColliderBase.ShapeType.Sphere
                            ? ContactBase.ShapeType.Sphere : ContactBase.ShapeType.Capsule;
                    if (fromContact) contact.size = size;
                }
                else
                {
                    var collider = (VRCPhysBoneColliderBase)target;
                    collider.rootTransform = explicitRoot;
                    collider.position = position;
                    collider.rotation = rotation;
                    collider.radius = radius;
                    collider.height = height;
                    collider.shapeType = fromContact
                        ? contactShape == ContactBase.ShapeType.Sphere
                            ? VRCPhysBoneColliderBase.ShapeType.Sphere : VRCPhysBoneColliderBase.ShapeType.Capsule
                        : colliderShape;
                }
            }
        }
    }
}
#endif
