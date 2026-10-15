using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed class ControllerParameterNotes : ScriptableObject
    {
        [Serializable]
        internal sealed class Entry { public string parameter; public string note; }
        private static readonly Dictionary<AnimatorController, ControllerParameterNotes> cache = new Dictionary<AnimatorController, ControllerParameterNotes>();
        static ControllerParameterNotes() { Undo.undoRedoPerformed += cache.Clear; EditorApplication.projectChanged += cache.Clear; }
        [SerializeField] private List<Entry> entries = new List<Entry>();
        internal static ControllerParameterNotes Find(AnimatorController controller)
        {
            if (!controller) return null;
            if (cache.TryGetValue(controller, out var known)) return known;
            string path = AssetDatabase.GetAssetPath(controller);
            return cache[controller] = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAllAssetsAtPath(path).OfType<ControllerParameterNotes>().FirstOrDefault();
        }
        internal static string Get(AnimatorController controller, string parameter)
        {
            var data = Find(controller);
            return data ? data.entries.FirstOrDefault(entry => entry.parameter == parameter)?.note ?? "" : "";
        }
        internal static void Set(AnimatorController controller, string parameter, string note)
        {
            if (!controller || string.IsNullOrWhiteSpace(parameter) || !AssetDatabase.Contains(controller) || !AssetDatabase.IsOpenForEdit(controller)) return;
            var data = Find(controller);
            if (!data && string.IsNullOrWhiteSpace(note)) return;
            if (!data)
            {
                data = CreateInstance<ControllerParameterNotes>();
                data.name = "Controller Editor Parameter Notes";
                data.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild;
                AssetDatabase.AddObjectToAsset(data, controller);
                cache[controller] = data;
                data.entries.Add(new Entry { parameter = parameter, note = note });
                Undo.RegisterCreatedObjectUndo(data, "Parameter Note");
            }
            else
            {
                Undo.RecordObject(data, "Parameter Note");
                data.entries.RemoveAll(entry => entry.parameter == parameter);
                if (!string.IsNullOrWhiteSpace(note)) data.entries.Add(new Entry { parameter = parameter, note = note });
            }
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssetIfDirty(data);
        }
        internal static void Rename(AnimatorController controller, string before, string after)
        {
            var data = Find(controller);
            var entry = data ? data.entries.FirstOrDefault(value => value.parameter == before) : null;
            if (entry == null) return;
            Undo.RecordObject(data, "Rename Parameter Note");
            data.entries.RemoveAll(value => value.parameter == after && value != entry);
            entry.parameter = after;
            EditorUtility.SetDirty(data);
        }
    }
}
