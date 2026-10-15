using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.Clipping
{
    public sealed class ClippingTesterWindow : EditorWindow
    {
        [Serializable] private sealed class SavedPose
        {
            public string Name;
            public AnimationClip Clip;
            public float Time;
        }
        [SerializeField] private GameObject avatar;
        [SerializeField] private List<AnimationClip> clips = new List<AnimationClip>();
        [SerializeField] private List<SavedPose> poses = new List<SavedPose>();
        [SerializeField] private AnimationClip selectedClip;
        [SerializeField] private bool prepareClothing;
        [SerializeField] private bool clothingDefaultsSet;
        [SerializeField] private bool simulatePhysBones = true;
        [SerializeField] private float speed = 1;
        [SerializeField] private bool cyclePlaylist;
        [SerializeField] private bool loopSection;
        [SerializeField] private Vector2 loop = new Vector2(0, 1);
        [SerializeField] private bool meshWireframe = true;
        private ClippingPreview preview;
        private readonly List<MeshSurface> bodyMeshes = new List<MeshSurface>();
        private readonly List<MeshSurface> clothingMeshes = new List<MeshSurface>();
        private bool contrast, playing, showClips, showInspection = true, showPoses;
        private float time;
        private double lastUpdate, lastSample;
        private string message, poseName = "Clipping issue";
        private Vector2 scroll;
        private AnimationClip addClip;
        private GUIStyle creditStyle;
        public ClippingPreview Preview => preview;
        public float PreviewTime => time;
        public bool Playing => playing;

        [MenuItem("Tools/Fynn's Tools/Clipping Tester", false, 31)]
        public static void ShowWindow()
        {
            var window = GetWindow<ClippingTesterWindow>("Clipping Tester");
            window.minSize = new Vector2(430, 520); window.Show();
        }
        private void OnEnable()
        {
            titleContent = new GUIContent("Clipping Tester");
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.quitting += Stop;
            if (clips.Count == 0) LoadDefaults();
            if (!clothingDefaultsSet && avatar) SetClothingDefault();
            lastUpdate = EditorApplication.timeSinceStartup;
        }
        private void OnDisable()
        {
            Stop();
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Stop;
            EditorApplication.quitting -= Stop;
        }
        private void PlayModeChanged(PlayModeStateChange change) { if (change == PlayModeStateChange.ExitingEditMode) Stop(); }
        private void LoadDefaults()
        {
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var folder = Path.GetDirectoryName(Path.GetDirectoryName(path))?.Replace('\\', '/') + "/Animations";
            if (!AssetDatabase.IsValidFolder(folder)) return;
            foreach (var guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { folder }))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid));
                if (clip && !clips.Contains(clip)) clips.Add(clip);
            }
            clips = clips.OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase).ToList();
            if (!selectedClip) selectedClip = clips.FirstOrDefault();
        }
        public void SetAvatar(GameObject target)
        {
            if (avatar == target) return;
            Stop(); bodyMeshes.Clear(); clothingMeshes.Clear(); avatar = target; SetClothingDefault(); message = null;
        }
        private void SetClothingDefault()
        {
            prepareClothing = ClippingPreview.ClothingAvailable && ClippingPreview.HasClothingTools(avatar);
            clothingDefaultsSet = true;
        }
        public void AddClip(AnimationClip clip)
        {
            if (!clip || clips.Contains(clip)) return;
            clips.Add(clip); if (!selectedClip) selectedClip = clip;
        }
        public void SelectClip(AnimationClip clip)
        {
            if (selectedClip == clip) return;
            selectedClip = clip; time = 0;
            if (preview != null) Try(() => preview.Sample(selectedClip, time));
        }
        public void StartPreview()
        {
            if (!clothingDefaultsSet) SetClothingDefault();
            if (!selectedClip) { message = "Choose an animation clip first."; return; }
            Try(() =>
            {
                if (preview == null || !preview.IsValid)
                {
                    float requestedTime = time;
                    Stop(); time = requestedTime; preview = new ClippingPreview(); preview.Start(avatar, prepareClothing, simulatePhysBones);
                    preview.SetWireframe(meshWireframe);
                    foreach (var surface in bodyMeshes.Concat(clothingMeshes))
                    {
                        surface.SourceRenderer = surface.Renderer;
                        surface.Renderer = preview.PreviewRenderer(surface.Renderer);
                    }
                    contrast = false;
                }
                preview.Sample(selectedClip, time);
                playing = true; lastUpdate = EditorApplication.timeSinceStartup;
                message = preview.ClothingStatus;
                if (bodyMeshes.Concat(clothingMeshes).Any(s => s.SourceRenderer && !s.Renderer))
                    message += " A build tool combined or removed a selected mesh. Choose its replacement in the preview before inspecting.";
            });
        }
        public void Stop()
        {
            playing = false;
            if (preview != null)
                foreach (var surface in bodyMeshes.Concat(clothingMeshes))
                {
                    if (surface.SourceRenderer || surface.Renderer && preview.Root && surface.Renderer.transform.IsChildOf(preview.Root.transform))
                        surface.Renderer = preview.OriginalRenderer(surface.Renderer) ?? surface.SourceRenderer;
                    surface.SourceRenderer = null;
                }
            preview?.Dispose(); preview = null; contrast = false; time = 0;
            Repaint();
        }
        public void Seek(float value)
        {
            if (!selectedClip) return;
            playing = false;
            time = Mathf.Clamp(value, 0, selectedClip.length);
            if (preview == null) { StartPreview(); playing = false; }
            else Try(() => preview.Sample(selectedClip, time));
            Repaint();
        }
        private void Tick()
        {
            double now = EditorApplication.timeSinceStartup;
            double delta = Math.Min(0.1, now - lastUpdate); lastUpdate = now;
            if (preview == null) return;
            if (!avatar || !preview.IsValid) { Stop(); message = "Preview ended. Press Play to start again."; return; }
            Try(() =>
            {
                if (!playing || !selectedClip) return;
                float start = loopSection ? loop.x * selectedClip.length : 0;
                float end = loopSection ? loop.y * selectedClip.length : selectedClip.length;
                if (time < start) time = start;
                time += (float)delta * speed;
                if (end <= start + 0.00001f) { time = start; playing = false; }
                else if (time > end)
                {
                    if (cyclePlaylist && !loopSection && clips.Count(c => c) > 1)
                    {
                        var available = clips.Where(c => c).ToList();
                        SelectClip(available[(available.IndexOf(selectedClip) + 1) % available.Count]);
                    }
                    else time = start + (time - start) % (end - start);
                }
                if (now - lastSample >= 1.0 / 30)
                { preview.Sample(selectedClip, time, Mathf.Clamp((float)(now - lastSample) * speed, 0.001f, 0.05f)); lastSample = now; Repaint(); }
            });
        }
        private void Try(Action action)
        {
            try { action(); }
            catch (Exception e)
            {
                Stop();
                message = e.Message; playing = false; Repaint();
            }
        }
        private void Defer(Action action)
        {
            EditorApplication.delayCall += () => { if (this) { Try(action); Repaint(); } };
        }
        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Clipping Tester", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Drop an avatar, choose an animation, and press Play. Preview changes stay on a temporary copy.", EditorStyles.wordWrappedMiniLabel);
            EditorGUI.BeginChangeCheck();
            var target = (GameObject)EditorGUILayout.ObjectField("Avatar", avatar, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck()) Defer(() => SetAvatar(target));
            if (GUILayout.Button("Use Selected Avatar"))
            {
                var selected = Selection.activeGameObject;
                var animator = selected ? selected.GetComponentsInParent<Animator>(true).LastOrDefault() : null;
                Defer(() => SetAvatar(animator ? animator.gameObject : selected));
            }
            using (new EditorGUI.DisabledScope(preview != null || !ClippingPreview.ClothingAvailable))
                prepareClothing = EditorGUILayout.Toggle(new GUIContent("Prepare Clothing", "Optional: run installed avatar build tools on the preview copy so Modular Avatar and VRCFury attachments follow the avatar. Rebuild the preview after changing clothing."), prepareClothing);
            if (prepareClothing) EditorGUILayout.LabelField("Uses installed build tools; they may generate their normal cached assets.", EditorStyles.wordWrappedMiniLabel);
            else if (ClippingPreview.HasClothingTools(avatar))
                EditorGUILayout.HelpBox(ClippingPreview.ClothingAvailable
                    ? "Clothing attachment is off. Enable Prepare Clothing before Play to attach VRCFury or Modular Avatar outfits."
                    : "Clothing attachment needs the Avatar SDK and VRCFury or Modular Avatar.", MessageType.Warning);
            using (new EditorGUI.DisabledScope(preview != null || !ClippingPhysics.Available))
                simulatePhysBones = EditorGUILayout.Toggle("Simulate PhysBones", simulatePhysBones);
            if (preview != null && simulatePhysBones) EditorGUILayout.LabelField(preview.PhysicsStatus, EditorStyles.wordWrappedMiniLabel);
            var available = clips.Where(c => c).ToList();
            if (available.Count > 0)
            {
                int index = Mathf.Max(0, available.IndexOf(selectedClip));
                int next = EditorGUILayout.Popup("Animation", index, available.Select(c => c.name).ToArray());
                if (next != index || !selectedClip) Defer(() => SelectClip(available[next]));
            }
            else EditorGUILayout.HelpBox("Add an animation below to start.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(playing ? "Pause" : "Play")) { if (playing) playing = false; else Defer(StartPreview); }
                if (GUILayout.Button("Stop")) Defer(Stop);
                using (new EditorGUI.DisabledScope(preview == null))
                    if (GUILayout.Button("Frame Avatar")) preview?.FrameAvatar();
            }
            speed = EditorGUILayout.Slider("Speed", speed, 0.1f, 3);
            using (new EditorGUILayout.HorizontalScope())
                foreach (float preset in new[] { 0.1f, 0.5f, 1f, 1.5f, 2f }) if (GUILayout.Button(preset + "x")) speed = preset;
            using (new EditorGUI.DisabledScope(!selectedClip))
            {
                EditorGUI.BeginChangeCheck();
                float nextTime = EditorGUILayout.Slider("Time (seconds)", time, 0, selectedClip ? selectedClip.length : 1);
                if (EditorGUI.EndChangeCheck()) Defer(() => Seek(nextTime));
                using (new EditorGUILayout.HorizontalScope())
                {
                    float frame = selectedClip && selectedClip.frameRate > 0 ? 1 / selectedClip.frameRate : 1 / 60f;
                    if (GUILayout.Button("Previous Frame")) Defer(() => Seek(time - frame));
                    if (GUILayout.Button("Next Frame")) Defer(() => Seek(time + frame));
                }
                loopSection = EditorGUILayout.Toggle("Loop Section", loopSection);
                if (loopSection)
                {
                    EditorGUILayout.MinMaxSlider("Loop Range", ref loop.x, ref loop.y, 0, 1);
                    EditorGUILayout.LabelField("", selectedClip ? (loop.x * selectedClip.length).ToString("0.00") + " – " + (loop.y * selectedClip.length).ToString("0.00") + " seconds" : "");
                }
                cyclePlaylist = EditorGUILayout.Toggle("Cycle Playlist", cyclePlaylist);
            }
            DrawClipList();
            DrawPoses();
            DrawInspection();
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
            EditorGUILayout.EndScrollView();
            if (creditStyle == null) creditStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
            if (GUILayout.Button(new GUIContent("Created by Fynn9563", "https://github.com/Fynn9563"), creditStyle)) Application.OpenURL("https://github.com/Fynn9563");
            HandleClipDrop();
        }
        private void DrawClipList()
        {
            showClips = EditorGUILayout.Foldout(showClips, "Animation Playlist (" + clips.Count(c => c) + ")", true);
            if (!showClips) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                addClip = (AnimationClip)EditorGUILayout.ObjectField(addClip, typeof(AnimationClip), false);
                if (GUILayout.Button("Add", GUILayout.Width(50))) { AddClip(addClip); addClip = null; }
                if (GUILayout.Button("Defaults", GUILayout.Width(70))) LoadDefaults();
            }
            for (int i = 0; i < clips.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var listedClip = clips[i];
                    if (GUILayout.Button(listedClip ? listedClip.name : "Missing clip")) Defer(() => SelectClip(listedClip));
                    if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0) { var c = clips[i]; clips[i] = clips[i - 1]; clips[i - 1] = c; }
                    if (GUILayout.Button("×", GUILayout.Width(25)))
                    {
                        var clip = clips[i]; clips.RemoveAt(i);
                        if (selectedClip == clip) { playing = false; Defer(() => SelectClip(clips.FirstOrDefault(c => c))); }
                        break;
                    }
                }
            }
            EditorGUILayout.LabelField("You can also drop clips or an FBX containing clips into this window.", EditorStyles.wordWrappedMiniLabel);
        }
        private void HandleClipDrop()
        {
            var evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;
            var dropped = new List<AnimationClip>();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is AnimationClip clip) dropped.Add(clip);
                else if (EditorUtility.IsPersistent(obj))
                    dropped.AddRange(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(obj)).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)));
            }
            if (dropped.Count == 0) return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform) { DragAndDrop.AcceptDrag(); foreach (var clip in dropped) AddClip(clip); }
            evt.Use();
        }
        private void DrawPoses()
        {
            showPoses = EditorGUILayout.Foldout(showPoses, "Saved Problem Poses (" + poses.Count + ")", true);
            if (!showPoses) return;
            using (new EditorGUILayout.HorizontalScope())
            {
                poseName = EditorGUILayout.TextField(poseName);
                using (new EditorGUI.DisabledScope(!selectedClip))
                    if (GUILayout.Button("Save Pose", GUILayout.Width(90))) poses.Add(new SavedPose { Name = poseName, Clip = selectedClip, Time = time });
            }
            for (int i = 0; i < poses.Count; i++)
                using (new EditorGUILayout.HorizontalScope())
                {
                    var pose = poses[i];
                    using (new EditorGUI.DisabledScope(!pose.Clip))
                        if (GUILayout.Button(pose.Name + " · " + (pose.Clip ? pose.Clip.name : "Missing") + " · " + pose.Time.ToString("0.00") + "s"))
                        Defer(() => { AddClip(pose.Clip); SelectClip(pose.Clip); Seek(pose.Time); });
                    if (GUILayout.Button("×", GUILayout.Width(25))) { poses.RemoveAt(i); break; }
                }
        }
        private Renderer RendererPopup(string label, Renderer current)
        {
            var renderers = preview?.Renderers ?? (avatar ? avatar.GetComponentsInChildren<Renderer>(true).Where(ClippingPreview.IsMeshRenderer).ToArray() : Array.Empty<Renderer>());
            var root = preview?.Root ?? avatar;
            var options = new[] { "Choose mesh…" }.Concat(renderers.Select(r => AnimationUtility.CalculateTransformPath(r.transform, root.transform) + " (" + r.GetType().Name + ")")).ToArray();
            int index = Array.IndexOf(renderers, current) + 1;
            int chosen = EditorGUILayout.Popup(label, index, options);
            return chosen <= 0 ? null : renderers[chosen - 1];
        }
        private int SlotPopup(string label, Renderer renderer, int current)
        {
            Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : renderer ? renderer.GetComponent<MeshFilter>()?.sharedMesh : null;
            if (!mesh) return -1;
            var materials = renderer.sharedMaterials;
            var options = new[] { "All material slots" }.Concat(Enumerable.Range(0, mesh.subMeshCount).Select(i => "Slot " + i + (i < materials.Length && materials[i] ? " · " + materials[i].name : ""))).ToArray();
            return EditorGUILayout.Popup(label, Mathf.Clamp(current + 1, 0, options.Length - 1), options) - 1;
        }
        private void DrawInspection()
        {
            showInspection = EditorGUILayout.Foldout(showInspection, "Visual Inspection", true);
            if (!showInspection) return;
            using (new EditorGUI.DisabledScope(!avatar))
            {
                EditorGUI.BeginChangeCheck();
                DrawMeshGroup("Body", bodyMeshes);
                DrawMeshGroup("Clothing", clothingMeshes);
                bool validSelection = bodyMeshes.Concat(clothingMeshes).All(s => s.Renderer) && MeshSurface.ValidPair(bodyMeshes, clothingMeshes);
                if (!validSelection && contrast) { contrast = false; GUI.changed = true; }
                using (new EditorGUI.DisabledScope(!validSelection || preview == null))
                    contrast = EditorGUILayout.Toggle("Contrast Colours", contrast);
                if (EditorGUI.EndChangeCheck()) Try(() => preview?.SetContrast(bodyMeshes, clothingMeshes, contrast));
                bool nextWireframe = EditorGUILayout.Toggle("Mesh Wireframe", meshWireframe);
                if (nextWireframe != meshWireframe)
                { meshWireframe = nextWireframe; preview?.SetWireframe(meshWireframe); }
            }
            if (contrast) EditorGUILayout.LabelField("Contrast replaces materials. Turn it off to check the actual appearance.", EditorStyles.wordWrappedMiniLabel);
        }
        private void DrawMeshGroup(string label, List<MeshSurface> surfaces)
        {
            EditorGUILayout.LabelField(label + " Meshes", EditorStyles.boldLabel);
            for (int i = 0; i < surfaces.Count; i++)
            {
                var surface = surfaces[i];
                bool remove;
                using (new EditorGUILayout.HorizontalScope())
                {
                    var next = RendererPopup("Mesh " + (i + 1), surface.Renderer);
                    if (next != surface.Renderer)
                    { surface.Renderer = next; surface.SourceRenderer = preview?.OriginalRenderer(next); surface.Slot = -1; }
                    remove = GUILayout.Button("×", GUILayout.Width(25));
                }
                if (remove) { surfaces.RemoveAt(i); GUI.changed = true; break; }
                surface.Slot = SlotPopup("Material Slots", surface.Renderer, surface.Slot);
            }
            if (GUILayout.Button("Add " + label + " Mesh")) { surfaces.Add(new MeshSurface()); GUI.changed = true; }
        }
    }
}
