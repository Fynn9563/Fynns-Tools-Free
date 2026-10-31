using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.MultiUploader.Editor
{
    [CustomEditor(typeof(MultiUploaderSettings))]
    public sealed class MultiUploaderInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Optional per-avatar conversion settings and material overrides. MultiUploader can convert Quest builds without this component; its queue toggle takes precedence. Default: 1024px, ASTC 6×6.", MessageType.Info);
            DrawDefaultInspector();
            if (GUILayout.Button("Compare PC / Quest and review warnings"))
                MultiUploaderWindow.Open(((MultiUploaderSettings)target).gameObject);
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge.Editor", "Fynn.QuestBridge.Editor", "QuestBridgeWindow")]
    public sealed class MultiUploaderWindow : EditorWindow
    {
        [SerializeField] GameObject avatar;
        [SerializeField] bool showSettings;
        [SerializeField] List<GameObject> excluded = new List<GameObject>();
        GameObject[] avatars = Array.Empty<GameObject>();
        PreviewRenderUtility pcPreview, questPreview;
        readonly List<Object> ownedAssets = new List<Object>();
        string report = "Choose an avatar below to preview its Quest conversion.";
        Vector2 scroll;
        Vector2 orbit = new Vector2(180, 0);
        float zoom = 1;
        Bounds bounds;
        bool errors;
        bool startPending;

        [MenuItem("Tools/Fynn's Tools/MultiUploader", false, 94)]
        static void ShowWindow()
        {
            var window = GetWindow<MultiUploaderWindow>("MultiUploader");
            window.minSize = new Vector2(700, 600);
            window.Show();
        }

        public static void Open(GameObject root)
        {
            var window = GetWindow<MultiUploaderWindow>("MultiUploader");
            window.avatar = root;
            window.showSettings = true;
            window.minSize = new Vector2(700, 600);
            window.Show();
        }

        static GameObject FindRoot(GameObject selected)
        {
            if (selected == null) return null;
            foreach (var t in selected.GetComponentsInParent<Transform>(true))
                if (t.GetComponents<Component>().Any(c => c != null
                    && c.GetType().FullName == "VRC.SDK3.Avatars.Components.VRCAvatarDescriptor")) return t.gameObject;
            return selected;
        }

        void OnGUI()
        {
            var preferences = MultiUploaderPreferences.instance;
            preferences.EnsureOpaquePreference();
            var queue = MultiUploaderRunner.Queue;
            bool busy = MultiUploaderRunner.Busy || startPending;
            EditorGUILayout.LabelField("MultiUploader", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Batch avatar uploads · PC and Quest", EditorStyles.miniLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            using (new EditorGUI.DisabledScope(busy))
            {
                EditorGUI.BeginChangeCheck();
                int selectedMode = (int)preferences.platforms;
                selectedMode = GUILayout.Toolbar(selectedMode, new[] { "PC", "Quest", "PC + Quest" });
                preferences.platforms = (UploadPlatforms)selectedMode;
                preferences.quest.convertQuest = EditorGUILayout.ToggleLeft("Auto-convert Quest materials to VRChat Toon Standard", preferences.quest.convertQuest);
                if (EditorGUI.EndChangeCheck()) { preferences.Persist(); Cleanup(); }

                EditorGUILayout.Space(8);
                if (QuestConversionIgnoreEditor.Draw(avatars)) Cleanup();

                EditorGUILayout.Space(8);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Avatars in open scenes", EditorStyles.boldLabel);
                if (GUILayout.Button("All", GUILayout.Width(44))) excluded.Clear();
                if (GUILayout.Button("None", GUILayout.Width(48))) excluded = avatars.Where(a => a != null).ToList();
                if (GUILayout.Button("Refresh", GUILayout.Width(70))) RefreshAvatars();
                EditorGUILayout.EndHorizontal();
                foreach (var root in avatars)
                {
                    if (root == null) continue;
                    EditorGUILayout.BeginHorizontal();
                    bool chosen = !excluded.Contains(root);
                    bool next = EditorGUILayout.Toggle(chosen, GUILayout.Width(20));
                    if (next != chosen) { if (next) excluded.Remove(root); else excluded.Add(root); }
                    EditorGUILayout.ObjectField(root, typeof(GameObject), true);
                    var pipeline = root.GetComponent<VRC.Core.PipelineManager>();
                    GUILayout.Label(string.IsNullOrWhiteSpace(pipeline?.blueprintId) ? "No ID · publish skipped" : pipeline.blueprintId,
                        EditorStyles.miniLabel, GUILayout.MaxWidth(305));
                    EditorGUILayout.EndHorizontal();
                }
                if (avatars.Length == 0) EditorGUILayout.HelpBox("No active scene avatars with a descriptor and PipelineManager were found.", MessageType.Info);
                UploadIgnoreEditor.Draw(excluded);
                int selected = avatars.Count(a => a != null && !excluded.Contains(a));
                EditorGUILayout.LabelField($"{selected} avatar(s) selected · current platform: {MultiUploaderRunner.PlatformName(EditorUserBuildSettings.activeBuildTarget)}", EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(selected == 0))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Build Only All", GUILayout.Height(30))) StartQueue(UploadOperation.BuildOnly);
                    if (GUILayout.Button("Build & Test All", GUILayout.Height(30))) StartQueue(UploadOperation.BuildAndTest);
                    if (GUILayout.Button("Build & Publish All", GUILayout.Height(30))) StartQueue(UploadOperation.Publish);
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (queue.jobs.Count > 0)
            {
                EditorGUILayout.Space(8);
                Rect progress = GUILayoutUtility.GetRect(100, 20, GUILayout.ExpandWidth(true));
                int finished = queue.jobs.Count(j => j.status != UploadJobStatus.Pending && j.status != UploadJobStatus.Running);
                EditorGUI.ProgressBar(progress, (float)finished / queue.jobs.Count, $"{finished}/{queue.jobs.Count} jobs · {queue.Completed} succeeded");
                EditorGUILayout.HelpBox(queue.status ?? "", queue.phase == QueuePhase.Interrupted ? MessageType.Warning : MessageType.Info);
                EditorGUILayout.LabelField("This queue: Quest conversion " + (queue.quest.convertQuest ? "on" : "off")
                    + " · opaque approximation " + (queue.quest.approximateTransparencyAsOpaque ? "on" : "off"), EditorStyles.miniLabel);
                if (busy && GUILayout.Button(queue.cancelRequested ? "Cancellation requested…" : "Cancel queue")) MultiUploaderRunner.Cancel();
                foreach (var job in queue.jobs)
                    EditorGUILayout.LabelField($"{job.status} · {job.avatarName} · {MultiUploaderRunner.PlatformName(job.platform)}\n{job.detail}", EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.Space(10);
            showSettings = EditorGUILayout.Foldout(showSettings, "Settings · Quest conversion and preview", true);
            if (showSettings)
            {
                using (new EditorGUI.DisabledScope(busy))
                {
                    var serialized = new SerializedObject(preferences);
                    serialized.Update();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(serialized.FindProperty("restoreStartingPlatform"), new GUIContent("Return to starting platform"));
                    EditorGUILayout.PropertyField(serialized.FindProperty("continueAfterFailure"), new GUIContent("Continue after a failed avatar"));
                    var quest = serialized.FindProperty("quest");
                    int size = quest.FindPropertyRelative("maxTextureSize").intValue;
                    int[] sizes = { 256, 512, 1024, 2048 };
                    int index = Mathf.Max(0, Array.IndexOf(sizes, size));
                    index = EditorGUILayout.Popup("Quest texture size", index, new[] { "256", "512", "1024", "2048" });
                    quest.FindPropertyRelative("maxTextureSize").intValue = sizes[index];
                    EditorGUILayout.PropertyField(quest.FindPropertyRelative("bakeStaticPoiyomiDecals"), new GUIContent("Bake static Poiyomi decals"));
                    EditorGUILayout.PropertyField(quest.FindPropertyRelative("removeQuestAudio"), new GUIContent("Remove unsupported Quest audio"));
                    EditorGUILayout.PropertyField(quest.FindPropertyRelative("approximateTransparencyAsOpaque"), new GUIContent("Approximate transparency as opaque"));
                    EditorGUILayout.PropertyField(quest.FindPropertyRelative("blockUnsafeConversions"), new GUIContent("Block unsafe conversions"));
                    EditorGUILayout.PropertyField(quest.FindPropertyRelative("useAvatarOverrides"), new GUIContent("Use existing per-avatar overrides"));
                    EditorGUILayout.PropertyField(quest.FindPropertyRelative("overrides"), new GUIContent("Shared material overrides"), true);
                    bool changed = EditorGUI.EndChangeCheck();
                    if (serialized.ApplyModifiedProperties() || changed) { preferences.Persist(); Cleanup(); }
                    EditorGUILayout.HelpBox("Conversion runs only for Quest jobs when enabled. PC materials stay untouched. No converter component is required on the avatar. Publish updates existing blueprint IDs; avatars with no ID are skipped.", MessageType.Info);
                    EditorGUI.BeginChangeCheck();
                    avatar = (GameObject)EditorGUILayout.ObjectField("Preview avatar", avatar, typeof(GameObject), true);
                    if (EditorGUI.EndChangeCheck()) { avatar = FindRoot(avatar); Cleanup(); }
                    using (new EditorGUI.DisabledScope(avatar == null || EditorUtility.IsPersistent(avatar) || !preferences.quest.convertQuest))
                        if (GUILayout.Button("Generate PC / Quest comparison")) Generate(preferences.quest);
                }
                if (!preferences.quest.convertQuest) EditorGUILayout.HelpBox("Quest conversion is off. Quest jobs use the avatar's existing materials, and the SDK still checks mobile compatibility.", MessageType.Info);
                if (pcPreview != null && questPreview != null)
                {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("PC source appearance");
                EditorGUILayout.LabelField("Quest · Toon Standard");
                EditorGUILayout.EndHorizontal();
                Rect area = GUILayoutUtility.GetRect(100, 270, GUILayout.ExpandWidth(true));
                HandleOrbit(area);
                DrawPreview(pcPreview, new Rect(area.x, area.y, area.width / 2 - 2, area.height));
                DrawPreview(questPreview, new Rect(area.x + area.width / 2 + 2, area.y, area.width / 2 - 2, area.height));
                EditorGUILayout.LabelField("Drag to rotate · scroll to zoom. Static authored preview; final Modular Avatar/VRCFury output is processed at build time.", EditorStyles.wordWrappedMiniLabel);
                }
                EditorGUILayout.HelpBox(report, errors ? MessageType.Error : MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
        }

        void StartQueue(UploadOperation operation)
        {
            if (startPending || MultiUploaderRunner.Busy) return;
            var selected = avatars.Where(a => a != null && !excluded.Contains(a)).ToArray();
            startPending = true;
            EditorApplication.delayCall += () =>
            {
                startPending = false;
                if (!this) return;
                Cleanup();
                try { MultiUploaderRunner.Start(selected, operation); }
                catch (Exception e) { EditorUtility.DisplayDialog("MultiUploader", e.GetBaseException().Message, "OK"); }
                Repaint();
            };
            Repaint();
        }

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            MultiUploaderRunner.Changed += Repaint;
            EditorApplication.hierarchyChanged += RefreshAvatars;
            RefreshAvatars();
        }

        void RefreshAvatars() { avatars = MultiUploaderRunner.FindAvatars(); Repaint(); }

        void OnUndoRedo() { Cleanup(); Repaint(); }

        void Generate(QuestUploadOptions options)
        {
            Cleanup();
            GameObject pc = null, quest = null;
            try
            {
                pc = Object.Instantiate(avatar);
                quest = Object.Instantiate(avatar);
                pc.name = "PC comparison";
                quest.name = "Quest comparison";
                pc.hideFlags = quest.hideFlags = HideFlags.HideAndDontSave;
                var settings = quest.GetComponent<MultiUploaderSettings>() ?? quest.AddComponent<MultiUploaderSettings>();
                options.Apply(settings);
                var converter = new QuestConverter(settings, asset => ownedAssets.Add(asset));
                converter.Convert(quest);
                report = converter.Report;
                errors = converter.HasErrors;
                foreach (var root in new[] { pc, quest })
                    foreach (var script in root.GetComponentsInChildren<MonoBehaviour>(true))
                        if (script != null) Object.DestroyImmediate(script);
                var renderers = pc.GetComponentsInChildren<Renderer>(true);
                bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(pc.transform.position, Vector3.one);
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                pcPreview = MakePreview(pc); pc = null;
                questPreview = MakePreview(quest); quest = null;
            }
            catch (Exception e) { report = "Comparison failed: " + e.GetBaseException().Message; errors = true; Cleanup(); Debug.LogException(e); }
            finally { if (pc != null) Object.DestroyImmediate(pc); if (quest != null) Object.DestroyImmediate(quest); }
        }

        static PreviewRenderUtility MakePreview(GameObject root)
        {
            var preview = new PreviewRenderUtility();
            preview.cameraFieldOfView = 30;
            preview.ambientColor = new Color(.35f, .35f, .35f);
            preview.lights[0].intensity = 1;
            preview.lights[0].transform.rotation = Quaternion.Euler(40, 40, 0);
            preview.lights[1].intensity = .4f;
            preview.AddSingleGO(root);
            return preview;
        }

        void HandleOrbit(Rect rect)
        {
            var e = Event.current;
            if (!rect.Contains(e.mousePosition)) return;
            if (e.type == EventType.MouseDrag && e.button == 0) { orbit += e.delta * .5f; e.Use(); Repaint(); }
            if (e.type == EventType.ScrollWheel) { zoom = Mathf.Clamp(zoom * (1 + e.delta.y * .05f), .2f, 4); e.Use(); Repaint(); }
        }

        void DrawPreview(PreviewRenderUtility preview, Rect rect)
        {
            if (Event.current.type != EventType.Repaint) return;
            preview.BeginPreview(rect, GUIStyle.none);
            float distance = Mathf.Max(.1f, bounds.extents.magnitude) * 4 * zoom;
            Quaternion rotation = Quaternion.Euler(-orbit.y, -orbit.x, 0);
            preview.camera.transform.SetPositionAndRotation(bounds.center + rotation * Vector3.back * distance, rotation);
            preview.camera.nearClipPlane = Mathf.Max(.001f, distance / 1000);
            preview.camera.farClipPlane = distance * 10 + 100;
            preview.camera.clearFlags = CameraClearFlags.Color;
            preview.camera.backgroundColor = new Color(.12f, .13f, .15f);
            preview.camera.Render();
            GUI.DrawTexture(rect, preview.EndPreview(), ScaleMode.StretchToFill, false);
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            MultiUploaderRunner.Changed -= Repaint;
            EditorApplication.hierarchyChanged -= RefreshAvatars;
            Cleanup();
        }
        void Cleanup()
        {
            pcPreview?.Cleanup(); pcPreview = null;
            questPreview?.Cleanup(); questPreview = null;
            foreach (var asset in ownedAssets) if (asset != null && !EditorUtility.IsPersistent(asset)) Object.DestroyImmediate(asset);
            ownedAssets.Clear();
        }
    }
}
