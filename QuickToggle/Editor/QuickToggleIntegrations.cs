#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsToolkit.QuickToggle
{
    // Discover optional integrations without assembly dependencies.
    internal static class QuickToggleIntegrations
    {
        private const string Avatar = "VRC.SDK3.Avatars.Components.VRCAvatarDescriptor";
        private const string Menu = "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu";
        private const string Params = "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters";
        private const string MA = "nadena.dev.modular_avatar.core.";
        private const string Fury = "com.vrcfury.api.FuryComponents";
        internal static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).FirstOrDefault(t => t != null);
        internal static Component Descriptor(GameObject root) => root == null || Find(Avatar) == null ? null : root.GetComponent(Find(Avatar));
        internal static bool Available(SetupMode mode)
        {
            try { CheckSchema(mode); return true; } catch { return false; }
        }
        private static Type Require(string name) => Find(name) ?? throw new InvalidOperationException("Required package type is unavailable: " + name + ". Install or update the selected package.");
        private static FieldInfo Field(Type type, string name) => type.GetField(name, BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidOperationException("Unsupported package version: " + type.Name + "." + name + " is unavailable.");
        private static object Get(object obj, string field) => Field(obj.GetType(), field).GetValue(obj);
        private static void Set(object obj, string field, object value) => Field(obj.GetType(), field).SetValue(obj, value);
        private static void SetEnum(object obj, string field, string value)
        { var f = Field(obj.GetType(), field); f.SetValue(obj, Enum.Parse(f.FieldType, value)); }
        private static MethodInfo Method(Type type, string name, params Type[] args) => type.GetMethod(name, args) ?? throw new InvalidOperationException("Unsupported package version: " + type.Name + "." + name + " is unavailable.");
        private static object Call(object obj, string name, params object[] args)
        { return Method(obj.GetType(), name, args.Select(a => a.GetType()).ToArray()).Invoke(obj, args); }

        private static void CheckSchema(SetupMode mode)
        {
            if (mode == SetupMode.ClipsOnly) return;
            var avatar = Require(Avatar);
            foreach (var f in new[] { "baseAnimationLayers", "expressionParameters", "expressionsMenu", "customExpressions", "customizeAnimationLayers" }) Field(avatar, f);
            var menu = Require(Menu); Field(menu, "controls");
            var parameters = Require(Params); Field(parameters, "parameters");
            if (mode == SetupMode.VRCFury)
            {
                var toggle = Method(Require(Fury), "CreateToggle", typeof(GameObject)).ReturnType;
                Method(toggle, "SetMenuPath", typeof(string)); Method(toggle, "SetSaved"); Method(toggle, "SetDefaultOn");
                var actions = Method(toggle, "GetActions").ReturnType;
                Method(actions, "AddAnimationClip", typeof(AnimationClip));
                Method(actions, "AddTurnOn", typeof(GameObject));
                Method(actions, "AddBlendshape", typeof(string), typeof(float), typeof(Renderer));
                foreach (var f in new[] { "obj", "mode" }) Field(Require("VF.Model.StateAction.ObjectToggleAction"), f);
                Field(Require("VF.Model.VRCFury"), "content");
                Field(Require("VF.Model.Feature.Toggle"), "state");
                Field(Require("VF.Model.State"), "actions");
            }
            if (mode == SetupMode.ModularAvatar)
            {
                foreach (var f in new[] { "animator", "layerType", "pathMode", "matchAvatarWriteDefaults", "mergeAnimatorMode" }) Field(Require(MA + "ModularAvatarMergeAnimator"), f);
                foreach (var f in new[] { "menuToAppend", "installTargetMenu" }) Field(Require(MA + "ModularAvatarMenuInstaller"), f);
                Field(Require(MA + "ModularAvatarParameters"), "parameters");
                foreach (var f in new[] { "nameOrPrefix", "internalParameter", "syncType", "defaultValue", "saved", "hasExplicitDefaultValue" }) Field(Require(MA + "ParameterConfig"), f);
            }
        }

        internal static void Validate(ToggleRequest r, List<TogglePlan> plans)
        {
            CheckSchema(r.setup);
            if (r.setup == SetupMode.ClipsOnly) return;
            var avatar = Descriptor(r.root);
            if (avatar == null) throw new InvalidOperationException("Choose the avatar root with its Avatar Descriptor for this setup.");
            if (r.setup == SetupMode.VRChat && !r.fx && !r.parameters && !r.menu)
                throw new InvalidOperationException("Choose at least one VRChat setup option, or use Animation Clips.");
            if (r.targetMenu != null && !Require(Menu).IsInstanceOfType(r.targetMenu)) throw new InvalidOperationException("Choose a VRChat expression menu asset.");
            if (r.targetMenu != null && !EditorUtility.IsPersistent(r.targetMenu)) throw new InvalidOperationException("Save the target menu as an asset first.");
            if (r.setup == SetupMode.VRChat)
            {
                var layers = BaseLayers(avatar);
                var fx = FxLayer(layers);
                var controller = Get(fx, "animatorController") as RuntimeAnimatorController;
                if (r.fx && !(bool)Get(fx, "isDefault") && controller != null && !(controller is AnimatorController))
                    throw new InvalidOperationException("Direct setup requires an Animator Controller in the FX layer, rather than an override controller.");
                if (r.fx && !(bool)Get(fx, "isDefault") && controller != null) Editable(controller);
                var parameters = Get(avatar, "expressionParameters") as Object;
                var menu = r.targetMenu != null ? r.targetMenu : Get(avatar, "expressionsMenu") as Object;
                if (r.parameters && parameters != null)
                {
                    Editable(parameters);
                    var existingParameters = Get(parameters, "parameters") as Array;
                    var cost = existingParameters == null ? 0 : Convert.ToInt32(Method(parameters.GetType(), "CalcTotalCost").Invoke(parameters, null));
                    var max = Convert.ToInt32(parameters.GetType().GetField("MAX_PARAMETER_COST", BindingFlags.Public | BindingFlags.Static).GetValue(null));
                    var array = Get(parameters, "parameters") as Array;
                    var maxCount = Convert.ToInt32(parameters.GetType().GetField("MAX_PARAMETER_COUNT", BindingFlags.Public | BindingFlags.Static).GetValue(null));
                    if (cost + plans.Count > max || (array == null ? 0 : array.Length) + plans.Count > maxCount)
                        throw new InvalidOperationException("Expression Parameters does not have enough space for " + plans.Count + " new Boolean parameters.");
                }
                if (r.menu)
                {
                    if (menu != null) Editable(menu);
                    var count = menu == null ? 0 : ((IList)Get(menu, "controls"))?.Count ?? 0;
                    var limit = Convert.ToInt32(Require(Menu).GetField("MAX_CONTROLS", BindingFlags.Public | BindingFlags.Static).GetValue(null));
                    if (count + plans.Count > limit) throw new InvalidOperationException("The target menu needs " + plans.Count + " free slots. Choose a submenu or use VRCFury / Modular Avatar.");
                }
                if (string.IsNullOrWhiteSpace(r.parameter) || r.parameter.Any(char.IsControl)) throw new InvalidOperationException("Enter a parameter name.");
                var used = new HashSet<string>(StringComparer.Ordinal);
                foreach (var layer in layers)
                {
                    var c = Get(layer, "animatorController") as AnimatorController;
                    if (c != null) foreach (var p in c.parameters) used.Add(p.name);
                }
                if (parameters != null && Get(parameters, "parameters") is Array arrayParams)
                    foreach (var p in arrayParams) if (p != null) used.Add((string)Get(p, "name"));
                foreach (var p in plans)
                {
                    var candidate = r.parameter.Trim(); var suffix = 2;
                    while (!used.Add(candidate)) candidate = r.parameter.Trim() + " " + suffix++;
                    p.parameter = candidate;
                }
            }
            else foreach (var p in plans) p.parameter = "FTK_" + Guid.NewGuid().ToString("N");
        }

        private static void Editable(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !AssetDatabase.IsOpenForEdit(asset))
                throw new InvalidOperationException("Choose an editable asset under Assets: " + asset.name);
        }
        private static object FxLayer(Array layers)
        {
            if (layers == null) throw new InvalidOperationException("The Avatar Descriptor has no animation layer array.");
            foreach (var layer in layers) if (Get(layer, "type").ToString() == "FX") return layer;
            throw new InvalidOperationException("The Avatar Descriptor has no FX layer entry.");
        }
        private static Array BaseLayers(Component avatar)
        {
            var existing = Get(avatar, "baseAnimationLayers") as Array;
            var element = Field(avatar.GetType(), "baseAnimationLayers").FieldType.GetElementType();
            var names = existing == null || existing.Length == 0 ? new[] { "Base", "Additive", "Gesture", "Action", "FX" } : new[] { "FX" };
            if (existing != null && existing.Cast<object>().Any(l => Get(l, "type").ToString() == "FX")) return existing;
            var layers = Array.CreateInstance(element, (existing == null ? 0 : existing.Length) + names.Length);
            if (existing != null) Array.Copy(existing, layers, existing.Length);
            for (var i = 0; i < names.Length; i++)
            {
                var layer = Activator.CreateInstance(element); SetEnum(layer, "type", names[i]); Set(layer, "isDefault", true);
                layers.SetValue(layer, (existing == null ? 0 : existing.Length) + i);
            }
            return layers;
        }

        internal static void Create(ToggleRequest r, List<TogglePlan> plans, ToggleResult result)
        {
            if (r.setup == SetupMode.ClipsOnly) return;
            if (r.setup == SetupMode.VRChat) { CreateDirect(r, plans, result); return; }
            foreach (var p in plans)
            {
                var host = new GameObject(p.name + " (" + (r.setup == SetupMode.VRCFury ? "VRCFury" : "MA") + ")");
                Undo.RegisterCreatedObjectUndo(host, "Create toggle setup");
                Undo.SetTransformParent(host.transform, r.root.transform, "Parent toggle setup");
                host.transform.localPosition = Vector3.zero; host.transform.localRotation = Quaternion.identity; host.transform.localScale = Vector3.one;
                result.objects.Add(host);
                if (r.setup == SetupMode.VRCFury)
                {
                    var toggle = Method(Require(Fury), "CreateToggle", typeof(GameObject)).Invoke(null, new object[] { host });
                    foreach (var component in host.GetComponents<Component>().Where(c => !(c is Transform)))
                    {
                        Undo.RegisterCreatedObjectUndo(component, "Create VRCFury toggle");
                        Undo.RegisterCompleteObjectUndo(component, "Configure VRCFury toggle");
                    }
                    Call(toggle, "SetMenuPath", MenuLabel(r, p));
                    if (r.saved) Call(toggle, "SetSaved");
                    if (r.defaultOn) Call(toggle, "SetDefaultOn");
                    var actions = Call(toggle, "GetActions");
                    foreach (var binding in AnimationUtility.GetCurveBindings(p.on))
                    {
                        var target = string.IsNullOrEmpty(binding.path) ? r.root.transform : r.root.transform.Find(binding.path);
                        var value = AnimationUtility.GetEditorCurve(p.on, binding).Evaluate(0);
                        if (binding.type == typeof(GameObject) && binding.propertyName == "m_IsActive")
                        {
                            if (value > .5f) Call(actions, "AddTurnOn", target.gameObject);
                            else
                            {
                                var action = Activator.CreateInstance(Require("VF.Model.StateAction.ObjectToggleAction"), true);
                                Set(action, "obj", target.gameObject); SetEnum(action, "mode", "TurnOff");
                                var component = host.GetComponent(Require("VF.Model.VRCFury"));
                                ((IList)Get(Get(Get(component, "content"), "state"), "actions")).Add(action);
                            }
                            AnimationUtility.SetEditorCurve(p.on, binding, null);
                        }
                        else if (binding.type == typeof(SkinnedMeshRenderer) && binding.propertyName.StartsWith("blendShape.", StringComparison.Ordinal))
                        {
                            Method(actions.GetType(), "AddBlendshape", typeof(string), typeof(float), typeof(Renderer)).Invoke(actions,
                                new object[] { binding.propertyName.Substring("blendShape.".Length), value, target.GetComponent<SkinnedMeshRenderer>() });
                            AnimationUtility.SetEditorCurve(p.on, binding, null);
                        }
                    }
                    // VRCFury component-enabled bindings still require a clip.
                    if (AnimationUtility.GetCurveBindings(p.on).Length > 0)
                    {
                        QuickToggleCore.SaveAsset(p.on, r.folder, result);
                        Call(actions, "AddAnimationClip", p.on);
                    }
                    foreach (var component in host.GetComponents<Component>()) EditorUtility.SetDirty(component);
                }
                else
                {
                    var controller = new AnimatorController { name = p.name + " FX" };
                    QuickToggleCore.SaveAsset(controller, r.folder, result);
                    AddLayer(controller, p, r);
                    var merge = Undo.AddComponent(host, Require(MA + "ModularAvatarMergeAnimator"));
                    Undo.RegisterCompleteObjectUndo(merge, "Configure MA animator");
                    Set(merge, "animator", controller); SetEnum(merge, "layerType", "FX"); SetEnum(merge, "pathMode", "Absolute");
                    SetEnum(merge, "mergeAnimatorMode", "Append"); Set(merge, "matchAvatarWriteDefaults", true);
                    var parameters = Undo.AddComponent(host, Require(MA + "ModularAvatarParameters"));
                    Undo.RegisterCompleteObjectUndo(parameters, "Configure MA parameters");
                    var config = Activator.CreateInstance(Require(MA + "ParameterConfig"));
                    Set(config, "nameOrPrefix", p.parameter); Set(config, "internalParameter", true); SetEnum(config, "syncType", "Bool");
                    Set(config, "defaultValue", r.defaultOn ? 1f : 0f); Set(config, "saved", r.saved); Set(config, "hasExplicitDefaultValue", true);
                    ((IList)Get(parameters, "parameters")).Add(config);
                    var menu = NewMenu(p.name + " Menu", new[] { p }, r, result);
                    var installer = Undo.AddComponent(host, Require(MA + "ModularAvatarMenuInstaller"));
                    Undo.RegisterCompleteObjectUndo(installer, "Configure MA menu");
                    Set(installer, "menuToAppend", menu); Set(installer, "installTargetMenu", r.targetMenu);
                    foreach (var component in new[] { merge, parameters, installer }) { EditorUtility.SetDirty(component); PrefabUtility.RecordPrefabInstancePropertyModifications(component); }
                }
            }
        }
        private static string MenuLabel(ToggleRequest r, TogglePlan plan) => string.IsNullOrWhiteSpace(r.menuPath) ? plan.name : r.menuPath.Trim().Trim('/') + "/" + plan.name;

        private static Object NewMenu(string name, IEnumerable<TogglePlan> plans, ToggleRequest r, ToggleResult result)
        {
            var menu = ScriptableObject.CreateInstance(Require(Menu)); menu.name = name;
            var controls = (IList)Get(menu, "controls");
            if (controls == null) { controls = (IList)Activator.CreateInstance(Field(menu.GetType(), "controls").FieldType); Set(menu, "controls", controls); }
            foreach (var p in plans) controls.Add(Control(p, p.name));
            QuickToggleCore.SaveAsset(menu, r.folder, result);
            return menu;
        }
        private static object Control(TogglePlan p, string label)
        {
            var control = Activator.CreateInstance(Require(Menu + "+Control"));
            Set(control, "name", label); SetEnum(control, "type", "Toggle"); Set(control, "value", 1f);
            var parameter = Activator.CreateInstance(Require(Menu + "+Control+Parameter")); Set(parameter, "name", p.parameter); Set(control, "parameter", parameter);
            return control;
        }

        private static void CreateDirect(ToggleRequest r, List<TogglePlan> plans, ToggleResult result)
        {
            var avatar = Descriptor(r.root);
            Array configuredLayers = null;
            Object configuredParameters = null, configuredMenu = null;
            if (r.fx)
            {
                var layers = (Array)BaseLayers(avatar).Clone(); var originalLayer = FxLayer(layers);
                var layer = Activator.CreateInstance(originalLayer.GetType());
                foreach (var field in originalLayer.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)) field.SetValue(layer, field.GetValue(originalLayer));
                var controller = (bool)Get(layer, "isDefault") ? null : Get(layer, "animatorController") as AnimatorController;
                if (controller == null)
                {
                    controller = new AnimatorController { name = r.name + " FX" };
                    QuickToggleCore.SaveAsset(controller, r.folder, result);
                    Set(layer, "animatorController", controller); Set(layer, "isDefault", false);
                    for (var i = 0; i < layers.Length; i++) if (Get(layers.GetValue(i), "type").ToString() == "FX") layers.SetValue(layer, i);
                    configuredLayers = layers;
                }
                Undo.RegisterCompleteObjectUndo(controller, "Add toggle layers");
                foreach (var p in plans) AddLayer(controller, p, r);
                EditorUtility.SetDirty(controller);
            }
            if (r.parameters)
            {
                var parameters = Get(avatar, "expressionParameters") as Object;
                if (parameters == null)
                {
                    parameters = ScriptableObject.CreateInstance(Require(Params)); parameters.name = r.name + " Parameters";
                    QuickToggleCore.SaveAsset(parameters, r.folder, result); configuredParameters = parameters;
                }
                Undo.RegisterCompleteObjectUndo(parameters, "Add expression parameters");
                var type = Require(Params + "+Parameter"); var current = Get(parameters, "parameters") as Array;
                var next = Array.CreateInstance(type, (current == null ? 0 : current.Length) + plans.Count);
                if (current != null) Array.Copy(current, next, current.Length);
                for (var i = 0; i < plans.Count; i++)
                {
                    var p = Activator.CreateInstance(type); Set(p, "name", plans[i].parameter); SetEnum(p, "valueType", "Bool");
                    Set(p, "defaultValue", r.defaultOn ? 1f : 0f); Set(p, "saved", r.saved);
                    next.SetValue(p, next.Length - plans.Count + i);
                }
                Set(parameters, "parameters", next); EditorUtility.SetDirty(parameters);
            }
            if (r.menu)
            {
                var menu = r.targetMenu != null ? r.targetMenu : Get(avatar, "expressionsMenu") as Object;
                if (menu == null)
                {
                    menu = NewMenu(r.name + " Menu", new TogglePlan[0], r, result); configuredMenu = menu;
                }
                Undo.RegisterCompleteObjectUndo(menu, "Add menu toggles");
                var controls = Get(menu, "controls") as IList;
                if (controls == null) { controls = (IList)Activator.CreateInstance(Field(menu.GetType(), "controls").FieldType); Set(menu, "controls", controls); }
                foreach (var p in plans) controls.Add(Control(p, p.name));
                EditorUtility.SetDirty(menu);
            }
            // Assign descriptor references after creation records to preserve Redo.
            Undo.RegisterCompleteObjectUndo(avatar, "Configure avatar toggles");
            if (configuredLayers != null) { Set(avatar, "baseAnimationLayers", configuredLayers); Set(avatar, "customizeAnimationLayers", true); }
            if (r.fx) Set(avatar, "customizeAnimationLayers", true);
            if (configuredParameters != null) Set(avatar, "expressionParameters", configuredParameters);
            if (configuredMenu != null) Set(avatar, "expressionsMenu", configuredMenu);
            if (r.parameters || r.menu) Set(avatar, "customExpressions", true);
            EditorUtility.SetDirty(avatar); PrefabUtility.RecordPrefabInstancePropertyModifications(avatar);
        }
        internal static void AddLayer(AnimatorController controller, TogglePlan p, ToggleRequest r)
        {
            controller.AddParameter(new AnimatorControllerParameter { name = p.parameter, type = AnimatorControllerParameterType.Bool, defaultBool = r.defaultOn });
            var machine = new AnimatorStateMachine { name = p.name };
            AssetDatabase.AddObjectToAsset(machine, controller);
            Undo.RegisterCreatedObjectUndo(machine, "Add toggle state machine");
            var off = machine.AddState("Off", new Vector3(250, 80)); var on = machine.AddState("On", new Vector3(250, 180));
            off.motion = p.off; on.motion = p.on; off.writeDefaultValues = on.writeDefaultValues = r.writeDefaults;
            machine.defaultState = r.defaultOn ? on : off;
            var toOn = off.AddTransition(on); var toOff = on.AddTransition(off);
            foreach (var t in new[] { toOn, toOff }) { t.hasExitTime = false; t.duration = 0; }
            toOn.AddCondition(AnimatorConditionMode.If, 0, p.parameter); toOff.AddCondition(AnimatorConditionMode.IfNot, 0, p.parameter);
            controller.AddLayer(new AnimatorControllerLayer { name = p.name, defaultWeight = 1, stateMachine = machine });
            EditorUtility.SetDirty(controller); EditorUtility.SetDirty(machine);
        }
    }
}
#endif
