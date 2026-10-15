#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HierarchyPlus
{
    [Serializable]
    internal class SmartCollection
    {
        public string name;
        public string id = Guid.NewGuid().ToString("N");
        public bool alwaysVisible;
        public List<string> displayOrder = new List<string>();
        [SerializeField] private List<string> persistentPaths = new List<string>(); // Persist hierarchy paths; instance IDs change across sessions.
        [NonSerialized] public List<int> objectIDs = new List<int>(); 
        public Color color = Color.white;
        public bool showInHierarchy = true;

        [NonSerialized] private HashSet<int> idCache;
        [NonSerialized] private bool cacheValid;
        [NonSerialized] private bool needsRebuild = true;

        public SmartCollection(string name) => this.name = name;

        public bool Contains(GameObject go)
        {
            if (go == null) return false;
            RebuildIDsIfNeeded();
            if (!cacheValid)
            {
                idCache = new HashSet<int>(objectIDs);
                cacheValid = true;
            }
            return idCache.Contains(go.GetInstanceID());
        }

        public void Add(GameObject go)
        {
            if (go == null) return;
            if (Contains(go)) return;
            
            string path = GetGameObjectPath(go);
            if (!persistentPaths.Contains(path))
            {
                persistentPaths.Add(path);
                needsRebuild = true;
                cacheValid = false;
            }
        }

        public void Remove(GameObject go)
        {
            if (go == null) return;
            
            if (persistentPaths.RemoveAll(path => TryFindGameObjectByPath(path, out var member) && member == go) > 0)
            {
                needsRebuild = true;
                cacheValid = false;
            }
        }

        public void CleanupInvalidReferences()
        {
            RebuildIDsIfNeeded();
            
            persistentPaths.RemoveAll(path => !TryFindGameObjectByPath(path, out _));
            needsRebuild = true;
            cacheValid = false;
        }

        internal void RebuildIDsIfNeeded()
        {
            if (!needsRebuild) return;
            
            objectIDs.Clear();
            foreach (var path in persistentPaths)
            {
                if (TryFindGameObjectByPath(path, out GameObject go))
                {
                    objectIDs.Add(go.GetInstanceID());
                }
            }
            
            needsRebuild = false;
            cacheValid = false;
        }

        internal void InvalidateIDs()
        {
            needsRebuild = true;
            cacheValid = false;
        }

        internal static string GetGameObjectPath(GameObject go)
        {
            if (go == null) return string.Empty;
            if (!string.IsNullOrEmpty(go.scene.path))
            {
                var id = GlobalObjectId.GetGlobalObjectIdSlow(go);
                if (id.targetObjectId != 0) return id.ToString();
            }
            return "Session:" + SessionKey + ":" + go.GetInstanceID();
        }

        private static string SessionKey
        {
            get
            {
                const string key = "HierarchyPlus.CollectionSession";
                string value = SessionState.GetString(key, "");
                if (string.IsNullOrEmpty(value))
                {
                    value = Guid.NewGuid().ToString("N");
                    SessionState.SetString(key, value);
                }
                return value;
            }
        }

        private static bool TryFindGameObjectByPath(string fullPath, out GameObject result)
        {
            result = null;
            if (string.IsNullOrEmpty(fullPath)) return false;

            if (fullPath.StartsWith("GlobalObjectId_V1-", StringComparison.Ordinal))
            {
                if (GlobalObjectId.TryParse(fullPath, out var id))
                    result = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                return result != null;
            }
            if (fullPath.StartsWith("Session:", StringComparison.Ordinal))
            {
                string prefix = "Session:" + SessionKey + ":";
                if (fullPath.StartsWith(prefix, StringComparison.Ordinal) &&
                    int.TryParse(fullPath.Substring(prefix.Length), out int id))
                    result = EditorUtility.InstanceIDToObject(id) as GameObject;
                return result != null;
            }
            
            string[] parts = fullPath.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length != 2) return false;
            
            string scenePath = parts[0];
            string hierarchyPath = parts[1];
            
            UnityEngine.SceneManagement.Scene targetScene = default;
            bool sceneFound = false;
            
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.path == scenePath || scene.name == scenePath)
                {
                    targetScene = scene;
                    sceneFound = true;
                    break;
                }
            }
            
            if (!sceneFound) return false;
            
            GameObject[] rootObjects = targetScene.GetRootGameObjects();
            
            string[] pathParts = hierarchyPath.Split('/');
            
            GameObject current = null;
            foreach (var root in rootObjects)
            {
                if (root.name == pathParts[0])
                {
                    current = root;
                    break;
                }
            }
            
            if (current == null) return false;
            
            for (int i = 1; i < pathParts.Length; i++)
            {
                Transform child = current.transform.Find(pathParts[i]);
                if (child == null) return false;
                current = child.gameObject;
            }
            
            result = current;
            return true;
        }

        public List<string> GetPersistentPaths()
        {
            for (int i = 0; i < persistentPaths.Count; i++)
                if (TryFindGameObjectByPath(persistentPaths[i], out var go))
                    persistentPaths[i] = GetGameObjectPath(go);
            return new List<string>(persistentPaths);
        }
        public void SetPersistentPaths(List<string> paths) 
        { 
            persistentPaths = paths ?? new List<string>();
            needsRebuild = true;
        }
    }

    [InitializeOnLoad]
    internal static class Collections
    {
        private const string CollectionsFilePath = "ProjectSettings/HierarchyPlusCollections.json";
        private const string PrefsRemoveModeKey  = "HierarchyPlus_RemoveFilteredRows";
        private const string PrefsHideInSceneKey = "HierarchyPlus_HideFilteredInScene";
        private const string PrefsEditorVisibilityKey = "HierarchyPlus_EditorOnlySceneHiding";
        private const string kCreatePath         = "GameObject/HierarchyPlus/Create Collection\u2026";

        internal enum DisplayMode
        {
            CompactBadge,
            ColorBar,
            IconOnly
        }

        internal static List<SmartCollection> collections   = new List<SmartCollection>();
        internal static List<string> unassignedOrder = new List<string>();
        internal static SmartCollection       activeFilter;
        internal static bool                  isFiltering;
        private static HashSet<int>           visibleObjects           = new HashSet<int>();
        private static Dictionary<int, List<SmartCollection>> objectToCollectionsCache = new();
        private static bool                   cacheValid;
        
        private static Dictionary<string, Texture2D> visualCache = new Dictionary<string, Texture2D>();
        private static Dictionary<string, GUIContent> guiContentCache = new Dictionary<string, GUIContent>();
        private static Queue<string> cacheOrder = new Queue<string>();
        private const int MAX_CACHE_SIZE = 100;
        
        private static Dictionary<int, bool> originalActiveStates = new Dictionary<int, bool>();
        private static HashSet<int> modifiedByFilter = new HashSet<int>();
        private static readonly HashSet<int> hiddenByFilter = new HashSet<int>();
        private static readonly HashSet<int> sceneHiddenByFilter = new HashSet<int>();

        static Collections()
        {
            Load();
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged          += OnHierarchyChanged;
            EditorApplication.playModeStateChanged      += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload  += OnBeforeAssemblyReload;
            EditorApplication.quitting += ClearFilter;
            
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (scene, mode) => RebuildAllCollectionIDs();
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorApplication.delayCall += RebuildAllCollectionIDs;
        }

        private static void RebuildAllCollectionIDs()
        {
            foreach (var collection in collections)
            {
                collection.InvalidateIDs();
                collection.RebuildIDsIfNeeded();
            }
            cacheValid = false;
        }

        private static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            if (collections.Count > 0) Save();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode ||
                state == PlayModeStateChange.ExitingPlayMode)
            {
                if (isFiltering)
                    ClearFilter();
            }
        }
        
        private static void OnBeforeAssemblyReload()
        {
            ClearFilter();
            CleanupVisualCache();
        }

        private static void OnHierarchyChanged()
        {
            RebuildAllCollectionIDs();
            if (isFiltering)
            {
                UpdateFilter();
                if (CollectionsHierarchy.Enabled)
                {
                    SuppressVisibilityUndo = true;
                    try { ApplyAllFlags(); }
                    finally { SuppressVisibilityUndo = false; }
                }
            }
        }
        
        internal static void InvalidateVisualCache()
        {
            CleanupVisualCache();
            EditorApplication.RepaintHierarchyWindow();
        }
        
        private static void CleanupVisualCache()
        {
            foreach (var texture in visualCache.Values)
            {
                if (texture != null)
                    UnityEngine.Object.DestroyImmediate(texture);
            }
            visualCache.Clear();
            guiContentCache.Clear();
            cacheOrder.Clear();
        }

        public static void SetActiveFilter(SmartCollection filter)
        {
            ClearFilter();
            
            activeFilter = filter;
            UpdateFilter();
            ApplyAllFlags();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void ClearFilter()
        {
            activeFilter = null;
            isFiltering  = false;
            visibleObjects.Clear();
            
            RestoreOriginalStates();
            originalActiveStates.Clear();
            modifiedByFilter.Clear();
            
            ApplyAllFlags();
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void UpdateFilter()
        {
            visibleObjects.Clear();
            isFiltering = activeFilter != null;
            if (!isFiltering) return;

            foreach (var collection in collections.Where(c => c == activeFilter || c.alwaysVisible))
            {
                collection.RebuildIDsIfNeeded();
                foreach (var id in collection.objectIDs)
                    if (EditorUtility.InstanceIDToObject(id) is GameObject go)
                        AddWithHierarchy(go);
            }
        }

        private static void AddWithHierarchy(GameObject go)
        {
            visibleObjects.Add(go.GetInstanceID());
            foreach (Transform child in go.transform)
                AddWithHierarchy(child.gameObject);
            var parent = go.transform.parent;
            while (parent != null)
            {
                visibleObjects.Add(parent.gameObject.GetInstanceID());
                parent = parent.parent;
            }
        }

        internal static bool ShouldShowInHierarchy(GameObject go)
            => !isFiltering || visibleObjects.Contains(go.GetInstanceID());

        internal static bool RemoveFilteredRows
        {
            get => EditorPrefs.GetBool(PrefsRemoveModeKey, false);
            set => EditorPrefs.SetBool(PrefsRemoveModeKey, value);
        }

        internal static bool HideFilteredInScene
        {
            get => EditorPrefs.GetBool(PrefsHideInSceneKey, false);
            set => EditorPrefs.SetBool(PrefsHideInSceneKey, value);
        }

        internal static bool UseEditorVisibility
        {
            get => EditorPrefs.GetBool(PrefsEditorVisibilityKey, false);
            set => EditorPrefs.SetBool(PrefsEditorVisibilityKey, value);
        }


        private static void OnHierarchyGUI(int id, Rect rect)
        {
            var go = EditorUtility.InstanceIDToObject(id) as GameObject;
            if (go == null) return;
            
            if (EditorApplication.isPlaying) return;

            if (isFiltering && !visibleObjects.Contains(id))
            {
                if (!RemoveFilteredRows)
                    EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.5f));
                return;
            }

        }

        private static void DrawCompactBadge(Rect rect, List<SmartCollection> collections, float rightOffset)
        {
            float badgeWidth = collections.Count == 1 ? 20f : 24f;
            var badgeRect = new Rect(rect.xMax - badgeWidth - rightOffset, rect.y + 2, badgeWidth, rect.height - 4);
            
            var cachedContent = GetCachedVisual(collections, DisplayMode.CompactBadge);
            if (cachedContent.image != null)
            {
                GUI.DrawTexture(badgeRect, cachedContent.image, ScaleMode.StretchToFill);
            }
            
            if (collections.Count == 1)
            {
                var iconRect = new Rect(badgeRect.x + 2, badgeRect.y + 1, 16, 16);
                var oldColor = GUI.color;
                GUI.color = Color.white; 
                GUI.Label(iconRect, EditorGUIUtility.IconContent("d_Folder Icon"));
                GUI.color = oldColor;
            }
            else
            {
                var style = new GUIStyle(EditorStyles.miniLabel);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 9;
                style.normal.textColor = Color.white;
                GUI.Label(badgeRect, collections.Count.ToString(), style);
            }
            
            GUI.tooltip = cachedContent.tooltip;
            HandleInteraction(badgeRect, collections);
        }
        
        private static void DrawColorBar(Rect rect, List<SmartCollection> collections, float rightOffset)
        {
            float barWidth = 20f;
            float barHeight = 12f;
            var barRect = new Rect(rect.xMax - barWidth - rightOffset, rect.y + (rect.height - barHeight) * 0.5f, barWidth, barHeight);
            
            var cachedContent = GetCachedVisual(collections, DisplayMode.ColorBar);
            if (cachedContent.image != null)
            {
                GUI.DrawTexture(barRect, cachedContent.image, ScaleMode.StretchToFill);
            }
            else
            {
                EditorGUI.DrawRect(barRect, new Color(0.15f, 0.15f, 0.15f, 0.8f));
                
                float segmentWidth = barRect.width / collections.Count;
                for (int i = 0; i < collections.Count; i++)
                {
                    var segmentRect = new Rect(
                        barRect.x + i * segmentWidth,
                        barRect.y,
                        segmentWidth,
                        barHeight
                    );
                    EditorGUI.DrawRect(segmentRect, collections[i].color);
                }
                
                DrawOutline(barRect, new Color(0, 0, 0, 0.5f));
            }
            
            GUI.tooltip = cachedContent.tooltip;
            HandleInteraction(barRect, collections);
        }
        
        
        private static void DrawIconOnly(Rect rect, List<SmartCollection> collections, float rightOffset)
        {
            var iconRect = new Rect(rect.xMax - 18 - rightOffset, rect.y, 18, 18);
            
            var cachedContent = GetCachedVisual(collections, DisplayMode.IconOnly);
            
            if (cachedContent.image != null)
            {
                var bgRect = new Rect(iconRect.x - 1, iconRect.y + 1, iconRect.width, iconRect.height - 2);
                GUI.DrawTexture(bgRect, cachedContent.image, ScaleMode.StretchToFill);
            }
            
            var oldColor = GUI.color;
            GUI.color = Color.white;
            GUI.Label(iconRect, EditorGUIUtility.IconContent("d_Folder Icon"));
            GUI.color = oldColor;
            
            if (collections.Count > 1)
            {
                var numberRect = new Rect(iconRect.x + 8, iconRect.y + 6, 12, 12);
                var style = new GUIStyle(EditorStyles.miniLabel);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 8;
                style.normal.textColor = Color.white;
                
                EditorGUI.DrawRect(new Rect(numberRect.x - 1, numberRect.y - 1, numberRect.width, numberRect.height), 
                    new Color(0.8f, 0.2f, 0.2f, 0.9f));
                GUI.Label(numberRect, collections.Count.ToString(), style);
            }
            
            GUI.tooltip = cachedContent.tooltip;
            HandleInteraction(iconRect, collections);
        }
        
        private static void HandleInteraction(Rect interactionRect, List<SmartCollection> collections)
        {
            if (interactionRect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(interactionRect, MouseCursor.Link);
                
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    if (collections.Count == 1)
                    {
                        if (activeFilter == collections[0])
                            ClearFilter();
                        else
                            SetActiveFilter(collections[0]);
                    }
                    else
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Filter by Collection"), false, () => {});
                        menu.AddSeparator("");
                        foreach (var col in collections)
                        {
                            var c = col; 
                            menu.AddItem(new GUIContent($"{c.name}"), activeFilter == c, () => {
                                if (activeFilter == c)
                                    ClearFilter();
                                else
                                    SetActiveFilter(c);
                            });
                        }
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Clear Filter"), false, () => ClearFilter());
                        menu.ShowAsContext();
                    }
                    Event.current.Use();
                }
            }
        }
        
        private static void DrawRoundedRect(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
            
            DrawOutline(rect, new Color(0, 0, 0, 0.2f));
        }
        
        private static void DrawOutline(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), color); 
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color); 
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), color); 
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), color); 
        }
        
        private static string GetCacheKey(List<SmartCollection> collections, DisplayMode mode)
        {
            if (collections.Count == 0) return "";
            
            var sortedIds = collections.Select(c => $"{c.name}_{ColorUtility.ToHtmlStringRGBA(c.color)}").OrderBy(s => s);
            return $"{mode}_{string.Join(",", sortedIds)}";
        }
        
        
        private static Texture2D GenerateColorBarTexture(List<SmartCollection> collections)
        {
            int width = 20;
            int height = 12;
            var texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            
            int segmentWidth = width / collections.Count;
            for (int i = 0; i < collections.Count; i++)
            {
                int startX = i * segmentWidth;
                int endX = (i == collections.Count - 1) ? width : (i + 1) * segmentWidth;
                
                for (int x = startX; x < endX; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        pixels[y * width + x] = collections[i].color;
                    }
                }
            }
            
            for (int x = 0; x < width; x++)
            {
                pixels[x] = new Color(0, 0, 0, 0.5f); 
                pixels[(height - 1) * width + x] = new Color(0, 0, 0, 0.5f); 
            }
            for (int y = 0; y < height; y++)
            {
                pixels[y * width] = new Color(0, 0, 0, 0.5f); 
                pixels[y * width + width - 1] = new Color(0, 0, 0, 0.5f); 
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        
        private static GUIContent GetCachedVisual(List<SmartCollection> collections, DisplayMode mode)
        {
            string key = GetCacheKey(collections, mode);
            if (string.IsNullOrEmpty(key)) return GUIContent.none;
            
            if (guiContentCache.TryGetValue(key, out var cached))
                return cached;
            
            if (cacheOrder.Count >= MAX_CACHE_SIZE)
            {
                string oldestKey = cacheOrder.Dequeue();
                if (visualCache.TryGetValue(oldestKey, out var oldTexture))
                {
                    if (oldTexture != null)
                        UnityEngine.Object.DestroyImmediate(oldTexture);
                    visualCache.Remove(oldestKey);
                }
                guiContentCache.Remove(oldestKey);
            }
            
            Texture2D texture = null;
            string tooltip = string.Join("\n", collections.Select(c => $"\u2022 {c.name}").Take(10));
            if (collections.Count > 10) tooltip += $"\n... and {collections.Count - 10} more";
            
            switch (mode)
            {
                case DisplayMode.ColorBar:
                    texture = GenerateColorBarTexture(collections);
                    break;
                case DisplayMode.CompactBadge:
                case DisplayMode.IconOnly:
                    return new GUIContent((Texture2D)null, tooltip);
            }
            
            var content = new GUIContent(texture, tooltip);
            visualCache[key] = texture;
            guiContentCache[key] = content;
            cacheOrder.Enqueue(key);
            
            return content;
        }

        private static List<SmartCollection> GetCollectionsForObject(GameObject go)
        {
            if (!cacheValid) RebuildCache();
            return objectToCollectionsCache.TryGetValue(go.GetInstanceID(), out var list)
                ? list
                : new List<SmartCollection>();
        }
        
        internal static List<SmartCollection> GetCollectionsForObjectStatic(GameObject go)
        {
            return GetCollectionsForObject(go);
        }
        
        internal static void DrawCollectionIcon(Rect iconRect, List<SmartCollection> collections, bool withBackground, float alpha = 1f)
        {
            if (collections == null || collections.Count == 0) return;
            
            DrawCompactBadgeIcon(iconRect, collections, withBackground, alpha);
        }
        
        private static void DrawCompactBadgeIcon(Rect rect, List<SmartCollection> collections, bool withBackground, float alpha = 1f)
        {
            float badgeSize = 18f;
            var badgeRect = new Rect(rect.x - 1, rect.y, badgeSize, rect.height);
            
            if (collections.Count == 1)
            {
                Color badgeColor = collections[0].color;
                badgeColor.a *= alpha;
                EditorGUI.DrawRect(badgeRect, badgeColor);
                
                var borderColor = new Color(0f, 0f, 0f, 0.3f * alpha);
                DrawBorder(badgeRect, borderColor);
                
                var contentColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);
                var iconContent = EditorGUIUtility.IconContent("d_Folder Icon");
                var iconRect = new Rect(badgeRect.x + 1, badgeRect.y + 1, 16, 16);
                GUI.Label(iconRect, iconContent);
                GUI.color = contentColor;
            }
            else
            {
                DrawMultiColorStripes(badgeRect, collections, alpha);
                
                var borderColor = new Color(0f, 0f, 0f, 0.4f * alpha);
                DrawBorder(badgeRect, borderColor);
                
                var numberStyle = new GUIStyle(EditorStyles.miniLabel);
                numberStyle.alignment = TextAnchor.MiddleCenter;
                numberStyle.fontSize = 9;
                numberStyle.fontStyle = FontStyle.Bold;
                numberStyle.normal.textColor = new Color(1f, 1f, 1f, alpha);
                
                var textBgRect = new Rect(badgeRect.x + 2, badgeRect.y + 6, badgeRect.width - 4, 8);
                EditorGUI.DrawRect(textBgRect, new Color(0f, 0f, 0f, 0.5f * alpha));
                GUI.Label(badgeRect, collections.Count.ToString(), numberStyle);
            }
            
            string tooltip = "Collections:\n" + string.Join("\n", collections.Select(c => $"\u2022 {c.name}").Take(5));
            if (collections.Count > 5) tooltip += $"\n... and {collections.Count - 5} more";
            GUI.tooltip = tooltip;
            
            if (alpha > 0.5f) HandleInteraction(badgeRect, collections);
        }
        
        private static void DrawColorBarIcon(Rect rect, List<SmartCollection> collections, bool withBackground, float alpha = 1f)
        {
            float barHeight = 12f;
            var barRect = new Rect(rect.x - 1, rect.y + (rect.height - barHeight) * 0.5f, 20f, barHeight);
            
            var cachedContent = GetCachedVisual(collections, DisplayMode.ColorBar);
            if (cachedContent.image != null)
            {
                var prevColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.DrawTexture(barRect, cachedContent.image, ScaleMode.StretchToFill);
                GUI.color = prevColor;
            }
            
            GUI.tooltip = cachedContent.tooltip;
            if (alpha > 0.5f) HandleInteraction(barRect, collections);
        }
        
        private static void DrawIconOnlyIcon(Rect rect, List<SmartCollection> collections, bool withBackground, float alpha = 1f)
        {
            var iconRect = new Rect(rect.x - 1, rect.y, 18, rect.height);
            
            if (withBackground)
            {
                if (collections.Count == 1)
                {
                    Color bgColor = collections[0].color;
                    bgColor.a *= alpha;
                    EditorGUI.DrawRect(iconRect, bgColor);
                }
                else
                {
                    Color bgColor = new Color(0.3f, 0.3f, 0.3f, 0.8f * alpha);
                    EditorGUI.DrawRect(iconRect, bgColor);
                    
                    DrawColorDots(iconRect, collections, alpha);
                }
                
                var borderColor = new Color(0f, 0f, 0f, 0.2f * alpha);
                DrawBorder(iconRect, borderColor);
            }
            
            var iconColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            var folderIcon = EditorGUIUtility.IconContent("d_Folder Icon");
            GUI.Label(iconRect, folderIcon);
            GUI.color = iconColor;
            
            if (collections.Count > 1)
            {
                var badgeSize = 8f;
                var badgeRect = new Rect(iconRect.xMax - badgeSize, iconRect.y, badgeSize, badgeSize);
                
                var badgeColor = new Color(0.9f, 0.2f, 0.2f, 0.95f * alpha);
                EditorGUI.DrawRect(badgeRect, badgeColor);
                
                var numberStyle = new GUIStyle(EditorStyles.miniLabel);
                numberStyle.alignment = TextAnchor.MiddleCenter;
                numberStyle.fontSize = 7;
                numberStyle.fontStyle = FontStyle.Bold;
                numberStyle.normal.textColor = new Color(1f, 1f, 1f, alpha);
                GUI.Label(badgeRect, collections.Count.ToString(), numberStyle);
            }
            
            string tooltip = "Collections:\n" + string.Join("\n", collections.Select(c => $"\u2022 {c.name}").Take(5));
            if (collections.Count > 5) tooltip += $"\n... and {collections.Count - 5} more";
            GUI.tooltip = tooltip;
            
            if (alpha > 0.5f) HandleInteraction(iconRect, collections);
        }

        private static void DrawBorder(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), color);
        }

        private static void DrawMultiColorStripes(Rect rect, List<SmartCollection> collections, float alpha = 1f)
        {
            int maxStripes = Mathf.Min(3, collections.Count);
            float stripeHeight = rect.height / maxStripes;
            
            for (int i = 0; i < maxStripes; i++)
            {
                Color stripeColor = collections[i].color;
                stripeColor.a *= alpha;
                
                var stripeRect = new Rect(rect.x, rect.y + i * stripeHeight, rect.width, stripeHeight);
                EditorGUI.DrawRect(stripeRect, stripeColor);
            }
        }

        private static void DrawColorDots(Rect rect, List<SmartCollection> collections, float alpha = 1f)
        {
            int maxDots = Mathf.Min(4, collections.Count);
            float dotSize = 3f;
            float spacing = 1f;
            float totalWidth = maxDots * dotSize + (maxDots - 1) * spacing;
            float startX = rect.x + (rect.width - totalWidth) * 0.5f;
            float dotY = rect.yMax - dotSize - 1;
            
            for (int i = 0; i < maxDots; i++)
            {
                Color dotColor = collections[i].color;
                dotColor.a *= alpha;
                
                var dotRect = new Rect(startX + i * (dotSize + spacing), dotY, dotSize, dotSize);
                EditorGUI.DrawRect(dotRect, dotColor);
            }
        }

        private static void RebuildCache()
        {
            objectToCollectionsCache.Clear();
            foreach (var col in collections)
            {
                if (!col.showInHierarchy) continue;
                col.RebuildIDsIfNeeded();
                foreach (var id in col.objectIDs)
                {
                    if (!objectToCollectionsCache.TryGetValue(id, out var list))
                        objectToCollectionsCache[id] = list = new List<SmartCollection>();
                    list.Add(col);
                }
            }
            cacheValid = true;
        }

        internal static string CaptureState()
        {
            var data = new SerializationData
            {
                unassignedOrder = unassignedOrder,
                collections = collections.Select(c => new CollectionData
                {
                    id              = c.id,
                    alwaysVisible   = c.alwaysVisible,
                    name            = c.name,
                    color           = ColorUtility.ToHtmlStringRGBA(c.color),
                    showInHierarchy = c.showInHierarchy,
                    persistentPaths = c.GetPersistentPaths(),
                    displayOrder = c.displayOrder
                }).ToList()
            };

            return JsonUtility.ToJson(data, true);
        }

        internal static void Save()
        {
            System.IO.File.WriteAllText(CollectionsFilePath, CaptureState());
            cacheValid = false;
            InvalidateVisualCache();
        }

        internal static void RestoreState(string json)
        {
            var data = JsonUtility.FromJson<SerializationData>(json);
            unassignedOrder = data.unassignedOrder ?? new List<string>();
            collections.Clear();
            foreach (var cd in data.collections)
            {
                var c = new SmartCollection(cd.name)
                {
                    id = cd.id, alwaysVisible = cd.alwaysVisible,
                    color = ColorUtility.TryParseHtmlString("#" + cd.color, out var color) ? color : Color.white,
                    showInHierarchy = cd.showInHierarchy
                };
                c.SetPersistentPaths(cd.persistentPaths);
                c.displayOrder = cd.displayOrder ?? new List<string>();
                collections.Add(c);
            }
            Save();
        }

        private static void Load()
        {
            collections.Clear();
            unassignedOrder.Clear();

            if (System.IO.File.Exists(CollectionsFilePath))
            {
                string json = System.IO.File.ReadAllText(CollectionsFilePath);
                if (!string.IsNullOrEmpty(json))
                {
                    var data = JsonUtility.FromJson<SerializationData>(json);
                    unassignedOrder = data?.unassignedOrder ?? new List<string>();
                    if (data?.collections != null)
                    {
                        foreach (var cd in data.collections)
                        {
                            var c = new SmartCollection(cd.name)
                            {
                                color           = ColorUtility.TryParseHtmlString("#" + cd.color, out var col) ? col : Color.white,
                                id = string.IsNullOrEmpty(cd.id) ? Guid.NewGuid().ToString("N") : cd.id,
                                alwaysVisible = cd.alwaysVisible,
                                showInHierarchy = cd.showInHierarchy
                            };
                            c.SetPersistentPaths(cd.persistentPaths);
                            c.displayOrder = cd.displayOrder ?? new List<string>();
                            collections.Add(c);
                        }
                    }
                }
            }

            cacheValid = false;
        }

        [Serializable] private class SerializationData { public List<CollectionData> collections; public List<string> unassignedOrder; }
        [Serializable] private class CollectionData     
        { 
            public string name, color, id;
            public bool alwaysVisible; 
            public bool showInHierarchy; 
            public List<string> persistentPaths; 
            public List<string> displayOrder;
        }

        [MenuItem("GameObject/HierarchyPlus/Add to Collection...", false, 0)]
        private static void Menu_Add() => AddRemoveWindow.Show(true, Selection.gameObjects);
        [MenuItem("GameObject/HierarchyPlus/Add to Collection...", true)]
        private static bool Validate_Add() => Selection.gameObjects.Length > 0 && collections.Count > 0;

        [MenuItem("GameObject/HierarchyPlus/Remove from Collection...", false, 1)]
        private static void Menu_Remove() => AddRemoveWindow.Show(false, Selection.gameObjects);
        [MenuItem("GameObject/HierarchyPlus/Remove from Collection...", true)]
        private static bool Validate_Remove() =>
            Selection.gameObjects.Length > 0 &&
            collections.Any(c => Selection.gameObjects.Any(go => c.Contains(go)));

        [MenuItem(kCreatePath, false, 2)]
        private static void Menu_Create() => PromptForNewCollection(Selection.gameObjects);
        [MenuItem(kCreatePath, true)]
        private static bool Validate_Create() => Selection.gameObjects.Length > 0;

        internal static void PromptForNewCollection(GameObject[] targets)
        {
            if (Resources.FindObjectsOfTypeAll<CollectionNameDialog>().Length > 0) return;
            CollectionNameDialog.Show("New Collection", name =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    EditorUtility.DisplayDialog("Invalid Name", "Collection name cannot be empty.", "OK");
                    return;
                }
                if (collections.Any(c => c.name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    EditorUtility.DisplayDialog("Duplicate", $"Collection '{name}' exists.", "OK");
                    return;
                }
                var c = new SmartCollection(name) { color = GetRandomColor() };
                foreach (var go in targets) c.Add(go);
                CollectionsHierarchy.Change("Create Collection", () => collections.Add(c));
            });
        }

        private static readonly Color[] extendedPalette = {
            new Color(0.95f, 0.26f, 0.21f),    
            new Color(0.13f, 0.59f, 0.95f),    
            new Color(0.30f, 0.69f, 0.31f),    
            new Color(1.00f, 0.76f, 0.03f),    
            new Color(0.61f, 0.15f, 0.69f),    
            new Color(0.00f, 0.74f, 0.83f),    
            new Color(1.00f, 0.34f, 0.13f),    
            new Color(0.00f, 0.59f, 0.53f),    

            new Color(0.91f, 0.12f, 0.39f),    
            new Color(0.40f, 0.23f, 0.72f),    
            new Color(0.25f, 0.32f, 0.71f),    
            new Color(0.01f, 0.66f, 0.96f),    
            new Color(0.55f, 0.76f, 0.29f),    
            new Color(0.80f, 0.86f, 0.22f),    
            new Color(1.00f, 0.92f, 0.23f),    
            new Color(1.00f, 0.60f, 0.00f),    

            new Color(0.47f, 0.33f, 0.28f),    
            new Color(0.62f, 0.62f, 0.62f),    
            new Color(0.38f, 0.49f, 0.55f),    
            new Color(0.69f, 0.46f, 0.77f),    
            new Color(0.85f, 0.56f, 0.40f),    
            new Color(0.56f, 0.82f, 0.82f),    
            new Color(0.93f, 0.67f, 0.78f),    
            new Color(0.73f, 0.78f, 0.42f),    

            new Color(0.48f, 0.00f, 0.48f),    
            new Color(0.00f, 0.50f, 0.50f),    
            new Color(0.50f, 0.00f, 0.00f),    
            new Color(0.00f, 0.39f, 0.00f),    
            new Color(0.72f, 0.53f, 0.04f),    
            new Color(0.58f, 0.00f, 0.83f),    
            new Color(1.00f, 0.08f, 0.58f),    
            new Color(0.00f, 0.75f, 1.00f),    
        };

        internal static Color GetRandomColor()
        {
            int randomIndex = UnityEngine.Random.Range(0, extendedPalette.Length);
            Color color = extendedPalette[randomIndex];

            Color.RGBToHSV(color, out float h, out float s, out float v);

            h = (h + UnityEngine.Random.Range(-0.014f, 0.014f) + 1f) % 1f;

            s = Mathf.Clamp01(s + UnityEngine.Random.Range(-0.1f, 0.1f));
            v = Mathf.Clamp01(v + UnityEngine.Random.Range(-0.1f, 0.1f));

            return Color.HSVToRGB(h, s, v);
        }

        internal static void ApplyAllFlags()
        {
            ClearHideFlags();
            ApplySceneActivation();
            ApplySceneVisibility();
            ApplyHideFlags();
        }

        private static void ApplyHideFlags()
        {
            bool remove = RemoveFilteredRows && !CollectionsHierarchy.Enabled;
            foreach (var go in GameObject.FindObjectsOfType<GameObject>(true))
            {
                int id = go.GetInstanceID();
                bool shouldShow = !isFiltering || visibleObjects.Contains(id);
                if (remove && !shouldShow)
                {
                    if ((go.hideFlags & HideFlags.HideInHierarchy) == 0)
                    {
                        go.hideFlags |= HideFlags.HideInHierarchy;
                        hiddenByFilter.Add(id);
                        EditorUtility.SetDirty(go);
                    }
                }
                else if (hiddenByFilter.Remove(id))
                {
                    go.hideFlags &= ~HideFlags.HideInHierarchy;
                    EditorUtility.SetDirty(go);
                }
            }
        }

        private static void ApplySceneActivation()
        {
            if (!isFiltering || !HideFilteredInScene || UseEditorVisibility)
            {
                RestoreOriginalStates();
                originalActiveStates.Clear();
                modifiedByFilter.Clear();
                return;
            }
            
            foreach (var go in GameObject.FindObjectsOfType<GameObject>(true))
            {
                int id = go.GetInstanceID();
                bool shouldShow = visibleObjects.Contains(id);
                
                if (!shouldShow)
                {
                    if (!originalActiveStates.ContainsKey(id))
                    {
                        originalActiveStates[id] = go.activeSelf;
                        modifiedByFilter.Add(id);
                    }
                    go.SetActive(false);
                }
                else if (modifiedByFilter.Contains(id))
                {
                    if (originalActiveStates.TryGetValue(id, out bool originalState))
                    {
                        go.SetActive(originalState);
                        originalActiveStates.Remove(id);
                        modifiedByFilter.Remove(id);
                    }
                }
            }
        }
        
        internal static bool SuppressVisibilityUndo;
        private static void SetSceneHidden(GameObject go, bool hidden)
        {
            if (SuppressVisibilityUndo)
            {
                // Exclude view focus from collection Undo records.
                var type = typeof(Editor).Assembly.GetType("UnityEditor.SceneVisibilityState", true);
                type.GetMethod("SetGameObjectHidden", System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                    .Invoke(null, new object[] { go, hidden, false });
            }
            else if (hidden) SceneVisibilityManager.instance.Hide(go, false);
            else SceneVisibilityManager.instance.Show(go, false);
        }

        private static void ApplySceneVisibility()
        {
            var visibility = SceneVisibilityManager.instance;
            if (!isFiltering || !HideFilteredInScene || !UseEditorVisibility)
            {
                foreach (int id in sceneHiddenByFilter)
                {
                    var go = EditorUtility.InstanceIDToObject(id) as GameObject;
                    if (go != null) SetSceneHidden(go, false);
                }
                sceneHiddenByFilter.Clear();
                return;
            }

            foreach (var go in GameObject.FindObjectsOfType<GameObject>(true))
            {
                int id = go.GetInstanceID();
                if (!visibleObjects.Contains(id))
                {
                    if (!visibility.IsHidden(go))
                    {
                        SetSceneHidden(go, true);
                        sceneHiddenByFilter.Add(id);
                    }
                }
                else if (sceneHiddenByFilter.Remove(id))
                    SetSceneHidden(go, false);
            }
        }

        private static void RestoreOriginalStates()
        {
            foreach (var kvp in originalActiveStates)
            {
                var go = EditorUtility.InstanceIDToObject(kvp.Key) as GameObject;
                if (go != null)
                {
                    go.SetActive(kvp.Value);
                }
            }
        }

        private static void ClearHideFlags()
        {
            foreach (int id in hiddenByFilter)
            {
                var go = EditorUtility.InstanceIDToObject(id) as GameObject;
                if (go != null && (go.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    go.hideFlags &= ~HideFlags.HideInHierarchy;
                    EditorUtility.SetDirty(go);
                }
            }
            hiddenByFilter.Clear();
        }

        private class CollectionNameDialog : EditorWindow
        {
            private Action<string> onConfirm;
            private string         field;
            private bool           first = true;

            public static void Show(string defaultName, Action<string> cb)
            {
                var w = CreateInstance<CollectionNameDialog>();
                w.onConfirm    = cb;
                w.field        = defaultName;
                w.titleContent = new GUIContent("Collection Name");
                w.position     = new Rect(200, 200, 300, 80);
                w.ShowUtility();
            }

            void OnGUI()
            {
                if (first) { first = false; GUI.FocusControl("fld"); }
                EditorGUILayout.LabelField("Name:", EditorStyles.boldLabel);
                GUI.SetNextControlName("fld");
                field = EditorGUILayout.TextField(field);
                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    onConfirm?.Invoke(field);
                    Close();
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("OK"))     { onConfirm?.Invoke(field); Close(); }
                if (GUILayout.Button("Cancel")) Close();
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public class AddRemoveWindow : EditorWindow
    {
        private bool                   isAdd;
        private GameObject[]           targets;
        private Vector2                scroll;
        private bool[]                 sel;
        private List<SmartCollection>  cols;
        private static AddRemoveWindow inst;

        public static void Show(bool add, GameObject[] objs)
        {
            if (inst != null) inst.Close();
            if (!add && !Collections.collections.Any(c => objs.Any(go => c.Contains(go)))) return;
            inst              = CreateInstance<AddRemoveWindow>();
            inst.titleContent = new GUIContent(add ? "Add to Collection" : "Remove from Collection");
            inst.isAdd        = add;
            inst.targets      = objs;
            inst.cols         = add
                                 ? Collections.collections
                                 : Collections.collections.Where(c => objs.Any(go => c.Contains(go))).ToList();
            inst.sel          = new bool[inst.cols.Count];
            inst.position     = new Rect(200, 200, 300, 400);
            inst.ShowUtility();
        }

        void OnDestroy() { if (inst == this) inst = null; }

        void OnGUI()
        {
            EditorGUILayout.LabelField(isAdd ? "Add to:" : "Remove from:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Objects: {targets.Length}", EditorStyles.miniLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < cols.Count; i++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    var colorRect = GUILayoutUtility.GetRect(4, 16, GUILayout.Width(4));
                    EditorGUI.DrawRect(colorRect, cols[i].color);
                    
                    GUILayout.Space(4);
                    sel[i] = EditorGUILayout.ToggleLeft(cols[i].name, sel[i]);
                    
                    GUILayout.FlexibleSpace();
                    var r = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16));
                    EditorGUI.DrawRect(r, cols[i].color);
                }
            }
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel")) Close();
            using (new EditorGUI.DisabledScope(!sel.Any(x => x)))
            {
                if (GUILayout.Button(isAdd ? "Add" : "Remove"))
                {
                    CollectionsHierarchy.Change(isAdd ? "Add to Collections" : "Remove from Collections", () =>
                    {
                        for (int i = 0; i < sel.Length; i++)
                            if (sel[i])
                                foreach (var go in targets)
                                    if (isAdd) cols[i].Add(go);
                                    else cols[i].Remove(go);
                    });
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public class CollectionsWindow : EditorWindow
    {
        private Vector2 scroll;
        private string newName = "";
        private bool options;

        [MenuItem("Tools/Fynn's Tools/Hierarchy+/Collections Manager", false, 96)]
        public static void Open()
        {
            var w = GetWindow<CollectionsWindow>("Collections Manager");
            w.minSize = new Vector2(440, 300);
        }

        private void OnInspectorUpdate() { Repaint(); }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Collections", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Virtual folders keep objects under their real parents. Shared collections stay visible when you focus another collection.", MessageType.None);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(CollectionsHierarchy.Enabled ? "Restore Normal Hierarchy" : "Show Virtual Folders")) CollectionsHierarchy.Toggle();
                if (GUILayout.Button("Show All")) CollectionsHierarchy.FocusCollection(null);
            }
            options = EditorGUILayout.Foldout(options, "Display and Scene Options", true);
            if (options)
            {
                Collections.RemoveFilteredRows = EditorGUILayout.Toggle("Remove Filtered Rows", Collections.RemoveFilteredRows);
                bool hide = EditorGUILayout.Toggle("Hide Others in Scene", Collections.HideFilteredInScene);
                bool editorOnly = EditorGUILayout.Popup("Scene Hiding", Collections.UseEditorVisibility ? 1 : 0, new[] { "Turn Objects Off", "Hide in Editor" }) == 1;
                if (hide != Collections.HideFilteredInScene || editorOnly != Collections.UseEditorVisibility)
                {
                    Collections.HideFilteredInScene = hide;
                    Collections.UseEditorVisibility = editorOnly;
                    CollectionsHierarchy.FocusCollection(Collections.activeFilter?.id);
                }
                if (GUILayout.Button("Clean Missing Members"))
                    CollectionsHierarchy.Change("Clean Collection Members", () => { foreach (var c in Collections.collections) c.CleanupInvalidReferences(); });
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var c in Collections.collections.ToArray())
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        string name = EditorGUILayout.DelayedTextField(c.name);
                        if (name != c.name) CollectionsHierarchy.RenameCollection(c.id, name);
                        Color color = EditorGUILayout.ColorField(GUIContent.none, c.color, false, true, false, GUILayout.Width(45));
                        if (color != c.color) CollectionsHierarchy.SetColor(c.id, color);
                        if (GUILayout.Button("Delete", GUILayout.Width(55)))
                        {
                            CollectionsHierarchy.DeleteCollection(c.id);
                            GUIUtility.ExitGUI();
                        }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        bool shared = EditorGUILayout.ToggleLeft("Always Visible (Shared)", c.alwaysVisible);
                        if (shared != c.alwaysVisible) CollectionsHierarchy.SetShared(c.id, shared);
                        if (GUILayout.Button(c == Collections.activeFilter ? "Show All" : "Focus", GUILayout.Width(65)))
                            CollectionsHierarchy.FocusCollection(c == Collections.activeFilter ? null : c.id);
                    }
                    c.RebuildIDsIfNeeded();
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(c.objectIDs.Count + " members", EditorStyles.miniLabel);
                        if (GUILayout.Button("Select", GUILayout.Width(55))) Selection.objects = c.objectIDs.Select(EditorUtility.InstanceIDToObject).Where(o => o != null).ToArray();
                        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
                        {
                            if (GUILayout.Button("Add Selected")) CollectionsHierarchy.SetMembership(c.id, Selection.gameObjects, true);
                            if (GUILayout.Button("Remove Selected")) CollectionsHierarchy.SetMembership(c.id, Selection.gameObjects, false);
                        }
                    }
                    foreach (int id in c.objectIDs.ToArray())
                    {
                        var go = EditorUtility.InstanceIDToObject(id) as GameObject;
                        if (go == null) continue;
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                            if (GUILayout.Button("Remove", GUILayout.Width(65)))
                                CollectionsHierarchy.SetMembership(c.id, new[] { go }, false);
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            using (new GUILayout.HorizontalScope())
            {
                newName = EditorGUILayout.TextField("New Collection", newName);
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newName)))
                    if (GUILayout.Button("Create", GUILayout.Width(65))) { CollectionsHierarchy.CreateCollection(newName); newName = ""; }
            }
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Created by Fynn9563", EditorStyles.miniLabel);
                if (GUILayout.Button("GitHub", EditorStyles.linkLabel, GUILayout.Width(50))) Application.OpenURL("https://github.com/Fynn9563");
            }
        }
    }
}
#endif
