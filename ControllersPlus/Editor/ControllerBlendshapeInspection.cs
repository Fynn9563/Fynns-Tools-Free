using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
using VRC.SDK3.Avatars.Components;
#endif

namespace FynnsTools.ControllerEditor
{
    internal static class ControllerBlendshapeInspection
    {
        internal static string[] Unused(SkinnedMeshRenderer renderer)
        {
            if (!renderer || !renderer.sharedMesh) return Array.Empty<string>();
            var animator = renderer.GetComponentInParent<Animator>();
            Transform root = animator ? animator.transform : renderer.transform.root;
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            var avatar = renderer.GetComponentInParent<VRCAvatarDescriptor>();
            if (avatar) root = avatar.transform;
#endif
            string path = AnimationUtility.CalculateTransformPath(renderer.transform, root);
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var clip in ControllerEditorInspection.AvatarMotions(renderer.gameObject).OfType<AnimationClip>())
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    if (binding.type == typeof(SkinnedMeshRenderer) && binding.path == path && binding.propertyName.StartsWith("blendShape.", StringComparison.Ordinal))
                        used.Add(binding.propertyName.Substring("blendShape.".Length));
#if FYNNS_CONTROLLER_EDITOR_AVATARS
            if (avatar)
                using (var serialized = new SerializedObject(avatar))
                {
                    if (serialized.FindProperty("VisemeSkinnedMesh")?.objectReferenceValue == renderer)
                    {
                        var visemes = serialized.FindProperty("VisemeBlendShapes");
                        if (visemes != null) for (int i = 0; i < visemes.arraySize; i++) used.Add(visemes.GetArrayElementAtIndex(i).stringValue);
                        var mouth = serialized.FindProperty("MouthOpenBlendShapeName");
                        if (mouth != null) used.Add(mouth.stringValue);
                    }
                    if (serialized.FindProperty("customEyeLookSettings.eyelidsSkinnedMesh")?.objectReferenceValue == renderer)
                    {
                        var eyelids = serialized.FindProperty("customEyeLookSettings.eyelidsBlendshapes");
                        if (eyelids != null) for (int i = 0; i < eyelids.arraySize; i++)
                        {
                            int index = eyelids.GetArrayElementAtIndex(i).intValue;
                            if (index >= 0 && index < renderer.sharedMesh.blendShapeCount) used.Add(renderer.sharedMesh.GetBlendShapeName(index));
                        }
                    }
                }
#endif
            return Enumerable.Range(0, renderer.sharedMesh.blendShapeCount).Select(renderer.sharedMesh.GetBlendShapeName).Where(name => !used.Contains(name)).ToArray();
        }
    }
    internal sealed partial class ControllerEditor
    {
        [MenuItem("CONTEXT/SkinnedMeshRenderer/List Unused Blendshapes")]
        private static void ListUnusedBlendshapes(MenuCommand command)
        {
            var renderer = command.context as SkinnedMeshRenderer;
            if (!renderer) return;
            inspectionResults = ControllerBlendshapeInspection.Unused(renderer).Select(name => new ControllerEditorInspection.Usage(renderer, "Unused blendshape / " + name) { CopyText = name }).ToList();
            inspectionTitle = "Unused blendshapes: " + renderer.name; inspectionExpanded = true; InterruptWrapper();
        }
#if FYNNS_CONTROLLER_EDITOR_AVATARS
        internal static List<ControllerEditorInspection.Usage> FxMotions(GameObject target)
        {
            var result = new List<ControllerEditorInspection.Usage>();
            var avatar = target ? target.GetComponentInParent<VRCAvatarDescriptor>() : null;
            if (!avatar) return result;
            foreach (var layer in avatar.baseAnimationLayers ?? Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>())
                if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX && !layer.isDefault && layer.animatorController is AnimatorController controller)
                    result.AddRange(MotionReferences(controller, null));
            return result;
        }
        [MenuItem("GameObject/Fynn's Tools/View FX Animations", false, 51)]
        private static void ViewFxAnimations()
        {
            inspectionResults = FxMotions(Selection.activeGameObject);
            inspectionTitle = "FX animations"; inspectionExpanded = true; InterruptWrapper();
        }
        [MenuItem("GameObject/Fynn's Tools/View FX Animations", true)]
        private static bool CanViewFxAnimations() { return Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<VRCAvatarDescriptor>(); }
#endif
    }
}
