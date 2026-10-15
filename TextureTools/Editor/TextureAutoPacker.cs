using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TextureUtility
{
    [CustomEditor(typeof(TextureAutoPackerModule))]
    public class TextureAutoPacker : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var list = serializedObject.FindProperty("packedTextures");
            if (GUILayout.Button("Add Recipe"))
            {
                Undo.RecordObject(target, "Add texture recipe");
                ((TextureAutoPackerModule)target).packedTextures.Add(new AutoPackedTexture());
                EditorUtility.SetDirty(target); serializedObject.Update();
            }
            for (int i = 0; i < list.arraySize; i++)
            {
                var recipe = list.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        recipe.FindPropertyRelative("expanded").boolValue = EditorGUILayout.Foldout(recipe.FindPropertyRelative("expanded").boolValue, recipe.FindPropertyRelative("name").stringValue, true);
                        if (GUILayout.Button("Remove", GUILayout.Width(65))) { list.DeleteArrayElementAtIndex(i); break; }
                    }
                    if (!recipe.FindPropertyRelative("expanded").boolValue) continue;
                    foreach (var field in new[] { "name", "automatic", "encoding", "linear", "sourceSize" }) EditorGUILayout.PropertyField(recipe.FindPropertyRelative(field));
                    if (recipe.FindPropertyRelative("encoding").enumValueIndex == 1) EditorGUILayout.PropertyField(recipe.FindPropertyRelative("jpgQuality"));
                    if (!recipe.FindPropertyRelative("sourceSize").boolValue)
                    {
                        EditorGUILayout.PropertyField(recipe.FindPropertyRelative("width"));
                        EditorGUILayout.PropertyField(recipe.FindPropertyRelative("height"));
                    }
                    var channels = recipe.FindPropertyRelative("channels");
                    for (int c = 0; c < channels.arraySize; c++)
                    {
                        var ch = channels.GetArrayElementAtIndex(c);
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField(new[] { "Red", "Green", "Blue", "Alpha" }[c], EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(ch.FindPropertyRelative("texture"), new GUIContent("Source"));
                            EditorGUILayout.PropertyField(ch.FindPropertyRelative("mode"), new GUIContent("Channel"));
                            if (!ch.FindPropertyRelative("texture").objectReferenceValue) EditorGUILayout.PropertyField(ch.FindPropertyRelative("defaultColor"), new GUIContent("Default"));
                            EditorGUILayout.PropertyField(ch.FindPropertyRelative("invert"));
                        }
                    }
                    EditorGUILayout.PropertyField(recipe.FindPropertyRelative("packed"), new GUIContent("Output Texture"));
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(recipe.FindPropertyRelative("outputPath"), new GUIContent("Output File"));
                        if (GUILayout.Button("...", GUILayout.Width(30)))
                        {
                            int encoding = recipe.FindPropertyRelative("encoding").enumValueIndex;
                            string ext = encoding == 1 ? "jpg" : encoding == 2 ? "tga" : "png";
                            var path = EditorUtility.SaveFilePanelInProject("Packed texture", recipe.FindPropertyRelative("name").stringValue, ext, "Choose an output file.");
                            if (!string.IsNullOrEmpty(path)) { recipe.FindPropertyRelative("outputPath").stringValue = path; recipe.FindPropertyRelative("packed").objectReferenceValue = null; }
                        }
                    }
                    if (GUILayout.Button("Pack Recipe"))
                    {
                        serializedObject.ApplyModifiedProperties();
                        try
                        {
                            AutoPackEngine.ValidateCycles(TextureAutoPackerData.Find(), (TextureAutoPackerModule)target);
                            ((TextureAutoPackerModule)target).packedTextures[i].Pack();
                            EditorUtility.SetDirty(target); AssetDatabase.SaveAssets();
                        }
                        catch (Exception e) { EditorUtility.DisplayDialog("Pack recipe", e.Message, "OK"); }
                        serializedObject.Update();
                    }
                }
            }
            if (serializedObject.ApplyModifiedProperties()) AutoPackEngine.Queue();
        }
        public static bool InitiateAutoPacking() { return AutoPackEngine.Run(); }
    }

    [InitializeOnLoad]
    public static class AutoPackEngine
    {
        public static bool Running { get; private set; }
        public static string LastStatus = "";
        static bool queued;
        static AutoPackEngine() { EditorApplication.update += ProcessQueue; Queue(); }
        public static void Queue()
        {
            if (queued) return;
            queued = true; EditorApplication.QueuePlayerLoopUpdate();
        }
        static void ProcessQueue()
        {
            if (!queued || Running || EditorApplication.isUpdating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            Drain();
        }
        static void Drain() { queued = false; Run(); }
        public static void ValidateCycles(TextureAutoPackerData data, TextureAutoPackerModule extra = null)
        {
            var recipes = (data ? data.activeModules : new List<TextureAutoPackerModule>()).Where(m => m).Concat(extra ? new[] { extra } : new TextureAutoPackerModule[0]).Distinct().SelectMany(m => m.packedTextures).Where(r => r != null && !string.IsNullOrEmpty(r.Output)).ToList();
            var outputs = new HashSet<string>();
            foreach (var r in recipes) if (!outputs.Add(r.Output)) throw new InvalidOperationException("Two recipes share an output file. Give each recipe its own output.");
            var visiting = new HashSet<AutoPackedTexture>(); var visited = new HashSet<AutoPackedTexture>();
            Action<AutoPackedTexture> visit = null;
            visit = r =>
            {
                if (visited.Contains(r)) return;
                if (!visiting.Add(r)) throw new InvalidOperationException("Recipes contain a circular input/output chain.");
                foreach (var ch in r.channels.Where(c => c.texture))
                {
                    string source = AssetDatabase.GetAssetPath(ch.texture);
                    foreach (var dependency in recipes.Where(other => other.Output == source)) visit(dependency);
                }
                visiting.Remove(r); visited.Add(r);
            };
            foreach (var r in recipes) visit(r);
        }
        public static bool Run()
        {
            if (Running || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return false;
            var data = TextureAutoPackerData.Find();
            if (!data || !data.active) return false;
            Running = true; int count = 0; int failures = 0;
            try
            {
                ValidateCycles(data);
                foreach (var module in data.activeModules.Where(m => m).Distinct())
                foreach (var recipe in module.packedTextures.Where(r => r != null && r.automatic && !string.IsNullOrEmpty(r.Output)))
                {
                    try
                    {
                        if (!recipe.WasModified()) continue;
                        recipe.Pack(); EditorUtility.SetDirty(module); count++;
                    }
                    catch (Exception e) { failures++; Debug.LogWarning("[Texture Tools] " + recipe.name + ": " + e.Message, module); }
                }
                if (count > 0) AssetDatabase.SaveAssets();
                LastStatus = count + " recipe(s) packed" + (failures > 0 ? ", " + failures + " failed. See Console." : ".");
                return count > 0;
            }
            catch (Exception e) { LastStatus = e.Message; Debug.LogWarning("[Texture Tools] " + e.Message); return false; }
            finally { Running = false; }
        }
        public static void Imported(IEnumerable<string> paths)
        {
            var data = TextureAutoPackerData.Find();
            if (!data || !data.active) return;
            var changed = new HashSet<string>(paths);
            if (data.activeModules.Where(m => m).Any(m => changed.Contains(AssetDatabase.GetAssetPath(m)) || m.packedTextures.Where(r => r != null && r.automatic).Any(r => r.channels.Any(c => c.texture && changed.Contains(AssetDatabase.GetAssetPath(c.texture)))))) Queue();
        }
    }
    public class TextureAutoPackerProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] from) { AutoPackEngine.Imported(imported.Concat(deleted).Concat(moved).Concat(from)); }
    }
}
