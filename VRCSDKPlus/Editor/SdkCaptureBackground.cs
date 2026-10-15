#if VRC_SDK_VRCSDK3 && !UDON
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.VRCSDKPlus
{
    [InitializeOnLoad]
    internal static class SdkCaptureBackground
    {
        static bool enabled;
        static Texture2D image;
        static RenderTexture target;
        static Material material;
        static Mesh quad;
        struct CameraState { internal CameraClearFlags flags; internal Color color; internal UnityEngine.Rendering.CommandBuffer command; }
        static readonly Dictionary<Camera, CameraState> cameras = new Dictionary<Camera, CameraState>();

        static SdkCaptureBackground()
        {
            Camera.onPreCull += Before;
            Camera.onPostRender += After;
            EditorApplication.update += RestoreCameras;
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            EditorApplication.quitting += Cleanup;
        }

        internal static void Attach(VisualElement settings)
        {
            if (settings.Q("fynn-capture-background") != null) return;
            var section = new VisualElement { name = "fynn-capture-background" };
            section.style.flexShrink = 0;
            var toggle = new Toggle("Image Background") { value = enabled };
            var fill = settings.Q<Toggle>("thumbnail-fill-background-toggle");
            var picker = new UnityEditor.UIElements.ObjectField
            {
                objectType = typeof(Texture2D), allowSceneObjects = false,
                tooltip = "Image to place behind the avatar. Fills the thumbnail, cropping to fit."
            };
            picker.SetValueWithoutNotify(image);
            picker.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            toggle.RegisterValueChangedCallback(evt =>
            {
                enabled = evt.newValue;
                if (enabled && fill != null) fill.value = false;
                picker.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            });
            if (fill != null)
            {
                if (enabled) fill.value = false;
                EventCallback<ChangeEvent<bool>> onFill = evt => { if (evt.newValue) toggle.value = false; };
                fill.RegisterValueChangedCallback(onFill);
                section.RegisterCallback<DetachFromPanelEvent>(_ => fill.UnregisterValueChangedCallback(onFill));
            }
            picker.RegisterValueChangedCallback(evt => image = evt.newValue as Texture2D);
            section.Add(toggle);
            section.Add(picker);
            if (fill != null) settings.Insert(settings.IndexOf(fill) + 1, section);
            else settings.Add(section);
        }

        static void Before(Camera camera)
        {
            if (!SdkPanelAppearance.LayoutEnabled || !enabled || !image || !camera) return;
            if (!target) target = Resources.Load<RenderTexture>("ThumbnailCapture");
            if (!target || camera.targetTexture != target || cameras.ContainsKey(camera)) return;
            if (!material)
            {
                var shader = Resources.Load<Shader>("FynnSdkCaptureBackground");
                if (!shader || !shader.isSupported) return;
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            if (!quad)
            {
                quad = new Mesh { name = "Capture background quad", hideFlags = HideFlags.HideAndDontSave };
                quad.vertices = new[] { new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0) };
                quad.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
                quad.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                quad.UploadMeshData(true);
            }
            material.SetTexture("_Background", image);
            float outputAspect = target.width / (float)target.height;
            float imageAspect = image.width / (float)image.height;
            material.SetVector("_BackgroundScale", new Vector4(Mathf.Min(1, outputAspect / imageAspect), Mathf.Min(1, imageAspect / outputAspect), 0, 0));
            var command = new UnityEngine.Rendering.CommandBuffer { name = "SDK Capture Image Background" };
            command.DrawMesh(quad, Matrix4x4.identity, material);
            cameras.Add(camera, new CameraState { flags = camera.clearFlags, color = camera.backgroundColor, command = command });
            // Draw backgrounds after opaque geometry and before transparency.
            camera.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.BeforeForwardAlpha, command);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
        }

        static void After(Camera camera)
        {
            if (camera && cameras.ContainsKey(camera)) Restore(camera);
        }
        static void Restore(Camera camera)
        {
            CameraState state;
            if (!cameras.TryGetValue(camera, out state)) return;
            if (camera)
            {
                camera.RemoveCommandBuffer(UnityEngine.Rendering.CameraEvent.BeforeForwardAlpha, state.command);
                camera.clearFlags = state.flags;
                camera.backgroundColor = state.color;
            }
            state.command.Release();
            cameras.Remove(camera);
        }

        static void RestoreCameras()
        {
            foreach (var camera in new List<Camera>(cameras.Keys)) Restore(camera);
        }

        internal static void Cleanup()
        {
            RestoreCameras();
            if (material) Object.DestroyImmediate(material);
            if (quad) Object.DestroyImmediate(quad);
        }
    }
}
#endif
