using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace FynnsTools.ControllerEditor
{
    [InitializeOnLoad]
    internal static partial class ControllerParameterTools
    {
        internal static readonly Dictionary<string, AnimatorControllerParameterType> Builtins = CreateBuiltins();
#if FYNNS_CONTROLLER_EDITOR_AVATARS
        private static readonly Dictionary<AnimatorController, VRCAvatarDescriptor> avatars = new Dictionary<AnimatorController, VRCAvatarDescriptor>();
        private static readonly Dictionary<VRCAvatarDescriptor, HashSet<string>> dynamics = new Dictionary<VRCAvatarDescriptor, HashSet<string>>();
#endif
        private static readonly Dictionary<AnimatorController, Dictionary<string, List<ControllerEditorInspection.Usage>>> usages = new Dictionary<AnimatorController, Dictionary<string, List<ControllerEditorInspection.Usage>>>();
        static ControllerParameterTools() { Undo.undoRedoPerformed += ClearCache; EditorApplication.projectChanged += ClearCache; EditorApplication.hierarchyChanged += ClearCache; }
        internal static void ClearCache()
        {
            usages.Clear(); ClearPresentationCache();
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            avatars.Clear(); dynamics.Clear();
#endif
        }
        private static Dictionary<string, AnimatorControllerParameterType> CreateBuiltins()
        {
            var result = new Dictionary<string, AnimatorControllerParameterType>(StringComparer.Ordinal);
            foreach (string name in ControllerEditorUtility.m_AdapterProcessor) result[name] = AnimatorControllerParameterType.Float;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            foreach (string name in new[] { "IsLocal", "Grounded", "Seated", "AFK", "MuteSelf", "InStation", "Earmuffs", "IsOnFriendsList", "ScaleModified" }) result[name] = AnimatorControllerParameterType.Bool;
            foreach (string name in new[] { "Viseme", "GestureLeft", "GestureRight", "TrackingType", "VRMode", "AvatarVersion" }) result[name] = AnimatorControllerParameterType.Int;
#endif
            return result;
        }
#if FYNNS_CONTROLLER_EDITOR_AVATARS
        internal static VRCAvatarDescriptor Avatar(AnimatorController controller)
        {
            if (!controller) return null;
            if (avatars.TryGetValue(controller, out var known) && known) return known;
            var found = Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>().Where(avatar => avatar && avatar.gameObject.scene.IsValid() && !EditorUtility.IsPersistent(avatar))
                .FirstOrDefault(avatar => Uses(avatar, controller));
            avatars[controller] = found; return found;
        }
        private static bool Uses(VRCAvatarDescriptor avatar, AnimatorController controller)
        {
            if (avatar.baseAnimationLayers.Any(layer => !layer.isDefault && layer.animatorController == controller) ||
                avatar.specialAnimationLayers.Any(layer => !layer.isDefault && layer.animatorController == controller)) return true;
            var animator = avatar.GetComponent<Animator>();
            if (animator && animator.runtimeAnimatorController == controller) return true;
            foreach (var component in avatar.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!component || component == avatar) continue;
                using (var serialized = new SerializedObject(component))
                {
                    var property = serialized.GetIterator();
                    while (property.Next(true)) if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == controller) return true;
                }
            }
            return false;
        }
#endif
        internal static List<ControllerEditorInspection.Usage> Usages(AnimatorController controller, string parameter)
        {
            if (!controller) return new List<ControllerEditorInspection.Usage>();
            if (!usages.TryGetValue(controller, out var cache)) usages[controller] = cache = new Dictionary<string, List<ControllerEditorInspection.Usage>>();
            if (!cache.TryGetValue(parameter, out var result)) cache[parameter] = result = ControllerEditorInspection.FindParameter(controller, parameter);
            return result;
        }
#if FYNNS_CONTROLLER_EDITOR_AVATARS
        internal static bool WrittenByDynamics(AnimatorController controller, string parameter)
        {
            var avatar = Avatar(controller);
            if (!avatar) return false;
            if (!dynamics.TryGetValue(avatar, out var names))
            {
                dynamics[avatar] = names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var component in avatar.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (!component) continue;
                    string type = component.GetType().Name;
                    if (type != "VRCPhysBone" && type != "VRCContactReceiver") continue;
                    using (var serialized = new SerializedObject(component))
                    {
                        var property = serialized.FindProperty("parameter");
                        if (property == null || property.propertyType != SerializedPropertyType.String || string.IsNullOrEmpty(property.stringValue)) continue;
                        if (type == "VRCContactReceiver") names.Add(property.stringValue);
                        else foreach (string suffix in new[] { "_IsGrabbed", "_IsPosed", "_Angle", "_Stretch", "_Squish" }) names.Add(property.stringValue + suffix);
                    }
                }
            }
            return names.Contains(parameter);
        }
        internal static string Status(AnimatorController controller, AnimatorControllerParameter parameter)
        {
            return Present(controller, parameter).Tooltip;
        }
        private static AnimatorControllerParameterType ToAnimator(VRCExpressionParameters.ValueType type)
        { return type == VRCExpressionParameters.ValueType.Bool ? AnimatorControllerParameterType.Bool : type == VRCExpressionParameters.ValueType.Int ? AnimatorControllerParameterType.Int : AnimatorControllerParameterType.Float; }
#else
        internal static bool WrittenByDynamics(AnimatorController controller, string parameter) { return false; }
        internal static string Status(AnimatorController controller, AnimatorControllerParameter parameter) { return "Controller parameter"; }
#endif
        internal static float DrawBadge(Rect rect, AnimatorController controller, AnimatorControllerParameter parameter, Action show)
        {
            return DrawPresentation(rect, controller, parameter, show);
        }
        internal static void AddBuiltins(GenericMenu menu, AnimatorController controller)
        {
            if (!controller || Builtins.Count == 0) return;
            menu.AddSeparator("");
            foreach (var builtin in Builtins.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var value = builtin;
                var content = new GUIContent("VRChat Built-in/" + value.Key);
                if (controller.parameters.Any(parameter => parameter.name == value.Key)) menu.AddDisabledItem(content, true);
                else menu.AddItem(content, false, () => { Undo.RecordObject(controller, "Add Built-in Parameter"); controller.AddParameter(value.Key, value.Value); EditorUtility.SetDirty(controller); ClearCache(); });
            }
        }
        internal static void AddMenu(GenericMenu menu, AnimatorController controller, AnimatorControllerParameter parameter)
        {
            AddOrderingMenu(menu, controller, parameter);
            menu.AddItem(new GUIContent("Copy Name"), false, () => EditorGUIUtility.systemCopyBuffer = parameter.name);
            if (parameter.type == AnimatorControllerParameterType.Float)
                menu.AddItem(new GUIContent("Create Constant Animation…"), false, () => PopupWindow.Show(new Rect(Event.current?.mousePosition ?? Vector2.zero, Vector2.zero), new ConstantParameterPopup(parameter.name, parameter.defaultFloat)));
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            var avatar = Avatar(controller);
            if (avatar && avatar.expressionsMenu) menu.AddItem(new GUIContent("View in Expressions Menu"), false, () => ControllerEditor.ShowExpressionMenuUsages(avatar, parameter.name));
            if (Builtins.ContainsKey(parameter.name)) menu.AddItem(new GUIContent("Open VRChat Parameter Documentation"), false, () => Application.OpenURL("https://creators.vrchat.com/avatars/animator-parameters/"));
            if (!avatar || !avatar.expressionParameters || Builtins.ContainsKey(parameter.name) || parameter.type == AnimatorControllerParameterType.Trigger)
            { menu.AddDisabledItem(new GUIContent("VRChat Parameter/" + (Builtins.ContainsKey(parameter.name) ? "Built-in parameter" : "Assign avatar expression parameters first"))); return; }
            var file = avatar.expressionParameters;
            var expression = (file.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).FirstOrDefault(item => item != null && item.name == parameter.name);
            menu.AddItem(new GUIContent("VRChat Parameter/Synced"), expression != null && expression.networkSynced, () => SetExpression(file, parameter, true, expression?.saved ?? true));
            menu.AddItem(new GUIContent("VRChat Parameter/Local"), expression != null && !expression.networkSynced, () => SetExpression(file, parameter, false, expression?.saved ?? true));
            if (expression != null)
            {
                menu.AddItem(new GUIContent("VRChat Parameter/Saved"), expression.saved, () => SetExpression(file, parameter, expression.networkSynced, !expression.saved));
                menu.AddItem(new GUIContent("VRChat Parameter/Match Controller Type and Default"), false, () => SetExpression(file, parameter, expression.networkSynced, expression.saved));
                menu.AddItem(new GUIContent("VRChat Parameter/Remove"), false, () => { Undo.RecordObject(file, "Remove Expression Parameter"); file.parameters = (file.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).Where(item => item == null || item.name != parameter.name).ToArray(); EditorUtility.SetDirty(file); ClearCache(); });
            }
#endif
        }
#if FYNNS_CONTROLLER_EDITOR_AVATARS
        internal static bool SetExpression(VRCExpressionParameters file, AnimatorControllerParameter parameter, bool synced, bool saved)
        {
            if (!file || !AssetDatabase.IsOpenForEdit(file) || string.IsNullOrWhiteSpace(parameter.name) || parameter.type == AnimatorControllerParameterType.Trigger || Builtins.ContainsKey(parameter.name)) return false;
            var type = parameter.type == AnimatorControllerParameterType.Bool ? VRCExpressionParameters.ValueType.Bool : parameter.type == AnimatorControllerParameterType.Int ? VRCExpressionParameters.ValueType.Int : VRCExpressionParameters.ValueType.Float;
            var existing = (file.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).FirstOrDefault(item => item != null && item.name == parameter.name);
            int oldCost = existing != null && existing.networkSynced ? (existing.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8) : 0;
            int newCost = synced ? (type == VRCExpressionParameters.ValueType.Bool ? 1 : 8) : 0;
            if ((file.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).Where(item => item != null && item.networkSynced).Sum(item => item.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8) - oldCost + newCost > VRCExpressionParameters.MAX_PARAMETER_COST)
            { Debug.LogWarning("The expression parameter would exceed the synced memory limit.", file); return false; }
            Undo.RecordObject(file, "Update Expression Parameter");
            if (existing == null) { existing = new VRCExpressionParameters.Parameter { name = parameter.name }; file.parameters = (file.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).Concat(new[] { existing }).ToArray(); }
            existing.valueType = type; existing.networkSynced = synced; existing.saved = saved;
            existing.defaultValue = parameter.type == AnimatorControllerParameterType.Bool ? (parameter.defaultBool ? 1 : 0) : parameter.type == AnimatorControllerParameterType.Int ? parameter.defaultInt : parameter.defaultFloat;
            EditorUtility.SetDirty(file); ClearCache(); return true;
        }
#endif
    }
}
