using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TextureUtility;

namespace FynnsTextureTools
{
    public class TextureToolsWindow : EditorWindow
    {
        [SerializeField] int tab;
        [SerializeField] List<Texture2D> batch = new List<Texture2D>();
        [SerializeField] bool keepSize = true;
        [SerializeField] int batchOperation;
        [SerializeField] string outputFolder = "Assets/Texture Tools/Batch";
        [SerializeField] TextToTexture.TextToTexture text;
        [SerializeField] FynnsToolkit.Texture2Normalmap normals;
        [SerializeField] TextureUtility.TextureUtility utility;
        [SerializeField] TextureAutoPackerModule selectedModule;
        UnityEditor.Editor moduleEditor;
        Vector2 scroll;
        string status;
        Texture2D preview, channelPreview, colorPreview;
        int previewChannel, batchPreviewIndex, recipePreviewIndex;
        bool showSource;
        bool previewDirty = true;
        double previewAt;
        string previewError;

        static readonly string[] Channels = { "RGBA", "R", "G", "B", "A" };
        void InvalidatePreview() { previewDirty = true; previewAt = EditorApplication.timeSinceStartup + .25; }
        void OnProjectChange() { InvalidatePreview(); }
        void Update()
        {
            if (!previewDirty || EditorApplication.timeSinceStartup < previewAt || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            previewDirty = false;
            RefreshPreview(); Repaint();
        }
        void ClearPreview()
        {
            if (preview) DestroyImmediate(preview);
            if (channelPreview) DestroyImmediate(channelPreview);
            if (colorPreview) DestroyImmediate(colorPreview);
            preview = channelPreview = colorPreview = null;
        }
        void RefreshPreview()
        {
            ClearPreview(); previewError = null;
            try
            {
                if (tab == 0) preview = text.BuildPreview();
                else if (tab == 1) preview = normals.BuildPreview();
                else if (tab <= 4) preview = utility.BuildPreview(tab);
                else if (tab == 5 && selectedModule && selectedModule.packedTextures.Count > 0)
                {
                    recipePreviewIndex = Mathf.Clamp(recipePreviewIndex, 0, selectedModule.packedTextures.Count - 1);
                    preview = selectedModule.packedTextures[recipePreviewIndex].Render(512);
                }
                else if (tab == 6)
                {
                    var items = batch.Where(t => t).Distinct().ToArray();
                    if (items.Length > 0)
                    {
                        batchPreviewIndex = Mathf.Clamp(batchPreviewIndex, 0, items.Length - 1);
                        preview = batchOperation == 0 ? normals.BuildPreview(items[batchPreviewIndex]) : utility.BuildPreview(batchOperation == 2 ? 7 : 2, items[batchPreviewIndex], keepSize);
                    }
                }
                if (preview) preview.hideFlags = HideFlags.HideAndDontSave;
            }
            catch (Exception e) { previewError = e.Message; }
        }
        Texture PreviewSource()
        {
            if (tab == 1) return normals.PreviewSource;
            if (tab == 2 || tab == 4) return utility.PreviewSource;
            if (tab == 6) { var items = batch.Where(t => t).Distinct().ToArray(); if (items.Length > 0) return items[Mathf.Clamp(batchPreviewIndex, 0, items.Length - 1)]; }
            return null;
        }
        void DrawPreviewPane(float width)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(width)))
            {
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                if (tab == 6)
                {
                    var names = batch.Where(t => t).Distinct().Select(t => t.name).ToArray();
                    if (names.Length > 0) batchPreviewIndex = EditorGUILayout.Popup("Texture", Mathf.Clamp(batchPreviewIndex, 0, names.Length - 1), names);
                }
                if (tab == 5 && selectedModule && selectedModule.packedTextures.Count > 0)
                    recipePreviewIndex = EditorGUILayout.Popup("Recipe", Mathf.Clamp(recipePreviewIndex, 0, selectedModule.packedTextures.Count - 1), selectedModule.packedTextures.Select(r => r.name).ToArray());
                if (EditorGUI.EndChangeCheck()) InvalidatePreview();
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (PreviewSource()) showSource = GUILayout.Toolbar(showSource ? 0 : 1, new[] { "Source", "Result" }) == 0;
                    if (GUILayout.Button("Refresh", GUILayout.Width(65))) InvalidatePreview();
                }
                if (tab == 2 && previewChannel != utility.EditChannel)
                { previewChannel = utility.EditChannel; if (channelPreview) DestroyImmediate(channelPreview); }
                int next = GUILayout.Toolbar(previewChannel, Channels);
                if (next != previewChannel)
                {
                    previewChannel = next; if (channelPreview) DestroyImmediate(channelPreview);
                    if (next == 0 && colorPreview) DestroyImmediate(colorPreview);
                    if (tab == 2) { utility.EditChannel = next; GUI.FocusControl(null); InvalidatePreview(); Repaint(); }
                }
                var shown = showSource && PreviewSource() ? PreviewSource() : preview;
                bool showColorResult = previewChannel > 0 && (tab == 2 || (tab == 6 && batchOperation == 1));
                var fullColor = preview;
                if (!showColorResult && colorPreview) { DestroyImmediate(colorPreview); colorPreview = null; }
                if (previewChannel > 0 && shown)
                {
                    if (!channelPreview || channelPreview.name != shown.GetInstanceID().ToString())
                    {
                        if (channelPreview) DestroyImmediate(channelPreview);
                        float scale = Mathf.Min(1, 512f / Mathf.Max(shown.width, shown.height));
                        channelPreview = TextureIO.Read(shown, Mathf.Max(1, Mathf.RoundToInt(shown.width * scale)), Mathf.Max(1, Mathf.RoundToInt(shown.height * scale)), true);
                        channelPreview.hideFlags = HideFlags.HideAndDontSave; channelPreview.name = shown.GetInstanceID().ToString();
                        var pixels = channelPreview.GetPixels();
                        for (int i = 0; i < pixels.Length; i++) { float v = pixels[i][previewChannel - 1]; pixels[i] = new Color(v, v, v, 1); }
                        channelPreview.SetPixels(pixels); channelPreview.Apply();
                    }
                    shown = channelPreview;
                }
                float size = Mathf.Max(96, Mathf.Min(width - 30, (position.height - 260) / (showColorResult && fullColor ? 2 : 1)));
                EditorGUILayout.LabelField(previewChannel == 0 ? "RGBA / Transparency" : Channels[previewChannel] + " Channel", EditorStyles.miniBoldLabel);
                DrawPreviewImage(shown, size);
                if (showColorResult && fullColor)
                {
                    EnsureColorPreview(fullColor);
                    EditorGUILayout.Space(6);
                    EditorGUILayout.LabelField("Edited Result / RGB", EditorStyles.miniBoldLabel);
                    DrawPreviewImage(colorPreview, size);
                }
                if (preview) EditorGUILayout.LabelField(preview.width + " x " + preview.height + " preview", EditorStyles.centeredGreyMiniLabel);
                if (tab == 0 && TextToTexture.TextToTexture.generationType == TextToTexture.TextToTexture.TextureGenerationType.GenerateMaterial)
                    EditorGUILayout.LabelField("Material bevel / normal map", EditorStyles.centeredGreyMiniLabel);
                if (!string.IsNullOrEmpty(previewError)) EditorGUILayout.HelpBox(previewError, MessageType.Info);
                GUILayout.FlexibleSpace();
            }
        }
        void EnsureColorPreview(Texture fullColor)
        {
                    if (!colorPreview || colorPreview.name != fullColor.GetInstanceID().ToString())
                    {
                        if (colorPreview) DestroyImmediate(colorPreview);
                        float scale = Mathf.Min(1, 512f / Mathf.Max(fullColor.width, fullColor.height));
                        colorPreview = TextureIO.Read(fullColor, Mathf.Max(1, Mathf.RoundToInt(fullColor.width * scale)), Mathf.Max(1, Mathf.RoundToInt(fullColor.height * scale)), false);
                        colorPreview.hideFlags = HideFlags.HideAndDontSave;
                        colorPreview.name = fullColor.GetInstanceID().ToString();
                        var pixels = colorPreview.GetPixels();
                        for (int i = 0; i < pixels.Length; i++) pixels[i].a = 1;
                        colorPreview.SetPixels(pixels); colorPreview.Apply();
                    }
        }
        void DrawPreviewImage(Texture image, float size)
        {
            var rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(.12f, .12f, .12f));
            if (image) EditorGUI.DrawTextureTransparent(rect, image, ScaleMode.ScaleToFit);
            else GUI.Label(rect, string.IsNullOrEmpty(previewError) ? "Choose an input to preview" : "Preview unavailable", new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleCenter });
        }
        static readonly string[] Tabs = { "Text", "Normal Map", "Edit", "Create", "Pack", "Auto-Pack", "Batch" };

        [MenuItem("Tools/Fynn's Tools/Texture Tools", false, 51)]
        public static void ShowWindow() { Open(0); }
        public static void Open(int page)
        {
            var window = GetWindow<TextureToolsWindow>("Texture Tools");
            window.minSize = new Vector2(780, 500); window.tab = page; window.Show();
        }
        public static void OpenBatch()
        {
            Open(6);
            var window = GetWindow<TextureToolsWindow>(); window.AddSelection(); window.batchOperation = 0;
        }
        public static void RepaintOpen()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<TextureToolsWindow>()) { window.InvalidatePreview(); window.Repaint(); }
        }
        void OnEnable()
        {
            Undo.undoRedoPerformed += InvalidatePreview;
            if (!text) text = CreateInstance<TextToTexture.TextToTexture>();
            if (!normals) normals = CreateInstance<FynnsToolkit.Texture2Normalmap>();
            if (!utility) utility = CreateInstance<TextureUtility.TextureUtility>();
            text.hideFlags = normals.hideFlags = utility.hideFlags = HideFlags.HideAndDontSave;
        }
        void OnDisable()
        {
            Undo.undoRedoPerformed -= InvalidatePreview;
            ClearPreview();
            if (moduleEditor) DestroyImmediate(moduleEditor);
            if (text) DestroyImmediate(text);
            if (normals) DestroyImmediate(normals);
            if (utility) DestroyImmediate(utility);
        }
        void OnGUI()
        {
            float width = EditorGUIUtility.labelWidth; var color = GUI.backgroundColor;
            try
            {
                if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.KeyUp) InvalidatePreview();
                GUILayout.Space(8);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Texture Tools", new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 }, GUILayout.Height(25));
                    if ((tab == 1 || tab == 2 || tab == 4 || tab == 6) && GUILayout.Button("Use Selection", GUILayout.Width(110))) AddSelection();
                }
                int next = GUILayout.Toolbar(tab, Tabs, GUILayout.Height(28));
                if (next != tab) { tab = next; status = null; scroll = Vector2.zero; showSource = false; ClearPreview(); InvalidatePreview(); GUI.FocusControl(null); }
                GUILayout.Space(8);
                using (new EditorGUILayout.HorizontalScope())
                {
                    float previewWidth = Mathf.Clamp(position.width * .4f, 240, 620);
                    float settingsWidth = Mathf.Max(260, position.width - previewWidth - 14);
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(settingsWidth)))
                    {
                        scroll = EditorGUILayout.BeginScrollView(scroll, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(settingsWidth));
                        EditorGUIUtility.labelWidth = Mathf.Min(145, settingsWidth * .4f);
                        EditorGUI.BeginChangeCheck();
                        try
                        {
                            if (tab == 0) text.DrawPanel();
                            else if (tab == 1) normals.DrawPanel();
                            else if (tab <= 4) utility.DrawPanel(tab);
                            else if (tab == 5) DrawAutoPack();
                            else DrawBatch();
                        }
                        finally { if (EditorGUI.EndChangeCheck()) InvalidatePreview(); EditorGUILayout.EndScrollView(); }
                    }
                    DrawPreviewPane(previewWidth);
                }
                if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.Info);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Created by Fynn9563", EditorStyles.linkLabel)) Application.OpenURL("https://github.com/Fynn9563");
                }
            }
            catch (ExitGUIException) { throw; }
            catch (Exception e) { status = e.Message; Debug.LogException(e); }
            finally { EditorGUIUtility.labelWidth = width; GUI.backgroundColor = color; }
        }
        void AddSelection()
        {
            foreach (var texture in Selection.objects.OfType<Texture2D>()) if (!batch.Contains(texture)) batch.Add(texture);
            var first = Selection.objects.OfType<Texture2D>().FirstOrDefault();
            if (first && tab == 1) normals.UseSource(first);
            if (first && (tab == 2 || tab == 4)) utility.UseSource(first);
            InvalidatePreview();
            status = tab == 6 ? batch.Count + " texture(s) in the batch queue." : first ? "Using " + first.name + "." : "Select a texture in the Project window.";
        }
        void DrawAutoPack()
        {
            var data = TextureAutoPackerData.GetInstance();
            using (var so = new SerializedObject(data))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("active"), new GUIContent("Auto-Pack on Import"));
                EditorGUILayout.PropertyField(so.FindProperty("activeModules"), new GUIContent("Recipe Collections"), true);
                if (so.ApplyModifiedProperties()) AutoPackEngine.Queue();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("New Recipe Collection"))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Texture recipes", "Texture Pack Recipes", "asset", "Save a recipe collection.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var module = CreateInstance<TextureAutoPackerModule>();
                        module.packedTextures.Add(new AutoPackedTexture()); AssetDatabase.CreateAsset(module, path);
                        Undo.RecordObject(data, "Add recipe collection"); data.activeModules.Add(module); EditorUtility.SetDirty(data);
                        selectedModule = module;
                    }
                }
                if (GUILayout.Button("Check Recipes")) { AutoPackEngine.Run(); status = AutoPackEngine.LastStatus; }
            }
            foreach (var module in data.activeModules.Where(m => m).Distinct())
            {
                if (GUILayout.Toggle(selectedModule == module, module.name, EditorStyles.miniButton)) selectedModule = module;
            }
            selectedModule = (TextureAutoPackerModule)EditorGUILayout.ObjectField("Edit Recipes", selectedModule, typeof(TextureAutoPackerModule), false);
            if (selectedModule)
            {
                UnityEditor.Editor.CreateCachedEditor(selectedModule, typeof(TextureAutoPacker), ref moduleEditor);
                moduleEditor.OnInspectorGUI();
            }
        }
        void DrawBatch()
        {
            batchOperation = EditorGUILayout.Popup("Operation", batchOperation, new[] { "Normal Maps", "Edit Textures", "Unpack Channels" });
            if (batchOperation == 0) EditorGUILayout.LabelField("Uses the current Normal Map settings.", EditorStyles.wordWrappedMiniLabel);
            else if (batchOperation == 1) EditorGUILayout.LabelField("Uses the current Edit settings.", EditorStyles.wordWrappedMiniLabel);
            if (batchOperation != 2 && GUILayout.Button(batchOperation == 0 ? "Edit Normal Map Settings" : "Edit Texture Settings"))
            { tab = batchOperation == 0 ? 1 : 2; InvalidatePreview(); }
            if (batchOperation != 0) keepSize = EditorGUILayout.Toggle("Keep Source Dimensions", keepSize);
            if (batchOperation == 0) TextureUtility.TextureUtility.AssetFolderPath(ref outputFolder, "Save To", "FynnTextureBatchPath");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Selected Textures")) AddSelection();
                if (GUILayout.Button("Clear Queue")) { batch.Clear(); InvalidatePreview(); }
            }
            var drop = GUILayoutUtility.GetRect(0, 38, GUILayout.ExpandWidth(true)); GUI.Box(drop, "Drop textures or folders here");
            if (drop.Contains(Event.current.mousePosition) && (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag(); InvalidatePreview();
                    foreach (var item in DragAndDrop.objectReferences)
                    {
                        var texture = item as Texture2D;
                        if (texture && !batch.Contains(texture)) batch.Add(texture);
                        var folder = AssetDatabase.GetAssetPath(item);
                        if (AssetDatabase.IsValidFolder(folder)) foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
                        {
                            var found = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
                            if (found && !batch.Contains(found)) batch.Add(found);
                        }
                    }
                }
                Event.current.Use();
            }
            for (int i = 0; i < batch.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    batch[i] = (Texture2D)EditorGUILayout.ObjectField(batch[i], typeof(Texture2D), false);
                    if (GUILayout.Button("Remove", GUILayout.Width(65))) { batch.RemoveAt(i); InvalidatePreview(); break; }
                }
            }
            using (new EditorGUI.DisabledScope(!batch.Any(t => t)))
                if (GUILayout.Button("Process " + batch.Count(t => t) + " Textures", GUILayout.Height(30))) RunBatch();
        }
        void RunBatch()
        {
            int done = 0, failed = 0; bool cancelled = false;
            var textures = batch.Where(t => t).Distinct().ToArray();
            try
            {
                for (int i = 0; i < textures.Length; i++)
                {
                    var input = textures[i];
                    if (EditorUtility.DisplayCancelableProgressBar("Texture Tools", input.name, (float)i / textures.Length)) { cancelled = true; break; }
                    try
                    {
                        if (batchOperation == 0)
                        {
                            TextureIO.ValidateOutput(outputFolder + "/output.png"); Directory.CreateDirectory(outputFolder);
                            string path = AssetDatabase.GenerateUniqueAssetPath(outputFolder + "/" + input.name + "_NormalMap.png");
                            normals.ConvertBatch(input, path);
                        }
                        else utility.ApplyBatch(input, batchOperation == 2, keepSize);
                        done++;
                    }
                    catch (Exception e) { failed++; Debug.LogError("[Texture Tools] " + input.name + ": " + e.Message, input); }
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            status = done + " completed, " + failed + " failed." + (cancelled ? " Cancelled; completed outputs kept." : "");
        }
    }
}
