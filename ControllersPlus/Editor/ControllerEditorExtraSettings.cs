using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    [Serializable]
    internal sealed class ControllerEditorExtraSettings
    {
        [Serializable]
        internal sealed class ColourTag { public string text = ""; public Color colour = Color.white; }
        private const string Key = "FynnsTools.ControllerEditor.Extras";
        private static ControllerEditorExtraSettings current;
        internal static ControllerEditorExtraSettings Current => current ?? (current = Load());
        public bool motionSearch = true;
        public bool transitionLabels = true;
        public bool horizontalTransitionLabels;
        public bool transitionLabelOutline = true;
        public int transitionLabelSize = 10;
        public bool inclusiveIntComparisons = true;
        public bool preserveConditionMode = true;
        public bool compactVrcComparisons = true;
        public List<ColourTag> transitionColours = new List<ColourTag>();
        public bool parameterIndicators = true;
        public bool parameterTypes = true;
        public bool colourParameters = true;
        public bool shortParameterTypes;
        public bool parameterNotes = true;
        public bool clipDetails = true;
        public bool blendTreeDetails = true;
        public bool colourBlendTrees = true;
        public bool graphShortcuts = true;
        public bool inspectorShortcuts = true;
        public bool reparentTrees = true;
        public bool externalBlendTrees;
        public bool instantTransitionHighlight = true;
        public bool writeDefaultsWarnings = true;
        public string blendTreeFolder = "Assets/Fynn's Tools/Generated/Controller Editor/Blend Trees";
        public string directWeightParameter = "Weight";
        public List<ColourTag> clipColours = new List<ColourTag>();
        private static ControllerEditorExtraSettings Load()
        {
            var result = new ControllerEditorExtraSettings();
            string json = EditorPrefs.GetString(Key, "");
            if (!string.IsNullOrEmpty(json))
            {
                try { JsonUtility.FromJsonOverwrite(json, result); }
                catch (ArgumentException) { }
            }
            if (result.transitionColours == null) result.transitionColours = new List<ColourTag>();
            if (result.clipColours == null) result.clipColours = new List<ColourTag>();
            return result;
        }
        internal static void Save() { EditorPrefs.SetString(Key, JsonUtility.ToJson(Current)); }
        internal static void Draw()
        {
            var settings = Current;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Motion and parameter tools", EditorStyles.boldLabel);
            settings.motionSearch = EditorGUILayout.ToggleLeft("Search motions by name", settings.motionSearch);
            settings.parameterIndicators = EditorGUILayout.ToggleLeft("Show parameter usage and VRChat status", settings.parameterIndicators);
            settings.parameterTypes = EditorGUILayout.ToggleLeft("Show parameter types", settings.parameterTypes);
            settings.colourParameters = EditorGUILayout.ToggleLeft("Colour parameter types and status", settings.colourParameters);
            settings.shortParameterTypes = EditorGUILayout.ToggleLeft("Use short parameter type labels", settings.shortParameterTypes);
            settings.parameterNotes = EditorGUILayout.ToggleLeft("Enable parameter notes", settings.parameterNotes);
            settings.clipDetails = EditorGUILayout.ToggleLeft("Show clip duration and loop status", settings.clipDetails);
            settings.blendTreeDetails = EditorGUILayout.ToggleLeft("Show blend type and child weights", settings.blendTreeDetails);
            settings.colourBlendTrees = EditorGUILayout.ToggleLeft("Colour BlendTrees by type", settings.colourBlendTrees);
            settings.instantTransitionHighlight = EditorGUILayout.ToggleLeft("Highlight instant transitions", settings.instantTransitionHighlight);
            settings.writeDefaultsWarnings = EditorGUILayout.ToggleLeft("Warn about mixed Write Defaults", settings.writeDefaultsWarnings);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transition labels", EditorStyles.boldLabel);
            settings.transitionLabels = EditorGUILayout.ToggleLeft("Show transition conditions", settings.transitionLabels);
            settings.horizontalTransitionLabels = EditorGUILayout.ToggleLeft("Keep labels horizontal", settings.horizontalTransitionLabels);
            settings.transitionLabelOutline = EditorGUILayout.ToggleLeft("Outline labels", settings.transitionLabelOutline);
            settings.transitionLabelSize = EditorGUILayout.IntSlider("Font size", settings.transitionLabelSize, 8, 20);
            settings.preserveConditionMode = EditorGUILayout.ToggleLeft("Keep compatible condition modes when changing parameters", settings.preserveConditionMode);
            settings.inclusiveIntComparisons = EditorGUILayout.ToggleLeft("Show inclusive integer ranges", settings.inclusiveIntComparisons);
            settings.compactVrcComparisons = EditorGUILayout.ToggleLeft("Show gesture names in comparisons", settings.compactVrcComparisons);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shortcuts", EditorStyles.boldLabel);
            settings.graphShortcuts = EditorGUILayout.ToggleLeft("Enable graph shortcuts", settings.graphShortcuts);
            EditorGUILayout.HelpBox("T: make transition   S: new state   C: copy transitions   V: paste transitions", MessageType.None);
            settings.inspectorShortcuts = EditorGUILayout.ToggleLeft("Enable Inspector shortcuts", settings.inspectorShortcuts);
            EditorGUILayout.HelpBox("L or Ctrl+L: lock Inspector or Project   Ctrl+W: close Inspector   D or I: duplicate Inspector   Shift+I: split vertically   Ctrl+Shift+I: new tab", MessageType.None);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("BlendTrees", EditorStyles.boldLabel);
            settings.reparentTrees = EditorGUILayout.ToggleLeft("Drag input slots to reparent BlendTree children", settings.reparentTrees);
            settings.externalBlendTrees = EditorGUILayout.ToggleLeft("Create external BlendTrees", settings.externalBlendTrees);
            using (new EditorGUI.DisabledScope(!settings.externalBlendTrees)) settings.blendTreeFolder = EditorGUILayout.TextField("Output folder", settings.blendTreeFolder);
            settings.directWeightParameter = EditorGUILayout.TextField("Direct weight parameter", settings.directWeightParameter);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion name colours", EditorStyles.boldLabel);
            for (int index = 0; index < settings.clipColours.Count; index++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    settings.clipColours[index].text = EditorGUILayout.TextField(settings.clipColours[index].text);
                    settings.clipColours[index].colour = EditorGUILayout.ColorField(settings.clipColours[index].colour, GUILayout.Width(70));
                    if (GUILayout.Button("Ã—", GUILayout.Width(24))) { settings.clipColours.RemoveAt(index--); GUI.changed = true; }
                }
            }
            if (GUILayout.Button("Add motion colour rule")) { settings.clipColours.Add(new ColourTag()); GUI.changed = true; }
            EditorGUILayout.LabelField("Transition name colours", EditorStyles.boldLabel);
            for (int index = 0; index < settings.transitionColours.Count; index++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    settings.transitionColours[index].text = EditorGUILayout.TextField(settings.transitionColours[index].text);
                    settings.transitionColours[index].colour = EditorGUILayout.ColorField(settings.transitionColours[index].colour, GUILayout.Width(70));
                    if (GUILayout.Button("×", GUILayout.Width(24))) { settings.transitionColours.RemoveAt(index--); GUI.changed = true; }
                }
            }
            if (GUILayout.Button("Add transition colour rule")) { settings.transitionColours.Add(new ColourTag()); GUI.changed = true; }
            if (EditorGUI.EndChangeCheck()) { Save(); UnityEditorInternal.InternalEditorUtility.RepaintAllViews(); }
        }
    }
}
