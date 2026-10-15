using UnityEditor;
using UnityEngine;

namespace FynnsTools.Cameras
{
    public sealed class CameraUtilityWindow : EditorWindow
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool matchLens;
        private string status;
        private bool failed;
        private GUIStyle creditStyle;
        private Vector2 scroll;

        [MenuItem("Tools/Fynn's Tools/Camera Utility/Settings", false, 92)]
        public static void ShowWindow()
        {
            var window = GetWindow<CameraUtilityWindow>("Camera Utility");
            window.minSize = new Vector2(440, 230);
            window.Show();
        }

        private void OnSelectionChange() => Repaint();
        private void OnFocus() => Repaint();

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Camera Utility", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Match either direction immediately, or choose a camera override below.", EditorStyles.wordWrappedMiniLabel);
            EditorGUI.BeginChangeCheck();
            targetCamera = (Camera)EditorGUILayout.ObjectField("Camera Override", targetCamera, typeof(Camera), true);
            if (EditorGUI.EndChangeCheck()) status = null;
            using (new EditorGUI.DisabledScope(!Selection.activeGameObject || !Selection.activeGameObject.GetComponent<Camera>()))
                if (GUILayout.Button("Use Selected Camera"))
                {
                    targetCamera = Selection.activeGameObject.GetComponent<Camera>();
                    status = null;
                }
            matchLens = EditorGUILayout.Toggle(new GUIContent("Match Lens", "Also match perspective/orthographic mode and field of view or orthographic size."), matchLens);
            var view = SceneView.lastActiveSceneView;
            var camera = targetCamera ? targetCamera : CameraMatching.FindCamera();
            EditorGUILayout.LabelField("Using Camera", camera ? camera.name : "None found");
            EditorGUILayout.LabelField("Scene View", view ? view.titleContent.text : "None open");
            string error = CameraMatching.Validate(camera, view, false);
            string editError = CameraMatching.Validate(camera, view, true);
            if (error != null) EditorGUILayout.HelpBox(error, MessageType.Info);
            else if (editError != null) EditorGUILayout.HelpBox(editError, MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(error != null))
                    if (GUILayout.Button("Match Scene to Game"))
                    {
                        failed = !CameraMatching.MatchSceneToCamera(camera, view, matchLens, out status);
                        if (!failed) status = "Scene view matched. Use Undo to restore it.";
                    }
                using (new EditorGUI.DisabledScope(editError != null))
                    if (GUILayout.Button("Match Game to Scene"))
                    {
                        failed = !CameraMatching.MatchCameraToScene(camera, view, matchLens, out status);
                        if (!failed) status = "Camera matched. Use Undo to restore it.";
                    }
            }
            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, failed ? MessageType.Warning : MessageType.Info);
            EditorGUILayout.EndScrollView();
            if (creditStyle == null) creditStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, alignment = TextAnchor.MiddleRight };
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Created by Fynn9563", "https://github.com/Fynn9563"), creditStyle, GUILayout.Width(120)))
                    Application.OpenURL("https://github.com/Fynn9563");
            }
        }
    }
}
