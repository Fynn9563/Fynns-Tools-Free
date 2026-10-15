using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerConditionEditing
    {
        internal static bool Compatible(AnimatorConditionMode mode, AnimatorControllerParameterType type)
        {
            if (type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger)
                return mode == AnimatorConditionMode.If || mode == AnimatorConditionMode.IfNot;
            if (mode == AnimatorConditionMode.Greater || mode == AnimatorConditionMode.Less) return true;
            return type == AnimatorControllerParameterType.Int && (mode == AnimatorConditionMode.Equals || mode == AnimatorConditionMode.NotEqual);
        }
    }
    internal sealed partial class ControllerEditor
    {
        private struct ConditionSnapshot { internal string parameter; internal AnimatorConditionMode mode; }
        private static void ConditionElementPrefix(object __instance, int index, out ConditionSnapshot __state)
        {
            __state = default;
            if (!ControllerEditorExtraSettings.Current.preserveConditionMode) return;
            var array = HarmonyLib.AccessTools.Field(__instance.GetType(), "m_Conditions")?.GetValue(__instance) as SerializedProperty;
            if (array == null || index < 0 || index >= array.arraySize) return;
            var condition = array.GetArrayElementAtIndex(index);
            __state.parameter = condition.FindPropertyRelative("m_ConditionEvent")?.stringValue;
            __state.mode = (AnimatorConditionMode)(condition.FindPropertyRelative("m_ConditionMode")?.intValue ?? 0);
        }
        private static void ConditionElementPost(object __instance, int index, ConditionSnapshot __state)
        {
            if (__state.parameter == null) return;
            var array = HarmonyLib.AccessTools.Field(__instance.GetType(), "m_Conditions")?.GetValue(__instance) as SerializedProperty;
            var controller = HarmonyLib.AccessTools.Field(__instance.GetType(), "m_Controller")?.GetValue(__instance) as AnimatorController;
            if (array == null || !controller || index < 0 || index >= array.arraySize) return;
            var condition = array.GetArrayElementAtIndex(index);
            string parameter = condition.FindPropertyRelative("m_ConditionEvent")?.stringValue;
            if (parameter == __state.parameter) return;
            var definition = Array.Find(controller.parameters, value => value.name == parameter);
            if (definition != null && ControllerConditionEditing.Compatible(__state.mode, definition.type))
                condition.FindPropertyRelative("m_ConditionMode").intValue = (int)__state.mode;
        }
    }
}
