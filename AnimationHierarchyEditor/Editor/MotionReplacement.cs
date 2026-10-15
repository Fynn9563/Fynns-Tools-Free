using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.AnimationHierarchy
{
    [Serializable] internal sealed class MotionRule
    {
        public Motion source, replacement;
        public bool enabled;
    }
    internal sealed class MotionSlot
    {
        internal Object owner;
        internal Motion before, after;
        internal string location;
        internal Func<Motion> read;
        internal Action<Motion> write;
    }
    internal static class MotionReplacement
    {
        internal static RuntimeAnimatorController[] Controllers(Object target)
        {
            if (!target) return Array.Empty<RuntimeAnimatorController>();
            if (target is RuntimeAnimatorController controller) return new[] { controller };
            var go = target as GameObject ?? (target as Component)?.gameObject;
            if (!go) throw new InvalidOperationException("Choose a controller, Animator or avatar object.");
            var result = new List<RuntimeAnimatorController>();
            var animator = go.GetComponent<Animator>();
            if (animator && animator.runtimeAnimatorController) result.Add(animator.runtimeAnimatorController);
            var descriptor = go.GetComponents<Component>().FirstOrDefault(c => c && c.GetType().FullName == "VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
            if (descriptor)
                foreach (var name in new[] { "baseAnimationLayers", "specialAnimationLayers" })
                {
                    var custom = descriptor.GetType().GetField("customizeAnimationLayers");
                    if (custom != null && !(bool)custom.GetValue(descriptor)) continue;
                    var layers = descriptor.GetType().GetField(name)?.GetValue(descriptor) as Array;
                    if (layers == null) continue;
                    foreach (var layer in layers)
                    {
                        if (layer == null) continue;
                        var type = layer.GetType();
                        if ((bool)(type.GetField("isDefault")?.GetValue(layer) ?? true)) continue;
                        var value = type.GetField("animatorController")?.GetValue(layer) as RuntimeAnimatorController;
                        if (value) result.Add(value);
                    }
                }
            return result.Distinct().ToArray();
        }

        internal static List<MotionSlot> Scan(IEnumerable<RuntimeAnimatorController> controllers, IEnumerable<MotionRule> rules = null)
        {
            var selected = (controllers ?? Enumerable.Empty<RuntimeAnimatorController>()).Where(c => c).Distinct().ToArray();
            if (selected.Length == 0) throw new InvalidOperationException("Choose an object with a controller, or assign a controller asset.");
            var active = (rules ?? Enumerable.Empty<MotionRule>()).Where(r => r != null && r.enabled && r.source != r.replacement).ToArray();
            for (var i = 0; i < active.Length; i++)
                if (active.Take(i).Any(r => r.source == active[i].source)) throw new InvalidOperationException("Choose only one replacement for each source motion.");
            var slots = new List<MotionSlot>();
            var seen = new HashSet<string>();
            var ancestors = new HashSet<BlendTree>();
            Func<Motion, MotionRule> rule = motion => active.FirstOrDefault(r => r.source == motion);
            Action<Object, string, Func<Motion>, Action<Motion>> slot = (owner, key, read, write) =>
            {
                if (!seen.Add(owner.GetInstanceID() + ":" + key)) return;
                var before = read(); var replacement = rule(before);
                slots.Add(new MotionSlot { owner = owner, before = before, after = replacement != null ? replacement.replacement : before, location = key, read = read, write = write });
            };
            Action<Motion, string> tree = null;
            tree = (motion, path) =>
            {
                var blend = motion as BlendTree;
                if (!blend || rule(blend) != null) return;
                if (!ancestors.Add(blend)) throw new InvalidOperationException("Cyclic Blend Tree at " + path + ". Repair it before replacing motions.");
                try
                {
                    var children = blend.children;
                    for (var i = 0; i < children.Length; i++)
                    {
                        var index = i;
                        slot(blend, "Child " + i, () => blend.children[index].motion, value =>
                        {
                            var edited = blend.children; edited[index].motion = value; blend.children = edited;
                        });
                        tree(children[i].motion, path + "/" + blend.name + "[" + i + "]");
                    }
                }
                finally { ancestors.Remove(blend); }
            };
            foreach (var selectedController in selected)
            {
                if (selectedController is AnimatorOverrideController overrides)
                {
                    if (!overrides.runtimeAnimatorController) throw new InvalidOperationException(overrides.name + " has no base controller.");
                    var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>(); overrides.GetOverrides(pairs);
                    foreach (var pair in pairs)
                    {
                        var key = pair.Key;
                        var effective = pair.Value ? pair.Value : key;
                        var replacement = rule(effective);
                        if (replacement != null && !(replacement.replacement is AnimationClip))
                            throw new InvalidOperationException("Override controllers require an Animation Clip replacement. Empty slots and Blend Trees cannot be assigned as overrides.");
                        slot(overrides, "Override: " + key.name + " #" + key.GetInstanceID(),
                            () => { var current = new List<KeyValuePair<AnimationClip, AnimationClip>>(); overrides.GetOverrides(current); var value = current.First(p => p.Key == key).Value; return value ? value : key; },
                            value => { var current = new List<KeyValuePair<AnimationClip, AnimationClip>>(); overrides.GetOverrides(current); var index = current.FindIndex(p => p.Key == key); current[index] = new KeyValuePair<AnimationClip, AnimationClip>(key, (AnimationClip)value); overrides.ApplyOverrides(current); });
                    }
                    continue;
                }
                var controller = selectedController as AnimatorController;
                if (!controller) throw new InvalidOperationException("Unsupported controller: " + selectedController.name);
                var layers = controller.layers;
                for (var i = 0; i < layers.Length; i++)
                {
                    var index = i; var layer = layers[index];
                    var machineIndex = index; var synced = new HashSet<int>();
                    while (layers[machineIndex].syncedLayerIndex >= 0)
                    {
                        if (!synced.Add(machineIndex)) throw new InvalidOperationException("Cyclic synced layers in " + controller.name);
                        machineIndex = layers[machineIndex].syncedLayerIndex;
                        if (machineIndex >= layers.Length) throw new InvalidOperationException("Invalid synced layer in " + controller.name);
                    }
                    Action<AnimatorStateMachine, string> visit = null;
                    var visitedMachines = new HashSet<AnimatorStateMachine>();
                    visit = (machine, path) =>
                    {
                        if (!machine || !visitedMachines.Add(machine)) return;
                        foreach (var child in machine.states)
                        {
                            var state = child.state; if (!state) continue;
                            if (layer.syncedLayerIndex >= 0)
                            {
                                var overrideMotion = layer.GetOverrideMotion(state);
                                if (!overrideMotion) continue; 
                                slot(controller, layer.name + "/" + path + state.name + " #" + state.GetInstanceID(),
                                    () => controller.layers[index].GetOverrideMotion(state), value => { var edited = controller.layers; edited[index].SetOverrideMotion(state, value); controller.layers = edited; });
                                tree(overrideMotion, layer.name + "/" + path + state.name);
                            }
                            else
                            {
                                slot(state, layer.name + "/" + path + state.name, () => state.motion, value => state.motion = value);
                                tree(state.motion, layer.name + "/" + path + state.name);
                            }
                        }
                        foreach (var child in machine.stateMachines) visit(child.stateMachine, path + child.stateMachine.name + "/");
                    };
                    visit(layers[machineIndex].stateMachine, "");
                }
            }
            return slots;
        }

        internal static List<MotionSlot> Preview(IEnumerable<RuntimeAnimatorController> controllers, IEnumerable<MotionRule> rules)
        {
            var edits = Scan(controllers, rules).Where(s => s.before != s.after).ToList();
            foreach (var edit in edits)
            {
                Editable(edit.owner);
                if (edit.after && !EditorUtility.IsPersistent(edit.after)) throw new InvalidOperationException("Save replacement motions as assets first.");
                if (edit.after is BlendTree replacement)
                {
                    var visited = new HashSet<BlendTree>(); var stack = new HashSet<BlendTree>();
                    Action<BlendTree> check = null;
                    check = node => { if (!node) return; if (node == edit.owner || !stack.Add(node)) throw new InvalidOperationException("This replacement would create a Blend Tree cycle."); if (visited.Add(node)) foreach (var child in node.children) check(child.motion as BlendTree); stack.Remove(node); };
                    check(replacement);
                }
            }
            var changedTrees = edits.Select(e => e.owner).OfType<BlendTree>().Distinct().ToArray();
            var checkedTrees = new HashSet<BlendTree>(); var activeTrees = new HashSet<BlendTree>();
            Action<BlendTree> validate = null;
            validate = node =>
            {
                if (!node) return;
                if (!activeTrees.Add(node)) throw new InvalidOperationException("The combined replacements would create a Blend Tree cycle.");
                if (checkedTrees.Add(node))
                    for (var i = 0; i < node.children.Length; i++)
                    {
                        var changed = edits.FirstOrDefault(e => e.owner == node && e.location == "Child " + i);
                        validate((changed != null ? changed.after : node.children[i].motion) as BlendTree);
                    }
                activeTrees.Remove(node);
            };
            foreach (var node in changedTrees) validate(node);
            return edits;
        }

        private static void Editable(Object owner)
        {
            var path = AssetDatabase.GetAssetPath(owner);
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !(path.EndsWith(".controller", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".overrideController", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) ||
                !AssetDatabase.IsOpenForEdit(owner) || !File.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReadOnly) != 0)
                throw new InvalidOperationException(owner.name + " must belong to an editable controller or Blend Tree asset under Assets. Duplicate package/imported assets first.");
        }
        internal static int Apply(List<MotionSlot> edits)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before replacing motions.");
            foreach (var edit in edits) { Editable(edit.owner); if (edit.read() != edit.before) throw new InvalidOperationException("A motion changed since the preview. Preview the replacements again."); }
            if (edits.Count == 0) return 0;
            Undo.IncrementCurrentGroup(); var group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Replace Animation Motions");
            try
            {
                var owners = edits.Select(e => e.owner).Distinct().ToArray();
                Undo.RegisterCompleteObjectUndo(owners, "Replace Animation Motions");
                foreach (var edit in edits) edit.write(edit.after);
                foreach (var owner in owners) EditorUtility.SetDirty(owner);
                Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group);
                return edits.Count;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
    }
}
