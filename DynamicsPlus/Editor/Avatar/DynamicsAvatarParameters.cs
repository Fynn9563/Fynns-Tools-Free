#if UNITY_EDITOR && FYNNS_ADO_AVATARS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;

namespace FynnsToolkit.Dynamics
{
    [InitializeOnLoad]
    internal static class DynamicsAvatarParameters
    {
        private static int layerIndex;
        private static int contactType = 2;
        private static VRCAvatarDescriptor selectedAvatar;
        static DynamicsAvatarParameters() { DynamicsAvatarBridge.Drawer = Draw; }

        private static void Draw(Component component)
        {
            if (!(component is VRCPhysBoneBase) && !(component is VRCContactReceiver)) return;
            var original = DynamicsPreview.Active?.OriginalOf(component) as Component;
            if (original == null) original = component;
            var parent = original.GetComponentInParent<VRCAvatarDescriptor>(true);
            var avatar = selectedAvatar != null ? selectedAvatar : parent;
            EditorGUI.BeginChangeCheck();
            avatar = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", avatar, typeof(VRCAvatarDescriptor), true);
            if (EditorGUI.EndChangeCheck()) selectedAvatar = avatar;
            if (avatar == null)
            {
                EditorGUILayout.HelpBox("Choose an avatar for Animator parameters.", MessageType.Info);
                return;
            }
            var layers = (avatar.baseAnimationLayers ?? Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>())
                .Concat(avatar.specialAnimationLayers ?? Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>())
                .Where(layer => !layer.isDefault && layer.animatorController is AnimatorController).ToArray();
            if (layers.Length == 0)
            {
                EditorGUILayout.HelpBox("Assign a custom playable-layer Animator Controller to add parameters.", MessageType.Info);
                return;
            }
            layerIndex = Mathf.Clamp(layerIndex, 0, layers.Length - 1);
            EditorGUILayout.LabelField("Avatar Animator parameters", EditorStyles.boldLabel);
            layerIndex = EditorGUILayout.Popup("Playable layer", layerIndex, layers.Select(layer => layer.type.ToString()).ToArray());
            var controller = (AnimatorController)layers[layerIndex].animatorController;
            string prefix = component is VRCPhysBoneBase bone ? bone.parameter : ((VRCContactReceiver)component).parameter;
            if (GUILayout.Button("Choose existing parameter"))
            {
                var menu = new GenericMenu();
                var names = controller.parameters.Select(p => Prefix(component, p.name)).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToArray();
                if (names.Length == 0) menu.AddDisabledItem(new GUIContent("No parameters"));
                foreach (string name in names)
                {
                    string chosen = name;
                    menu.AddItem(new GUIContent(chosen), prefix == chosen, () => DynamicsOperations.Change(new[] { component }, "Choose dynamics parameter", target =>
                    {
                        var value = new SerializedObject(target);
                        value.FindProperty("parameter").stringValue = chosen;
                        value.ApplyModifiedProperties();
                    }));
                }
                menu.ShowAsContext();
            }
            if (component is VRCContactReceiver)
                contactType = EditorGUILayout.Popup("Parameter type", contactType, new[] { "Bool", "Int", "Float" });
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(prefix) || DynamicsPreview.Active != null || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Add missing Animator parameters"))
                {
                    try { Add(controller, Parameters(component, contactType)); }
                    catch (Exception error) { EditorUtility.DisplayDialog("Dynamics+", error.Message, "OK"); }
                }
            }
            EditorGUILayout.LabelField("Uses the component's Parameter field. Existing parameters keep their defaults.", EditorStyles.wordWrappedMiniLabel);
        }

        internal static string Prefix(Component component, string name)
        {
            if (!(component is VRCPhysBoneBase)) return name;
            foreach (string suffix in new[] { "_IsGrabbed", "_IsPosed", "_Angle", "_Stretch", "_Squish" })
                if (name.EndsWith(suffix, StringComparison.Ordinal)) return name.Substring(0, name.Length - suffix.Length);
            return null;
        }

        internal static KeyValuePair<string, AnimatorControllerParameterType>[] Parameters(Component component, int receiverType)
        {
            if (component is VRCPhysBoneBase bone)
            {
                if (string.IsNullOrWhiteSpace(bone.parameter)) throw new InvalidOperationException("Set the PhysBone Parameter field first.");
                return new[] {
                    Pair(bone.parameter + "_IsGrabbed", AnimatorControllerParameterType.Bool),
                    Pair(bone.parameter + "_IsPosed", AnimatorControllerParameterType.Bool),
                    Pair(bone.parameter + "_Angle", AnimatorControllerParameterType.Float),
                    Pair(bone.parameter + "_Stretch", AnimatorControllerParameterType.Float),
                    Pair(bone.parameter + "_Squish", AnimatorControllerParameterType.Float)
                };
            }
            if (component is VRCContactReceiver receiver && !string.IsNullOrWhiteSpace(receiver.parameter))
            {
                var types = new[] { AnimatorControllerParameterType.Bool, AnimatorControllerParameterType.Int, AnimatorControllerParameterType.Float };
                if (receiverType < 0 || receiverType >= types.Length) throw new ArgumentOutOfRangeException(nameof(receiverType));
                return new[] { Pair(receiver.parameter, types[receiverType]) };
            }
            throw new InvalidOperationException("Set the Contact Receiver Parameter field first.");
        }

        private static KeyValuePair<string, AnimatorControllerParameterType> Pair(string name, AnimatorControllerParameterType type)
            => new KeyValuePair<string, AnimatorControllerParameterType>(name, type);

        internal static void Add(AnimatorController controller, IEnumerable<KeyValuePair<string, AnimatorControllerParameterType>> requested)
        {
            if (controller == null || !AssetDatabase.Contains(controller)) throw new InvalidOperationException("Choose a saved Animator Controller asset.");
            var parameters = requested.ToArray();
            var existing = controller.parameters;
            foreach (var parameter in parameters)
                if (existing.Any(p => p.name == parameter.Key && p.type != parameter.Value))
                    throw new InvalidOperationException("Parameter '" + parameter.Key + "' already exists with a different type. No parameters were changed.");
            var missing = parameters.Where(parameter => !existing.Any(p => p.name == parameter.Key)).ToArray();
            if (missing.Length == 0) return;
            Undo.RegisterCompleteObjectUndo(controller, "Add dynamics Animator parameters");
            foreach (var parameter in missing) controller.AddParameter(parameter.Key, parameter.Value);
            EditorUtility.SetDirty(controller);
        }
    }
}
#endif
