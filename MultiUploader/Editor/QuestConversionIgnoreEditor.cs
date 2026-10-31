using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace FynnsTools.MultiUploader.Editor
{
    internal static class QuestConversionIgnoreEditor
    {
        public static bool Draw(GameObject[] avatars)
        {
            bool changed = false;
            EditorGUILayout.LabelField("Ignore Quest material conversion", EditorStyles.boldLabel);
            Rect dropArea = GUILayoutUtility.GetRect(100, 42, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag objects from the Hierarchy here");
            var current = Event.current;
            if (GUI.enabled && dropArea.Contains(current.mousePosition) &&
                (current.type == EventType.DragUpdated || current.type == EventType.DragPerform))
            {
                var objects = DragAndDrop.objectReferences.Select(AsGameObject)
                    .Where(value => AvatarFor(value, avatars) != null).Distinct().ToArray();
                DragAndDrop.visualMode = objects.Length > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                if (current.type == EventType.DragPerform && objects.Length > 0)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var value in objects) changed |= Add(value, avatars);
                }
                current.Use();
            }
            if (GUILayout.Button("Add hierarchy selection"))
                foreach (var value in Selection.gameObjects) changed |= Add(value, avatars);

            foreach (var avatar in avatars)
            {
                if (avatar == null) continue;
                var settings = avatar.GetComponent<MultiUploaderSettings>();
                if (settings == null) continue;
                var ignored = settings.ignoredObjects ?? Array.Empty<GameObject>();
                if (ignored.Length == 0) continue;
                EditorGUILayout.LabelField(avatar.name, EditorStyles.miniBoldLabel);
                for (int index = 0; index < ignored.Length; index++)
                {
                    EditorGUILayout.BeginHorizontal();
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.ObjectField(ignored[index], typeof(GameObject), true);
                    bool remove = GUILayout.Button("Remove", GUILayout.Width(65));
                    EditorGUILayout.EndHorizontal();
                    if (!remove) continue;
                    Save(settings, ignored.Where((_, item) => item != index).ToArray());
                    changed = true;
                    break;
                }
            }
            EditorGUILayout.HelpBox("Ignored objects and their children keep their materials and animated material swaps. The list is saved on each avatar. VRChat still requires Quest-compatible materials.", MessageType.Info);
            return changed;
        }

        static GameObject AsGameObject(Object value) => value as GameObject ?? (value as Component)?.gameObject;

        static GameObject AvatarFor(GameObject value, GameObject[] avatars)
        {
            if (value == null || EditorUtility.IsPersistent(value)) return null;
            var descriptor = value.GetComponentInParent<VRCAvatarDescriptor>(true);
            return descriptor != null && avatars.Contains(descriptor.gameObject) ? descriptor.gameObject : null;
        }

        static bool Add(GameObject value, GameObject[] avatars)
        {
            var avatar = AvatarFor(value, avatars);
            if (avatar == null) return false;
            var settings = avatar.GetComponent<MultiUploaderSettings>() ?? Undo.AddComponent<MultiUploaderSettings>(avatar);
            var ignored = settings.ignoredObjects ?? Array.Empty<GameObject>();
            if (ignored.Contains(value)) return false;
            Save(settings, ignored.Where(item => item != null).Append(value).ToArray());
            return true;
        }

        static void Save(MultiUploaderSettings settings, GameObject[] ignored)
        {
            Undo.RecordObject(settings, "Change Quest conversion ignore list");
            settings.ignoredObjects = ignored;
            PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
            EditorUtility.SetDirty(settings);
            EditorSceneManager.MarkSceneDirty(settings.gameObject.scene);
        }
    }
}
