#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FynnsTools.VRCSDKPlus
{
    internal static class SdkContentManager
    {
        const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly ConditionalWeakTable<EditorWindow, State> states = new ConditionalWeakTable<EditorWindow, State>();
        static readonly string[] Categories = { "All", "Avatars", "Worlds", "Local Tests" };
        static readonly string[] Visibility = { "Any visibility", "Public", "Private" };
        static readonly string[] Sorts = { "SDK order", "Name A–Z", "Name Z–A" };
        internal sealed class Entry
        {
            internal string id, name, visibility;
            internal int category;
            internal object model;
        }
        internal sealed class State
        {
            internal string search = "", managedId;
            internal int category, visibility, sort;
            internal Vector2 scroll;
            internal int managedCategory;
            internal Rect listRect, searchRect, clearRect;
            internal string busyKey, confirmDeleteKey, error;
            internal bool loading;
        }

        internal static bool TryDraw(EditorWindow window)
        {
            var state = states.GetValue(window, _ => new State());
            var tab = window.rootVisualElement.Q("tab-content-manager");
            if (tab == null || !tab.ClassListContains("active") || EditorApplication.isPlaying || !VRC.Core.APIUser.IsLoggedIn)
            {
                state.managedId = null;
                return false;
            }
            var type = window.GetType();
            var avatarField = type.GetField("uploadedAvatars", Flags);
            var worldField = type.GetField("uploadedWorlds", Flags);
            var testField = type.GetField("testAvatars", Flags);
            if (avatarField == null || worldField == null || testField == null)
            {
                EditorGUILayout.HelpBox("Content data is unavailable in this SDK version.", MessageType.Warning);
                return true;
            }
            if (avatarField.GetValue(null) == null || worldField.GetValue(null) == null || testField.GetValue(null) == null)
            {
                foreach (var field in new[] { avatarField, worldField, testField })
                    if (field.GetValue(null) == null) field.SetValue(null, Activator.CreateInstance(field.FieldType));
                var fetch = type.GetMethod("FetchUploadedData", BindingFlags.Instance | BindingFlags.NonPublic);
                var coroutine = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("EditorCoroutine")).FirstOrDefault(item => item != null);
                var start = coroutine?.GetMethod("Start", new[] { typeof(IEnumerator) });
                if (fetch != null && start != null) start.Invoke(null, new[] { fetch.Invoke(window, null) });
                else state.error = "Could not start the SDK content loader.";
                state.loading = true;
            }
            else state.loading = type.GetField("fetchingAvatars", Flags)?.GetValue(null) != null ||
                type.GetField("fetchingWorlds", Flags)?.GetValue(null) != null;

            var host = window.rootVisualElement.Q("sdk-container")?.Children().OfType<IMGUIContainer>().FirstOrDefault();
            if (host == null || host.contentRect.width <= 0 || host.contentRect.height <= 0) return true;
            var panel = window.rootVisualElement.Q("root");
            float height = panel != null ? panel.worldBound.yMax - host.worldBound.y - panel.resolvedStyle.paddingBottom : host.contentRect.height;
            return DrawLibrary(window, state, new Rect(0, 0, host.contentRect.width, Mathf.Max(100, height)));
        }

        internal static bool DrawLibrary(EditorWindow window, State state, Rect area)
        {
            var type = window.GetType();
            // Reserve an explicit area; IMGUI ignores the stretched host height.
            using (new GUILayout.AreaScope(area))
            {
            var all = ReadEntries(type);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Your content", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("VRChat website", GUILayout.Width(120), GUILayout.Height(28)))
                    Application.OpenURL("https://vrchat.com/home/avatars");
                using (new EditorGUI.DisabledScope(state.loading))
                if (GUILayout.Button("Refresh", GUILayout.Width(80), GUILayout.Height(28)))
                {
                    // Invalidate after the current IMGUI pass.
                    EditorApplication.delayCall += () => { if (window) { type.GetMethod("ClearContent", Flags)?.Invoke(null, null); window.Repaint(); } };
                }
            }
            GUILayout.Space(8);
            int category = state.category;
            using (new EditorGUILayout.HorizontalScope())
                for (int i = 0; i < Categories.Length; i++)
                    if (GUILayout.Toggle(category == i, Categories[i], GUI.skin.button, GUILayout.Height(30))) category = i;
            GUILayout.Space(8);
            {
                var row = EditorGUILayout.GetControlRect(false, 28);
                var searchRect = new Rect(row.x, row.y, Mathf.Max(0, row.width - 34), row.height);
                var clearRect = new Rect(row.xMax - 28, row.y, 28, row.height);
                string search = GUI.TextField(searchRect, state.search, GUI.skin.textField);
                if (string.IsNullOrEmpty(search) && Event.current.type == EventType.Repaint)
                    GUI.Label(new Rect(searchRect.x + 8, searchRect.y, searchRect.width - 16, searchRect.height), "Search names or content IDs…", EditorStyles.centeredGreyMiniLabel);
                if (search != state.search || category != state.category) state.scroll = Vector2.zero;
                state.search = search;
                if (GUI.Button(clearRect, new GUIContent("×", "Clear search"), GUI.skin.button)) { state.search = ""; state.scroll = Vector2.zero; GUI.FocusControl(null); }
                if (Event.current.type == EventType.Repaint) { state.searchRect = searchRect; state.clearRect = clearRect; }
            }
            state.category = category;
            GUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                int visibility = EditorGUILayout.Popup(state.visibility, Visibility, GUILayout.Height(26));
                GUILayout.Space(8);
                int sort = EditorGUILayout.Popup(state.sort, Sorts, GUILayout.Height(26));
                if (visibility != state.visibility || sort != state.sort) state.scroll = Vector2.zero;
                state.visibility = visibility;
                state.sort = sort;
            }
            var entries = Filter(all, state.search, state.category, state.visibility, state.sort).ToList();
            GUILayout.Label(entries.Count + " of " + all.Count + " items", EditorStyles.miniLabel);
            GUILayout.Space(5);
            float listTop = state.listRect.y > 0 ? state.listRect.y : 162;
            using (var scroll = new EditorGUILayout.ScrollViewScope(state.scroll, GUILayout.Height(Mathf.Max(80, area.height - listTop - 4))))
            {
                state.scroll = scroll.scrollPosition;
                if (state.loading) EditorGUILayout.HelpBox("Loading your content…", MessageType.Info);
                if (!string.IsNullOrEmpty(state.error)) EditorGUILayout.HelpBox(state.error, MessageType.Error);
                if (entries.Count == 0 && !state.loading)
                    EditorGUILayout.HelpBox(all.Count == 0 ? "No content loaded. Use Refresh to check your uploads and local tests." : "No matching content. Try another search or visibility filter.", MessageType.Info);
                foreach (var entry in entries) DrawCard(window, type, state, entry);
            }
            if (Event.current.type == EventType.Repaint) state.listRect = GUILayoutUtility.GetLastRect();
            return true;
            }
        }

        static void DrawCard(EditorWindow window, Type type, State state, Entry entry)
        {
            var card = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(10, 10, 10, 10) };
            using (new EditorGUILayout.VerticalScope(card))
            {
            using (new EditorGUILayout.HorizontalScope())
            {
                var cache = type.GetField("ImageCache", Flags)?.GetValue(null) as IDictionary;
                var texture = cache != null && cache.Contains(entry.id) ? cache[entry.id] as Texture : null;
                var rect = GUILayoutUtility.GetRect(88, 88, GUILayout.Width(88), GUILayout.Height(88));
                if (texture) GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
                else GUI.Label(rect, entry.category == 3 ? "Local test" : "No preview", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(10);
                using (new EditorGUILayout.VerticalScope())
                {
                    var title = new GUIStyle(EditorStyles.boldLabel) { wordWrap = true };
                    GUILayout.Label(new GUIContent(entry.name, entry.name), title);
                    GUILayout.Label((entry.category == 1 ? "Avatar" : entry.category == 2 ? "World" : "Local test") +
                        (string.IsNullOrEmpty(entry.visibility) ? "" : "  ·  " + entry.visibility), EditorStyles.miniLabel);
                    EditorGUILayout.SelectableLabel(entry.id, EditorStyles.miniLabel, GUILayout.Height(16));
                    GUILayout.Space(6);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool expanded = state.managedId == entry.id && state.managedCategory == entry.category;
                        if (GUILayout.Button(new GUIContent(expanded ? "Close" : "Manage", "Show or hide item actions"), GUILayout.Width(80), GUILayout.Height(28)))
                            EditorApplication.delayCall += () => { ToggleManage(state, entry); if (window) window.Repaint(); };
                        GUILayout.Space(5);
                        using (new EditorGUI.DisabledScope(entry.category == 3 || string.IsNullOrEmpty(entry.id)))
                            if (GUILayout.Button("Open on web", GUILayout.Width(100), GUILayout.Height(28)))
                                Application.OpenURL(entry.category == 2 ? "https://vrchat.com/home/content/worlds/" + entry.id + "/edit" : "https://vrchat.com/home/avatar/" + entry.id);
                        GUILayout.Space(5);
                        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(entry.id)))
                            if (GUILayout.Button("Copy ID", GUILayout.Width(75), GUILayout.Height(28))) EditorGUIUtility.systemCopyBuffer = entry.id;
                    }
                }
            }
            if (state.managedId == entry.id && state.managedCategory == entry.category) DrawManageControls(window, state, entry);
            }
            GUILayout.Space(8);
        }

        internal static void ToggleManage(State state, Entry entry)
        {
            state.managedId = state.managedId == entry.id && state.managedCategory == entry.category ? null : entry.id;
            state.managedCategory = entry.category;
            state.confirmDeleteKey = null;
        }

        static string Key(Entry entry) { return entry.category + ":" + entry.id; }

        static void DrawManageControls(EditorWindow window, State state, Entry entry)
        {
            GUILayout.Space(8);
            string key = Key(entry);
            using (new EditorGUI.DisabledScope(state.busyKey != null))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (entry.category == 1 && GUILayout.Button(entry.visibility == "public" ? "Make private" : "Make public", GUILayout.Height(28)))
                        EditorApplication.delayCall += () => ChangeVisibility(window, state, entry);
                    if (entry.category == 2 && GUILayout.Button("Edit world on web", GUILayout.Height(28)))
                        Application.OpenURL("https://vrchat.com/home/content/worlds/" + entry.id + "/edit");
                    if (GUILayout.Button(entry.category == 3 ? "Delete local test…" : "Delete…", GUILayout.Width(125), GUILayout.Height(28)))
                        state.confirmDeleteKey = key;
                }
                if (state.confirmDeleteKey == key)
                {
                    GUILayout.Label("Delete “" + entry.name + "”? This cannot be undone.", EditorStyles.wordWrappedLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Delete permanently", GUILayout.Height(28)))
                            EditorApplication.delayCall += () => DeleteEntry(window, state, entry);
                        if (GUILayout.Button("Cancel", GUILayout.Height(28))) state.confirmDeleteKey = null;
                    }
                }
            }
            if (state.busyKey == key) GUILayout.Label("Updating…", EditorStyles.miniLabel);
        }

        static void ChangeVisibility(EditorWindow window, State state, Entry entry)
        {
            var avatar = entry.model as VRC.Core.ApiAvatar;
            if (avatar == null || state.busyKey != null) return;
            string previous = avatar.releaseStatus;
            state.busyKey = Key(entry); state.error = null;
            avatar.releaseStatus = entry.visibility == "public" ? "private" : "public";
            avatar.SaveReleaseStatus(container =>
            {
                var saved = container.Model as VRC.Core.ApiAvatar;
                var field = window.GetType().GetField("justUpdatedAvatars", Flags);
                var updates = field.GetValue(null) as List<VRC.Core.ApiAvatar> ?? new List<VRC.Core.ApiAvatar>();
                updates.RemoveAll(item => item.id == entry.id);
                updates.Add(saved ?? avatar); field.SetValue(null, updates);
                state.busyKey = null; if (window) window.Repaint();
            }, container =>
            {
                avatar.releaseStatus = previous; state.busyKey = null;
                state.error = "Could not change visibility: " + container.Error; if (window) window.Repaint();
            });
        }

        static void DeleteEntry(EditorWindow window, State state, Entry entry)
        {
            if (state.busyKey != null) return;
            state.busyKey = Key(entry); state.error = null; state.confirmDeleteKey = null;
            Action complete = () =>
            {
                RemoveCachedEntry(window.GetType(), entry);
                if (entry.category != 3)
                    foreach (var pipeline in UnityEngine.Object.FindObjectsByType<VRC.Core.PipelineManager>(FindObjectsSortMode.None))
                        if (pipeline.blueprintId == entry.id)
                        {
                            Undo.RecordObject(pipeline, "Clear deleted content ID"); pipeline.blueprintId = ""; EditorUtility.SetDirty(pipeline);
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(pipeline.gameObject.scene);
                        }
                state.busyKey = null; state.managedId = null; if (window) window.Repaint();
            };
            Action<VRC.Core.ApiContainer> failed = container => { state.busyKey = null; state.error = "Could not delete content: " + container.Error; if (window) window.Repaint(); };
            if (entry.category == 3)
            {
                try
                {
                    string path = System.IO.Path.GetFullPath(Read(entry.model, "assetUrl"));
                    string folder = System.IO.Path.GetFullPath(VRC.SDKBase.Editor.VRC_SdkBuilder.GetLocalLowPath() + "/VRChat/VRChat/Avatars/");
                    if (!path.StartsWith(folder.TrimEnd('/', '\\') + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(System.IO.Path.GetExtension(path), ".vrca", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid local test path.");
                    System.IO.File.Delete(path); complete();
                }
                catch (Exception exception) { state.busyKey = null; state.error = exception.Message; }
            }
            else if (entry.category == 1) VRC.Core.API.Delete<VRC.Core.ApiAvatar>(entry.id, _ => complete(), failed);
            else VRC.Core.API.Delete<VRC.Core.ApiWorld>(entry.id, _ => complete(), failed);
        }

        internal static void RemoveCachedEntry(Type type, Entry entry)
        {
            var field = type.GetField(new[] { "uploadedAvatars", "uploadedWorlds", "testAvatars" }[entry.category - 1], Flags);
            var list = field.GetValue(null) as IList;
            if (list != null) for (int i = list.Count - 1; i >= 0; i--) if (Read(list[i], "id") == entry.id) list.RemoveAt(i);
            var cache = type.GetField("ImageCache", Flags)?.GetValue(null) as IDictionary;
            cache?.Remove(entry.id);
            if (entry.category == 3) return;
            var deletedField = type.GetField("justDeletedContents", Flags);
            var deleted = deletedField.GetValue(null) as List<string> ?? new List<string>();
            if (!deleted.Contains(entry.id)) deleted.Add(entry.id);
            deletedField.SetValue(null, deleted);
        }

        internal static List<Entry> ReadEntries(Type type)
        {
            var result = new List<Entry>();
            var deleted = type.GetField("justDeletedContents", Flags)?.GetValue(null) as IEnumerable;
            var deletedIds = new HashSet<string>();
            if (deleted != null) foreach (var id in deleted) if (id is string) deletedIds.Add((string)id);
            var updates = type.GetField("justUpdatedAvatars", Flags)?.GetValue(null) as IEnumerable;
            var updated = new Dictionary<string, object>();
            if (updates != null) foreach (var item in updates) if (item != null) updated[Read(item, "id")] = item;
            var fields = new[] { "uploadedAvatars", "uploadedWorlds", "testAvatars" };
            for (int category = 1; category <= fields.Length; category++)
            {
                var items = type.GetField(fields[category - 1], Flags)?.GetValue(null) as IEnumerable;
                if (items == null) continue;
                foreach (var original in items)
                {
                    if (original == null) continue;
                    var item = original;
                    string id = Read(item, "id");
                    if (category != 3 && deletedIds.Contains(id)) continue;
                    if (category == 1 && updated.ContainsKey(id)) item = updated[id];
                    string name = Read(item, "name");
                    result.Add(new Entry { id = id, name = string.IsNullOrEmpty(name) ? "Unnamed" : name,
                        visibility = category == 3 ? "" : Read(item, "releaseStatus"), category = category, model = item });
                }
            }
            return result;
        }

        static string Read(object item, string name)
        {
            var type = item.GetType();
            return Convert.ToString(type.GetProperty(name)?.GetValue(item, null) ?? type.GetField(name)?.GetValue(item)) ?? "";
        }

        internal static IEnumerable<Entry> Filter(IEnumerable<Entry> entries, string search, int category, int visibility, int sort)
        {
            string query = (search ?? "").Trim();
            var filtered = entries.Where(entry => (category == 0 || entry.category == category) &&
                (visibility == 0 || string.Equals(entry.visibility, visibility == 1 ? "public" : "private", StringComparison.OrdinalIgnoreCase)) &&
                (entry.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || entry.id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
            if (sort == 1) return filtered.OrderBy(entry => entry.name, StringComparer.OrdinalIgnoreCase).ThenBy(entry => entry.id, StringComparer.Ordinal);
            if (sort == 2) return filtered.OrderByDescending(entry => entry.name, StringComparer.OrdinalIgnoreCase).ThenBy(entry => entry.id, StringComparer.Ordinal);
            return filtered;
        }
    }
}
#endif
