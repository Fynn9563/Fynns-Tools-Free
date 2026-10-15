using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FynnsTools.Clipping
{
    internal sealed class ClippingPhysics : IDisposable
    {
        private readonly GameObject root;
        private readonly Type managerType, schedulerType, contactType;
        private readonly PropertyInfo constraints;
        private readonly Behaviour[] bones, colliders;
        private readonly Dictionary<Transform, Pose> animated = new Dictionary<Transform, Pose>();
        private Component manager;
        public static bool Available => ClippingPreview.FindType("VRC.Dynamics.PhysBoneManager") != null;
        public string Status => bones.Length == 0 ? "No active PhysBones" : "PhysBones: " + bones.Length + " (simulated during playback)";

        public static void CheckOwnership(GameObject source)
        {
            var type = ClippingPreview.FindType("VRC.Dynamics.VRCPhysBoneBase");
            if (type == null || !source.GetComponentsInChildren(type, true).OfType<Behaviour>().Any(b => b.enabled && b.gameObject.activeInHierarchy)) return;
            foreach (var name in new[] { "VRC.Dynamics.PhysBoneManager", "VRC.Dynamics.ContactManager" })
                if (ClippingPreview.FindType(name)?.GetField("Inst")?.GetValue(null) != null)
                    throw new InvalidOperationException("Stop the other dynamics preview before playing PhysBones here.");
        }

        public ClippingPhysics(GameObject root)
        {
            this.root = root;
            managerType = ClippingPreview.FindType("VRC.Dynamics.PhysBoneManager");
            schedulerType = ClippingPreview.FindType("VRC.Dynamics.VRCDynamicsScheduler");
            contactType = ClippingPreview.FindType("VRC.Dynamics.ContactManager");
            constraints = ClippingPreview.FindType("VRC.Dynamics.VRCConstraintManager")?.GetProperty("CanExecuteConstraintJobsInEditMode");
            var boneType = ClippingPreview.FindType("VRC.Dynamics.VRCPhysBoneBase");
            var colliderType = ClippingPreview.FindType("VRC.Dynamics.VRCPhysBoneColliderBase");
            bones = Active(boneType);
            colliders = Active(colliderType);
            foreach (var component in bones.Concat(colliders))
            {
                var serialized = new UnityEditor.SerializedObject(component);
                var property = serialized.GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType != UnityEditor.SerializedPropertyType.ObjectReference) continue;
                    var reference = property.objectReferenceValue as Component;
                    if (reference && !reference.transform.IsChildOf(root.transform)) property.objectReferenceValue = null;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private Behaviour[] Active(Type type) => type == null ? Array.Empty<Behaviour>() :
            root.GetComponentsInChildren(type, true).OfType<Behaviour>().Where(b => b.enabled && b.gameObject.activeInHierarchy).ToArray();
        public bool Contains(Behaviour behaviour) => bones.Contains(behaviour) || colliders.Contains(behaviour);
        private object Get(string name) => managerType.GetField(name, BindingFlags.Public | BindingFlags.Static).GetValue(null);
        private void Set(string name, object value) => managerType.GetField(name, BindingFlags.Public | BindingFlags.Static).SetValue(null, value);
        private static void Call(object target, string name, params object[] args) =>
            target.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(target, args);

        public void BeforeSample(bool reset)
        {
            if (manager) schedulerType.GetMethod("FinalizeJob").Invoke(null, null);
            foreach (var entry in animated) if (entry.Key) entry.Key.SetLocalPositionAndRotation(entry.Value.position, entry.Value.rotation);
            animated.Clear();
            if (reset) ReleaseManager();
        }

        public void Step(float delta)
        {
            if (bones.Length == 0) return;
            if (!manager)
            {
                if (Get("Inst") != null || contactType?.GetField("Inst")?.GetValue(null) != null)
                    throw new InvalidOperationException("Stop the other dynamics preview before playing PhysBones here.");
                if (schedulerType == null || constraints == null || !constraints.CanWrite)
                    throw new NotSupportedException("This SDK version cannot run an isolated PhysBone preview.");
                var host = new GameObject("Clipping preview physics") { hideFlags = HideFlags.HideAndDontSave };
                SceneManager.MoveGameObjectToScene(host, root.scene);
                manager = host.AddComponent(managerType);
                Set("Inst", manager);
                managerType.GetField("IsSDK").SetValue(manager, true);
                Call(manager, "Init");
                foreach (var collider in colliders)
                { Call(collider, "InitShape"); Call(manager, "AddCollider", collider); }
                foreach (var bone in bones) Call(bone, "Init");
            }
            if (!ReferenceEquals(Get("Inst"), manager))
                throw new InvalidOperationException("PhysBone preview ownership changed. Stop and restart the preview.");
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                animated[transform] = new Pose(transform.localPosition, transform.localRotation);
            var timing = Get("DisableTiming");
            var elapsed = Get("DebugTimeElapsed");
            var previousConstraints = constraints.GetValue(null);
            try
            {
                Set("DisableTiming", true);
                Set("DebugTimeElapsed", Mathf.Clamp(delta, 0.001f, 0.05f));
                constraints.SetValue(null, false);
                schedulerType.GetMethod("PreScheduleDynamics").Invoke(null, new object[] { true });
                schedulerType.GetMethod("FinalizeJob").Invoke(null, null);
            }
            finally
            {
                Set("DisableTiming", timing); Set("DebugTimeElapsed", elapsed);
                constraints.SetValue(null, previousConstraints);
            }
        }

        private void ReleaseManager()
        {
            if (!manager) return;
            try { Call(manager, "Dispose"); }
            finally
            {
                if (ReferenceEquals(Get("Inst"), manager)) Set("Inst", null);
                Object.DestroyImmediate(manager.gameObject); manager = null;
            }
        }
        public void Dispose() { BeforeSample(true); }
    }
}
