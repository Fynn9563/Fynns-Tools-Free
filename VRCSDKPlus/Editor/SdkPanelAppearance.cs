#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.VRCSDKPlus
{
    [InitializeOnLoad]
    internal static class SdkPanelAppearance
    {
        const string LayoutPreference = "Fynn.SDKPlus.PanelLayout";
        internal static bool LayoutEnabled { get { return EditorPrefs.GetBool(LayoutPreference, true); } }
        static double nextCheck;
        static StyleSheet sheet;
        static Type sdkPanelType;
        static readonly HashSet<int> nativeStylesInitialized = new HashSet<int>();
        static readonly Dictionary<int, GUISkin> skins = new Dictionary<int, GUISkin>();
        static readonly ConditionalWeakTable<IMGUIContainer, object> wrapped = new ConditionalWeakTable<IMGUIContainer, object>();
        static readonly Dictionary<Color, Texture2D> backgrounds = new Dictionary<Color, Texture2D>();
        static SdkPanelAppearance()
        {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
        }

        static void Cleanup()
        {
            foreach (var skin in skins.Values) if (skin) UnityEngine.Object.DestroyImmediate(skin);
            skins.Clear();
            foreach (var texture in backgrounds.Values) if (texture) UnityEngine.Object.DestroyImmediate(texture);
            backgrounds.Clear();
        }

        static void Update()
        {
            if (EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + 1;
            if (!sheet) sheet = Resources.Load<StyleSheet>("FynnSdkPanelAppearance");
            if (!sheet) return;
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window.GetType().FullName != "VRCSdkControlPanel") continue;
                sdkPanelType = window.GetType();
                var root = window.rootVisualElement;
                ApplyLayout(root, LayoutEnabled);
                if (LayoutEnabled) SdkCaptureIgnore.Attach(root);
                else SdkCaptureIgnore.RestoreInjected(root);
                root.Query<Button>().ForEach(button =>
                {
                    if (button.name == "avatar-selector-options" || button.name == "modal-close-btn")
                    {
                        button.RemoveFromClassList("fynn-sdk-neutral-button");
                        return;
                    }
                    if (button.GetType().Name == "ToolbarButton" || button.ClassListContains("help-button")) return;
                    var color = button.resolvedStyle.backgroundColor;
                    if (color.a > 0 && Mathf.Max(color.r, Mathf.Max(color.g, color.b)) - Mathf.Min(color.r, Mathf.Min(color.g, color.b)) > 0.08f) return;
                    button.AddToClassList("fynn-sdk-neutral-button");
                });
                root.Query<IMGUIContainer>().ForEach(container =>
                {
                    bool contentHost = container.parent != null && container.parent.name == "sdk-container";
                    var contentTab = root.Q("tab-content-manager");
                    container.EnableInClassList("fynn-content-host", LayoutEnabled && contentHost && contentTab != null && contentTab.ClassListContains("active"));
                    object marker;
                    if (wrapped.TryGetValue(container, out marker) || container.onGUIHandler == null) return;
                    var original = container.onGUIHandler;
                    container.onGUIHandler = () =>
                    {
                        // Initialize cached SDK styles from the native skin.
                        if (contentHost) EnsureNativeStyles(window);
                        Action contents = () =>
                        {
                            if (contentHost) DrawLayoutSetting(window);
                            if (!contentHost || !LayoutEnabled || !SdkContentManager.TryDraw(window)) original();
                        };
                        if (LayoutEnabled) Draw(contents); else contents();
                    };
                    wrapped.Add(container, new object());
                });
            }
        }

        internal static void SetLayoutEnabled(bool enabled)
        {
            EditorPrefs.SetBool(LayoutPreference, enabled);
            nativeStylesInitialized.Clear();
            if (!enabled)
            {
                SdkCaptureIgnore.RestoreAll();
                SdkCapturePose.Restore();
                SdkCaptureBackground.Cleanup();
            }
            SdkPlusRegistration.Refresh();
            nextCheck = 0;
            Update();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        static void EnsureNativeStyles(EditorWindow window)
        {
            int id = window.GetInstanceID();
            if (nativeStylesInitialized.Contains(id)) return;
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var initialize = window.GetType().GetMethod("InitializeStyles", flags);
            if (initialize == null) return;
            initialize.Invoke(window, null);
            var initialized = window.GetType().GetField("_stylesInitialized", flags);
            if (initialized != null) initialized.SetValue(window, true);
            nativeStylesInitialized.Add(id);
        }

        static void DrawLayoutSetting(EditorWindow window)
        {
            var tab = window.rootVisualElement.Q("tab-settings");
            if (tab == null || !tab.ClassListContains("active")) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(504)))
                {
                    GUILayout.Label("VRCSDK+", EditorStyles.boldLabel);
                    bool enabled = EditorGUILayout.ToggleLeft(new GUIContent("Enable VRCSDK+ layout", "Enable VRCSDK+ enhancements. Turn off to restore standard SDK panels, capture tools and inspectors."), LayoutEnabled);
                    if (enabled != LayoutEnabled) EditorApplication.delayCall += () => SetLayoutEnabled(enabled);
                    EditorGUILayout.Space(6);
                    GUILayout.Label("SDK Patches", EditorStyles.boldLabel);
                    foreach (var patch in VRCSdkPatchRunner.Patches)
                    {
                        patch.Refresh();
                        bool patchEnabled = VRCSdkPatchesSettings.IsEnabled(patch.Id);
                        using (new EditorGUI.DisabledScope(!patch.IsAvailable))
                        {
                            bool wanted = EditorGUILayout.ToggleLeft(new GUIContent(patch.Title, patch.Description), patchEnabled);
                            if (wanted != patchEnabled)
                            {
                                VRCSdkPatchesSettings.SetEnabled(patch.Id, wanted);
                                VRCSdkPatchRunner.Sync(patch);
                            }
                        }
                        if (!patch.IsAvailable) EditorGUILayout.HelpBox(patch.UnavailableReason, MessageType.Info);
                    }
                }
                GUILayout.FlexibleSpace();
            }
        }

        internal static void ApplyLayout(VisualElement root, bool enabled)
        {
            root.EnableInClassList("fynn-sdk-panel", enabled);
            if (enabled)
            {
                if (sheet && !root.styleSheets.Contains(sheet)) root.styleSheets.Add(sheet);
                EnsureHorizontalViewport(root);
            }
            else
            {
                if (sheet && root.styleSheets.Contains(sheet)) root.styleSheets.Remove(sheet);
                var viewport = root.Q<ScrollView>("fynn-sdk-viewport");
                var content = root.Q<VisualElement>("root");
                if (viewport != null && content != null)
                {
                    int index = root.IndexOf(viewport);
                    content.RemoveFromHierarchy();
                    viewport.RemoveFromHierarchy();
                    root.Insert(Mathf.Max(0, index), content);
                    content.style.width = StyleKeyword.Null;
                    content.style.height = StyleKeyword.Null;
                    content.style.flexShrink = StyleKeyword.Null;
                }
                root.Query<IMGUIContainer>().ForEach(container => container.RemoveFromClassList("fynn-content-host"));
            }
        }

        static void EnsureHorizontalViewport(VisualElement root)
        {
            var content = root.Q<VisualElement>("root");
            if (content == null || content.parent is ScrollView) return;
            var existing = root.Q<ScrollView>("fynn-sdk-viewport");
            if (existing != null && content.parent == existing.contentContainer) return;
            var viewport = new ScrollView(ScrollViewMode.Horizontal)
            {
                name = "fynn-sdk-viewport",
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Hidden
            };
            viewport.style.flexGrow = 1;
            viewport.style.minWidth = 0;
            viewport.contentContainer.style.flexGrow = 1;
            viewport.contentContainer.style.minWidth = Length.Percent(100);
            content.RemoveFromHierarchy();
            viewport.Add(content);
            root.Add(viewport);
            viewport.contentViewport.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (LayoutEnabled && content.parent == viewport.contentContainer)
                    ApplyViewportSize(content, evt.newRect.width, evt.newRect.height);
            });
        }

        internal static void ApplyViewportSize(VisualElement content, float width, float height)
        {
            content.style.width = Mathf.Max(520, width);
            content.style.height = Mathf.Max(0, height);
            content.style.flexShrink = 0;
        }

        static void Draw(Action original)
        {
            var previous = GUI.skin;
            var restore = new List<Action>();
            try
            {
                GUISkin skin;
                int key = previous.GetInstanceID();
                if (!skins.TryGetValue(key, out skin) || !skin)
                {
                    skin = UnityEngine.Object.Instantiate(previous);
                    skin.hideFlags = HideFlags.HideAndDontSave;
                    skin.button = new GUIStyle(previous.button)
                    {
                        padding = new RectOffset(9, 9, 4, 4),
                        margin = new RectOffset(3, 3, 3, 3)
                    };
                    skins[key] = skin;
                }
                GUI.skin = skin;
                ApplyInnerColors(skin, restore);
                original();
            }
            finally
            {
                for (int i = restore.Count - 1; i >= 0; i--) restore[i]();
                GUI.skin = previous;
            }
        }

        // Restore shared styles after each SDK callback, including GUI exits.
        static void ApplyInnerColors(GUISkin skin, List<Action> restore)
        {
            bool dark = EditorGUIUtility.isProSkin;
            Color panel = dark ? new Color32(25, 54, 63, 255) : new Color32(218, 237, 242, 255);
            Color control = dark ? new Color32(32, 69, 76, 255) : new Color32(188, 222, 230, 255);
            Color hover = dark ? new Color32(39, 91, 103, 255) : new Color32(163, 208, 219, 255);
            var seen = new HashSet<GUIStyle>();
            Action<GUIStyle, Color> paint = (style, color) =>
            {
                if (style == null || !seen.Add(style)) return;
                // Copy border values; RectOffset aliases the style's live border.
                var border = new RectOffset(style.border.left, style.border.right, style.border.top, style.border.bottom);
                restore.Add(() => style.border = border);
                style.border = new RectOffset(6, 6, 6, 6);
                PaintState(style.normal, color, restore);
                PaintState(style.hover, hover, restore);
                PaintState(style.active, hover, restore);
                PaintState(style.focused, hover, restore);
                PaintState(style.onNormal, hover, restore);
                PaintState(style.onHover, hover, restore);
                PaintState(style.onActive, hover, restore);
                PaintState(style.onFocused, hover, restore);
            };
            paint(skin.button, control);
            paint(skin.box, panel);
            paint(skin.window, panel);
            paint(skin.textField, control);
            paint(skin.textArea, control);
            paint(EditorStyles.popup, control);
            paint(EditorStyles.textField, control);
            paint(EditorStyles.textArea, control);
            paint(EditorStyles.objectField, control);
            paint(EditorStyles.helpBox, panel);
            var sdk = sdkPanelType;
            foreach (var name in new[] { "boxGuiStyle", "accountWindowStyle", "searchBarStyle", "listButtonStyleEven", "listButtonStyleOdd", "listButtonStyleSelected" })
            {
                var field = sdk == null ? null : sdk.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (field != null) paint(field.GetValue(null) as GUIStyle, panel);
            }
        }

        static void PaintState(GUIStyleState state, Color color, List<Action> restore)
        {
            var previous = state.background;
            var scaled = state.scaledBackgrounds;
            restore.Add(() => { state.background = previous; state.scaledBackgrounds = scaled; });
            Texture2D texture;
            if (!backgrounds.TryGetValue(color, out texture) || !texture)
            {
                texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
                {
                    name = "SDK inner surface",
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp
                };
                for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float dx = Mathf.Max(0, Mathf.Abs(x + 0.5f - 8) - 2);
                    float dy = Mathf.Max(0, Mathf.Abs(y + 0.5f - 8) - 2);
                    var pixel = color;
                    pixel.a *= Mathf.Clamp01(6.5f - Mathf.Sqrt(dx * dx + dy * dy));
                    texture.SetPixel(x, y, pixel);
                }
                texture.Apply();
                backgrounds[color] = texture;
            }
            state.background = texture;
            state.scaledBackgrounds = new Texture2D[0];
        }
    }
}
#endif
