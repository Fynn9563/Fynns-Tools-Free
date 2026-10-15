#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace FynnsTools.VRCSDKPlus
{
    internal enum SdkPlusFeature { Usages, Rename, Tree, Validation, Presets }

    internal sealed class SdkPlusFeaturePanel : IDisposable
    {
        [SerializeField] Object source;
        [SerializeField] VRCAvatarDescriptor avatar;
        [SerializeField] SdkPlusFeature feature;
        [SerializeField] string parameterName = "", newName = "";
        [SerializeField] VRCExpressionParameters preset;
        SdkPlusScope scope;
        ParameterRenamePlan preview;
        VRCExpressionParameters.Parameter selectedRenameParameter;
        string selectedRenameName;
        string message;
        readonly HashSet<string> selectedPresetNames = new HashSet<string>(StringComparer.Ordinal);
        VRCExpressionParameters selectionSource;
        readonly HashSet<int> expanded = new HashSet<int>();
        List<string> report;

        sealed class Request { internal SdkPlusFeature feature; internal string name; internal VRCAvatarDescriptor avatar; internal bool toggle; internal List<ImportableParameter> import; }
        static readonly Dictionary<int, Request> requests = new Dictionary<int, Request>();
        bool open = true;
        List<ImportableParameter> pendingImport;
        string presetName = "Parameter Preset";
        DefaultAsset presetFolder;
        internal static void OpenImport(Object source, VRCAvatarDescriptor avatar, List<ImportableParameter> entries)
        {
            Open(source, avatar, SdkPlusFeature.Presets);
            if (source) requests[source.GetInstanceID()].import = entries;
        }
        internal static void Open(Object source, VRCAvatarDescriptor avatar, SdkPlusFeature feature, string parameter = "", bool toggle = false)
        {
            if (!source) return;
            requests[source.GetInstanceID()] = new Request { feature = feature, name = parameter ?? "", avatar = avatar, toggle = toggle };
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        internal static void Draw(ref SdkPlusFeaturePanel panel, Object source, VRCAvatarDescriptor avatar, bool parameters)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Find Usages")) Open(source, avatar, SdkPlusFeature.Usages, "", true);
                if (parameters && GUILayout.Button("Rename")) Open(source, avatar, SdkPlusFeature.Rename, "", true);
                if (!parameters && GUILayout.Button("Menu Tree")) Open(source, avatar, SdkPlusFeature.Tree, "", true);
                if (GUILayout.Button("Validate")) Open(source, avatar, SdkPlusFeature.Validation, "", true);
                if (parameters && GUILayout.Button("Presets")) Open(source, avatar, SdkPlusFeature.Presets, "", true);
            }
            Request request;
            if (requests.TryGetValue(source.GetInstanceID(), out request))
            {
                requests.Remove(source.GetInstanceID());
                if (panel != null && panel.open && panel.feature == request.feature && request.toggle) panel.open = false;
                else
                {
                    bool sameFeature = panel != null && panel.feature == request.feature;
                    if (panel == null) { panel = new SdkPlusFeaturePanel(); Undo.undoRedoPerformed += panel.OnUndoRedo; }
                    panel.source = source; panel.avatar = request.avatar; panel.feature = request.feature;
                    if (!sameFeature || !string.IsNullOrEmpty(request.name)) panel.parameterName = request.name;
                    panel.open = true; panel.Rebuild();
                    if (request.import != null) panel.pendingImport = request.import;
                }
            }
            if (panel != null) panel.DrawPanel();
        }
        public void Dispose()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            if (source) requests.Remove(source.GetInstanceID());
        }
        internal static void AddParameterContext(GenericMenu menu, Object source, VRCAvatarDescriptor avatar, IEnumerable<string> names, bool rename)
        {
            var values = names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToArray();
            foreach (var name in values)
            {
                string value = name;
                menu.AddItem(new GUIContent(values.Length == 1 ? "Find Usages" : "Find Usages/" + name.Replace('/', '∕')), false, () => Open(source, avatar, SdkPlusFeature.Usages, value));
                if (rename) menu.AddItem(new GUIContent("Rename Everywhere…"), false, () => Open(source, avatar, SdkPlusFeature.Rename, value));
            }
        }
        void Rebuild()
        {
            scope = SdkPlusScope.Build(source as VRCExpressionParameters, source as VRCExpressionsMenu, avatar);
            preview = null; report = null; message = null;
            if (source is VRCExpressionsMenu) expanded.Add(source.GetInstanceID());
        }
        void OnUndoRedo() { scope = null; preview = null; report = null; VRCSDKPlus.RefreshAvatarInfo(); UnityEditorInternal.InternalEditorUtility.RepaintAllViews(); }
        void DrawPanel()
        {
            if (!source) { EditorGUILayout.HelpBox("The source asset is no longer available. Open this feature from its Inspector again.", MessageType.Info); return; }
            using (new EditorGUILayout.HorizontalScope())
            {
                string title = feature == SdkPlusFeature.Usages ? "Find Usages" : feature == SdkPlusFeature.Rename ? "Rename Everywhere" : feature == SdkPlusFeature.Tree ? "Menu Tree" : feature == SdkPlusFeature.Validation ? "Validation" : "Parameter Presets";
                open = EditorGUILayout.Foldout(open, title, true, EditorStyles.foldoutHeader);
                if (GUILayout.Button("Close", GUILayout.Width(55))) open = false;
            }
            if (!open) return;
            EditorGUI.BeginChangeCheck();
            avatar = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar Scope", avatar, typeof(VRCAvatarDescriptor), true);
            if (EditorGUI.EndChangeCheck() || scope == null) Rebuild();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(scope.menus.Count + " menus • " + scope.controllers.Count + " controllers", EditorStyles.miniLabel);
                if (GUILayout.Button("Refresh", GUILayout.Width(75))) Rebuild();
            }
            if (feature != SdkPlusFeature.Tree && feature != SdkPlusFeature.Presets)
                EditorGUILayout.HelpBox("Searches this parameter file, connected menus and the chosen avatar's controllers. Shared assets are edited directly; unrelated avatars and build-generated assets are outside this scope.", MessageType.Info);
            switch (feature)
            {
                case SdkPlusFeature.Usages: DrawUsages(); break;
                case SdkPlusFeature.Rename: DrawRename(); break;
                case SdkPlusFeature.Tree: DrawTree(); break;
                case SdkPlusFeature.Validation: DrawValidation(); break;
                case SdkPlusFeature.Presets: DrawPresets(); break;
            }
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
        }
        internal string[] ParameterChoices()
        {
            var names = feature == SdkPlusFeature.Rename
                ? (scope.parameters ? scope.parameters.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>() : Array.Empty<VRCExpressionParameters.Parameter>()).Where(p => p != null).Select(p => p.name)
                : scope.references.Select(r => r.name);
            return names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToArray();
        }
        internal void SynchronizeRenameParameter()
        {
            if (feature != SdkPlusFeature.Rename) return;
            var parameters = scope.parameters ? scope.parameters.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>() : Array.Empty<VRCExpressionParameters.Parameter>();
            if (selectedRenameParameter != null && parameterName == selectedRenameName)
            {
                string currentName = parameters.Contains(selectedRenameParameter) ? selectedRenameParameter.name : parameters.Any(p => p != null && p.name == parameterName) ? parameterName : "";
                if (parameterName != currentName) { parameterName = currentName; preview = null; }
            }
            selectedRenameParameter = parameters.FirstOrDefault(p => p != null && p.name == parameterName);
            selectedRenameName = parameterName;
        }
        void DrawParameterChoice()
        {
            SynchronizeRenameParameter();
            var names = ParameterChoices();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                parameterName = EditorGUILayout.TextField("Parameter", parameterName);
                if (names.Length > 0)
                {
                    int index = EditorGUILayout.Popup(0, new[] { "Choose…" }.Concat(names).ToArray(), GUILayout.Width(120));
                    if (index > 0) parameterName = names[index - 1];
                }
                if (EditorGUI.EndChangeCheck()) preview = null;
            }
            SynchronizeRenameParameter();
        }
        void DrawUsages()
        {
            DrawParameterChoice();
            var usages = scope.references.Where(r => r.name == parameterName).ToArray();
            if (string.IsNullOrWhiteSpace(parameterName)) { EditorGUILayout.HelpBox("Choose a parameter, or right-click a parameter/control in the Inspector and select Find Usages.", MessageType.Info); return; }
            GUILayout.Label(usages.Length + " references", EditorStyles.boldLabel);
            foreach (var reference in usages) DrawReference(reference);
        }
        static void DrawReference(ParameterReference reference)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(reference.definition ? "Definition" : "Usage", GUILayout.Width(70));
                    if (reference.owner && string.IsNullOrWhiteSpace(reference.owner.name))
                    {
                        var content = new GUIContent(ReferenceOwnerLabel(reference.owner), EditorGUIUtility.ObjectContent(reference.owner, reference.owner.GetType()).image, reference.location);
                        var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                        if (GUI.Button(rect, GUIContent.none, EditorStyles.objectField)) EditorGUIUtility.PingObject(reference.owner);
                        var iconRect = new Rect(rect.x + 2, rect.y + 1, 16, 16);
                        if (content.image) GUI.DrawTexture(iconRect, content.image, ScaleMode.ScaleToFit);
                        var labelRect = new Rect(rect.x + 20, rect.y, Mathf.Max(0, rect.width - 22), rect.height);
                        GUI.Label(labelRect, new GUIContent(content.text, content.tooltip), EditorStyles.label);
                    }
                    else EditorGUILayout.ObjectField(reference.owner, typeof(Object), false);
                    if (GUILayout.Button("Show", GUILayout.Width(50))) { Selection.activeObject = reference.owner; EditorGUIUtility.PingObject(reference.owner); }
                }
                EditorGUILayout.LabelField(reference.path, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(reference.owner), EditorStyles.wordWrappedMiniLabel);
            }
        }
        internal static string ReferenceOwnerLabel(Object owner)
        {
            if (!owner) return "Missing object";
            if (!string.IsNullOrWhiteSpace(owner.name)) return owner.name;
            var transition = owner as UnityEditor.Animations.AnimatorTransitionBase;
            if (transition)
            {
                if (transition.isExit) return "Transition to Exit";
                if (transition.destinationState) return "Transition to " + transition.destinationState.name;
                if (transition.destinationStateMachine) return "Transition to " + transition.destinationStateMachine.name;
                return "Transition (no destination)";
            }
            return "Unnamed " + ObjectNames.NicifyVariableName(owner.GetType().Name);
        }
        void DrawRename()
        {
            DrawParameterChoice();
            EditorGUI.BeginChangeCheck(); newName = EditorGUILayout.TextField("New Name", newName);
            if (EditorGUI.EndChangeCheck()) preview = null;
            bool scopeValid = avatar && scope.parameters && avatar.expressionParameters == scope.parameters;
            if (!scopeValid) EditorGUILayout.HelpBox("Choose an avatar that uses this parameter file before renaming across its menus and controllers.", MessageType.Warning);
            using (new EditorGUI.DisabledScope(!scopeValid))
                if (GUILayout.Button("Preview Rename"))
                {
                    Rebuild(); preview = ParameterRenamePlan.Create(scope, parameterName, newName);
                }
            if (preview == null) return;
            if (preview.error != null)
            {
                EditorGUILayout.HelpBox(preview.error, MessageType.Warning);
                foreach (var reference in preview.conflicts) DrawReference(reference);
                return;
            }
            if (preview.conflicts.Count > 0)
            {
                EditorGUILayout.HelpBox("The name \"" + newName + "\" is already used by the references below. Applying this rename will connect them to the renamed parameter too.", MessageType.Warning);
                foreach (var reference in preview.conflicts) DrawReference(reference);
            }
            GUILayout.Label(preview.changes.Count + " references will change", EditorStyles.boldLabel);
            foreach (var reference in preview.changes) DrawReference(reference);
            if (GUILayout.Button(preview.conflicts.Count > 0 ? "Apply Rename and Reconnect References" : "Apply Rename"))
            {
                try
                {
                    var latest = SdkPlusScope.Build(scope.parameters, source as VRCExpressionsMenu, avatar);
                    var current = ParameterRenamePlan.Create(latest, parameterName, newName);
                    string[] before = preview.changes.Select(r => r.Key).OrderBy(k => k).ToArray();
                    string[] after = current.changes.Select(r => r.Key).OrderBy(k => k).ToArray();
                    string[] previousConflicts = preview.conflicts.Select(r => r.Key).OrderBy(k => k).ToArray();
                    string[] latestConflicts = current.conflicts.Select(r => r.Key).OrderBy(k => k).ToArray();
                    if (current.error != null || !before.SequenceEqual(after) || !previousConflicts.SequenceEqual(latestConflicts)) { Rebuild(); message = "The scope changed. Preview the rename again."; return; }
                    GUI.FocusControl(null); EditorGUIUtility.editingTextField = false;
                    current.Apply(); VRCSDKPlus.RefreshAvatarInfo(); parameterName = newName; newName = ""; Rebuild(); message = "Parameter and references renamed. Use Undo to restore them together.";
                }
                catch (Exception e) { message = e.Message; preview = null; }
            }
        }
        void DrawValidation()
        {
            if (GUILayout.Button("Run Validation")) { Rebuild(); report = scope.Validate(); }
            if (report == null) return;
            if (report.Count == 0) EditorGUILayout.HelpBox("No issues found in the inspected scope.", MessageType.Info);
            foreach (var issue in report) EditorGUILayout.HelpBox(issue, MessageType.Warning);
        }
        void DrawTree()
        {
            var root = source as VRCExpressionsMenu;
            if (!root && avatar) root = avatar.expressionsMenu;
            if (!root) { EditorGUILayout.HelpBox("No root menu in this scope.", MessageType.Info); return; }
            DrawTreeNode(root, root.name, new HashSet<VRCExpressionsMenu>(), 0);
        }
        internal static string TreeLabel(string value)
            => System.Text.RegularExpressions.Regex.Replace(value ?? "", "</?(?:color(?:=[^>]*)?|b|i|size(?:=[^>]*)?)>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        void DrawTreeNode(VRCExpressionsMenu menu, string label, HashSet<VRCExpressionsMenu> ancestors, int depth)
        {
            label = TreeLabel(label);
            if (!menu) { EditorGUILayout.LabelField(label + " — Missing submenu"); return; }
            if (ancestors.Contains(menu)) { EditorGUILayout.LabelField(label + " — Cycle to " + menu.name); return; }
            if (depth > 32) { EditorGUILayout.LabelField("Further nesting hidden (32 levels)."); return; }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(depth * 14);
                string title = label + (label == menu.name ? "" : " → " + menu.name) + " (" + (menu.controls == null ? 0 : menu.controls.Count) + " controls)";
                bool open = EditorGUILayout.Foldout(expanded.Contains(menu.GetInstanceID()), title, true);
                if (open) expanded.Add(menu.GetInstanceID()); else expanded.Remove(menu.GetInstanceID());
                if (GUILayout.Button("Open", GUILayout.Width(50))) { Selection.activeObject = menu; EditorGUIUtility.PingObject(menu); }
            }
            if (!expanded.Contains(menu.GetInstanceID())) return;
            ancestors.Add(menu);
            foreach (var control in menu.controls ?? new List<VRCExpressionsMenu.Control>())
            {
                if (control == null) continue;
                if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu) DrawTreeNode(control.subMenu, control.name, ancestors, depth + 1);
                else using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space((depth + 1) * 14 + 12);
                    GUILayout.Label(TreeLabel(control.name));
                    GUILayout.Label(ObjectNames.NicifyVariableName(control.type.ToString()), EditorStyles.miniLabel, GUILayout.Width(110));
                }
            }
            if (menu.controls == null || menu.controls.Count == 0)
                using (new EditorGUILayout.HorizontalScope()) { GUILayout.Space((depth + 1) * 14 + 12); GUILayout.Label("Empty menu", EditorStyles.miniLabel); }
            ancestors.Remove(menu);
        }
        void DrawPresets()
        {
            var target = source as VRCExpressionParameters;
            if (!target) return;
            EditorGUI.BeginChangeCheck();
            preset = (VRCExpressionParameters)EditorGUILayout.ObjectField("Import From", preset, typeof(VRCExpressionParameters), false);
            if (EditorGUI.EndChangeCheck()) pendingImport = null;
            using (new EditorGUI.DisabledScope(!preset))
                if (GUILayout.Button("Choose Parameters to Import"))
                    pendingImport = (preset.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).Where(p => p != null).Select(p => new ImportableParameter
                    { name = p.name, valueType = p.valueType, defaultValue = p.defaultValue, saved = p.saved, networkSynced = p.networkSynced, selected = true, alreadyExists = (target.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).Any(t => t != null && t.name == p.name) }).ToList();
            if (pendingImport != null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select All Imports"))
                        for (int i = 0; i < pendingImport.Count; i++) { var item = pendingImport[i]; item.selected = !item.alreadyExists; pendingImport[i] = item; }
                    if (GUILayout.Button("Select No Imports"))
                        for (int i = 0; i < pendingImport.Count; i++) { var item = pendingImport[i]; item.selected = false; pendingImport[i] = item; }
                }
                for (int i = 0; i < pendingImport.Count; i++)
                {
                    var item = pendingImport[i];
                    using (new EditorGUI.DisabledScope(item.alreadyExists))
                        item.selected = EditorGUILayout.ToggleLeft(item.name + " (" + item.valueType + ")" + (item.alreadyExists ? " — Already exists" : ""), item.selected && !item.alreadyExists);
                    pendingImport[i] = item;
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Cancel Import")) pendingImport = null;
                    using (new EditorGUI.DisabledScope(pendingImport == null || !pendingImport.Any(p => p.selected && !p.alreadyExists) || !AssetDatabase.IsOpenForEdit(target)))
                        if (GUILayout.Button("Import Selected"))
                        {
                            int count = ParameterImportUtility.Import(target, pendingImport);
                            pendingImport = null; Rebuild(); message = count + " parameters imported. Undo is available.";
                        }
                }
            }
            EditorGUILayout.Space(); GUILayout.Label("Save a Reusable Group", EditorStyles.boldLabel);
            var parameters = target.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>();
            if (selectionSource != target)
            {
                selectionSource = target; selectedPresetNames.Clear();
                for (int i = 0; i < parameters.Length; i++) if (parameters[i] != null) selectedPresetNames.Add(parameters[i].name);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select All")) { for (int i = 0; i < parameters.Length; i++) if (parameters[i] != null) selectedPresetNames.Add(parameters[i].name); }
                if (GUILayout.Button("Select None")) selectedPresetNames.Clear();
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null) continue;
                bool selected = EditorGUILayout.ToggleLeft(parameters[i].name + "  (" + parameters[i].valueType + ")", selectedPresetNames.Contains(parameters[i].name));
                if (selected) selectedPresetNames.Add(parameters[i].name); else selectedPresetNames.Remove(parameters[i].name);
            }
            presetName = EditorGUILayout.TextField("Preset Name", presetName);
            presetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Save Folder", presetFolder, typeof(DefaultAsset), false);
            string folder = presetFolder ? AssetDatabase.GetAssetPath(presetFolder) : (System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)) ?? "Assets").Replace('\\', '/');
            if (string.IsNullOrEmpty(folder)) folder = "Assets";
            bool validFolder = (folder == "Assets" || folder.StartsWith("Assets/", StringComparison.Ordinal)) && AssetDatabase.IsValidFolder(folder);
            bool validName = !string.IsNullOrWhiteSpace(presetName) && presetName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0;
            EditorGUILayout.LabelField("Save to: " + folder, EditorStyles.wordWrappedMiniLabel);
            using (new EditorGUI.DisabledScope(!validFolder || !validName || !parameters.Any(p => p != null && selectedPresetNames.Contains(p.name))))
                if (GUILayout.Button("Save Selected as Preset"))
                {
                    string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + presetName.Trim() + ".asset");
                    var value = CreatePreset(target, Enumerable.Range(0, parameters.Length).Where(i => parameters[i] != null && selectedPresetNames.Contains(parameters[i].name)));
                    AssetDatabase.CreateAsset(value, path); Undo.RegisterCreatedObjectUndo(value, "Save Parameter Preset"); AssetDatabase.SaveAssetIfDirty(value);
                    preset = value; message = "Preset saved at " + path + ".";
                }
        }

        internal static VRCExpressionParameters CreatePreset(VRCExpressionParameters source, IEnumerable<int> indices)
        {
            var result = ScriptableObject.CreateInstance<VRCExpressionParameters>();
            var parameters = source.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>();
            result.parameters = indices.Distinct().OrderBy(i => i).Where(i => i >= 0 && i < parameters.Length && parameters[i] != null).Select(i =>
            {
                var p = parameters[i]; return new VRCExpressionParameters.Parameter { name = p.name, valueType = p.valueType, defaultValue = p.defaultValue, saved = p.saved, networkSynced = p.networkSynced };
            }).ToArray();
            result.isEmpty = result.parameters.Length == 0; return result;
        }
    }
}
#endif
