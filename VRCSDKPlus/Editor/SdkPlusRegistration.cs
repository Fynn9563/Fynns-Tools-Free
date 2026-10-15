#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace FynnsTools.VRCSDKPlus
{
    internal static class SdkPlusRegistration
    {
        const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic;
        [InitializeOnLoadMethod]
        static void Schedule()
        {
            EditorApplication.delayCall -= Register;
            EditorApplication.delayCall += Register;
        }
        static void Register()
        {
            Apply(typeof(VRCExpressionParameters));
            Apply(typeof(VRCExpressionsMenu));
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
        internal static void Refresh() { Register(); }
        internal static void Toggle(Type assetType)
        {
            if (!SdkPanelAppearance.LayoutEnabled) return;
            SetEnabled(assetType, EditorPrefs.GetBool("Fynn.SDKPlus.UseNative." + assetType.Name, false));
        }
        static void SetEnabled(Type assetType, bool enabled)
        {
            EditorPrefs.SetBool("Fynn.SDKPlus.UseNative." + assetType.Name, !enabled);
            Menu.SetChecked("CONTEXT/" + assetType.Name + "/[SDK+] Toggle Editor", enabled);
            Apply(assetType);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        internal static bool ValidateToggle(MenuCommand command, Type assetType)
        {
            Menu.SetChecked("CONTEXT/" + assetType.Name + "/[SDK+] Toggle Editor",
                SdkPanelAppearance.LayoutEnabled && !EditorPrefs.GetBool("Fynn.SDKPlus.UseNative." + assetType.Name, false));
            return SdkPanelAppearance.LayoutEnabled && command.context != null;
        }
        static void Apply(Type assetType)
        {
            try
            {
                var native = !SdkPanelAppearance.LayoutEnabled || EditorPrefs.GetBool("Fynn.SDKPlus.UseNative." + assetType.Name, false);
                Menu.SetChecked("CONTEXT/" + assetType.Name + "/[SDK+] Toggle Editor", !native);
                var parameters = assetType == typeof(VRCExpressionParameters);
                Type editorType = parameters ? typeof(VRCSDKPlus.VRCParamsPlus) : typeof(VRCSDKPlus.VRCMenuPlus);
                if (native)
                {
                    editorType = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        if (assembly.GetName().Name == "VRC.SDK3A.Editor")
                            editorType = assembly.GetType(parameters ? "VRC.SDK3.Editor.VRCExpressionParametersEditor" : "VRCExpressionsMenuEditor");
                }
                if (editorType == null) return;
                var attributes = typeof(Editor).Assembly.GetType("UnityEditor.CustomEditorAttributes");
                var initialized = attributes.GetField("s_Initialized", Static);
                if (!(bool)initialized.GetValue(null))
                {
                    attributes.GetMethod("Rebuild", Static).Invoke(null, null);
                    initialized.SetValue(null, true);
                }
                var dictionary = (IDictionary)attributes.GetField("kSCustomEditors", Static).GetValue(null);
                var entries = dictionary[assetType] as IList;
                if (entries == null || entries.Count == 0) return;
                var field = entries[0].GetType().GetField("m_InspectorType", BindingFlags.Public | BindingFlags.Instance);
                field.SetValue(entries[0], editorType);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[VRCSDK+] Inspector registration unavailable; the SDK editor remains accessible. " + e.Message);
            }
        }
    }
}
#endif
