#if VRC_SDK_VRCSDK3 && !UDON
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.VRCSDKPlus
{
    [InitializeOnLoad]
    internal static class SdkCaptureIgnore
    {
        internal static readonly List<GameObject> Objects = new List<GameObject>();
        sealed class RendererState { internal bool original; internal int users; }
        static readonly Dictionary<Camera, HashSet<Renderer>> hidden = new Dictionary<Camera, HashSet<Renderer>>();
        static readonly Dictionary<Renderer, RendererState> states = new Dictionary<Renderer, RendererState>();
        static string SessionKey => "Fynn.SDKPlus.CaptureIgnore." + Application.dataPath;
        static RenderTexture target;
        internal static bool IgnoreEnabled;
        sealed class Injection
        {
            internal VisualElement root;
            internal readonly List<System.Action> undo = new List<System.Action>();
        }
        static readonly Dictionary<VisualElement, Injection> injected = new Dictionary<VisualElement, Injection>();
        static SdkCaptureIgnore()
        {
            IgnoreEnabled = SessionState.GetBool(SessionKey + ".Enabled", false);
            foreach (var value in SessionState.GetString(SessionKey, "").Split(','))
            {
                int id;
                if (!int.TryParse(value, out id)) continue;
                var item = EditorUtility.InstanceIDToObject(id) as GameObject;
                if (item && item.scene.IsValid()) Objects.Add(item);
            }
            Camera.onPreCull += BeforeRender;
            Camera.onPostRender += AfterRender;
            EditorApplication.update += RestoreAll;
            AssemblyReloadEvents.beforeAssemblyReload += RestoreAll;
            EditorApplication.quitting += RestoreAll;
        }

        internal static void Attach(VisualElement root)
        {
            if (!SdkPanelAppearance.LayoutEnabled) return;
            root.Query<Toggle>("thumbnail-use-custom-camera-toggle").ForEach(toggle =>
            {
                var parent = toggle.parent;
                if (parent == null) return;
                for (var ancestor = parent; ancestor != null; ancestor = ancestor.parent)
                    if (ancestor.GetType().Name == "Modal")
                    {
                        CaptureOriginal(root, ancestor);
                        ancestor.AddToClassList("fynn-capture-modal");
                        var preview = ancestor.Q<IMGUIContainer>("thumbnail-capture-preview");
                        if (preview != null)
                        {
                            // Reserve the full 192x144 native preview size.
                            preview.style.width = 192;
                            preview.style.height = 144;
                            preview.style.minHeight = 144;
                            preview.style.flexShrink = 0;
                            preview.parent.style.alignSelf = Align.FlexStart;
                            preview.parent.style.marginTop = 0;
                            var contentRow = preview.parent.parent;
                            if (ancestor.Q("fynn-capture-settings-scroll") == null)
                            {
                                var body = contentRow.parent;
                                int index = body.IndexOf(contentRow);
                                var scroll = new ScrollView(ScrollViewMode.Vertical) { name = "fynn-capture-settings-scroll" };
                                scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                                scroll.style.minHeight = 0;
                                scroll.style.flexShrink = 1;
                                body.style.minHeight = 0;
                                body.style.flexShrink = 1;
                                contentRow.style.flexShrink = 0;
                                contentRow.RemoveFromHierarchy();
                                scroll.Add(contentRow);
                                body.Insert(index, scroll);
                            }
                        }
                        // Anchor outside the clipped SDK content area.
                        if (root.panel != null && ancestor.parent != root)
                        {
                            var anchor = ancestor.GetType().GetMethod("SetAnchor");
                            if (anchor != null) anchor.Invoke(ancestor, new object[] { root });
                            var container = ancestor.Q("modal-container");
                            if (container != null)
                            {
                                System.Action position = () =>
                                {
                                    float top = Mathf.Min(160, Mathf.Max(48, root.contentRect.height * 0.17f));
                                    container.style.marginTop = top;
                                    container.style.maxHeight = Mathf.Max(100, root.contentRect.height - top - 12);
                                };
                                EventCallback<GeometryChangedEvent> onGeometry = _ => position();
                                root.RegisterCallback(onGeometry);
                                injected[ancestor].undo.Add(() => root.UnregisterCallback(onGeometry));
                                position();
                            }
                        }
                        break;
                    }
                if (parent.Q("fynn-capture-ignore") != null) return;
                var section = new VisualElement { name = "fynn-capture-ignore" };
                section.style.height = IgnoreEnabled ? 76 : 20;
                section.style.flexShrink = 0;
                section.tooltip = "Excludes renderers on these scene objects and their children from the SDK thumbnail preview and capture. Scene visibility stays unchanged.";
                var enable = new Toggle("Ignore Objects") { name = "fynn-capture-ignore-enabled", value = IgnoreEnabled };
                section.Add(enable);
                var picker = new UnityEditor.UIElements.ObjectField
                {
                    name = "fynn-capture-ignore-picker", objectType = typeof(GameObject), allowSceneObjects = true,
                    tooltip = "Drop or choose a scene object to ignore, including its children."
                };
                picker.style.flexShrink = 0;
                picker.style.minWidth = 0;
                var list = new ScrollView(ScrollViewMode.Vertical) { name = "fynn-capture-ignore-list" };
                list.style.flexGrow = 1;
                list.style.minHeight = 0;
                list.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                System.Action rebuild = null;
                rebuild = () =>
                {
                    list.Clear();
                    foreach (var item in Objects.Where(o => o).ToArray())
                    {
                        var row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.flexShrink = 0;
                        var field = new UnityEditor.UIElements.ObjectField { objectType = typeof(GameObject), allowSceneObjects = true };
                        field.SetValueWithoutNotify(item);
                        field.style.flexGrow = 1;
                        field.style.flexBasis = 0;
                        field.style.minWidth = 0;
                        field.RegisterValueChangedCallback(evt =>
                        {
                            var replacement = evt.newValue as GameObject;
                            int index = Objects.IndexOf(item);
                            if (index >= 0 && (!replacement || (replacement.scene.IsValid() && !Objects.Contains(replacement))))
                            {
                                if (replacement) Objects[index] = replacement; else Objects.RemoveAt(index);
                                Save();
                            }
                            rebuild();
                        });
                        var remove = new Button(() => { Objects.Remove(item); Save(); rebuild(); }) { text = "×", tooltip = "Stop ignoring this object" };
                        remove.style.width = 22;
                        remove.style.minWidth = 22;
                        remove.style.flexShrink = 0;
                        row.Add(field);
                        row.Add(remove);
                        list.Add(row);
                    }
                };
                picker.RegisterValueChangedCallback(evt =>
                {
                    var item = evt.newValue as GameObject;
                    if (item && item.scene.IsValid() && !Objects.Contains(item)) { Objects.Add(item); Save(); rebuild(); }
                    picker.SetValueWithoutNotify(null);
                });
                section.Add(picker);
                section.Add(list);
                System.Action<bool> show = value =>
                {
                    section.style.height = value ? 76 : 20;
                    picker.style.display = list.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                };
                show(IgnoreEnabled);
                enable.RegisterValueChangedCallback(evt =>
                {
                    IgnoreEnabled = evt.newValue;
                    SessionState.SetBool(SessionKey + ".Enabled", IgnoreEnabled);
                    show(IgnoreEnabled);
                });
                rebuild();
                parent.Add(section);
                SdkCapturePose.Attach(root, parent);
                SdkCaptureBackground.Attach(parent);
            });
        }

        static void CaptureOriginal(VisualElement root, VisualElement modal)
        {
            if (injected.ContainsKey(modal)) return;
            var state = new Injection { root = root };
            injected.Add(modal, state);
            var parent = modal.parent;
            var originalIndex = parent == null ? 0 : parent.IndexOf(modal);
            var anchor = modal.GetType().GetField("_anchor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var oldAnchor = anchor?.GetValue(modal);
            state.undo.Add(() =>
            {
                modal.RemoveFromClassList("fynn-capture-modal");
                anchor?.SetValue(modal, oldAnchor);
                if (parent != null && modal.parent != parent)
                {
                    modal.RemoveFromHierarchy();
                    parent.Insert(Mathf.Min(originalIndex, parent.childCount), modal);
                }
            });
            var preview = modal.Q<IMGUIContainer>("thumbnail-capture-preview");
            if (preview != null)
            {
                var width = preview.style.width; var height = preview.style.height;
                var minHeight = preview.style.minHeight; var shrink = preview.style.flexShrink;
                state.undo.Add(() => { preview.style.width = width; preview.style.height = height; preview.style.minHeight = minHeight; preview.style.flexShrink = shrink; });
                var previewParent = preview.parent;
                var align = previewParent.style.alignSelf; var margin = previewParent.style.marginTop;
                state.undo.Add(() => { previewParent.style.alignSelf = align; previewParent.style.marginTop = margin; });
                var row = previewParent.parent; var body = row.parent; int index = body.IndexOf(row);
                var bodyMin = body.style.minHeight; var bodyShrink = body.style.flexShrink; var rowShrink = row.style.flexShrink;
                state.undo.Add(() =>
                {
                    var scroll = modal.Q("fynn-capture-settings-scroll");
                    if (scroll != null) { row.RemoveFromHierarchy(); scroll.RemoveFromHierarchy(); body.Insert(Mathf.Min(index, body.childCount), row); }
                    body.style.minHeight = bodyMin; body.style.flexShrink = bodyShrink; row.style.flexShrink = rowShrink;
                });
            }
            var container = modal.Q("modal-container");
            if (container != null)
            {
                var margin = container.style.marginTop; var maxHeight = container.style.maxHeight;
                state.undo.Add(() => { container.style.marginTop = margin; container.style.maxHeight = maxHeight; });
            }
        }

        internal static void RestoreInjected(VisualElement root)
        {
            RestoreAll();
            foreach (var pair in injected.Where(pair => pair.Value.root == root).ToArray())
            {
                var modal = pair.Key;
                modal.GetType().GetMethod("Close")?.Invoke(modal, null);
                foreach (var name in new[] { "fynn-capture-ignore", "fynn-capture-pose", "fynn-capture-background" })
                    modal.Q(name)?.RemoveFromHierarchy();
                for (int i = pair.Value.undo.Count - 1; i >= 0; i--) pair.Value.undo[i]();
                injected.Remove(modal);
            }
        }

        static void Save() => SessionState.SetString(SessionKey, string.Join(",", Objects.Where(o => o).Select(o => o.GetInstanceID().ToString())));

        static void BeforeRender(Camera camera)
        {
            if (!target) target = Resources.Load<RenderTexture>("ThumbnailCapture");
            if (!camera || !target || camera.targetTexture != target) return;
            Begin(camera);
        }

        internal static void Begin(Camera camera)
        {
            if (!SdkPanelAppearance.LayoutEnabled || !IgnoreEnabled || !camera || hidden.ContainsKey(camera)) return;
            var snapshot = new HashSet<Renderer>();
            hidden.Add(camera, snapshot);
            foreach (var item in Objects.Where(o => o && o.scene.IsValid()))
                foreach (var renderer in SdkCapturePose.IgnoredRenderers(item))
                {
                    if (!snapshot.Add(renderer)) continue;
                    RendererState state;
                    if (!states.TryGetValue(renderer, out state))
                    {
                        state = new RendererState { original = renderer.forceRenderingOff };
                        states.Add(renderer, state);
                    }
                    state.users++;
                    renderer.forceRenderingOff = true;
                }
        }

        static void AfterRender(Camera camera) { End(camera); }
        internal static void End(Camera camera)
        {
            HashSet<Renderer> snapshot;
            if (!hidden.TryGetValue(camera, out snapshot)) return;
            foreach (var renderer in snapshot)
            {
                var state = states[renderer];
                if (--state.users != 0) continue;
                if (renderer) renderer.forceRenderingOff = state.original;
                states.Remove(renderer);
            }
            hidden.Remove(camera);
        }

        internal static void RestoreAll()
        {
            foreach (var camera in hidden.Keys.ToArray()) End(camera);
        }
    }
}
#endif
