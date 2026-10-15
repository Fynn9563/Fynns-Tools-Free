#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.VRCSDKPlus
{
    [InitializeOnLoad]
    internal static class SdkCapturePose
    {
        static bool enabled, ownsMode;
        static AnimationClip clip;
        static float time;
        static GameObject sampledAvatar;
        static GameObject previewSource, previewAvatar, failedSource;
        static readonly Dictionary<Renderer, bool> originalVisibility = new Dictionary<Renderer, bool>();
        static readonly Dictionary<GameObject, GameObject> previewObjects = new Dictionary<GameObject, GameObject>();
        static VisualElement modal, root;
        static Label status;

        static SdkCapturePose()
        {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Restore;
            EditorApplication.quitting += Restore;
            EditorApplication.playModeStateChanged += _ => Restore();
        }

        internal static void Attach(VisualElement windowRoot, VisualElement settings)
        {
            if (settings.Q("fynn-capture-pose") != null) return;
            root = windowRoot;
            modal = settings;
            while (modal != null && modal.GetType().Name != "Modal") modal = modal.parent;
            var section = new VisualElement { name = "fynn-capture-pose" };
            section.style.flexShrink = 0;
            var toggle = new Toggle("Animation Pose") { value = enabled };
            var controls = new VisualElement();
            controls.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            var picker = new UnityEditor.UIElements.ObjectField
            {
                objectType = typeof(AnimationClip), allowSceneObjects = false,
                tooltip = "Animation to sample on the avatar selected in the SDK."
            };
            picker.SetValueWithoutNotify(clip);
            var slider = new Slider(0, clip ? clip.length : 1) { showInputField = true, value = time, tooltip = "Pose time in seconds" };
            status = new Label();
            status.style.whiteSpace = WhiteSpace.Normal;
            controls.Add(picker);
            controls.Add(slider);
            controls.Add(status);
            section.Add(toggle);
            section.Add(controls);
            settings.Add(section);
            toggle.RegisterValueChangedCallback(evt =>
            {
                enabled = evt.newValue;
                failedSource = null;
                controls.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
                if (!enabled) Restore(); else Update();
            });
            picker.RegisterValueChangedCallback(evt =>
            {
                ResetSampling();
                clip = evt.newValue as AnimationClip;
                time = 0;
                slider.highValue = clip ? clip.length : 1;
                slider.SetValueWithoutNotify(0);
                Update();
            });
            slider.RegisterValueChangedCallback(evt => { time = evt.newValue; Update(); });
            section.RegisterCallback<DetachFromPanelEvent>(_ => Restore());
        }

        static GameObject SelectedAvatar()
        {
            if (root == null) return null;
            GameObject selected = null;
            root.Query<VisualElement>().ForEach(element =>
            {
                if (element.GetType().Name != "AvatarSelector") return;
                var property = element.GetType().GetProperty("SelectedAvatar");
                var component = property == null ? null : property.GetValue(element, null) as Component;
                if (component) selected = component.gameObject;
            });
            return selected;
        }

        static void Update()
        {
            bool open = modal != null && modal.panel != null && !modal.ClassListContains("d-none");
            if (!SdkPanelAppearance.LayoutEnabled || !enabled || !open || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Restore();
                if (status != null) status.text = enabled && !clip ? "Choose an animation." : "";
                return;
            }
            var avatar = SelectedAvatar();
            if (sampledAvatar != avatar) Restore();
            if (!avatar) { if (status != null) status.text = "Select an avatar in the SDK."; return; }
            if (!clip)
            {
                ResetSampling();
                if (status != null) status.text = "Choose an animation.";
                return;
            }
            if (!ownsMode && AnimationMode.InAnimationMode())
            {
                if (status != null) status.text = "Close the other animation preview first.";
                return;
            }
            var target = PrepareAvatar(avatar);
            if (!target) return;
            if (!ownsMode) { AnimationMode.StartAnimationMode(); ownsMode = true; }
            sampledAvatar = avatar;
            SampleAvatar(target);
            if (status != null) status.text = "";
            SceneView.RepaintAll();
        }

        static void SampleAvatar(GameObject avatar)
        {
            AnimationMode.BeginSampling();
            try { AnimationMode.SampleAnimationClip(avatar, clip, Mathf.Clamp(time, 0, clip.length)); }
            finally { AnimationMode.EndSampling(); }
        }

        static void ResetSampling()
        {
            if (ownsMode && AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
            ownsMode = false;
        }

        internal static void Restore()
        {
            ResetSampling();
            sampledAvatar = null;
            if (previewAvatar) UnityEngine.Object.DestroyImmediate(previewAvatar);
            previewAvatar = previewSource = null;
            foreach (var pair in originalVisibility) if (pair.Key) pair.Key.forceRenderingOff = pair.Value;
            originalVisibility.Clear();
            previewObjects.Clear();
        }

        static GameObject PrepareAvatar(GameObject source)
        {
            if (previewSource == source && previewAvatar) return previewAvatar;
            bool hasAttachments = source.GetComponentsInChildren<Component>(true).Any(component => component &&
                (component.GetType().Name == "VRCFury" ||
                 (component.GetType().Namespace ?? "").StartsWith("nadena.dev.modular_avatar", StringComparison.Ordinal)));
            if (!hasAttachments) return source;
            if (failedSource == source) return null;
            try
            {
                previewSource = source;
                previewAvatar = UnityEngine.Object.Instantiate(source, source.transform.parent);
                previewAvatar.name = source.name + "(Clone)";
                foreach (var transform in source.GetComponentsInChildren<Transform>(true))
                {
                    var path = AnimationUtility.CalculateTransformPath(transform, source.transform);
                    var copy = path.Length == 0 ? previewAvatar.transform : previewAvatar.transform.Find(path);
                    if (copy) previewObjects[transform.gameObject] = copy.gameObject;
                }
                var pipeline = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType("VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks"))
                    .FirstOrDefault(type => type != null);
                var process = pipeline == null ? null : pipeline.GetMethod("OnPreprocessAvatar", new[] { typeof(GameObject) });
                if (process == null || !(bool)process.Invoke(null, new object[] { previewAvatar }))
                    throw new InvalidOperationException("Avatar attachment processing did not complete.");
                // Hide processed copies from the SDK avatar selector.
                foreach (var component in previewAvatar.GetComponents<Component>())
                    if (component && component.GetType().Name == "VRCAvatarDescriptor") UnityEngine.Object.DestroyImmediate(component);
                foreach (var transform in previewAvatar.GetComponentsInChildren<Transform>(true))
                    transform.gameObject.hideFlags = HideFlags.HideAndDontSave;
                foreach (var renderer in source.GetComponentsInChildren<Renderer>(true))
                {
                    originalVisibility[renderer] = renderer.forceRenderingOff;
                    renderer.forceRenderingOff = true;
                }
                return previewAvatar;
            }
            catch (Exception exception)
            {
                Restore();
                failedSource = source;
                if (status != null) status.text = "Could not prepare outfits. Turn Animation Pose off and on to retry.";
                Debug.LogWarning("Capture pose: " + (exception.InnerException ?? exception).Message);
                return null;
            }
        }

        internal static IEnumerable<Renderer> IgnoredRenderers(GameObject item)
        {
            foreach (var renderer in item.GetComponentsInChildren<Renderer>(true)) yield return renderer;
            if (!previewAvatar) yield break;
            foreach (var pair in previewObjects)
                if (pair.Key && pair.Value && (pair.Key == item || pair.Key.transform.IsChildOf(item.transform)))
                    foreach (var renderer in pair.Value.GetComponentsInChildren<Renderer>(true)) yield return renderer;
        }
    }
}
#endif
