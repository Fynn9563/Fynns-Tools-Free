using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    [InitializeOnLoad]
    internal static class ControllerTransitionClipboard
    {
        static ControllerTransitionClipboard() { AssemblyReloadEvents.beforeAssemblyReload += Clear; EditorApplication.quitting += Clear; }
        private static void Clear() { foreach (var old in transitions) if (old) Object.DestroyImmediate(old); transitions = Array.Empty<AnimatorTransitionBase>(); }
        private static AnimatorTransitionBase[] transitions = Array.Empty<AnimatorTransitionBase>();
        private static AnimatorControllerParameter[] parameters = Array.Empty<AnimatorControllerParameter>();
        internal static bool HasTransitions => transitions.Any(value => value);
        internal static void Copy(IEnumerable<AnimatorTransitionBase> source, AnimatorController controller)
        {
            foreach (var old in transitions) if (old) Object.DestroyImmediate(old);
            transitions = source.Where(value => value).Distinct().Select(value => { var copy = Object.Instantiate(value); copy.hideFlags = HideFlags.HideAndDontSave; return copy; }).ToArray();
            parameters = controller ? controller.parameters : Array.Empty<AnimatorControllerParameter>();
        }
        internal static List<AnimatorTransitionBase> Paste(AnimatorController controller, AnimatorStateMachine machine, IEnumerable<UnityAnimatorBridge.TokenizerTests> targets)
        {
            var created = new List<AnimatorTransitionBase>();
            if (!controller || !machine || !HasTransitions) return created;
            var allStates = new List<AnimatorState>(); var allMachines = new List<AnimatorStateMachine>();
            void Gather(AnimatorStateMachine current)
            {
                if (allMachines.Contains(current)) return;
                allMachines.Add(current); allStates.AddRange(current.states.Select(value => value.state));
                foreach (var child in current.stateMachines) Gather(child.stateMachine);
            }
            foreach (var layer in controller.layers) Gather(layer.stateMachine);
            T Resolve<T>(T original, IEnumerable<T> choices) where T : Object
            {
                if (!original) return null;
                if (AssetDatabase.GetAssetPath(original) == AssetDatabase.GetAssetPath(controller)) return original;
                var matches = choices.Where(value => value.name == original.name).ToArray();
                return matches.Length == 1 ? matches[0] : null;
            }
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            try
            {
                Undo.RecordObject(controller, "Paste Transitions");
                foreach (var target in targets)
                foreach (var snapshot in transitions.Where(value => value))
                {
                    var destinationState = Resolve(snapshot.destinationState, allStates);
                    var destinationMachine = Resolve(snapshot.destinationStateMachine, allMachines);
                    if (!snapshot.isExit && !destinationState && !destinationMachine) continue;
                    AnimatorTransitionBase copy;
                    switch (target.objectTests)
                    {
                        case UnityAnimatorBridge.TokenizerTests.NodeType.state:
                            Undo.RegisterCompleteObjectUndo(target.comparatorTests, "Paste Transitions");
                            copy = target.comparatorTests.AddTransition((AnimatorState)null); break;
                        case UnityAnimatorBridge.TokenizerTests.NodeType.entry:
                            Undo.RegisterCompleteObjectUndo(machine, "Paste Transitions");
                            copy = machine.AddEntryTransition((AnimatorState)null); break;
                        case UnityAnimatorBridge.TokenizerTests.NodeType.any:
                            if (snapshot.isExit) continue;
                            Undo.RegisterCompleteObjectUndo(machine, "Paste Transitions");
                            copy = machine.AddAnyStateTransition((AnimatorState)null); break;
                        case UnityAnimatorBridge.TokenizerTests.NodeType.machine:
                            Undo.RegisterCompleteObjectUndo(machine, "Paste Transitions");
                            copy = machine.AddStateMachineTransition(target.m_ExceptionTests, (AnimatorState)null); break;
                        default: continue;
                    }
                    if (copy.GetType() == snapshot.GetType()) EditorUtility.CopySerialized(snapshot, copy);
                    else { copy.conditions = snapshot.conditions; copy.mute = snapshot.mute; copy.solo = snapshot.solo; copy.name = snapshot.name; }
                    copy.destinationState = destinationState; copy.destinationStateMachine = destinationMachine; copy.isExit = snapshot.isExit;
                    copy.hideFlags = HideFlags.HideInHierarchy;
                    foreach (var condition in copy.conditions)
                    {
                        if (controller.parameters.Any(parameter => parameter.name == condition.parameter)) continue;
                        var parameter = parameters.FirstOrDefault(value => value.name == condition.parameter);
                        if (parameter != null) controller.AddParameter(new AnimatorControllerParameter { name = parameter.name, type = parameter.type,
                            defaultBool = parameter.defaultBool, defaultInt = parameter.defaultInt, defaultFloat = parameter.defaultFloat });
                    }
                    EditorUtility.SetDirty(copy); Undo.RegisterCreatedObjectUndo(copy, "Paste Transitions"); created.Add(copy);
                }
                Undo.CollapseUndoOperations(group); ControllerParameterTools.ClearCache();
                return created;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
    }
    internal sealed partial class ControllerEditor
    {
        private static bool HandleGraphShortcuts(object graphGUI)
        {
            var current = Event.current;
            if (!ControllerEditorExtraSettings.Current.graphShortcuts || current == null || current.type != EventType.KeyDown || current.control || current.command || current.alt || EditorGUIUtility.editingTextField || GUIUtility.hotControl != 0) return false;
            var selected = UnityAnimatorBridge.RefTests.CalcPolicy()?.ToList() ?? new List<UnityAnimatorBridge.TokenizerTests>();
            var graph = UnityAnimatorBridge.RefTests.CancelPolicy();
            var hovered = graph ? graph.nodes.FirstOrDefault(node => node.position.Contains(current.mousePosition)) : null;
            var source = hovered ? new UnityAnimatorBridge.TokenizerTests(hovered) : selected.FirstOrDefault();
            switch (current.keyCode)
            {
                case KeyCode.S:
                    if (!RevertMapper()) return false;
                    Undo.RegisterCompleteObjectUndo(RevertMapper(), "Create State");
                    var state = RevertMapper().AddState(AddInitializer().defaultState.name, current.mousePosition);
                    QueryAlgo(state); state.motion = AddInitializer().defaultState.motion;
                    Undo.RegisterCreatedObjectUndo(state, "Create State"); Selection.activeObject = state; PatchAlgo(); break;
                case KeyCode.T:
                    if (source == null || !source.FillPolicy()) return false;
                    if (source.objectTests == UnityAnimatorBridge.TokenizerTests.NodeType.exit || source.objectTests == UnityAnimatorBridge.TokenizerTests.NodeType.tree) return false;
                    var slot = source.FillPolicy().outputSlots.FirstOrDefault();
                    var edgeGUI = UnityAnimatorBridge.RefTests.DefinePolicy();
                    if (slot == null || edgeGUI == null) return false;
                    edgeGUI.BeginSlotDragging(slot, true, false); break;
                case KeyCode.C:
                    var edges = UnityAnimatorBridge.RefTests.DestroyPolicy()?.ToArray();
                    var edgeSelection = UnityAnimatorBridge.RefTests.DefinePolicy()?.edgeSelection;
                    if (edges == null || edgeSelection == null) return false;
                    var transitions = edgeSelection.Where(index => index >= 0 && index < edges.Length).OrderBy(index => index)
                        .SelectMany(index => edges[index].FindSerializer()).Select(edge => edge.itemTests).Where(value => value).ToArray();
                    if (transitions.Length == 0) return false;
                    ControllerTransitionClipboard.Copy(transitions, LogoutMapper()); break;
                case KeyCode.V:
                    if (!ControllerTransitionClipboard.HasTransitions) return false;
                    if (hovered && !selected.Any(node => node.FillPolicy() == hovered)) selected = new List<UnityAnimatorBridge.TokenizerTests> { source };
                    if (selected.Count == 0) return false;
                    var pasted = ControllerTransitionClipboard.Paste(LogoutMapper(), RevertMapper(), selected);
                    if (pasted.Count == 0) QueryAnnotation()?.ShowNotification(new GUIContent("No matching transition destinations in this controller."));
                    else { Selection.objects = pasted.Cast<Object>().ToArray(); PatchAlgo(); }
                    break;
                default: return false;
            }
            current.Use(); QueryAnnotation()?.Repaint(); return true;
        }
    }
}
