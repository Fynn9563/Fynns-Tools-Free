using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif
using Object = UnityEngine.Object;

namespace FynnsTools.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        private static bool buildingParameterMenu;
        private static AnimatorController noteController;
        private static string noteParameter;
        private static string noteDraft;
        private static void BeginParameterMenu() { buildingParameterMenu = true; }
        private static void EndParameterMenu() { buildingParameterMenu = false; }
        private static void ShowAsContextPrefix(object __instance)
        {
            if (!buildingParameterMenu) return;
            buildingParameterMenu = false;
            ControllerParameterTools.AddBuiltins((GenericMenu)__instance, LogoutMapper());
        }
        private static void RegisterAdditionalGraphExtensions()
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            EditorPatchRegistry.MapReg(mappingVisitor, "OnAddParameter", typeof(ControllerEditor).GetMethod(nameof(BeginParameterMenu), flags), typeof(ControllerEditor).GetMethod(nameof(EndParameterMenu), flags));
            RegisterNativeBlendTreeTools();
            var dropdown = typeof(GenericMenu).GetMethod("DropDown", new[] { typeof(Rect) });
            EditorPatchRegistry.RateReg(dropdown, typeof(ControllerEditor).GetMethod(nameof(ShowAsContextPrefix), flags));
            var clipPopup = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindowClipPopup");
            var clipMenu = clipPopup?.GetMethod("DisplayClipMenu", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (clipMenu != null) EditorPatchRegistry.RateReg(clipMenu, typeof(ControllerEditor).GetMethod(nameof(ClipMenuPrefix), flags));
            var inspector = _StatusVisitor.Assembly.GetType("UnityEditor.Graphs.AnimationStateMachine.AnimatorTransitionInspectorBase");
            var condition = inspector?.GetMethod("DrawConditionsElement", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (condition != null) EditorPatchRegistry.RateReg(condition, typeof(ControllerEditor).GetMethod(nameof(ConditionElementPrefix), flags), typeof(ControllerEditor).GetMethod(nameof(ConditionElementPost), flags));
            var help = typeof(Help).GetMethod("ShowHelpForObject", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Object) }, null);
            if (help != null) EditorPatchRegistry.RateReg(help, typeof(ControllerEditor).GetMethod(nameof(ShowVRChatHelpPrefix), flags));
            EditorPatchRegistry.MapReg(_StatusVisitor, "DrawEdge", typeof(ControllerEditor).GetMethod(nameof(DrawEdgeExtrasPrefix), flags), typeof(ControllerEditor).GetMethod(nameof(DrawEdgeExtrasPost), flags));
        }
        private static bool ShowVRChatHelpPrefix(Object __0)
        {
            if (!__0 || !(__0 is StateMachineBehaviour) || !(__0.GetType().Namespace ?? "").StartsWith("VRC", StringComparison.Ordinal)) return true;
            Application.OpenURL("https://creators.vrchat.com/avatars/state-behaviors/"); return false;
        }
        [MenuItem("CONTEXT/StateMachineBehaviour/Open VRChat Documentation", true)]
        private static bool CanOpenBehaviourDocs(MenuCommand command) { return command.context && (command.context.GetType().Namespace ?? "").StartsWith("VRC", StringComparison.Ordinal); }
        [MenuItem("CONTEXT/StateMachineBehaviour/Open VRChat Documentation")]
        private static void OpenBehaviourDocs(MenuCommand command) { Application.OpenURL("https://creators.vrchat.com/avatars/state-behaviors/"); }
        private static void DrawParameterNote(AnimatorController controller, string parameter)
        {
            if (!ControllerEditorExtraSettings.Current.parameterNotes || string.IsNullOrEmpty(parameter)) return;
            if (noteController != controller || noteParameter != parameter)
            { noteController = controller; noteParameter = parameter; noteDraft = ControllerParameterNotes.Get(controller, parameter); }
            EditorGUILayout.LabelField("Parameter note", EditorStyles.miniLabel);
            noteDraft = EditorGUILayout.TextArea(noteDraft ?? "", GUILayout.MinHeight(38));
            using (new EditorGUI.DisabledScope(!AssetDatabase.IsOpenForEdit(controller)))
                if (noteDraft != ControllerParameterNotes.Get(controller, parameter) && GUILayout.Button("Save Note")) ControllerParameterNotes.Set(controller, parameter, noteDraft);
        }
#if FYNNS_CONTROLLER_EDITOR_AVATARS
        internal static void ShowExpressionMenuUsages(VRCAvatarDescriptor avatar, string parameter)
        {
            inspectionResults = new List<ControllerEditorInspection.Usage>();
            var visited = new HashSet<VRCExpressionsMenu>();
            void Visit(VRCExpressionsMenu menu, string path)
            {
                if (!menu || !visited.Add(menu)) return;
                foreach (var control in menu.controls ?? new List<VRCExpressionsMenu.Control>())
                {
                    if (control == null) continue;
                    string location = path + " / " + control.name;
                    if (control.parameter?.name == parameter || (control.subParameters != null && control.subParameters.Any(value => value != null && value.name == parameter)))
                        inspectionResults.Add(new ControllerEditorInspection.Usage(menu, location));
                    if (control.subMenu) Visit(control.subMenu, location);
                }
            }
            if (avatar) Visit(avatar.expressionsMenu, avatar.name);
            inspectionTitle = "Expression menu references: " + parameter;
            inspectionExpanded = true; InterruptWrapper();
        }
#endif
        private static List<ControllerEditorInspection.Usage> MotionReferences(AnimatorController controller, Motion target = null, int layerIndex = -1)
        {
            var result = new List<ControllerEditorInspection.Usage>();
            if (!controller) return result;
            var machines = new HashSet<AnimatorStateMachine>();
            var pathTrees = new HashSet<BlendTree>();
            void Motion(Motion motion, Object owner, string path)
            {
                if (!motion) return;
                if (target == null || target == motion) result.Add(new ControllerEditorInspection.Usage(target ? owner : motion, path + " / " + motion.name));
                if (motion is BlendTree tree && pathTrees.Add(tree))
                {
                    for (int index = 0; index < tree.children.Length; index++) Motion(tree.children[index].motion, tree, path + " / " + tree.name + " [" + index + "]");
                    pathTrees.Remove(tree);
                }
            }
            void Visit(AnimatorStateMachine machine, string path)
            {
                if (!machine || !machines.Add(machine)) return;
                foreach (var child in machine.states) Motion(child.state.motion, child.state, path + " / " + child.state.name);
                foreach (var child in machine.stateMachines) Visit(child.stateMachine, path + " / " + child.stateMachine.name);
            }
            for (int index = 0; index < controller.layers.Length; index++)
                if (layerIndex < 0 || layerIndex == index) Visit(controller.layers[index].stateMachine, controller.name + " / " + controller.layers[index].name);
            return result;
        }
        internal static void ShowMotionUsages(Motion motion)
        {
            inspectionResults = new List<ControllerEditorInspection.Usage>();
            var active = LogoutMapper();
            var controllers = active ? new[] { active } : AssetDatabase.FindAssets("t:AnimatorController").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<AnimatorController>).Where(value => value);
            foreach (var controller in controllers) inspectionResults.AddRange(MotionReferences(controller, motion));
            inspectionTitle = "Motion usages: " + (motion ? motion.name : "None"); inspectionExpanded = true; InterruptWrapper();
        }
        private static void ShowLayerAnimations(AnimatorController controller, int layer)
        {
            inspectionResults = MotionReferences(controller, null, layer);
            inspectionTitle = "Motions in " + controller.layers[layer].name; inspectionExpanded = true; InterruptWrapper();
        }
        [MenuItem("GameObject/Fynn's Tools/View All Animations", false, 50)]
        private static void ViewAllAvatarAnimations()
        {
            inspectionResults = ControllerEditorInspection.AvatarMotions(Selection.activeGameObject).Select(motion => new ControllerEditorInspection.Usage(motion, AssetDatabase.GetAssetPath(motion))).ToList();
            inspectionTitle = "Avatar motions"; inspectionExpanded = true; InterruptWrapper();
        }
        [MenuItem("GameObject/Fynn's Tools/View All Animations", true)]
        private static bool CanViewAllAvatarAnimations() { return Selection.activeGameObject; }
        [MenuItem("Assets/Fynn's Tools/Find Motion Usages", false, 49)]
        private static void FindSelectedMotionUsages() { ShowMotionUsages(Selection.activeObject as Motion); }
        [MenuItem("Assets/Fynn's Tools/Find Motion Usages", true)]
        private static bool CanFindSelectedMotionUsages() { return Selection.activeObject is Motion; }
        [MenuItem("CONTEXT/AnimationClip/Find Usages in Animator")]
        private static void ClipUsages(MenuCommand command) { ShowMotionUsages(command.context as Motion); }
    }
}
