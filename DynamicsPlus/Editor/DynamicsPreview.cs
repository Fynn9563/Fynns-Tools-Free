#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Dynamics;

namespace FynnsToolkit.Dynamics
{
    internal sealed class DynamicsPreview : IDisposable
    {
        internal static DynamicsPreview Active { get; private set; }
        private readonly Dictionary<UnityEngine.Object, UnityEngine.Object> copies = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
        private readonly Dictionary<Component, string> configurations = new Dictionary<Component, string>();
        private Scene scene;
        private PhysBoneManager manager;
        private SceneView view;
        private Scene previousViewScene;
        private readonly DynamicsSceneTools testingOverlay = new DynamicsSceneTools();
        private GameObject originalRoot;
        private GameObject[] originalRoots = Array.Empty<GameObject>();
        private double lastUpdate;
        private bool ownsView;
        private bool showTestView;
        private PhysBoneManager.Grab grab;
        private Plane grabPlane;
        private int grabControl;
        private bool hidTools;
        private bool previousToolsHidden;
        private readonly HashSet<VRCPhysBoneColliderBase> registeredColliders = new HashSet<VRCPhysBoneColliderBase>();
        private static readonly PropertyInfo constraintEditMode = typeof(VRCPhysBoneBase).Assembly
            .GetType("VRC.Dynamics.VRCConstraintManager", false)?.GetProperty("CanExecuteConstraintJobsInEditMode");
        internal GameObject Root { get; private set; }
        internal bool Running => scene.IsValid();
        internal string Error { get; private set; }

        internal UnityEngine.Object CopyOf(UnityEngine.Object original)
        {
            return original != null && copies.TryGetValue(original, out var copy) ? copy : null;
        }

        internal bool ContainsCopy(Component component) => component != null && copies.ContainsValue(component);
        internal UnityEngine.Object OriginalOf(UnityEngine.Object copy) => copies.FirstOrDefault(pair => pair.Value == copy).Key;

        internal void Start(GameObject root, bool showView = true)
            => Start(root, showView, null);

        private void Start(GameObject root, bool showView, GameObject[] scope, SceneView targetView = null)
        {
            if (root != null && (EditorUtility.IsPersistent(root) || !root.scene.IsValid()))
                throw new InvalidOperationException("Choose an avatar or world scene hierarchy to test.");
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before starting this preview.");
            if (Active != null || PhysBoneManager.Inst != null || ContactManager.Inst != null)
                throw new InvalidOperationException("Stop the other PhysBone preview before starting this test.");
            if (constraintEditMode == null || !constraintEditMode.CanRead || !constraintEditMode.CanWrite)
                throw new NotSupportedException("This SDK version does not expose the scoped Constraint scheduler control required by this preview.");
            originalRoots = scope ?? (root != null ? new[] { root } : Selection.transforms
                .Concat(Selection.objects.OfType<Component>().Select(component => component.transform))
                .Select(transform => transform.root.gameObject)
                .Where(value => value.scene.IsValid() && value.scene.isLoaded && !EditorUtility.IsPersistent(value))
                .Distinct().ToArray());
            if (!originalRoots.SelectMany(r => r.GetComponentsInChildren<VRCPhysBoneBase>(true)).Any(b => b.enabled && b.gameObject.activeInHierarchy))
                throw new InvalidOperationException("Select an avatar or world hierarchy containing active PhysBones.");
            originalRoot = root;
            showTestView = showView;
            Error = null;
            scene = EditorSceneManager.NewPreviewScene();
            Active = this;
            try
            {
                previousToolsHidden = Tools.hidden;
                hidTools = DynamicsSettings.instance.hideToolsDuringTesting;
                if (hidTools) Tools.hidden = true;
                foreach (var transform in originalRoots.SelectMany(r => r.GetComponentsInChildren<Transform>(true))) EnsureTransform(transform);
                if (root != null) Root = (GameObject)copies[root];
                else
                {
                    Root = new GameObject("Physbone Tester") { hideFlags = HideFlags.DontSave };
                    SceneManager.MoveGameObjectToScene(Root, scene);
                    if (Selection.activeTransform != null) Root.transform.position = Selection.activeTransform.position;
                    foreach (var source in originalRoots) ((GameObject)copies[source]).transform.SetParent(Root.transform, true);
                }
                foreach (var component in originalRoots.SelectMany(r => r.GetComponentsInChildren<Component>(true)))
                    if (component is Renderer || component is MeshFilter || component is VRCPhysBoneBase || component is VRCPhysBoneColliderBase)
                        CopyComponent(component);
                foreach (var bone in originalRoots.SelectMany(r => r.GetComponentsInChildren<VRCPhysBoneBase>(true)))
                {
                    foreach (var transform in bone.GetRootTransform().GetComponentsInChildren<Transform>(true)) EnsureTransform(transform);
                    foreach (var collider in bone.colliders ?? new List<VRCPhysBoneColliderBase>())
                        if (collider != null) { EnsureTransform(collider.GetRootTransform()); CopyComponent(collider); }
                }
                foreach (var component in copies.Keys.OfType<Component>().ToArray())
                    Remap((Component)copies[component], copies);

                var managerHost = new GameObject("Dynamics+ preview physics") { hideFlags = HideFlags.HideAndDontSave };
                SceneManager.MoveGameObjectToScene(managerHost, scene);
                manager = managerHost.AddComponent<PhysBoneManager>();
                PhysBoneManager.Inst = manager;
                manager.IsSDK = true;
                manager.Init();
                foreach (var collider in copies.Values.OfType<VRCPhysBoneColliderBase>())
                    if (collider.enabled && collider.gameObject.activeInHierarchy)
                    {
                        collider.InitShape();
                        manager.AddCollider(collider);
                        registeredColliders.Add(collider);
                    }
                foreach (var bone in copies.Values.OfType<VRCPhysBoneBase>())
                    if (bone.enabled && bone.gameObject.activeInHierarchy) bone.Init();
                foreach (var component in copies.Values.OfType<Component>().Where(value => value is VRCPhysBoneBase || value is VRCPhysBoneColliderBase))
                    configurations[component] = EditorJsonUtility.ToJson(component);
                if (showView)
                {
                    view = targetView != null ? targetView : SceneView.lastActiveSceneView;
                    ownsView = view == null;
                    if (ownsView)
                    {
                        view = ScriptableObject.CreateInstance<SceneView>();
                        view.titleContent = new GUIContent("Physbone Tester");
                    }
                    previousViewScene = ViewSceneProperty().GetValue(view) is Scene previous ? previous : default;
                    SetViewScene(view, scene);
                    if (ownsView)
                    {
                        view.Show();
                        view.LookAt(root == null ? Root.transform.position : root.transform.position, Quaternion.Euler(15, -30, 0), 2f);
                    }
                    view.Repaint();
                }
                lastUpdate = EditorApplication.timeSinceStartup;
                EditorApplication.update += Tick;
                Undo.undoRedoPerformed += RefreshConfiguration;
                AssemblyReloadEvents.beforeAssemblyReload += Dispose;
                EditorApplication.quitting += Dispose;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                EditorSceneManager.sceneSaving += OnSceneSaving;
                SceneView.duringSceneGui += DrawTestScene;
            }
            catch { Dispose(); throw; }
        }

        private Transform EnsureTransform(Transform source)
        {
            if (copies.TryGetValue(source, out var existing)) return (Transform)existing;
            Transform parent = source.parent != null ? EnsureTransform(source.parent) : null;
            var host = new GameObject(source.name) { hideFlags = HideFlags.DontSave, layer = source.gameObject.layer };
            SceneManager.MoveGameObjectToScene(host, scene);
            host.transform.SetParent(parent, false);
            host.transform.localPosition = source.localPosition;
            host.transform.localRotation = source.localRotation;
            host.transform.localScale = source.localScale;
            host.SetActive(source.gameObject.activeSelf);
            copies[source] = host.transform;
            copies[source.gameObject] = host;
            return host.transform;
        }

        private Component CopyComponent(Component source)
        {
            if (copies.TryGetValue(source, out var existing)) return (Component)existing;
            var transform = EnsureTransform(source.transform);
            var destination = transform.gameObject.AddComponent(source.GetType());
            EditorUtility.CopySerialized(source, destination);
            destination.hideFlags = HideFlags.DontSave;
            copies[source] = destination;
            return destination;
        }

        internal static void Remap(Component target, IDictionary<UnityEngine.Object, UnityEngine.Object> map)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.GetIterator();
            while (property.Next(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null &&
                    map.TryGetValue(property.objectReferenceValue, out var replacement))
                    property.objectReferenceValue = replacement;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void Tick()
        {
            if (Running)
            {
                if (DynamicsSettings.instance.hideToolsDuringTesting) { Tools.hidden = true; hidTools = true; }
                else if (hidTools) { Tools.hidden = previousToolsHidden; hidTools = false; }
            }
            if (!Running) return;
            if (!originalRoots.Any(r => r != null) || manager == null || PhysBoneManager.Inst != manager || ContactManager.Inst != null || showTestView && view == null) { Dispose(); return; }
            double now = EditorApplication.timeSinceStartup;
            if (now - lastUpdate < 1.0 / 60.0) return;
            float delta = Mathf.Clamp((float)(now - lastUpdate), 0.001f, 0.05f);
            lastUpdate = now;
            bool previousTiming = PhysBoneManager.DisableTiming;
            float previousDelta = PhysBoneManager.DebugTimeElapsed;
            bool previousConstraints = (bool)constraintEditMode.GetValue(null);
            try
            {
                PhysBoneManager.DisableTiming = true;
                PhysBoneManager.DebugTimeElapsed = delta;
                constraintEditMode.SetValue(null, false);
                VRCDynamicsScheduler.PreScheduleDynamics(true);
                SceneView.RepaintAll();
            }
            catch (Exception exception)
            {
                Error = "PhysBone preview stopped: " + exception.Message;
                Debug.LogException(exception);
                Dispose();
            }
            finally
            {
                PhysBoneManager.DisableTiming = previousTiming;
                PhysBoneManager.DebugTimeElapsed = previousDelta;
                constraintEditMode.SetValue(null, previousConstraints);
            }
        }

        internal void StepForValidation() { lastUpdate = 0; Tick(); }

        internal void RefreshConfiguration()
        {
            if (!Running || manager == null) return;
            VRCDynamicsScheduler.FinalizeJob();
            foreach (var bone in copies.Values.OfType<VRCPhysBoneBase>().ToArray())
            {
                var root = bone.GetRootTransform();
                if (!copies.ContainsValue(root))
                    foreach (var transform in root.GetComponentsInChildren<Transform>(true)) EnsureTransform(transform);
                foreach (var ignored in bone.ignoreTransforms ?? new List<Transform>())
                    if (ignored != null && !copies.ContainsValue(ignored)) EnsureTransform(ignored);
                foreach (var collider in bone.colliders ?? new List<VRCPhysBoneColliderBase>())
                    if (collider != null && !copies.ContainsValue(collider))
                    {
                        EnsureTransform(collider.GetRootTransform());
                        CopyComponent(collider);
                    }
            }
            foreach (var collider in copies.Values.OfType<VRCPhysBoneColliderBase>().ToArray())
                if (!copies.ContainsValue(collider.GetRootTransform())) EnsureTransform(collider.GetRootTransform());
            foreach (var component in copies.Values.OfType<Component>().ToArray())
            {
                Remap(component, copies);
            }
            foreach (var component in copies.Values.OfType<Component>().ToArray())
            {
                if (component is VRCPhysBoneColliderBase added && added.enabled && added.gameObject.activeInHierarchy && registeredColliders.Add(added))
                {
                    added.InitShape();
                    manager.AddCollider(added);
                }
                if (component is VRCPhysBoneColliderBase collider) collider.ApplyConfigurationChanges();
            }
            foreach (var bone in copies.Values.OfType<VRCPhysBoneBase>()) bone.ApplyConfigurationChanges();
        }

        internal bool IsView(SceneView candidate) => candidate == view;

        private void DrawTestScene(SceneView candidate)
        {
            if (candidate != view || manager == null) return;
            var current = Event.current;
            grabControl = GUIUtility.GetControlID("ADOPreviewGrab".GetHashCode(), FocusType.Passive);
            if (current.type == EventType.KeyDown && (current.keyCode == KeyCode.Escape || current.keyCode == KeyCode.Return))
            {
                current.Use(); Dispose(); return;
            }
            if (current.control && current.type == EventType.Layout) HandleUtility.AddDefaultControl(grabControl);
            if (current.type == EventType.MouseDown && current.button == 0 && current.control)
            {
                VRCDynamicsScheduler.FinalizeJob();
                grab = manager.AttemptGrab(-1, HandleUtility.GUIPointToWorldRay(current.mousePosition), out var point);
                if (grab != null)
                {
                    grabPlane = new Plane(candidate.camera.transform.forward, point);
                    GUIUtility.hotControl = grabControl;
                    current.Use();
                }
            }
            else if (grab != null && current.type == EventType.MouseDrag)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                if (grabPlane.Raycast(ray, out float distance)) grab.GlobalPosition = ray.GetPoint(distance);
                current.Use();
            }
            else if (grab != null && current.type == EventType.MouseUp)
            {
                VRCDynamicsScheduler.FinalizeJob();
                manager.ReleaseGrab(grab, current.shift, -1);
                grab = null;
                GUIUtility.hotControl = 0;
                current.Use();
            }
            if (grab == null && !current.control) DrawTestTransform();
            testingOverlay.DrawTestingOverlay(Restart, () => Apply(), Dispose, HasChanges());
        }

        private void DrawTestTransform()
        {
            var transform = Root.transform;
            var rotation = Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : transform.rotation;
            EditorGUI.BeginChangeCheck();
            var position = Handles.PositionHandle(transform.position, rotation);
            if (EditorGUI.EndChangeCheck())
            {
                VRCDynamicsScheduler.FinalizeJob();
                transform.position = position;
            }
            EditorGUI.BeginChangeCheck();
            var nextRotation = Handles.RotationHandle(rotation, transform.position);
            if (EditorGUI.EndChangeCheck())
            {
                VRCDynamicsScheduler.FinalizeJob();
                transform.rotation = nextRotation * Quaternion.Inverse(rotation) * transform.rotation;
            }
        }

        internal void Restart()
        {
            var root = originalRoot;
            var scope = originalRoots.ToArray();
            var targetView = ownsView ? null : view;
            var reverse = copies.ToDictionary(pair => pair.Value, pair => pair.Key);
            var selected = Selection.objects.Select(value => reverse.TryGetValue(value, out var original) ? original : value).ToArray();
            var active = Selection.activeObject;
            if (active != null && reverse.TryGetValue(active, out var activeOriginal)) active = activeOriginal;
            Dispose();
            Start(root, showTestView, scope, targetView);
            Selection.objects = selected.Select(value => CopyOf(value) ?? value).ToArray();
            if (active != null) Selection.activeObject = CopyOf(active) ?? active;
        }

        internal bool HasChanges(Component only = null)
        {
            if (only != null && CopyOf(only) is Component copy) only = copy;
            return configurations.Any(pair => (only == null || pair.Key == only) && pair.Key != null &&
                EditorJsonUtility.ToJson(pair.Key) != pair.Value);
        }

        internal void Apply(Component only = null)
        {
            if (!Running) return;
            VRCDynamicsScheduler.FinalizeJob();
            var reverse = copies.ToDictionary(pair => pair.Value, pair => pair.Key);
            var originals = copies.Keys.OfType<Component>().Where(c => c is VRCPhysBoneBase || c is VRCPhysBoneColliderBase)
                .Where(c => only == null || copies[c] == only).ToArray();
            DynamicsOperations.Change(originals, "Dynamics+ - Apply test changes", original =>
            {
                var source = new SerializedObject(copies[original]);
                var destination = new SerializedObject(original);
                var property = source.GetIterator();
                bool enter = true;
                while (property.Next(enter))
                {
                    enter = false;
                    if (property.propertyPath.StartsWith("m_", StringComparison.Ordinal) && property.propertyPath != "m_Enabled") continue;
                    destination.CopyFromSerializedProperty(property);
                }
                destination.ApplyModifiedProperties();
                Remap(original, reverse);
            });
            foreach (var original in originals)
            {
                var copy = (Component)copies[original];
                configurations[copy] = EditorJsonUtility.ToJson(copy);
            }
        }

        private void OnPlayModeChanged(PlayModeStateChange state) { Dispose(); }
        private void OnSceneSaving(Scene saved, string path) { Dispose(); }

        private static void SetViewScene(SceneView target, Scene value)
        {
            ViewSceneProperty().SetValue(target, value);
        }

        private static PropertyInfo ViewSceneProperty()
        {
            var property = typeof(SceneView).GetProperty("customScene", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanRead || !property.CanWrite)
                throw new NotSupportedException("This Unity version does not expose a preview scene view.");
            return property;
        }

        public void Dispose()
        {
            var reverse = copies.ToDictionary(pair => pair.Value, pair => pair.Key);
            bool selectedCopy = Selection.objects.Any(reverse.ContainsKey);
            var selected = Selection.objects.Select(value => reverse.TryGetValue(value, out var original) ? original : value).ToArray();
            var active = Selection.activeObject;
            bool activeCopy = active != null && reverse.TryGetValue(active, out _);
            if (activeCopy) active = reverse[active];
            EditorApplication.update -= Tick;
            Undo.undoRedoPerformed -= RefreshConfiguration;
            AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
            EditorApplication.quitting -= Dispose;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            SceneView.duringSceneGui -= DrawTestScene;
            grab = null;
            testingOverlay.CancelOverlayDrag();
            if (GUIUtility.hotControl == grabControl) GUIUtility.hotControl = 0;
            try { if (manager != null) manager.Dispose(); }
            finally
            {
                try
                {
                    if (view != null)
                    {
                        try { SetViewScene(view, previousViewScene); }
                        finally { if (ownsView) view.Close(); else view.Repaint(); }
                    }
                }
                finally
                {
                    view = null;
                    ownsView = false;
                    previousViewScene = default;
                    try { if (scene.IsValid()) EditorSceneManager.ClosePreviewScene(scene); }
                    finally
                    {
                        if (PhysBoneManager.Inst == manager) PhysBoneManager.Inst = null;
                        manager = null;
                        scene = default(Scene);
                        Root = null;
                        copies.Clear();
                        configurations.Clear();
                        registeredColliders.Clear();
                        originalRoots = Array.Empty<GameObject>();
                        if (hidTools) { Tools.hidden = previousToolsHidden; hidTools = false; }
                        if (Active == this) Active = null;
                        if (selectedCopy) Selection.objects = selected.Where(value => value != null).ToArray();
                        if (activeCopy && active != null) Selection.activeObject = active;
                    }
                }
            }
        }
    }
}
#endif
