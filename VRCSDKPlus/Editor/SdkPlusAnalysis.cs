#if VRC_SDK_VRCSDK3 && !UDON
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace FynnsTools.VRCSDKPlus
{
    internal sealed class ParameterReference
    {
        internal Object owner;
        internal string path, name, location;
        internal bool definition;
        internal string Key => owner.GetInstanceID() + ":" + path;
    }

    internal sealed class SdkPlusScope
    {
        internal VRCExpressionParameters parameters;
        internal readonly List<VRCExpressionsMenu> menus = new List<VRCExpressionsMenu>();
        internal readonly List<AnimatorController> controllers = new List<AnimatorController>();
        internal readonly List<ParameterReference> references = new List<ParameterReference>();
        internal readonly Dictionary<AnimatorController, List<ParameterReference>> controllerReferences = new Dictionary<AnimatorController, List<ParameterReference>>();
        internal readonly List<string> problems = new List<string>();

        internal static SdkPlusScope Build(VRCExpressionParameters parameters, VRCExpressionsMenu menu, VRCAvatarDescriptor avatar)
        {
            var scope = new SdkPlusScope { parameters = parameters ? parameters : (menu && menu.Parameters ? menu.Parameters : (avatar ? avatar.expressionParameters : null)) };
            var active = new HashSet<VRCExpressionsMenu>();
            Action<VRCExpressionsMenu> visit = null;
            visit = current =>
            {
                if (!current) return;
                if (active.Contains(current)) { scope.problems.Add("Submenu cycle reaches " + current.name); return; }
                if (scope.menus.Contains(current)) return;
                scope.menus.Add(current); active.Add(current);
                foreach (var control in current.controls ?? new List<VRCExpressionsMenu.Control>())
                    if (control != null && control.type == VRCExpressionsMenu.Control.ControlType.SubMenu) visit(control.subMenu);
                active.Remove(current);
            };
            visit(menu); if (avatar) visit(avatar.expressionsMenu);
            if (scope.parameters) scope.references.AddRange(Scan(scope.parameters));
            foreach (var current in scope.menus) scope.references.AddRange(Scan(current));
            if (avatar)
            {
                var layers = (avatar.baseAnimationLayers ?? Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>())
                    .Concat(avatar.specialAnimationLayers ?? Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>());
                var runtimes = layers.Select(l => l.animatorController).Concat(avatar.GetComponentsInChildren<Animator>(true).Select(a => a.runtimeAnimatorController));
                foreach (var runtime in runtimes)
                {
                    RuntimeAnimatorController baseController = runtime;
                    var seen = new HashSet<RuntimeAnimatorController>();
                    while (baseController is AnimatorOverrideController && seen.Add(baseController))
                        baseController = ((AnimatorOverrideController)baseController).runtimeAnimatorController;
                    var controller = baseController as AnimatorController;
                    if (controller && !scope.controllers.Contains(controller)) scope.controllers.Add(controller);
                }
            }
            foreach (var controller in scope.controllers)
            {
                var owners = new HashSet<Object> { controller };
                Action<Motion> motion = null;
                motion = value =>
                {
                    var tree = value as BlendTree;
                    if (!tree || !owners.Add(tree)) return;
                    foreach (var child in tree.children) motion(child.motion);
                };
                Action<AnimatorStateMachine> machine = null;
                machine = value =>
                {
                    if (!value || !owners.Add(value)) return;
                    foreach (var behaviour in value.behaviours) if (behaviour) owners.Add(behaviour);
                    foreach (var transition in value.anyStateTransitions.Concat(value.entryTransitions.Cast<AnimatorTransitionBase>())) if (transition) owners.Add(transition);
                    foreach (var child in value.states)
                    {
                        if (!child.state) continue;
                        owners.Add(child.state); motion(child.state.motion);
                        foreach (var behaviour in child.state.behaviours) if (behaviour) owners.Add(behaviour);
                        foreach (var transition in child.state.transitions) if (transition) owners.Add(transition);
                    }
                    foreach (var child in value.stateMachines)
                    {
                        foreach (var transition in value.GetStateMachineTransitions(child.stateMachine)) if (transition) owners.Add(transition);
                        machine(child.stateMachine);
                    }
                };
                foreach (var layer in controller.layers) machine(layer.stateMachine);
                // Include synced-layer overrides stored as controller subassets.
                var assetPath = AssetDatabase.GetAssetPath(controller);
                if (!string.IsNullOrEmpty(assetPath))
                    foreach (var owner in AssetDatabase.LoadAllAssetsAtPath(assetPath))
                    {
                        if (owner is AnimatorState || owner is AnimatorTransitionBase || owner is StateMachineBehaviour) owners.Add(owner);
                        if (owner is AnimatorState) motion(((AnimatorState)owner).motion);
                        if (owner is BlendTree) motion((BlendTree)owner);
                    }
                var refs = owners.Where(o => o).SelectMany(Scan).ToList();
                scope.controllerReferences[controller] = refs;
                scope.references.AddRange(refs);
            }
            var unique = scope.references.GroupBy(r => r.Key).Select(g => g.First()).ToArray();
            scope.references.Clear(); scope.references.AddRange(unique);
            return scope;
        }

        static IEnumerable<ParameterReference> Scan(Object owner)
        {
            bool parameters = owner is VRCExpressionParameters;
            bool menu = owner is VRCExpressionsMenu;
            bool controller = owner is AnimatorController;
            bool state = owner is AnimatorState;
            bool transition = owner is AnimatorTransitionBase;
            bool tree = owner is BlendTree;
            bool driver = owner is VRC.SDKBase.VRC_AvatarParameterDriver;
            if (!(parameters || menu || controller || state || transition || tree || driver)) yield break;
            using (var serialized = new SerializedObject(owner))
            {
                var property = serialized.GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.String || string.IsNullOrEmpty(property.stringValue)) continue;
                    var path = property.propertyPath;
                    bool match = parameters && path.StartsWith("parameters.Array.data[", StringComparison.Ordinal) && property.name == "name";
                    match |= menu && path.StartsWith("controls.Array.data[", StringComparison.Ordinal) && property.name == "name" && (path.Contains(".parameter.") || path.Contains(".subParameters."));
                    match |= controller && path.StartsWith("m_AnimatorParameters.Array.data[", StringComparison.Ordinal) && property.name == "m_Name";
                    match |= state && new[] { "m_SpeedParameter", "m_MirrorParameter", "m_CycleOffsetParameter", "m_TimeParameter" }.Contains(path);
                    match |= transition && path.StartsWith("m_Conditions.Array.data[", StringComparison.Ordinal) && property.name == "m_ConditionEvent";
                    match |= tree && (path == "m_BlendParameter" || path == "m_BlendParameterY" || (path.StartsWith("m_Childs.Array.data[", StringComparison.Ordinal) && property.name == "m_DirectBlendParameter"));
                    match |= driver && path.StartsWith("parameters.Array.data[", StringComparison.Ordinal) && (property.name == "name" || property.name == "source");
                    if (match) yield return new ParameterReference { owner = owner, path = path, name = property.stringValue, definition = parameters || controller, location = owner.name + " / " + path };
                }
            }
        }

        internal List<string> Validate()
        {
            var issues = new List<string>(problems);
            if (!parameters) issues.Add("No parameter file in scope; menu parameter checks are unavailable.");
            else
            {
                var entries = parameters.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>();
                foreach (var entry in entries) if (entry == null || string.IsNullOrWhiteSpace(entry.name)) issues.Add(parameters.name + ": blank parameter.");
                foreach (var duplicate in entries.Where(p => p != null).GroupBy(p => p.name).Where(g => g.Count() > 1)) issues.Add(parameters.name + ": duplicate parameter " + duplicate.Key);
                if (parameters.CalcTotalCost() > VRCExpressionParameters.MAX_PARAMETER_COST) issues.Add(parameters.name + ": synced memory exceeds " + VRCExpressionParameters.MAX_PARAMETER_COST + " bits.");
                if (entries.Length > VRCExpressionParameters.MAX_PARAMETER_COUNT) issues.Add(parameters.name + ": parameter count exceeds SDK limit.");
                foreach (var controller in controllers)
                {
                    foreach (var duplicate in controller.parameters.GroupBy(p => p.name).Where(g => g.Count() > 1)) issues.Add(controller.name + ": duplicate parameter " + duplicate.Key);
                    foreach (var entry in entries.Where(p => p != null && !string.IsNullOrWhiteSpace(p.name)))
                    {
                        foreach (var declared in controller.parameters.Where(p => p.name == entry.name))
                        {
                            var expected = entry.valueType == VRCExpressionParameters.ValueType.Bool ? AnimatorControllerParameterType.Bool : entry.valueType == VRCExpressionParameters.ValueType.Int ? AnimatorControllerParameterType.Int : AnimatorControllerParameterType.Float;
                            if (declared.type != expected) issues.Add(controller.name + ": " + entry.name + " type differs from " + parameters.name + ".");
                        }
                    }
                }
            }
            foreach (var menu in menus)
            {
                if (menu.controls == null) { issues.Add(menu.name + ": missing controls list."); continue; }
                if (menu.controls.Count > VRCExpressionsMenu.MAX_CONTROLS) issues.Add(menu.name + ": more than " + VRCExpressionsMenu.MAX_CONTROLS + " controls.");
                foreach (var control in menu.controls)
                {
                    if (control == null) { issues.Add(menu.name + ": missing control."); continue; }
                    if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu && !control.subMenu) issues.Add(menu.name + " / " + control.name + ": missing submenu.");
                }
                var file = menu.Parameters ? menu.Parameters : parameters;
                if (file)
                {
                    var names = new HashSet<string>((file.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).Where(p => p != null).Select(p => p.name));
                    foreach (var reference in references.Where(r => r.owner == menu && !names.Contains(r.name))) issues.Add(menu.name + ": missing parameter " + reference.name + " in " + file.name + ".");
                }
            }
            return issues.Distinct().ToList();
        }
    }

    internal sealed class ParameterRenamePlan
    {
        internal SdkPlusScope scope;
        internal string oldName, newName;
        internal List<ParameterReference> changes;
        internal List<ParameterReference> conflicts = new List<ParameterReference>();
        internal string error;
        internal static ParameterRenamePlan Create(SdkPlusScope scope, string oldName, string newName)
        {
            var plan = new ParameterRenamePlan { scope = scope, oldName = oldName, newName = newName, changes = scope.references.Where(r => r.name == oldName).ToList(), conflicts = scope.references.Where(r => r.name == newName).ToList() };
            if (!scope.parameters || !(scope.parameters.parameters ?? Array.Empty<VRCExpressionParameters.Parameter>()).Any(p => p != null && p.name == oldName)) plan.error = "Choose an existing parameter from this file.";
            else if (string.IsNullOrWhiteSpace(newName) || newName != newName.Trim() || oldName == newName) plan.error = "Enter a different, non-empty name without leading or trailing spaces.";
            else if (plan.conflicts.Any(r => r.definition))
                plan.error = "A parameter named \"" + newName + "\" is already defined in this scope. Choose a different name to avoid duplicate definitions.";
            else if (plan.changes.Any(r => !AssetDatabase.IsOpenForEdit(r.owner))) plan.error = "One or more affected assets are read-only.";
            // Update separate submenu parameter files together.
            foreach (var menu in scope.menus.Where(m => m.Parameters && m.Parameters != scope.parameters))
                if (plan.changes.Any(r => r.owner == menu)) plan.error = "A referenced submenu uses a different parameter file. Choose a scope with one parameter file before renaming.";
            return plan;
        }
        internal void Apply()
        {
            if (error != null) throw new InvalidOperationException(error);
            foreach (var reference in changes)
            {
                if (!reference.owner || !AssetDatabase.IsOpenForEdit(reference.owner)) throw new InvalidOperationException("An affected asset is unavailable or read-only. Preview again.");
                using (var serialized = new SerializedObject(reference.owner))
                {
                    var property = serialized.FindProperty(reference.path);
                    if (property == null || property.propertyType != SerializedPropertyType.String || property.stringValue != oldName) throw new InvalidOperationException("An affected reference changed. Preview again.");
                }
            }
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Rename SDK+ Parameter");
            var owners = changes.Select(c => c.owner).Distinct().ToArray();
            Undo.RegisterCompleteObjectUndo(owners, "Rename SDK+ Parameter");
            try
            {
                foreach (var owner in owners)
                    using (var serialized = new SerializedObject(owner))
                    {
                        foreach (var reference in changes.Where(c => c.owner == owner)) serialized.FindProperty(reference.path).stringValue = newName;
                        serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(owner);
                    }
                Undo.CollapseUndoOperations(group);
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
    }
}
#endif
