using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    [InitializeOnLoad]
    internal static class ControllerMotionDisplay
    {
        private static readonly Dictionary<AnimatorController, bool> mixed = new Dictionary<AnimatorController, bool>();
        static ControllerMotionDisplay()
        {
            Undo.undoRedoPerformed += mixed.Clear; EditorApplication.projectChanged += mixed.Clear;
            Undo.postprocessModifications += changes => { mixed.Clear(); ControllerParameterTools.ClearCache(); return changes; };
        }
        internal static string Summary(Motion motion)
        {
            if (!motion) return "None";
            if (motion is AnimationClip clip && ControllerEditorExtraSettings.Current.clipDetails)
                return motion.name + " · " + clip.length.ToString("0.##") + "s" + (clip.isLooping ? " ↻" : "");
            if (motion is BlendTree tree && ControllerEditorExtraSettings.Current.blendTreeDetails)
                return motion.name + " · " + tree.blendType + " · " + tree.children.Length;
            return motion.name;
        }
        internal static Color NameColour(string name, Color fallback)
        {
            foreach (var tag in ControllerEditorExtraSettings.Current.clipColours.OrderByDescending(tag => tag.text?.Length ?? 0))
                if (!string.IsNullOrEmpty(tag.text) && (name ?? "").IndexOf(tag.text, StringComparison.OrdinalIgnoreCase) >= 0) return tag.colour;
            return fallback;
        }
        internal static Color TreeColour(BlendTreeType type)
        {
            switch (type)
            {
                case BlendTreeType.Direct: return new Color(0.66f, 0.83f, 1f);
                case BlendTreeType.Simple1D: return new Color(0.67f, 1f, 0.78f);
                case BlendTreeType.SimpleDirectional2D: return new Color(1f, 0.85f, 0.55f);
                case BlendTreeType.FreeformDirectional2D: return new Color(1f, 0.7f, 0.7f);
                default: return new Color(0.87f, 0.72f, 1f);
            }
        }
        internal static bool MixedWriteDefaults(AnimatorController controller)
        {
            if (!controller || !ControllerEditorExtraSettings.Current.writeDefaultsWarnings) return false;
            if (mixed.TryGetValue(controller, out var known)) return known;
            var values = new HashSet<bool>(); var visited = new HashSet<AnimatorStateMachine>();
            void Visit(AnimatorStateMachine machine)
            {
                if (!machine || !visited.Add(machine)) return;
                foreach (var state in machine.states) values.Add(state.state.writeDefaultValues);
                foreach (var child in machine.stateMachines) Visit(child.stateMachine);
            }
            foreach (var layer in controller.layers) Visit(layer.stateMachine);
            return mixed[controller] = values.Count > 1;
        }
        internal static void DrawTreeDetails(BlendTree tree, Rect node)
        {
            if (!tree || !ControllerEditorExtraSettings.Current.blendTreeDetails) return;
            string text = tree.blendType.ToString();
            if (tree.blendType == BlendTreeType.Direct) text += " · " + string.Join(", ", tree.children.Select(child => child.directBlendParameter).Distinct());
            else text += " · " + tree.blendParameter + (tree.blendType == BlendTreeType.Simple1D ? "" : "/" + tree.blendParameterY);
            var style = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.LowerCenter, clipping = TextClipping.Clip };
            Color previous = GUI.contentColor;
            try
            {
                if (ControllerEditorExtraSettings.Current.colourBlendTrees) GUI.contentColor = TreeColour(tree.blendType);
                GUI.Label(new Rect(node.x + 4, node.yMax - 17, Mathf.Max(1, node.width - 8), 16), new GUIContent(text, text), style);
            }
            finally { GUI.contentColor = previous; }
        }
    }
}
