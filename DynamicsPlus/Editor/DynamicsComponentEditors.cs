#if UNITY_EDITOR && FYNNS_VRC_DYNAMICS
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace FynnsToolkit.Dynamics
{
    internal static class DynamicsSdkEditors
    {
        internal static bool IsWorld(Component component) => component is DynamicsComponent dynamics && dynamics.Usage == DynamicsUsage.World;

        internal static Type TypeFor(Component component)
        {
            string name = component is VRCPhysBoneBase ? "VRC.SDK3.Dynamics.PhysBone.VRCPhysBoneEditor" :
                component is VRCPhysBoneColliderBase ? "VRC.SDK3.Dynamics.PhysBone.VRCPhysBoneColliderEditor" :
                component is VRCContactReceiver ? "VRC.SDK3.Dynamics.Contact.VRCContactReceiverEditor" :
                component is VRCContactSender ? "VRC.SDK3.Dynamics.Contact.VRCContactSenderEditor" : null;
            if (name == null) return null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(name, false);
                if (type != null && typeof(UnityEditor.Editor).IsAssignableFrom(type)) return type;
            }
            return null;
        }
    }

    internal static class DynamicsAvatarBridge
    {
        internal static Action<Component> Drawer { get; set; }
        internal static void Draw(Component component)
        {
            Drawer?.Invoke(component);
        }
    }
}
#endif

