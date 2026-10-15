using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace HierarchyPlus
{
    internal static class ComponentIconHandler
    {
        private static readonly Dictionary<Type, ComponentInfo> componentInfoCache = new Dictionary<Type, ComponentInfo>();

        private struct ComponentInfo
        {
            public bool IsToggleable;
            public bool IsVRCComponent;
            public string VRCShapeType;
        }

        internal static GUIContent GetComponentIcon(Component component, Dictionary<string, GUIContent> customIconCache, GUIContent missingScriptContent, GUIContent defaultContent, Texture2D[] defaultTextures, System.Func<System.Type, Texture2D> getCustomIconFunc = null, bool showTooltips = true)
        {
            if (component == null)
            {
                var enhancedMissingContent = new GUIContent(missingScriptContent);
                enhancedMissingContent.tooltip = "Missing Script (Component reference is null)";
                return enhancedMissingContent;
            }

            Type type = component.GetType();

            if (getCustomIconFunc != null)
            {
                Texture2D registeredIcon = getCustomIconFunc(type);
                if (registeredIcon != null)
                {
                    string tooltip = showTooltips ? ObjectNames.NicifyVariableName(type.Name) : null;
                    return new GUIContent(registeredIcon, tooltip);
                }
            }

            if (IsMissingMonoBehaviour(component, type))
            {
                var missingMonoContent = new GUIContent(missingScriptContent);
                missingMonoContent.tooltip = "Missing MonoBehaviour Script";
                return missingMonoContent;
            }

            if (IsBrokenScriptReference(component))
            {
                var brokenScriptContent = new GUIContent(missingScriptContent);
                brokenScriptContent.tooltip = $"Missing Script ({type.Name})";
                return brokenScriptContent;
            }

            var vrcIcon = GetVRCComponentIcon(component, type, customIconCache, showTooltips);
            if (vrcIcon != null) return vrcIcon;

            if (customIconCache.TryGetValue(type.Name, out var customIcon))
                return new GUIContent(customIcon.image, showTooltips ? ObjectNames.NicifyVariableName(type.Name) : null);

            return GetUnityComponentIcon(component, type, defaultContent, defaultTextures, showTooltips);
        }

        internal static int GetMissingScriptCount(GameObject go)
        {
#if UNITY_2019_2_OR_NEWER
            return GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
#else
            int count = 0;
            var components = go.GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null) count++;
            }
            return count;
#endif
        }

        internal static GUIContent GetMissingScriptWarningIcon(int missingCount)
        {
            var warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            var content = new GUIContent(warningIcon.image, $"Missing Script(s): {missingCount}");
            return content;
        }

        internal static bool IsComponentActiveAndEnabled(Component component)
        {
            if (component == null) return false;
            if (!component.gameObject.activeInHierarchy) return false;

            Type type = component.GetType();
            if (!componentInfoCache.TryGetValue(type, out var info))
            {
                info = new ComponentInfo
                {
                    IsToggleable = IsComponentToggleable(component),
                    IsVRCComponent = type.Name.StartsWith("VRC"),
                    VRCShapeType = GetVRCShapeType(component)
                };
                componentInfoCache[type] = info;
            }

            if (!info.IsToggleable) return true;

            try
            {
                dynamic dynamicComponent = component;
                return dynamicComponent.enabled;
            }
            catch
            {
                return true;
            }
        }

        internal static bool IsComponentToggleable(Component component)
        {
            if (component == null) return false;

            return component is Behaviour ||
                   component is Renderer ||
                   component is Collider ||
                   component is AudioSource ||
                   component is Light;
        }

        internal static void SetComponentEnabled(Component component, bool enabled)
        {
            if (!IsComponentToggleable(component)) return;

            try
            {
                dynamic dynamicComponent = component;
                dynamicComponent.enabled = enabled;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to set component {component.GetType().Name} enabled state: {e.Message}");
            }
        }

        private static bool IsMissingMonoBehaviour(Component component, Type type)
        {
            return type == typeof(MonoBehaviour) || (component is MonoBehaviour && type.Name == "MonoBehaviour");
        }

        private static bool IsBrokenScriptReference(Component component)
        {
            try
            {
                var _ = component.GetType().FullName;
                var __ = component.name;
                return false;
            }
            catch
            {
                return true;
            }
        }

        private static GUIContent GetVRCComponentIcon(Component component, Type type, Dictionary<string, GUIContent> customIconCache, bool showTooltips = true)
        {
            string typeName = type.Name;
            string iconKey = null;
            string tooltip = typeName;

            switch (typeName)
            {
                case "VRCAvatarDescriptor":
                    iconKey = "VRCAvatarDescriptor";
                    break;

                case "PipelineManager":
                    iconKey = "VRCPipelineManager";
                    break;

                case "VRCPhysBone":
                    iconKey = "VRCPhysBone";
                    break;

                case "VRCPhysBoneCollider":
                    try
                    {
                        dynamic physBoneCollider = component;
                        var shapeType = physBoneCollider.shapeType;
                        string shapeTypeName = shapeType?.ToString() ?? "";

                        iconKey = shapeTypeName switch
                        {
                            "Plane" => "VRCPhysBoneColliderPlane",
                            "Sphere" => "VRCPhysBoneCollider",
                            "Capsule" => "VRCPhysBoneCollider",
                            "Box" => "VRCPhysBoneCollider",
                            _ => "VRCPhysBoneCollider"
                        };
                        tooltip = $"{typeName} ({shapeTypeName})";
                    }
                    catch
                    {
                        iconKey = "VRCPhysBoneCollider";
                    }
                    break;

                case "VRCContactReceiver":
                    iconKey = "VRCContactReceiver";
                    break;

                case "VRCContactSender":
                    iconKey = "VRCContactSender";
                    break;

                case "VRCImpostorSettings":
                    iconKey = "VRCImpostorSettings";
                    break;

                case "VRCImpostorEnvironment":
                    iconKey = "VRCImpostorSettings";
                    break;

                case "VRCHeadChop":
                    iconKey = "VRCHeadChop";
                    break;

                case "VRCParentConstraint":
                    iconKey = "VRCParentConstraint";
                    break;

                case "VRCPositionConstraint":
                    iconKey = "VRCPositionConstraint";
                    break;

                case "VRCRotationConstraint":
                    iconKey = "VRCRotationConstraint";
                    break;

                case "VRCScaleConstraint":
                    iconKey = "VRCScaleConstraint";
                    break;

                case "VRCAimConstraint":
                    iconKey = "VRCAimConstraint";
                    break;

                case "VRCLookAtConstraint":
                    iconKey = "VRCLookAtConstraint";
                    break;

                case "VRCPerPlatformOverrides":
                    iconKey = "VRCPerPlatformOverrides";
                    break;

                case "VRCSceneDescriptor":
                    iconKey = "VRCSceneDescriptor";
                    break;

                case "VRCPickup":
                    iconKey = "VRCPickup";
                    break;

                case "VRCMirrorReflection":
                    iconKey = "VRCMirrorReflection";
                    break;

                case "VRCStation":
                    iconKey = "VRCStation";
                    break;

                case "VRCObjectSync":
                    iconKey = "VRCObjectSync";
                    break;

                case "VRCObjectPool":
                    iconKey = "VRCObjectPool";
                    break;

                case "VRCPortalMarker":
                    iconKey = "VRCPortalMarker";
                    break;

                case "VRCAvatarPedestal":
                    iconKey = "VRCAvatarPedestal";
                    break;

                case "VRCAVProVideoPlayer":
                    iconKey = "VRCAVProVideoPlayer";
                    break;

                case "VRCAVProVideoScreen":
                    iconKey = "VRCAVProVideoScreen";
                    break;

                case "VRCAVProVideoSpeaker":
                    iconKey = "VRCAVProVideoSpeaker";
                    break;

                case "VRCUnityVideoPlayer":
                    iconKey = "VRCUnityVideoPlayer";
                    break;

                case "VRCSpatialAudioSource":
                    iconKey = "VRCSpatialAudioSource";
                    break;

                case "VRCUiShape":
                    iconKey = "VRCUiShape";
                    break;

                case "VRCUrlInputField":
                    iconKey = "VRCURLInputField";
                    break;

                case "VRCFury":
                    iconKey = "VRCFury";
                    break;

                case "VRCFuryComponent":
                    iconKey = "VRCFury";
                    break;

                case "VRCFuryGlobalCollider":
                    iconKey = "VRCFuryGlobalCollider";
                    break;

                case "VRCFuryHapticPlug":
                    iconKey = "VRCFuryHapticPlug";
                    break;

                case "VRCFuryHapticSocket":
                    iconKey = "VRCFuryHapticSocket";
                    break;

                case "VRCFuryHapticTouchReceiver":
                    iconKey = "VRCFuryHapticTouchReceiver";
                    break;

                case "VRCFuryHapticTouchSender":
                    iconKey = "VRCFuryHapticSender";
                    break;

                case "UdonBehaviour":
                    iconKey = "VRCUdonBehaviour";
                    break;

                case "UdonSharpBehaviour":
                    return GetUdonSharpIcon(component, customIconCache, showTooltips);

                default:
                    if (IsUdonSharpType(type))
                    {
                        return GetUdonSharpIcon(component, customIconCache, showTooltips);
                    }
                    return null;
            }

            if (!string.IsNullOrEmpty(iconKey) && customIconCache.TryGetValue(iconKey, out var vrcIcon))
            {
                return new GUIContent(vrcIcon.image,
                    showTooltips ? ObjectNames.NicifyVariableName(tooltip) : null);
            }

            return null;
        }

        private static string GetVRCShapeType(Component component)
        {
            if (component?.GetType().Name == "VRCPhysBoneCollider")
            {
                try
                {
                    dynamic physBoneCollider = component;
                    return physBoneCollider.shapeType?.ToString() ?? "";
                }
                catch
                {
                    return "";
                }
            }
            return "";
        }

        private static GUIContent GetUnityComponentIcon(Component component, Type type, GUIContent defaultContent, Texture2D[] defaultTextures, bool showTooltips = true)
        {
            string tooltip = showTooltips ? ObjectNames.NicifyVariableName(type.Name) : null;

            Texture2D icon = AssetPreview.GetMiniThumbnail(component);
            if (!icon || defaultTextures.Any(t => icon == t))
            {
                icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(component)) as Texture2D;
                if (!icon || defaultTextures.Any(t => icon == t))
                {
                    return new GUIContent(defaultContent.image, tooltip);
                }
            }

            return new GUIContent(icon, tooltip);
        }

        private static bool IsUdonSharpType(Type type)
        {
            for (var parent = type.BaseType; parent != null; parent = parent.BaseType)
                if (parent.Name == "UdonSharpBehaviour") return true;
            return false;
        }

        private static GUIContent GetUdonSharpIcon(Component component, Dictionary<string, GUIContent> customIconCache, bool showTooltips)
        {
            Texture2D scriptIcon = null;
            string tooltip = showTooltips ? "Udon Sharp Behaviour" : null;

#if UNITY_EDITOR
            try
            {
                var monoScript = MonoScript.FromMonoBehaviour(component as MonoBehaviour);
                if (monoScript != null)
                {
                    var candidate = AssetPreview.GetMiniThumbnail(monoScript);

                    if (!IsDefaultCSharpScriptIcon(candidate))
                        scriptIcon = candidate;

                    if (showTooltips)
                    {
                        var cls = monoScript.GetClass();
                        tooltip = cls?.Name ?? "Udon Sharp Behaviour";
                    }
                }
            }
            catch
            {
            }
#endif

            if (scriptIcon != null)
            {
                return new GUIContent(scriptIcon, tooltip);
            }

            var udonIcon = Resources.Load<Texture2D>("Icons/vrcUdonSharpBehaviour");
            if (udonIcon != null)
            {
                return new GUIContent(udonIcon, tooltip);
            }

            if (component != null && customIconCache.TryGetValue(component.GetType().Name, out var namedIcon))
                return new GUIContent(namedIcon.image, tooltip);

            if (customIconCache.TryGetValue("VRCUdonBehaviour", out var customIcon))
                return new GUIContent(customIcon.image, tooltip);

            return null;
        }

        private static bool IsDefaultCSharpScriptIcon(Texture2D tex)
        {
            if (tex == null) return true;
            var cs = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
            return ReferenceEquals(tex, cs);
        }

        internal static void ClearCache()
        {
            componentInfoCache.Clear();
        }
    }
}
