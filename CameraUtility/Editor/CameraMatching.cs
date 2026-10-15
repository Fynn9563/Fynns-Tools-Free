using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.Cameras
{
    public static class CameraMatching
    {
        public static Camera FindCamera()
        {
            var main = Camera.main;
            if (IsSceneCamera(main)) return main;
            foreach (var camera in Object.FindObjectsOfType<Camera>())
                if (IsSceneCamera(camera)) return camera;
            return null;
        }

        private static bool IsSceneCamera(Camera camera) => camera &&
            !EditorUtility.IsPersistent(camera) && camera.gameObject.scene.IsValid() &&
            camera.gameObject.scene.isLoaded &&
            !UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(camera.gameObject.scene) &&
            camera.cameraType != CameraType.SceneView && camera.cameraType != CameraType.Preview;

        [MenuItem("Tools/Fynn's Tools/Camera Utility/Match Scene to Game", false, 90)]
        public static void MatchSceneToGame()
        {
            if (!MatchSceneToCamera(FindCamera(), SceneView.lastActiveSceneView, false, out string error))
                Debug.LogWarning("Camera Utility: " + error);
        }

        [MenuItem("Tools/Fynn's Tools/Camera Utility/Match Game to Scene", false, 91)]
        public static void MatchGameToScene()
        {
            if (!MatchCameraToScene(FindCamera(), SceneView.lastActiveSceneView, false, out string error))
                Debug.LogWarning("Camera Utility: " + error);
        }

        public static string Validate(Camera camera, SceneView view, bool editCamera)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return "Stop Play Mode before matching cameras.";
            if (!camera) return "No scene camera found. Add a camera or choose one in Settings.";
            if (EditorUtility.IsPersistent(camera) || !camera.gameObject.scene.IsValid() || !camera.gameObject.scene.isLoaded)
                return "Choose a camera in an open scene, rather than a prefab asset.";
            if (camera.cameraType == CameraType.SceneView || camera.cameraType == CameraType.Preview)
                return "Choose a scene camera, rather than an editor preview camera.";
            if (!view || !view.camera) return "Open and focus a Scene view first.";
            if (editCamera && ((camera.hideFlags | camera.transform.hideFlags | camera.gameObject.hideFlags) & HideFlags.NotEditable) != 0)
                return "The selected camera is not editable.";
            string path = camera.gameObject.scene.path;
            if (editCamera && !string.IsNullOrEmpty(path) && (!AssetDatabase.IsOpenForEdit(path) ||
                (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReadOnly) != 0)))
                return "The camera's scene is read-only.";
            return null;
        }

        public static bool MatchSceneToCamera(Camera camera, SceneView view, bool matchLens, out string error)
        {
            error = Validate(camera, view, false);
            if (error != null) return false;
            float aspect = Mathf.Max(0.0001f, view.camera.aspect);
            bool ortho = matchLens ? camera.orthographic : view.orthographic;
            float fov = matchLens && !ortho ? Camera.VerticalToHorizontalFieldOfView(camera.fieldOfView, Mathf.Min(1, aspect)) : view.cameraSettings.fieldOfView;
            float size = matchLens && ortho ? camera.orthographicSize * Mathf.Min(1, aspect) : Mathf.Max(0.001f, Mathf.Abs(view.size));
            if (!ortho && fov <= 3) { error = "This field of view is too narrow for the Scene view. Turn off Match Lens or use a wider field of view."; return false; }
            if (!Finite(size) || !Finite(fov) || !Finite(camera.transform.position)) { error = "The camera or Scene view contains invalid values."; return false; }
            float distance = ortho ? size * 2 : size / Mathf.Sin(fov * 0.5f * Mathf.Deg2Rad);
            Vector3 pivot = camera.transform.position + camera.transform.forward * distance;
            if (!Finite(pivot)) { error = "The resulting view position is out of range."; return false; }
            int group = BeginUndo("Match Scene to Camera");
            Undo.RegisterCompleteObjectUndo(view, "Match Scene to Camera");
            view.in2DMode = false;
            if (matchLens && !ortho) view.cameraSettings.fieldOfView = fov;
            view.LookAt(pivot, camera.transform.rotation, size, ortho, true);
            FinishUndo(group);
            view.Repaint();
            return true;
        }

        public static bool MatchCameraToScene(Camera camera, SceneView view, bool matchLens, out string error)
        {
            error = Validate(camera, view, true);
            if (error != null) return false;
            var source = view.camera;
            if (!Finite(source.transform.position) || !Finite(source.fieldOfView) || !Finite(source.orthographicSize))
            { error = "The Scene view contains invalid values."; return false; }
            int group = BeginUndo("Match Camera to Scene");
            Undo.RecordObjects(matchLens ? new Object[] { camera.transform, camera } : new Object[] { camera.transform }, "Match Camera to Scene");
            camera.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            if (matchLens)
            {
                camera.orthographic = source.orthographic;
                if (source.orthographic) camera.orthographicSize = source.orthographicSize;
                else camera.fieldOfView = source.fieldOfView;
            }
            if (PrefabUtility.IsPartOfPrefabInstance(camera))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(camera.transform);
                if (matchLens) PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
            }
            FinishUndo(group);
            SceneView.RepaintAll();
            return true;
        }

        private static int BeginUndo(string name)
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(name);
            return group;
        }

        private static void FinishUndo(int group)
        {
            Undo.FlushUndoRecordObjects();
            Undo.CollapseUndoOperations(group);
            Undo.IncrementCurrentGroup();
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
    }
}
