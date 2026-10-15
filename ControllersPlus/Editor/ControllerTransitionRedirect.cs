using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerTransitionRedirect
    {
        internal static void Apply(AnimatorTransitionBase transition, AnimatorState source,
            AnimatorStateMachine machine, UnityAnimatorBridge.TokenizerTests.NodeType kind,
            AnimatorState[] destinations, bool includeExit)
        {
            if (!transition) return;
            var targets = destinations.Where(state => state).Distinct().ToArray();
            bool exit = includeExit && kind == UnityAnimatorBridge.TokenizerTests.NodeType.state;
            if (targets.Length == 0 && !exit) return;
            if (kind != UnityAnimatorBridge.TokenizerTests.NodeType.state &&
                kind != UnityAnimatorBridge.TokenizerTests.NodeType.entry &&
                kind != UnityAnimatorBridge.TokenizerTests.NodeType.any) return;

            Undo.RecordObject(transition, "Redirect Transitions");
            int count = targets.Length + (exit ? 1 : 0);
            for (int index = 0; index < count; index++)
            {
                AnimatorTransitionBase redirected = transition;
                if (index > 0)
                {
                    UnityEngine.Object owner = source ? (UnityEngine.Object)source : machine;
                    Undo.RegisterCompleteObjectUndo(owner, "Redirect Transitions");
                    switch (kind)
                    {
                        case UnityAnimatorBridge.TokenizerTests.NodeType.state:
                            redirected = source.AddTransition((AnimatorState)null);
                            break;
                        case UnityAnimatorBridge.TokenizerTests.NodeType.entry:
                            redirected = machine.AddEntryTransition((AnimatorState)null);
                            break;
                        default:
                            redirected = machine.AddAnyStateTransition((AnimatorState)null);
                            break;
                    }
                    EditorUtility.CopySerialized(transition, redirected);
                }
                redirected.destinationStateMachine = null;
                redirected.isExit = index >= targets.Length;
                redirected.destinationState = redirected.isExit ? null : targets[index];
                EditorUtility.SetDirty(redirected);
                if (index > 0) Undo.RegisterCreatedObjectUndo(redirected, "Redirect Transitions");
            }
        }
    }
}
