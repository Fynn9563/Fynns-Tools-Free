#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    internal sealed class DynamicsEditorWindow : EditorWindow
    {
        private static DynamicsEditorWindow activeWindow;
        private static readonly int[] TabOrder = { 0, 3, 4, 1, 2 };
        private static readonly string[] TabNames = { "Components", "Organize", "Remove", "Scene Tools", "Settings" };
        internal static bool EditingScene => activeWindow != null && activeWindow.tab < 3 && activeWindow.selected != null &&
            activeWindow.sceneTools.mode != DynamicsSceneTools.Mode.None && DynamicsSettings.instance.sceneTools;
        internal static bool OwnsScene(SceneView view) => activeWindow != null &&
            (activeWindow.preview.Running ? activeWindow.preview.IsView(view) : EditingScene);
        [SerializeField] private GameObject root;
        [SerializeField] private Component selected;
        [SerializeField] private List<Component> selection = new List<Component>();
        [SerializeField] private string search = "";
        [SerializeField] private int category;
        [SerializeField] private int tab;
        [SerializeField] private DynamicsManagementPanel management = new DynamicsManagementPanel();
        private bool replaceOriginal;
        private readonly List<Component> components = new List<Component>();
        private UnityEditor.Editor inspector;
        private Vector2 listScroll;
        private Vector2 inspectorScroll;
        private Vector2 settingsScroll;
        private bool needsScan = true;
        private readonly DynamicsSceneTools sceneTools = new DynamicsSceneTools();
        private readonly DynamicsPreview preview = new DynamicsPreview();
        private Component previewOriginal;
        private readonly List<Component> previewSelection = new List<Component>();
        private GameObject EditingRoot => preview.Running ? preview.Root : root;

        internal static void OpenMain()
        {
            OpenFor(null);
            activeWindow.tab = 0;
        }

        internal static void OpenSettings()
        {
            OpenFor(null);
            activeWindow.tab = 2;
        }

        internal static void OpenManagement(bool removing)
        {
            OpenFor(null);
            activeWindow.StopPreview();
            activeWindow.tab = removing ? 4 : 3;
            activeWindow.management.Invalidate();
        }

        internal static void OpenFor(Component component)
        {
            var window = GetWindow<DynamicsEditorWindow>("Dynamics+");
            activeWindow = window;
            window.minSize = new Vector2(760, 520);
            if (component != null)
            {
                window.StopPreview();
                window.root = component.transform.root.gameObject;
                window.Choose(component);
            }
            else if (window.root == null) window.root = Selection.activeGameObject;
            window.needsScan = true;
        }

        private void OnEnable()
        {
            activeWindow = this;
            EditorApplication.hierarchyChanged += Invalidate;
            Undo.undoRedoPerformed += Invalidate;
            SceneView.duringSceneGui += OnSceneGUI;
            needsScan = true;
        }

        private void OnDisable()
        {
            if (activeWindow == this) activeWindow = null;
            EditorApplication.hierarchyChanged -= Invalidate;
            Undo.undoRedoPerformed -= Invalidate;
            SceneView.duringSceneGui -= OnSceneGUI;
            preview.Dispose();
            ReleaseInspector();
        }

        private void Invalidate() { needsScan = true; management.Invalidate(); Repaint(); }

        private void ReleaseInspector()
        {
            if (inspector != null) DestroyImmediate(inspector);
            inspector = null;
        }

        private void Scan()
        {
            needsScan = false;
            components.Clear();
            if (EditingRoot != null)
            {
                foreach (var component in EditingRoot.GetComponentsInChildren<Component>(true))
                {
                    if (component is VRCPhysBoneBase || component is VRCPhysBoneColliderBase || component is ContactBase)
                        components.Add(component);
                }
            }
            if (selected != null && !components.Contains(selected)) selected = null;
            selection.RemoveAll(c => c == null || !components.Contains(c));
            if (selected != null && !selection.Contains(selected)) selection.Add(selected);
            if (selected == null) ReleaseInspector();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope()) DrawContent();
            DynamicsInfo.DrawCredit();
        }

        private void DrawContent()
        {
            RecoverPreviewSelection();
            if (DynamicsPreview.Active != null && DynamicsPreview.Active != preview)
            {
                EditorGUILayout.HelpBox("A PhysBone test is running. Use its component inspectors, or stop it to use this browser.", MessageType.Info);
                if (GUILayout.Button("Stop Testing")) DynamicsPreview.Active.Dispose();
                return;
            }
            EditorGUILayout.LabelField("Dynamics+", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("PhysBones, colliders, and Contacts for avatars and worlds.");
            var visibleTab = Mathf.Max(0, Array.IndexOf(TabOrder, tab));
            tab = TabOrder[GUILayout.Toolbar(visibleTab, TabNames, GUILayout.Height(28))];
            if (tab == 2)
            {
                settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll);
                DynamicsSettings.instance.Draw();
                EditorGUILayout.Space();
                DynamicsSettings.instance.DrawCosmetic();
                EditorGUILayout.EndScrollView();
                return;
            }
            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(preview.Running))
                root = (GameObject)EditorGUILayout.ObjectField("Root", root, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck()) { needsScan = true; management.Invalidate(); }
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(preview.Running))
                    if (GUILayout.Button("Use Selection")) { root = Selection.activeGameObject; needsScan = true; }
                if (GUILayout.Button("Refresh")) { needsScan = true; management.Invalidate(); }
            }
            DrawTesting();
            if (tab == 3 || tab == 4) { management.Draw(root, tab == 4); return; }
            if (tab == 1) { sceneTools.DrawControls(selected); }
            search = EditorGUILayout.TextField("Search", search);
            category = EditorGUILayout.Popup("Show", category, new[] { "All", "PhysBones", "Colliders", "Contacts" });
            if (needsScan) Scan();
            if (EditingRoot == null) return;
            if (EditorUtility.IsPersistent(EditingRoot) || !EditingRoot.scene.IsValid())
            {
                EditorGUILayout.HelpBox("Open the prefab in Prefab Mode to edit its components.", MessageType.Info);
                return;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(270)))
                {
                    EditorGUILayout.LabelField("Dynamics components", components.Count.ToString());
                    listScroll = EditorGUILayout.BeginScrollView(listScroll);
                    foreach (var component in components)
                    {
                        if (component == null) continue;
                        if (!Matches(component)) continue;
                        var label = new GUIContent(component.gameObject.name + " · " + component.GetType().Name,
                            AnimationUtility.CalculateTransformPath(component.transform, EditingRoot.transform));
                        if (GUILayout.Toggle(selection.Contains(component), label, "Button") != selection.Contains(component))
                        {
                            if ((Event.current.control || Event.current.command) && selection.Count > 0 && component.GetType() == selection[0].GetType())
                            {
                                if (selection.Contains(component)) selection.Remove(component); else selection.Add(component);
                                selected = selection.LastOrDefault();
                                RememberPreviewSelection();
                                ReleaseInspector();
                            }
                            else Choose(component);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    if (selected == null) { EditorGUILayout.LabelField("Choose a component to edit."); return; }
                    EditorGUILayout.LabelField(selection.Count > 1 ? selection.Count + " selected · Ctrl-click to change selection" : "Ctrl-click matching types to edit multiple components.", EditorStyles.wordWrappedMiniLabel);
                    if (GUILayout.Button("Select in Hierarchy")) Selection.activeGameObject = selected.gameObject;
                    using (new EditorGUI.DisabledScope(preview.Running)) DrawConversions();
                    DrawOperations();
                    inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
                    UnityEditor.Editor.CreateCachedEditor(selection.Count == 0 ? new UnityEngine.Object[] { selected } : selection.Cast<UnityEngine.Object>().ToArray(), DynamicsSdkEditors.TypeFor(selected), ref inspector);
                    using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                    {
                        EditorGUI.BeginChangeCheck();
                        inspector.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck() && preview.Running)
                            foreach (var component in selection)
                            {
                                if (component is VRCPhysBoneBase bone) bone.ApplyConfigurationChanges();
                                if (component is VRCPhysBoneColliderBase collider) collider.ApplyConfigurationChanges();
                            }
                    }
                    EditorGUILayout.EndScrollView();
                    using (new EditorGUI.DisabledScope(preview.Running || EditorApplication.isPlayingOrWillChangePlaymode))
                        DynamicsAvatarBridge.Draw(selected);
                }
            }
        }

        private void DrawConversions()
        {
            if (!(selected is ContactBase) && !(selected is VRCPhysBoneColliderBase)) return;
            EditorGUILayout.LabelField("Create from this shape", EditorStyles.boldLabel);
            replaceOriginal = EditorGUILayout.Toggle("Replace original component", replaceOriginal);
            EditorGUILayout.LabelField("New behavior settings use SDK defaults.", EditorStyles.wordWrappedMiniLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawConversion("Contact Sender", DynamicsConversions.ComponentKind.Sender);
                DrawConversion("Contact Receiver", DynamicsConversions.ComponentKind.Receiver);
                DrawConversion("PhysBone Collider", DynamicsConversions.ComponentKind.Collider);
            }
        }

        private void DrawConversion(string label, DynamicsConversions.ComponentKind kind)
        {
            bool valid = DynamicsConversions.CanCreate(selected, kind, out string reason);
            using (new EditorGUI.DisabledScope(!valid))
            {
                if (!GUILayout.Button(new GUIContent(label, reason))) return;
                try
                {
                    if (replaceOriginal && !DynamicsContextMenus.ConfirmReplacement(selected)) return;
                    Choose(DynamicsConversions.Create(selected, kind, replaceOriginal));
                    needsScan = true;
                    GUIUtility.ExitGUI();
                }
                catch (ExitGUIException) { throw; }
                catch (Exception error)
                {
                    Debug.LogException(error);
                    EditorUtility.DisplayDialog("Could not create component", error.Message, "OK");
                }
            }
        }

        private void Choose(Component component)
        {
            selected = component;
            selection.Clear();
            if (component != null) selection.Add(component);
            RememberPreviewSelection();
            ReleaseInspector();
            SceneView.RepaintAll();
            Repaint();
        }

        private void RememberPreviewSelection()
        {
            previewOriginal = preview.Running ? preview.OriginalOf(selected) as Component : null;
            previewSelection.Clear();
            if (preview.Running)
                previewSelection.AddRange(selection.Select(component => preview.OriginalOf(component) as Component).Where(component => component != null));
        }

        private void RecoverPreviewSelection()
        {
            if (!preview.Running || selected != null || previewOriginal == null) return;
            selected = preview.CopyOf(previewOriginal) as Component;
            selection.Clear();
            selection.AddRange(previewSelection.Select(component => preview.CopyOf(component) as Component).Where(component => component != null));
            ReleaseInspector();
            needsScan = true;
        }

        private bool Matches(Component component)
        {
            if (category == 1 && !(component is VRCPhysBoneBase)) return false;
            if (category == 2 && !(component is VRCPhysBoneColliderBase)) return false;
            if (category == 3 && !(component is ContactBase)) return false;
            return string.IsNullOrEmpty(search) || component.gameObject.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                component.GetType().Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void DrawOperations()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (selected is VRCPhysBoneBase)
                {
                    if (GUILayout.Button("Optimize ignored transforms")) Run(() => DynamicsOperations.OptimizeIgnores(selection.OfType<VRCPhysBoneBase>()));
                }
                using (new EditorGUI.DisabledScope(preview.Running))
                    if (selected is VRCPhysBoneCollider collider && GUILayout.Button("Move collider to empty child"))
                        Run(() => { Choose(DynamicsOperations.MoveCollider(collider)); needsScan = true; });
                if (GUILayout.Button("Set root to self"))
                    Run(() => DynamicsOperations.Change(selection, "Set dynamics root to self", component =>
                    {
                        if (component is VRCPhysBoneBase bone) bone.rootTransform = null;
                        if (component is VRCPhysBoneColliderBase shape) shape.rootTransform = null;
                        if (component is ContactBase contact) contact.rootTransform = null;
                    }));
            }
        }

        private void DrawTesting()
        {
            if (preview.Running)
            {
                EditorGUILayout.HelpBox("Testing a separate copy. Apply changes to keep edited component settings; stopping discards unapplied changes.", MessageType.Info);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(selected == null))
                        if (GUILayout.Button("Apply selected")) Run(() => preview.Apply(selected));
                    if (GUILayout.Button("Apply all changes")) Run(() => preview.Apply());
                    if (GUILayout.Button("Restart")) Run(() => { preview.Restart(); RecoverPreviewSelection(); needsScan = true; });
                    if (GUILayout.Button("Stop testing")) StopPreview();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(preview.Error)) EditorGUILayout.HelpBox(preview.Error, MessageType.Warning);
                using (new EditorGUI.DisabledScope(root == null || EditorUtility.IsPersistent(root) || EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("Test PhysBones in Scene"))
                        Run(() =>
                        {
                            var original = selected;
                            preview.Start(root);
                            Choose(preview.CopyOf(original) as Component ?? preview.Root.GetComponentInChildren<VRCPhysBoneBase>(true));
                            needsScan = true;
                        });
                if (selected == null || !selected.gameObject.scene.IsValid()) { Choose(null); needsScan = true; }
            }
        }

        private void StopPreview()
        {
            preview.Dispose();
            Choose(null);
            needsScan = true;
        }

        private void OnSceneGUI(SceneView view)
        {
            if (tab == 3 || tab == 4) { management.DrawHighlights(root, tab == 4); return; }
            RecoverPreviewSelection();
            if (sceneTools.mode == DynamicsSceneTools.Mode.None && !preview.Running) return;
            if (DynamicsPreview.Active != null && DynamicsPreview.Active != preview) return;
            if (preview.Running && !preview.IsView(view)) return;
            sceneTools.Draw(EditingRoot, selected, selection, Choose);
        }

        private void Run(Action action)
        {
            try { action(); }
            catch (ExitGUIException) { throw; }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Dynamics+", exception.Message, "OK");
            }
        }
    }
}
#endif
