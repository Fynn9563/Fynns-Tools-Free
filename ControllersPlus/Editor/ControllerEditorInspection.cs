using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerEditorInspection
    {
        internal sealed class Usage
        {
            internal Object Asset;
            internal string Detail;
            internal string CopyText;
            internal Usage(Object asset, string detail) { Asset = asset; Detail = detail; }
        }

        internal static string ReferenceName(Object asset)
        {
            if (!asset) return "Missing reference";
            if (!string.IsNullOrWhiteSpace(asset.name)) return asset.name;
            if (asset is AnimatorTransitionBase transition)
            {
                string destination = transition.isExit ? "Exit" : transition.destinationState ? transition.destinationState.name :
                    transition.destinationStateMachine ? transition.destinationStateMachine.name : "Missing destination";
                return "Transition to " + destination;
            }
            return ObjectNames.NicifyVariableName(asset.GetType().Name);
        }

        internal static List<Usage> FindParameter(AnimatorController controller, string parameter)
        {
            var results = new List<Usage>();
            if (!controller || string.IsNullOrEmpty(parameter)) return results;
            var seenMachines = new HashSet<AnimatorStateMachine>();
            var seenMotions = new HashSet<Motion>();
            Action<AnimatorTransitionBase, string> transition = (edge, location) =>
            {
                if (edge && edge.conditions.Any(condition => condition.parameter == parameter))
                    results.Add(new Usage(edge, location + " / condition"));
            };
            Action<Motion, string> motion = null;
            motion = (value, location) =>
            {
                if (!value || !seenMotions.Add(value)) return;
                if (value is BlendTree tree)
                {
                    if (tree.blendType != BlendTreeType.Direct && (tree.blendParameter == parameter ||
                        (tree.blendType != BlendTreeType.Simple1D && tree.blendParameterY == parameter)))
                        results.Add(new Usage(tree, location + " / blend parameter"));
                    foreach (var child in tree.children)
                    {
                        if (tree.blendType == BlendTreeType.Direct && child.directBlendParameter == parameter)
                            results.Add(new Usage(tree, location + " / direct blend weight"));
                        motion(child.motion, location + " / " + (child.motion ? child.motion.name : "Empty"));
                    }
                }
                else if (value is AnimationClip clip && AnimationUtility.GetCurveBindings(clip).Any(binding =>
                    binding.type == typeof(Animator) && binding.path == "" && binding.propertyName == parameter))
                    results.Add(new Usage(clip, location + " / animated parameter"));
            };
            Action<StateMachineBehaviour[], string> drivers = (behaviours, location) =>
            {
                foreach (var behaviour in behaviours.Where(b => b && b.GetType().Name.Contains("ParameterDriver")))
                {
                    using (var serialized = new SerializedObject(behaviour))
                    {
                        var property = serialized.GetIterator();
                        while (property.Next(true))
                            if (property.propertyType == SerializedPropertyType.String && property.stringValue == parameter &&
                                property.propertyPath.StartsWith("parameters.Array.data[", StringComparison.Ordinal))
                                results.Add(new Usage(behaviour, location + " / driver / " + property.propertyPath));
                    }
                }
            };
            Action<AnimatorStateMachine, string> visit = null;
            visit = (machine, location) =>
            {
                if (!machine || !seenMachines.Add(machine)) return;
                drivers(machine.behaviours, location);
                foreach (var edge in machine.entryTransitions) transition(edge, location + " / Entry");
                foreach (var edge in machine.anyStateTransitions) transition(edge, location + " / Any State");
                foreach (var child in machine.states)
                {
                    var state = child.state;
                    string path = location + " / " + state.name;
                    foreach (var edge in state.transitions) transition(edge, path);
                    if (state.speedParameterActive && state.speedParameter == parameter) results.Add(new Usage(state, path + " / speed"));
                    if (state.timeParameterActive && state.timeParameter == parameter) results.Add(new Usage(state, path + " / motion time"));
                    if (state.mirrorParameterActive && state.mirrorParameter == parameter) results.Add(new Usage(state, path + " / mirror"));
                    if (state.cycleOffsetParameterActive && state.cycleOffsetParameter == parameter) results.Add(new Usage(state, path + " / cycle offset"));
                    drivers(state.behaviours, path);
                    motion(state.motion, path);
                }
                foreach (var child in machine.stateMachines)
                {
                    foreach (var edge in machine.GetStateMachineTransitions(child.stateMachine)) transition(edge, location + " / " + child.stateMachine.name);
                    visit(child.stateMachine, location + " / " + child.stateMachine.name);
                }
            };
            foreach (var layer in controller.layers) visit(layer.stateMachine, layer.name);
            return results.GroupBy(result => result.Asset.GetInstanceID() + ":" + result.Detail).Select(group => group.First()).ToList();
        }

        internal static List<Usage> FindAnimations(GameObject target)
        {
            var results = new List<Usage>();
            if (!target) return results;
            Transform root = target.transform;
            var animator = target.GetComponentInParent<Animator>();
            if (animator) root = animator.transform;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            var descriptor = target.GetComponentInParent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if (descriptor) root = descriptor.transform;
#endif
            string path = AnimationUtility.CalculateTransformPath(target.transform, root);
            var clips = AvatarMotions(target).OfType<AnimationClip>();
            foreach (var clip in clips.OrderBy(clip => clip.name, StringComparer.OrdinalIgnoreCase))
                foreach (var binding in AnimationUtility.GetCurveBindings(clip).Concat(AnimationUtility.GetObjectReferenceCurveBindings(clip)))
                    if (binding.path == path || (path.Length == 0 || binding.path.StartsWith(path + "/", StringComparison.Ordinal)))
                        results.Add(new Usage(clip, (binding.path.Length == 0 ? "Avatar" : binding.path) + " / " + binding.propertyName));
            return results;
        }

        internal static List<Motion> AvatarMotions(GameObject target)
        {
            var result = new HashSet<Motion>();
            if (!target) return result.ToList();
            var animator = target.GetComponentInParent<Animator>();
            var root = animator ? animator.transform : target.transform;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            var avatar = target.GetComponentInParent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if (avatar) root = avatar.transform;
#endif
            void Add(Object value)
            {
                if (value is AnimatorController controller)
                {
                    var visited = new HashSet<AnimatorStateMachine>();
                    void Visit(AnimatorStateMachine machine)
                    {
                        if (!machine || !visited.Add(machine)) return;
                        foreach (var state in machine.states) Add(state.state.motion);
                        foreach (var child in machine.stateMachines) Visit(child.stateMachine);
                    }
                    foreach (var layer in controller.layers) Visit(layer.stateMachine);
                }
                else if (value is RuntimeAnimatorController runtime)
                { foreach (var clip in runtime.animationClips) Add(clip); }
                else if (value is Motion motion && result.Add(motion) && motion is BlendTree tree)
                { foreach (var child in tree.children) Add(child.motion); }
            }
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (!component) continue;
                if (component is Animator componentAnimator) Add(componentAnimator.runtimeAnimatorController);
#if FYNNS_CONTROLLER_EDITOR_AVATARS
                if (component is VRC.SDK3.Avatars.Components.VRCAvatarDescriptor descriptor)
                {
                    foreach (var layer in descriptor.baseAnimationLayers.Concat(descriptor.specialAnimationLayers)) if (!layer.isDefault) Add(layer.animatorController);
                    continue;
                }
#endif
                if (!(component is MonoBehaviour)) continue;
                using (var serialized = new SerializedObject(component))
                {
                    var property = serialized.GetIterator();
                    while (property.Next(true)) if (property.propertyType == SerializedPropertyType.ObjectReference) Add(property.objectReferenceValue);
                }
            }
            return result.OrderBy(motion => motion.name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        internal static void AddBlendTreeMenu(GenericMenu menu, BlendTree tree, AnimatorController controller = null, BlendTree parent = null)
        {
            menu.AddItem(new GUIContent("Copy BlendTree"), false, () => ControllerBlendTreeClipboard.Copy(tree));
            if (ControllerBlendTreeClipboard.HasTree) menu.AddItem(new GUIContent("Paste BlendTree as Child"), false, () => ControllerBlendTreeClipboard.Paste(tree, controller));
            else menu.AddDisabledItem(new GUIContent("Paste BlendTree as Child"));
            menu.AddItem(new GUIContent("Create Animation Clip"), false, () =>
            {
                string path = EditorUtility.SaveFilePanelInProject("New Child Animation", "Animation", "anim", "Save the animation clip");
                if (!string.IsNullOrEmpty(path)) ControllerBlendTreeCreation.NewClip(tree, controller, path);
            });
            if (controller) menu.AddItem(new GUIContent("Create Parameter Smoothing"), false, () => PopupWindow.Show(new Rect(Event.current?.mousePosition ?? Vector2.zero, Vector2.zero), new ParameterSmoothingPopup(controller, tree)));
            foreach (BlendTreeType type in Enum.GetValues(typeof(BlendTreeType)))
            {
                var selectedType = type;
                menu.AddItem(new GUIContent("Add BlendTree/" + type), false, () => ControllerBlendTreeTools.AddChild(tree, selectedType, controller));
            }
            menu.AddItem(new GUIContent("Create Constant Animation…"), false, () => PopupWindow.Show(new Rect(Event.current?.mousePosition ?? Vector2.zero, Vector2.zero), new ConstantParameterPopup("Parameter", 1, clip =>
            {
                Undo.RecordObject(tree, "Add Parameter Animation");
                tree.children = tree.children.Concat(new[] { new ChildMotion { motion = clip, timeScale = 1, directBlendParameter = tree.blendType == BlendTreeType.Direct ? ControllerBlendTreeTools.EnsureWeight(controller) : ControllerEditorExtraSettings.Current.directWeightParameter } }).ToArray();
                EditorUtility.SetDirty(tree);
            }, controller)));
            if (controller)
            {
                menu.AddItem(new GUIContent("Make Internal Copy"), false, () => { Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); var copy = ControllerBlendTreeTools.CopyTree(tree, controller, false); ControllerBlendTreeTools.ReplaceReferences(controller, tree, copy); Undo.CollapseUndoOperations(group); });
                menu.AddItem(new GUIContent("Make External Copy"), false, () => { Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); var copy = ControllerBlendTreeTools.CopyTree(tree, controller, true); ControllerBlendTreeTools.ReplaceReferences(controller, tree, copy); Undo.CollapseUndoOperations(group); });
                menu.AddItem(new GUIContent("Find Usages in Animator"), false, () => ControllerEditor.ShowMotionUsages(tree));
                if (parent) menu.AddItem(new GUIContent("Replace Parent"), false, () => ControllerBlendTreeTools.ReplaceParent(controller, parent, tree));
            }
            menu.AddItem(new GUIContent("Show in Project Window"), (tree.hideFlags & HideFlags.HideInHierarchy) == 0, () => ToggleVisibility(tree));
        }

        [MenuItem("CONTEXT/BlendTree/Show in Project Window")]
        private static void ShowBlendTree(MenuCommand command) { ToggleVisibility(command.context as BlendTree); }

        internal static void ToggleVisibility(BlendTree tree)
        {
            if (!tree) return;
            Undo.RegisterCompleteObjectUndo(tree, "Blend Tree Visibility");
            tree.hideFlags ^= HideFlags.HideInHierarchy;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssetIfDirty(tree);
            if ((tree.hideFlags & HideFlags.HideInHierarchy) == 0) EditorGUIUtility.PingObject(tree);
        }
    }

    internal sealed partial class ControllerEditor
    {
        private static bool inspectionExpanded;
        private static string inspectionParameter = "";
        private static string inspectionTitle = "Parameter usages";
        private static List<ControllerEditorInspection.Usage> inspectionResults;

        private static void ShowParameterUsages(AnimatorController controller, string parameter)
        {
            inspectionParameter = parameter;
            inspectionTitle = "Parameter usages: " + parameter;
            inspectionResults = ControllerEditorInspection.FindParameter(controller, parameter);
            inspectionExpanded = true;
            InterruptWrapper();
        }

        [MenuItem("GameObject/Fynn's Tools/Find Animations", false, 49)]
        private static void FindSelectedAnimations()
        {
            if (!Selection.activeGameObject) return;
            inspectionTitle = "Animations for " + Selection.activeGameObject.name + " and children";
            inspectionResults = ControllerEditorInspection.FindAnimations(Selection.activeGameObject);
            inspectionExpanded = true;
            InterruptWrapper();
        }

        [MenuItem("GameObject/Fynn's Tools/Find Animations", true)]
        private static bool CanFindSelectedAnimations() { return Selection.activeGameObject; }

        private static void DrawInspection()
        {
            inspectionExpanded = EditorGUILayout.Foldout(inspectionExpanded, "Find Usages", true);
            if (!inspectionExpanded) return;
            using (new EditorGUI.DisabledScope(!Selection.activeGameObject))
                if (GUILayout.Button("Find animations for selected object")) FindSelectedAnimations();
            var controller = LogoutMapper();
            if (controller)
            {
                string[] names = controller.parameters.Select(parameter => parameter.name).ToArray();
                using (new EditorGUILayout.HorizontalScope())
                {
                    int selected = Array.IndexOf(names, inspectionParameter);
                    if (names.Length > 0)
                    {
                        selected = EditorGUILayout.Popup(Mathf.Max(0, selected), names);
                        inspectionParameter = names[selected];
                    }
                    using (new EditorGUI.DisabledScope(names.Length == 0))
                        if (GUILayout.Button("Find usages", GUILayout.Width(100))) ShowParameterUsages(controller, inspectionParameter);
                }
                DrawParameterNote(controller, inspectionParameter);
            }
            if (inspectionResults == null) return;
            EditorGUILayout.LabelField(inspectionTitle, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(inspectionResults.Count + " references", EditorStyles.miniLabel);
            foreach (var result in inspectionResults)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    Rect reference = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
                    if (GUI.Button(reference, GUIContent.none, EditorStyles.objectField))
                    { Selection.activeObject = result.Asset; EditorGUIUtility.PingObject(result.Asset); }
                    var icon = result.Asset ? EditorGUIUtility.ObjectContent(result.Asset, result.Asset.GetType()).image : null;
                    if (icon) GUI.DrawTexture(new Rect(reference.x + 2, reference.y + 1, 16, 16), icon, ScaleMode.ScaleToFit);
                    GUI.Label(new Rect(reference.x + 21, reference.y, Mathf.Max(0, reference.width - 23), reference.height),
                        new GUIContent(ControllerEditorInspection.ReferenceName(result.Asset), result.Detail), EditorStyles.label);
                    if (!string.IsNullOrEmpty(result.CopyText) && GUILayout.Button("Copy", GUILayout.Width(45), GUILayout.Height(EditorGUIUtility.singleLineHeight))) EditorGUIUtility.systemCopyBuffer = result.CopyText;
                    if (GUILayout.Button("Show", GUILayout.Width(45), GUILayout.Height(EditorGUIUtility.singleLineHeight))) { Selection.activeObject = result.Asset; EditorGUIUtility.PingObject(result.Asset); }
                }
                EditorGUILayout.LabelField(result.Detail, EditorStyles.wordWrappedMiniLabel);
            }
        }
    }
}
