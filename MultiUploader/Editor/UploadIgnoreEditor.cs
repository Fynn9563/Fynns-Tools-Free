using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace FynnsTools.MultiUploader.Editor
{
    internal static class UploadIgnoreEditor
    {
        public static void Draw(List<GameObject> excluded)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Ignore uploads", EditorStyles.boldLabel);
            Rect dropArea = GUILayoutUtility.GetRect(100, 42, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag avatars or their hierarchy objects here");
            var current = Event.current;
            if (GUI.enabled && dropArea.Contains(current.mousePosition) &&
                (current.type == EventType.DragUpdated || current.type == EventType.DragPerform))
            {
                var roots = DragAndDrop.objectReferences.Select(AsGameObject).Select(FindAvatar)
                    .Where(root => root != null).Distinct().ToArray();
                DragAndDrop.visualMode = roots.Length > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                if (current.type == EventType.DragPerform && roots.Length > 0)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var root in roots) Add(excluded, root);
                }
                current.Use();
            }
            if (GUILayout.Button("Ignore hierarchy selection"))
                foreach (var value in Selection.gameObjects) Add(excluded, FindAvatar(value));

            for (int index = 0; index < excluded.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField(excluded[index], typeof(GameObject), true);
                bool remove = GUILayout.Button("Remove", GUILayout.Width(65));
                EditorGUILayout.EndHorizontal();
                if (!remove) continue;
                excluded.RemoveAt(index);
                break;
            }
            EditorGUILayout.LabelField("Skips the entire avatar for builds, tests and uploads. Uses the avatar checkboxes above.", EditorStyles.wordWrappedMiniLabel);
        }

        static GameObject AsGameObject(Object value) => value as GameObject ?? (value as Component)?.gameObject;

        static GameObject FindAvatar(GameObject value)
        {
            if (value == null || EditorUtility.IsPersistent(value) || !value.scene.IsValid() ||
                !value.scene.isLoaded || EditorSceneManager.IsPreviewScene(value.scene)) return null;
            var descriptor = value.GetComponentInParent<VRCAvatarDescriptor>(true);
            return descriptor != null && descriptor.GetComponent<PipelineManager>() != null ? descriptor.gameObject : null;
        }

        static void Add(List<GameObject> excluded, GameObject root)
        {
            if (root != null && !excluded.Contains(root)) excluded.Add(root);
        }
    }
}
