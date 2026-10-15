#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    internal static class DynamicsContextMenus
    {
        [MenuItem("CONTEXT/VRCPhysBone/Dynamics+/Open Dynamics Editor")]
        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/Open Dynamics Editor")]
        [MenuItem("CONTEXT/VRCContactReceiver/Dynamics+/Open Dynamics Editor")]
        [MenuItem("CONTEXT/VRCContactSender/Dynamics+/Open Dynamics Editor")]
        private static void Open(MenuCommand command) { DynamicsEditorWindow.OpenFor(command.context as Component); }

        [MenuItem("CONTEXT/VRCPhysBone/Dynamics+/Toggle Editor", false, 899)]
        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/Toggle Editor", false, 899)]
        [MenuItem("CONTEXT/VRCContactReceiver/Dynamics+/Toggle Editor", false, 899)]
        [MenuItem("CONTEXT/VRCContactSender/Dynamics+/Toggle Editor", false, 899)]
        private static void Toggle(MenuCommand command) { DynamicsSettings.instance.ToggleInspectors(command.context as Component); UnityEditorInternal.InternalEditorUtility.RepaintAllViews(); }

        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/Move To Empty", false, 896)]
        private static void Move(MenuCommand command)
        {
            Run(() => Selection.activeGameObject = DynamicsOperations.MoveCollider((VRCPhysBoneCollider)command.context).gameObject);
        }
        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/Move To Empty", true)]
        private static bool CanMove(MenuCommand command)
        {
            return command.context != null && !EditorUtility.IsPersistent(command.context) && !EditorApplication.isPlayingOrWillChangePlaymode && DynamicsPreview.Active == null;
        }

        [MenuItem("CONTEXT/VRCContactReceiver/Dynamics+/To Sender", false, 897)]
        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/To Sender", false, 897)]
        private static void ToSender(MenuCommand command) { Convert(command, DynamicsConversions.ComponentKind.Sender); }
        [MenuItem("CONTEXT/VRCContactReceiver/Dynamics+/To Sender", true)]
        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/To Sender", true)]
        private static bool CanSender(MenuCommand command) => DynamicsConversions.CanCreate(command.context as Component, DynamicsConversions.ComponentKind.Sender, out _);

        [MenuItem("CONTEXT/VRCContactSender/Dynamics+/To Receiver", false, 897)]
        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/To Receiver", false, 898)]
        private static void ToReceiver(MenuCommand command) { Convert(command, DynamicsConversions.ComponentKind.Receiver); }
        [MenuItem("CONTEXT/VRCContactSender/Dynamics+/To Receiver", true)]
        [MenuItem("CONTEXT/VRCPhysBoneCollider/Dynamics+/To Receiver", true)]
        private static bool CanReceiver(MenuCommand command) => DynamicsConversions.CanCreate(command.context as Component, DynamicsConversions.ComponentKind.Receiver, out _);

        [MenuItem("CONTEXT/VRCContactSender/Dynamics+/To Collider", false, 898)]
        [MenuItem("CONTEXT/VRCContactReceiver/Dynamics+/To Collider", false, 898)]
        private static void ToCollider(MenuCommand command) { Convert(command, DynamicsConversions.ComponentKind.Collider); }
        [MenuItem("CONTEXT/VRCContactSender/Dynamics+/To Collider", true)]
        [MenuItem("CONTEXT/VRCContactReceiver/Dynamics+/To Collider", true)]
        private static bool CanCollider(MenuCommand command) => DynamicsConversions.CanCreate(command.context as Component, DynamicsConversions.ComponentKind.Collider, out _);

        internal static bool ConfirmReplacement(Component source)
        {
            int references = DynamicsConversions.CountReferences(source);
            return references == 0 || EditorUtility.DisplayDialog("Convert dynamics component",
                references + " scene references point to this component. Converting to a different type will clear them. The original component will be replaced.", "Convert", "Cancel");
        }

        private static void Convert(MenuCommand command, DynamicsConversions.ComponentKind kind)
        {
            var source = command.context as Component;
            if (!ConfirmReplacement(source)) return;
            Run(() => Selection.activeObject = DynamicsConversions.Create(source, kind, true));
        }

        private static void Run(Action action)
        {
            try { action(); }
            catch (Exception exception) { Debug.LogException(exception); EditorUtility.DisplayDialog("Dynamics+", exception.Message, "OK"); }
        }
    }
}
#endif

