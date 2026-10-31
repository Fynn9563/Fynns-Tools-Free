using System;
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(FynnsTools.MultiUploader.Editor.MultiUploaderPlugin))]

namespace FynnsTools.MultiUploader.Editor
{
    public sealed class MultiUploaderPlugin : Plugin<MultiUploaderPlugin>
    {
        public override string QualifiedName => "com.fynn.tools.multiuploader";
        public override string DisplayName => "MultiUploader";

        protected override void Configure()
        {
            // Convert before VRCFury removes non-mobile materials.
            InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar")
                .AfterPlugin("nadena.dev.modular-avatar.late-transform-stages")
                .Run("Convert Quest materials before VRCFury", context =>
            {
                var settings = QuestBuildSettings.Prepare(context.AvatarRootObject,
                    EditorUserBuildSettings.activeBuildTarget, Application.isPlaying);
                if (settings != null)
                {
                    var run = context.GetState<QuestBuildRunState>();
                    run.enabled = true;
                    run.removeQuestAudio = settings.removeQuestAudio;
                    run.ignoredObjects = settings.ignoredObjects ?? Array.Empty<GameObject>();
                    var ignored = new QuestConversionIgnore(run.ignoredObjects);
                    run.initialNonNullSlots = context.AvatarRootObject.GetComponentsInChildren<Renderer>(true)
                        .Where(r => !ignored.Contains(r.transform))
                        .Sum(r => r.sharedMaterials.Count(m => m != null));
                    var converter = new QuestConverter(settings, context.AssetSaver.SaveAsset,
                        material => ObjectRegistry.GetReference(material)?.Object as Material ?? material);
                    converter.Convert(context.AvatarRootObject);
                    Debug.Log(converter.Report, context.AvatarRootObject);
                    if (converter.HasErrors) throw new InvalidOperationException(converter.ErrorSummary);
                    Debug.Log("[MultiUploader] Quest conversion completed BEFORE VRCFury. "
                        + run.initialNonNullSlots + " renderer material slots were prepared.", context.AvatarRootObject);
                }
            });

            InPhase(BuildPhase.Optimizing).AfterPlugin("nadena.dev.modular-avatar")
                .Run("Verify Quest materials after VRCFury", context =>
            {
                var run = context.GetState<QuestBuildRunState>();
                if (run.enabled)
                {
                    if (run.removeQuestAudio)
                        QuestAudioCleanup.Remove(context.AvatarRootObject);
                    var ignored = new QuestConversionIgnore(run.ignoredObjects);
                    var materials = context.AvatarRootObject.GetComponentsInChildren<Renderer>(true)
                        .Where(r => !ignored.Contains(r.transform))
                        .SelectMany(r => r.sharedMaterials).Where(m => m != null).ToArray();
                    if (run.initialNonNullSlots > 0 && materials.Length == 0)
                        throw new InvalidOperationException("Quest materials disappeared during avatar processing. "
                            + "Stopping before upload; inspect VRCFury/optimizer output.");
                    var invalid = materials.Where(m => m.shader == null || m.shader.name != "VRChat/Mobile/Toon Standard")
                        .Select(m => m.name).Distinct().Take(12).ToArray();
                    if (invalid.Length > 0)
                        throw new InvalidOperationException("Avatar tools introduced non-Toon-Standard Quest materials after conversion: "
                            + string.Join(", ", invalid));
                    Debug.Log("[MultiUploader] Post-VRCFury Quest material verification passed: "
                        + materials.Length + " assigned slots.", context.AvatarRootObject);
                }
                var remaining = context.AvatarRootObject.GetComponent<MultiUploaderSettings>();
                if (remaining != null) UnityEngine.Object.DestroyImmediate(remaining);
            });
        }
    }

    public sealed class QuestBuildRunState
    {
        public bool enabled;
        public GameObject[] ignoredObjects = Array.Empty<GameObject>();
        public bool removeQuestAudio;
        public int initialNonNullSlots;
    }
}
