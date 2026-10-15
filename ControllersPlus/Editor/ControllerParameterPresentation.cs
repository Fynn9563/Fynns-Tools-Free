using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.Animations;
using ReorderableList = UnityEditorInternal.ReorderableList;
using UnityEngine;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace FynnsTools.ControllerEditor
{
    internal static partial class ControllerParameterTools
    {
        internal sealed class ParameterPresentation
        {
            internal string Symbol = "";
            internal string Tooltip = "Controller parameter";
            internal Color Colour = Color.white;
            internal Component[] Writers = Array.Empty<Component>();
        }
        internal static Color TypeColour(AnimatorControllerParameterType type)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Float: return Color.yellow;
                case AnimatorControllerParameterType.Int: return Color.cyan;
                case AnimatorControllerParameterType.Bool: return Color.green;
                case AnimatorControllerParameterType.Trigger: return Color.magenta;
                default: return Color.white;
            }
        }
#if FYNNS_CONTROLLER_EDITOR_AVATARS
        private sealed class DynamicsWriter
        {
            internal string Name;
            internal Component Component;
            internal AnimatorControllerParameterType Type;
        }
        private static readonly Dictionary<VRCAvatarDescriptor, List<DynamicsWriter>> writerCache = new Dictionary<VRCAvatarDescriptor, List<DynamicsWriter>>();
        private static List<DynamicsWriter> Writers(VRCAvatarDescriptor avatar)
        {
            if (writerCache.TryGetValue(avatar, out var known)) return known;
            var result = new List<DynamicsWriter>();
            foreach (var component in avatar.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!component) continue;
                string type = component.GetType().Name;
                if (type != "VRCPhysBone" && type != "VRCContactReceiver") continue;
                using (var serialized = new SerializedObject(component))
                {
                    var parameter = serialized.FindProperty("parameter");
                    if (parameter == null || string.IsNullOrEmpty(parameter.stringValue)) continue;
                    if (type == "VRCContactReceiver")
                    {
                        var receiver = serialized.FindProperty("receiverType");
                        result.Add(new DynamicsWriter { Name = parameter.stringValue, Component = component,
                            Type = receiver != null && receiver.intValue == 2 ? AnimatorControllerParameterType.Float : AnimatorControllerParameterType.Bool });
                    }
                    else foreach (string suffix in new[] { "_IsGrabbed", "_IsPosed", "_Angle", "_Stretch", "_Squish" })
                        result.Add(new DynamicsWriter { Name = parameter.stringValue + suffix, Component = component,
                            Type = suffix == "_IsGrabbed" || suffix == "_IsPosed" ? AnimatorControllerParameterType.Bool : AnimatorControllerParameterType.Float });
                }
            }
            writerCache[avatar] = result; return result;
        }
#endif
        private static void ClearPresentationCache()
        {
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            writerCache.Clear();
#endif
        }
        internal static ParameterPresentation Present(AnimatorController controller, AnimatorControllerParameter parameter)
        {
            var result = new ParameterPresentation();
            if (parameter == null) return result;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            var avatar = Avatar(controller);
            var expression = avatar && avatar.expressionParameters ? avatar.expressionParameters.parameters?.FirstOrDefault(item => item != null && item.name == parameter.name) : null;
            var writers = avatar ? Writers(avatar).Where(item => item.Name == parameter.name).ToArray() : Array.Empty<DynamicsWriter>();
            bool builtin = Builtins.TryGetValue(parameter.name, out var builtinType);
            var expected = builtin ? builtinType : writers.Length == 1 ? writers[0].Type : expression != null ? ToAnimator(expression.valueType) : parameter.type;
            if (builtin) { result.Symbol = "V"; result.Colour = new Color(0.4f, 0.7f, 1); result.Tooltip = "VRChat built-in parameter"; }
            else if (expression != null)
            { result.Symbol = expression.networkSynced ? "S" : "L"; result.Colour = expression.networkSynced ? Color.green : Color.gray; result.Tooltip = "VRChat " + (expression.networkSynced ? "synced" : "local") + (expression.saved ? ", saved" : ""); }
            if (writers.Length > 0)
            {
                result.Writers = writers.Select(item => item.Component).Distinct().ToArray();
                result.Symbol = "Y"; result.Colour = Color.cyan;
                result.Tooltip += "; Avatar dynamics output: " + string.Join(", ", writers.Select(item => item.Component.name + " (" + item.Component.GetType().Name + ")"));
                if (writers.Length > 1 || builtin)
                { result.Symbol = "!"; result.Colour = Color.red; result.Tooltip += "; conflicting parameter writers"; }
                else if (expression != null)
                { result.Symbol = "!"; result.Colour = Color.yellow; result.Tooltip += "; dynamics output is also an expression parameter"; }
            }
            if (parameter.type != expected)
            { result.Symbol = "!"; result.Colour = Color.red; result.Tooltip += "; type mismatch: expected " + expected + ", controller uses " + parameter.type; }
            if (expression != null && ToAnimator(expression.valueType) != parameter.type && ToAnimator(expression.valueType) != expected)
            { result.Symbol = "!"; result.Colour = Color.red; result.Tooltip += "; expression type mismatch: expected " + ToAnimator(expression.valueType) + ", controller uses " + parameter.type; }
#endif
            return result;
        }
        internal static AnimatorControllerParameter ParameterAt(object view, int index)
        {
            if (view == null || index < 0) return null;
            var list = AccessTools.Field(view.GetType(), "m_ParameterList")?.GetValue(view) as ReorderableList;
            if (list == null || index >= list.list.Count) return null;
            var element = list.list[index];
            return element == null ? null : AccessTools.Field(element.GetType(), "m_Parameter")?.GetValue(element) as AnimatorControllerParameter;
        }
        private static float DrawPresentation(Rect rect, AnimatorController controller, AnimatorControllerParameter parameter, Action show)
        {
            var settings = ControllerEditorExtraSettings.Current;
            float used = 0;
            var presentation = Present(controller, parameter);
            var style = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip };
            if (settings.parameterIndicators && rect.width >= 220)
            {
                var references = Usages(controller, parameter.name);
                string usage = references.Count == 0 ? "-" : references.Any(item => item.Detail.Contains("animated parameter")) ? "A" : references.Any(item => item.Detail.Contains("/ driver /")) ? "D" : "*";
                string note = settings.parameterNotes ? ControllerParameterNotes.Get(controller, parameter.name) : "";
                style.normal.textColor = settings.colourParameters ? (usage == "A" ? Color.yellow : usage == "D" ? new Color(1, 0.7f, 0.3f) : usage == "-" ? Color.gray : Color.white) : EditorStyles.miniLabel.normal.textColor;
                if (GUI.Button(new Rect(rect.x, rect.y, 15, rect.height), new GUIContent(usage, references.Count + " references\n" + presentation.Tooltip + (note.Length > 0 ? "\n" + note : "")), style)) show();
                style.normal.textColor = settings.colourParameters ? presentation.Colour : EditorStyles.miniLabel.normal.textColor;
                if (GUI.Button(new Rect(rect.x + 15, rect.y, 15, rect.height), new GUIContent(presentation.Symbol, presentation.Tooltip), style))
                {
                    if (presentation.Writers.Length > 0) Selection.objects = presentation.Writers;
                    else if (Builtins.ContainsKey(parameter.name)) Application.OpenURL("https://creators.vrchat.com/avatars/animator-parameters/");
#if FYNNS_CONTROLLER_EDITOR_AVATARS
                    else { var avatar = Avatar(controller); if (avatar && avatar.expressionParameters) EditorGUIUtility.PingObject(avatar.expressionParameters); }
#endif
                }
                used = 31;
            }
            if (settings.parameterTypes && rect.width - used >= 160)
            {
                bool compact = settings.shortParameterTypes || rect.width - used < 280;
                string type = parameter.type.ToString();
                float width = compact ? 18 : 42;
                style.normal.textColor = settings.colourParameters ? TypeColour(parameter.type) : EditorStyles.miniLabel.normal.textColor;
                GUI.Label(new Rect(rect.x + used, rect.y, width, rect.height), new GUIContent(compact ? type.Substring(0, 1) : type, type + " parameter\n" + presentation.Tooltip), style);
                used += width + 2;
            }
            return used;
        }
        internal static bool Reorder(AnimatorController controller, Func<AnimatorControllerParameter, bool> match, bool first)
        {
            if (!controller || !AssetDatabase.IsOpenForEdit(controller)) return false;
            var parameters = controller.parameters;
            var selected = parameters.Where(match); var rest = parameters.Where(item => !match(item));
            Undo.RegisterCompleteObjectUndo(controller, "Reorder Parameters");
            controller.parameters = (first ? selected.Concat(rest) : rest.Concat(selected)).ToArray();
            EditorUtility.SetDirty(controller); ClearCache(); return true;
        }
        internal static string Insert(AnimatorController controller, string neighbour, AnimatorControllerParameterType type, bool below)
        {
            if (!controller || !AssetDatabase.IsOpenForEdit(controller)) return null;
            var parameters = controller.parameters.ToList(); int index = parameters.FindIndex(item => item.name == neighbour);
            if (index < 0) return null;
            string name = controller.MakeUniqueParameterName("New " + type);
            Undo.RegisterCompleteObjectUndo(controller, "Insert Parameter");
            parameters.Insert(index + (below ? 1 : 0), new AnimatorControllerParameter { name = name, type = type });
            controller.parameters = parameters.ToArray(); EditorUtility.SetDirty(controller); ClearCache(); return name;
        }
        private static void AddOrderingMenu(GenericMenu menu, AnimatorController controller, AnimatorControllerParameter parameter)
        {
            menu.AddItem(new GUIContent("Ordering/Move to Top"), false, () => Reorder(controller, item => item.name == parameter.name, true));
            menu.AddItem(new GUIContent("Ordering/Move to Bottom"), false, () => Reorder(controller, item => item.name == parameter.name, false));
            void Group(string label, Func<AnimatorControllerParameter, bool> match)
            {
                menu.AddItem(new GUIContent("Ordering/" + label + "/Move to Top"), false, () => Reorder(controller, match, true));
                menu.AddItem(new GUIContent("Ordering/" + label + "/Move to Bottom"), false, () => Reorder(controller, match, false));
            }
            Group("All " + parameter.type, item => item.type == parameter.type);
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            Group("VRChat Built-ins", item => Builtins.ContainsKey(item.name));
            Group("Avatar Dynamics", item => WrittenByDynamics(controller, item.name));
            Group("Synced Expressions", item => Status(controller, item).Contains("VRChat synced"));
            Group("Local Expressions", item => Status(controller, item).Contains("VRChat local"));
#endif
            foreach (AnimatorControllerParameterType type in Enum.GetValues(typeof(AnimatorControllerParameterType)))
            {
                var selected = type;
                menu.AddItem(new GUIContent("Add Above/" + selected), false, () => Insert(controller, parameter.name, selected, false));
                menu.AddItem(new GUIContent("Add Below/" + selected), false, () => Insert(controller, parameter.name, selected, true));
            }
        }
    }
}

