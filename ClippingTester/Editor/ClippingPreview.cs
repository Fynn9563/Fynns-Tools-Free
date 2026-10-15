using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FynnsTools.Clipping
{
    public sealed class ClippingPreview : IDisposable
    {
        public GameObject Root { get; private set; }
        public SceneView View { get; private set; }
        public string ClothingStatus { get; private set; }
        public string PhysicsStatus => physics?.Status;
        public bool IsValid => Root && View && driver && AnimationMode.InAnimationMode(driver);
        public Renderer[] Renderers => Root ? Root.GetComponentsInChildren<Renderer>(true).Where(IsMeshRenderer).ToArray() : Array.Empty<Renderer>();
        private Scene scene;
        private AnimationModeDriver driver;
        private readonly List<Object> owned = new List<Object>();
        private readonly HashSet<int> sourceAssets = new HashSet<int>();
        private readonly Dictionary<Renderer, Material[]> materials = new Dictionary<Renderer, Material[]>();
        private readonly Dictionary<Renderer, Renderer> rendererCopies = new Dictionary<Renderer, Renderer>();
        private readonly Dictionary<Transform, Pose> rest = new Dictionary<Transform, Pose>();
        private readonly Dictionary<Transform, Vector3> scales = new Dictionary<Transform, Vector3>();
        private AnimationClip lastClip;
        private ClippingPhysics physics;
        private float lastTime;
        private Material bodyMaterial, clothingMaterial;

        public static bool IsMeshRenderer(Renderer renderer) => renderer is SkinnedMeshRenderer || renderer is MeshRenderer;
        public static Type FindType(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).FirstOrDefault(t => t != null);
        public static bool HasClothingTools(GameObject avatar) => avatar && avatar.GetComponentsInChildren<Component>(true).Any(c => c &&
            (c.GetType().Namespace?.StartsWith("nadena.dev.modular_avatar", StringComparison.Ordinal) == true ||
             c.GetType().Namespace?.StartsWith("VF.", StringComparison.Ordinal) == true));
        public static bool ClothingAvailable => FindType("VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks") != null &&
            (FindType("nadena.dev.modular_avatar.core.ModularAvatarMergeArmature") != null || FindType("VF.Model.VRCFury") != null);

        public void Start(GameObject source, bool prepareClothing, bool simulatePhysBones = true)
        {
            Dispose();
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Unity Play Mode before previewing.");
            if (!source || EditorUtility.IsPersistent(source) || !source.scene.IsValid() || !source.scene.isLoaded)
                throw new InvalidOperationException("Choose an avatar from an open scene.");
            if (AnimationMode.InAnimationMode()) throw new InvalidOperationException("Another animation preview is active. Stop it before starting this preview.");
            if (simulatePhysBones) ClippingPhysics.CheckOwnership(source);
            try
            {
                foreach (var renderer in source.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (var material in renderer.sharedMaterials) Protect(material);
                    if (renderer is SkinnedMeshRenderer skinned) Protect(skinned.sharedMesh);
                    else if (renderer.TryGetComponent<MeshFilter>(out var filter)) Protect(filter.sharedMesh);
                }
                foreach (var animator in source.GetComponentsInChildren<Animator>(true))
                { Protect(animator.avatar); Protect(animator.runtimeAnimatorController); }
                scene = EditorSceneManager.NewPreviewScene();
                var holder = new GameObject("Clipping preview");
                SceneManager.MoveGameObjectToScene(holder, scene);
                Root = Object.Instantiate(source, holder.transform, true);
                Root.transform.SetParent(null, true);
                Object.DestroyImmediate(holder);
                Root.name = source.name;
                Root.SetActive(true);
                var originals = source.GetComponentsInChildren<Renderer>(true);
                var copies = Root.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < originals.Length; i++) rendererCopies[originals[i]] = copies[i];
                foreach (var t in Root.GetComponentsInChildren<Transform>(true)) t.gameObject.hideFlags = HideFlags.DontSave;
                ClothingStatus = "Original clothing setup";
                if (prepareClothing && HasClothingTools(source)) PrepareClothing();
                else if (prepareClothing) ClothingStatus = "No VRCFury or Modular Avatar components found; using normal preview.";
                TrackGeneratedAssets();
                if (simulatePhysBones) physics = new ClippingPhysics(Root);
                foreach (var behaviour in Root.GetComponentsInChildren<MonoBehaviour>(true))
                    if (behaviour && !(physics?.Contains(behaviour) ?? false)) behaviour.enabled = false;
                foreach (var camera in Root.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
                foreach (var audio in Root.GetComponentsInChildren<AudioSource>(true)) audio.enabled = false;
                foreach (var animator in Root.GetComponentsInChildren<Animator>(true))
                {
                    var controller = new AnimatorController { hideFlags = HideFlags.HideAndDontSave };
                    controller.AddLayer("Preview");
                    owned.Add(controller);
                    foreach (var layer in controller.layers) owned.Add(layer.stateMachine);
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }
                foreach (var t in Root.GetComponentsInChildren<Transform>(true))
                { rest[t] = new Pose(t.localPosition, t.localRotation); scales[t] = t.localScale; }
                var light = new GameObject("Preview Light").AddComponent<Light>();
                SceneManager.MoveGameObjectToScene(light.gameObject, scene);
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(40, -25, 0);
                var property = typeof(SceneView).GetProperty("customScene", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var maskProperty = typeof(SceneView).GetProperty("overrideSceneCullingMask", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var getMask = typeof(EditorSceneManager).GetMethod("GetSceneCullingMask", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (property == null || !property.CanWrite || maskProperty == null || !maskProperty.CanWrite || getMask == null)
                    throw new NotSupportedException("This Unity version cannot open the isolated Scene preview.");
                View = ScriptableObject.CreateInstance<SceneView>();
                View.Show();
                View.titleContent = new GUIContent("Clipping Preview");
                property.SetValue(View, scene);
                maskProperty.SetValue(View, getMask.Invoke(null, new object[] { scene }));
                View.sceneLighting = false;
                View.drawGizmos = false;
                View.showGrid = false;
                View.rotation = Quaternion.Euler(10, 180, 0);
                driver = ScriptableObject.CreateInstance<AnimationModeDriver>();
                driver.hideFlags = HideFlags.HideAndDontSave;
                AnimationMode.StartAnimationMode(driver);
                FrameAvatar();
            }
            catch { Dispose(); throw; }
        }

        private void PrepareClothing()
        {
            var callbacks = FindType("VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks");
            var method = callbacks?.GetMethod("OnPreprocessAvatar", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(GameObject) }, null);
            var descriptor = FindType("VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
            if (method == null || descriptor == null || !Root.GetComponent(descriptor))
                throw new InvalidOperationException("Clothing preparation needs the Avatar SDK and an avatar descriptor. Turn it off for a normal preview.");
            string buildError = null;
            Application.LogCallback capture = (message, stack, type) => { if (type == LogType.Error || type == LogType.Exception) buildError = message; };
            Application.logMessageReceived += capture;
            try
            {
                var optimizerType = FindType("d4rkAvatarOptimizer");
                if (optimizerType != null)
                {
                    var optimizers = Root.GetComponentsInChildren(optimizerType, true);
                    if (optimizers.Length == 0) optimizers = new[] { Root.AddComponent(optimizerType) };
                    foreach (var optimizer in optimizers)
                        optimizerType.GetProperty("ApplyOnUpload")?.SetValue(optimizer, false);
                }
                if (!(bool)method.Invoke(null, new object[] { Root }) || buildError != null)
                    throw new InvalidOperationException("Clothing preparation failed. " + (buildError ?? "Check the Unity Console."));
                ClothingStatus = "Clothing prepared on preview copy (installed avatar build tools)";
            }
            catch (TargetInvocationException e) { throw new InvalidOperationException("Clothing preparation failed: " + e.InnerException?.Message, e.InnerException); }
            finally { Application.logMessageReceived -= capture; TrackGeneratedAssets(); }
        }

        private void TrackGeneratedAssets()
        {
            if (!Root) return;
            foreach (var r in Root.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var m in r.sharedMaterials) Track(m);
                if (r is SkinnedMeshRenderer skinned) Track(skinned.sharedMesh);
                else if (r.TryGetComponent<MeshFilter>(out var filter)) Track(filter.sharedMesh);
            }
            foreach (var a in Root.GetComponentsInChildren<Animator>(true))
            { Track(a.avatar); Track(a.runtimeAnimatorController); }
        }
        private void Protect(Object asset) { if (asset) sourceAssets.Add(asset.GetInstanceID()); }
        public Renderer PreviewRenderer(Renderer original) => original && rendererCopies.TryGetValue(original, out var copy) && copy ? copy : null;
        public Renderer OriginalRenderer(Renderer copy) => copy ? rendererCopies.FirstOrDefault(pair => pair.Value == copy).Key : null;
        private void Track(Object asset) { if (asset && !sourceAssets.Contains(asset.GetInstanceID()) && !EditorUtility.IsPersistent(asset) && !owned.Contains(asset)) owned.Add(asset); }

        public void Sample(AnimationClip clip, float time, float physicsDelta = 0)
        {
            if (!IsValid) throw new InvalidOperationException("Preview closed or animation preview ownership changed. Press Play to restart.");
            if (!clip) throw new InvalidOperationException("Choose an animation clip.");
            var animator = Root.GetComponent<Animator>();
            if (clip.humanMotion && (!animator || !animator.avatar || !animator.avatar.isValid || !animator.avatar.isHuman))
                throw new InvalidOperationException("This humanoid clip needs a valid Humanoid Animator on the avatar root.");
            physics?.BeforeSample(physicsDelta <= 0 || clip != lastClip || time < lastTime);
            if (lastClip != clip)
            {
                AnimationMode.StopAnimationMode(driver);
                foreach (var entry in rest) if (entry.Key)
                { entry.Key.SetLocalPositionAndRotation(entry.Value.position, entry.Value.rotation); entry.Key.localScale = scales[entry.Key]; }
                AnimationMode.StartAnimationMode(driver);
                lastClip = clip;
            }
            AnimationMode.BeginSampling();
            try { AnimationMode.SampleAnimationClip(Root, clip, Mathf.Clamp(time, 0, clip.length)); }
            finally { AnimationMode.EndSampling(); }
            lastTime = time;
            if (physicsDelta > 0) physics?.Step(physicsDelta);
            View.Repaint();
        }

        public void SetContrast(Renderer body, Renderer clothing, bool enabled, int bodySlot = -1, int clothingSlot = -1)
            => SetContrast(new[] { new MeshSurface(body, bodySlot) }, new[] { new MeshSurface(clothing, clothingSlot) }, enabled);

        public void SetContrast(IEnumerable<MeshSurface> body, IEnumerable<MeshSurface> clothing, bool enabled)
        {
            RestoreMaterials();
            if (!enabled) return;
            var bodies = MeshSurface.Normalize(body); var clothes = MeshSurface.Normalize(clothing);
            if (!MeshSurface.ValidPair(bodies, clothes)) throw new InvalidOperationException("Choose separate body and clothing surfaces before enabling contrast.");
            if (!bodyMaterial)
            {
                var shader = Shader.Find("Unlit/Color");
                if (!shader) throw new InvalidOperationException("The contrast shader is unavailable.");
                bodyMaterial = new Material(shader) { color = new Color(0.15f, 0.8f, 1f), hideFlags = HideFlags.HideAndDontSave };
                clothingMaterial = new Material(shader) { color = new Color(1f, 0.65f, 0.1f), hideFlags = HideFlags.HideAndDontSave };
                owned.Add(bodyMaterial); owned.Add(clothingMaterial);
            }
            foreach (var renderer in bodies.Concat(clothes).Select(s => s.Renderer).Distinct()) if (renderer)
            {
                materials[renderer] = renderer.sharedMaterials;
                var assigned = renderer.sharedMaterials;
                for (int slot = 0; slot < assigned.Length; slot++)
                {
                    if (bodies.Any(s => s.Renderer == renderer && (s.Slot < 0 || slot == s.Slot))) assigned[slot] = bodyMaterial;
                    if (clothes.Any(s => s.Renderer == renderer && (s.Slot < 0 || slot == s.Slot))) assigned[slot] = clothingMaterial;
                }
                renderer.sharedMaterials = assigned;
            }
            View?.Repaint();
        }
        private void RestoreMaterials()
        {
            foreach (var entry in materials) if (entry.Key) entry.Key.sharedMaterials = entry.Value;
            materials.Clear();
        }
        public void FrameAvatar()
        {
            if (!View || !Root) return;
            var renderers = Renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
            var bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(Root.transform.position, Vector3.one);
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            View.Frame(bounds, true);
        }
        public void SetWireframe(bool enabled)
        {
            if (!View) return;
            View.cameraMode = SceneView.GetBuiltinCameraMode(enabled ? DrawCameraMode.TexturedWire : DrawCameraMode.Textured);
            View.Repaint();
        }
        public void Dispose()
        {
            physics?.Dispose(); physics = null;
            if (driver && AnimationMode.InAnimationMode(driver)) AnimationMode.StopAnimationMode(driver);
            if (driver) Object.DestroyImmediate(driver);
            driver = null;
            RestoreMaterials();
            if (View) View.Close();
            View = null;
            if (scene.IsValid()) EditorSceneManager.ClosePreviewScene(scene);
            scene = default;
            Root = null;
            foreach (var asset in owned) if (asset && !EditorUtility.IsPersistent(asset)) Object.DestroyImmediate(asset);
            owned.Clear(); sourceAssets.Clear(); rendererCopies.Clear(); rest.Clear(); scales.Clear(); lastClip = null;
            bodyMaterial = clothingMaterial = null;
        }
    }
}
