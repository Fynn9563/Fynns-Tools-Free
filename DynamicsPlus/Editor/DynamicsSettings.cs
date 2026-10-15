#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using UnityEditor;
using UnityEngine;

namespace FynnsToolkit.Dynamics
{
    [FilePath("Library/FynnsDynamicsSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DynamicsSettings : ScriptableSingleton<DynamicsSettings>
    {
        public bool enhancedInspectors = true;
        public bool physBoneInspector = true;
        public bool colliderInspector = true;
        public bool senderInspector = true;
        public bool receiverInspector = true;
        public bool sceneLabels = true;
        public bool sceneTools = true;
        public bool sceneOverlay = true;
        public bool showBoneChains = true;
        public bool animatedFoldouts = true;
        public bool globalGizmos = true;
        public bool gizmosActive = true;
        public float boneOpacity = 0.5f;
        public float limitOpacity = 0.5f;
        public bool toolOverlayAlwaysVisible = true;
        public bool editingOverlay = true;
        public bool overlayInterceptsClicks = true;
        public bool tooltips = true;
        public bool ignoreSceneClicks = true;
        public bool hideToolsDuringTesting = true;
        public float handleSizeMultiplier = 1f;
        public SceneAnchor toolOverlayPosition = SceneAnchor.BottomLeft;
        public SceneAnchor editingOverlayPosition = SceneAnchor.BottomRight;
        public Color labelColor = Color.white;
        public Color generalColor = Color.white;
        public Color activeColor = new Color(0.56f, 0.94f, 0.47f);
        public Color inactiveColor = new Color(1f, 0f, 0.3765f);
        public Color mixedColor = new Color(1f, 0.65f, 0f);
        internal enum SceneAnchor { Middle, Right, Left, Top, Bottom, TopRight, TopLeft, BottomRight, BottomLeft }
        public float handleSize = 0.06f;
        public Color selectedColor = new Color(1f, 0.65f, 0f);
        public Color ignoredColor = new Color(1f, 0.4f, 0.35f);
        public Color availableColor = new Color(0.65f, 0.85f, 0.55f);
        private bool editorFoldout;
        private bool handlesFoldout;
        private bool overlayFoldout;

        internal void SaveSettings() { Save(true); SceneView.RepaintAll(); }
        internal void SaveGizmos() { ApplyGlobalGizmos(); SaveSettings(); }
        internal void SaveLayout() { Save(true); SceneView.RepaintAll(); }

        internal void ApplyGlobalGizmos()
        {
            if (!globalGizmos || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var bones = System.Linq.Enumerable.Where(Resources.FindObjectsOfTypeAll<VRC.Dynamics.VRCPhysBoneBase>(), bone =>
                bone.gameObject.scene.IsValid() && bone.gameObject.scene.isLoaded && !EditorUtility.IsPersistent(bone) &&
                (DynamicsPreview.Active != null ? DynamicsPreview.Active.ContainsCopy(bone) : !UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(bone.gameObject.scene)) &&
                (bone.showGizmos != gizmosActive || !Mathf.Approximately(bone.boneOpacity, boneOpacity) || !Mathf.Approximately(bone.limitOpacity, limitOpacity)));
            DynamicsOperations.Change(System.Linq.Enumerable.Cast<Component>(bones), "Change global PhysBone gizmos", component =>
            {
                var bone = (VRC.Dynamics.VRCPhysBoneBase)component;
                bone.showGizmos = gizmosActive;
                bone.boneOpacity = boneOpacity;
                bone.limitOpacity = limitOpacity;
            });
        }

        internal void DrawCosmetic()
        {
            EditorGUI.BeginChangeCheck();
            editorFoldout = EditorGUILayout.Foldout(editorFoldout, "Editor", true);
            if (editorFoldout)
                animatedFoldouts = EditorGUILayout.Toggle("Animated Foldouts", animatedFoldouts);
            EditorGUILayout.Space();
            handlesFoldout = EditorGUILayout.Foldout(handlesFoldout, "Handles", true);
            if (handlesFoldout)
            {
                sceneLabels = EditorGUILayout.Toggle("Show Name Labels", sceneLabels);
                if (sceneLabels) labelColor = EditorGUILayout.ColorField("Label Color", labelColor);
                generalColor = EditorGUILayout.ColorField("General Color", generalColor);
                activeColor = EditorGUILayout.ColorField("Active Color", activeColor);
                inactiveColor = EditorGUILayout.ColorField("Inactive Color", inactiveColor);
                mixedColor = EditorGUILayout.ColorField("Mixed Color", mixedColor);
                selectedColor = EditorGUILayout.ColorField("Selection Color", selectedColor);
                handleSizeMultiplier = EditorGUILayout.FloatField("Handle Size Multiplier", handleSizeMultiplier);
                handleSizeMultiplier = Mathf.Max(0.1f, handleSizeMultiplier);
            }
            EditorGUILayout.Space();
            overlayFoldout = EditorGUILayout.Foldout(overlayFoldout, "Overlay", true);
            if (overlayFoldout)
            {
                sceneOverlay = EditorGUILayout.Toggle("Tool Overlay", sceneOverlay);
                using (new EditorGUI.DisabledScope(!sceneOverlay))
                {
                    toolOverlayPosition = (SceneAnchor)EditorGUILayout.EnumPopup("Position", toolOverlayPosition);
                    toolOverlayAlwaysVisible = EditorGUILayout.Toggle("Always Visible", toolOverlayAlwaysVisible);
                }
                editingOverlay = EditorGUILayout.Toggle("Property & Tip Overlay", editingOverlay);
                using (new EditorGUI.DisabledScope(!editingOverlay))
                    editingOverlayPosition = (SceneAnchor)EditorGUILayout.EnumPopup("Position", editingOverlayPosition);
                overlayInterceptsClicks = EditorGUILayout.Toggle("Intercept Overlay Clicks", overlayInterceptsClicks);
                tooltips = EditorGUILayout.Toggle("Tooltips", tooltips);
                ignoreSceneClicks = EditorGUILayout.Toggle("Ignore Scene Clicks", ignoreSceneClicks);
                hideToolsDuringTesting = EditorGUILayout.Toggle("Hide Tools During Testing", hideToolsDuringTesting);
            }
            if (EditorGUI.EndChangeCheck()) SaveSettings();
        }

        internal void ResetSettings()
        {
            enhancedInspectors = physBoneInspector = colliderInspector = senderInspector = receiverInspector = true;
            editorFoldout = handlesFoldout = overlayFoldout = false;
            sceneLabels = sceneTools = sceneOverlay = showBoneChains = animatedFoldouts = true;
            globalGizmos = gizmosActive = toolOverlayAlwaysVisible = editingOverlay = true;
            overlayInterceptsClicks = tooltips = ignoreSceneClicks = hideToolsDuringTesting = true;
            boneOpacity = limitOpacity = 0.5f;
            handleSizeMultiplier = 1f;
            handleSize = 0.06f;
            toolOverlayPosition = SceneAnchor.BottomLeft;
            editingOverlayPosition = SceneAnchor.BottomRight;
            labelColor = generalColor = Color.white;
            activeColor = new Color(0.56f, 0.94f, 0.47f);
            inactiveColor = new Color(1f, 0f, 0.3765f);
            mixedColor = selectedColor = new Color(1f, 0.65f, 0f);
            ignoredColor = new Color(1f, 0.4f, 0.35f);
            availableColor = new Color(0.65f, 0.85f, 0.55f);
            SaveSettings();
        }

        internal void Draw()
        {
            EditorGUI.BeginChangeCheck();
            enhancedInspectors = EditorGUILayout.Toggle("Dynamics+ inspector tools", enhancedInspectors);
            sceneLabels = EditorGUILayout.Toggle("Scene labels", sceneLabels);
            sceneTools = EditorGUILayout.Toggle("Scene handles", sceneTools);
            sceneOverlay = EditorGUILayout.Toggle("Scene tool overlay", sceneOverlay);
            showBoneChains = EditorGUILayout.Toggle("Draw bone chains", showBoneChains);
            handleSize = EditorGUILayout.Slider("Selection handle size", handleSize, 0.02f, 0.2f);
            selectedColor = EditorGUILayout.ColorField("Selected", selectedColor);
            ignoredColor = EditorGUILayout.ColorField("Ignored", ignoredColor);
            availableColor = EditorGUILayout.ColorField("Available", availableColor);
            if (EditorGUI.EndChangeCheck()) { Save(true); SceneView.RepaintAll(); }
            if (GUILayout.Button("Reset settings"))
            {
                ResetSettings();
            }
        }

        internal bool InspectorEnabled(Component component) => enhancedInspectors &&
            (component is VRC.Dynamics.VRCPhysBoneBase ? physBoneInspector :
             component is VRC.Dynamics.VRCPhysBoneColliderBase ? colliderInspector :
             component is VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver ? receiverInspector : senderInspector);

        internal void ToggleInspectors(Component component)
        {
            if (component is VRC.Dynamics.VRCPhysBoneBase) physBoneInspector = !physBoneInspector;
            else if (component is VRC.Dynamics.VRCPhysBoneColliderBase) colliderInspector = !colliderInspector;
            else if (component is VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver) receiverInspector = !receiverInspector;
            else senderInspector = !senderInspector;
            SaveSettings();
        }
    }
}
#endif
