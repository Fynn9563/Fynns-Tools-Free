using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.MultiUploader.Editor
{
    public enum UploadPlatforms { PC, Quest, PCAndQuest }
    public enum UploadOperation { BuildOnly, BuildAndTest, Publish }
    public enum QueuePhase { Idle, Ready, Switching, Executing, Restoring, Finished, Interrupted }
    public enum UploadJobStatus { Pending, Running, Succeeded, Failed, Skipped, Cancelled, Unknown }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge.Editor", "Fynn.QuestBridge.Editor", "QuestUploadOptions")]
    public sealed class QuestUploadOptions
    {
        public bool convertQuest = true;
        public int maxTextureSize = 1024;
        public bool bakeStaticPoiyomiDecals = true;
        public bool removeQuestAudio = true;
        public bool approximateTransparencyAsOpaque = true;
        public bool blockUnsafeConversions = true;
        public bool useAvatarOverrides = true;
        public MaterialOverride[] overrides = Array.Empty<MaterialOverride>();

        public void Apply(MultiUploaderSettings target)
        {
            target.convertOnAndroid = convertQuest;
            target.enabled = true;
            target.maxTextureSize = maxTextureSize;
            target.bakeStaticPoiyomiDecals = bakeStaticPoiyomiDecals;
            target.removeQuestAudio = removeQuestAudio;
            target.approximateTransparencyAsOpaque = approximateTransparencyAsOpaque;
            target.blockUnsafeConversions = blockUnsafeConversions;
            var local = useAvatarOverrides ? target.overrides : Array.Empty<MaterialOverride>();
            // Avatar overrides take priority.
            target.overrides = (local ?? Array.Empty<MaterialOverride>())
                .Concat(overrides ?? Array.Empty<MaterialOverride>())
                .Where(x => x != null).GroupBy(x => x.pcMaterial).Select(x => x.First()).ToArray();
        }

        public QuestUploadOptions Snapshot() => new QuestUploadOptions
        {
            convertQuest = convertQuest, maxTextureSize = maxTextureSize,
            bakeStaticPoiyomiDecals = bakeStaticPoiyomiDecals,
            removeQuestAudio = removeQuestAudio,
            approximateTransparencyAsOpaque = approximateTransparencyAsOpaque,
            blockUnsafeConversions = blockUnsafeConversions, useAvatarOverrides = useAvatarOverrides,
            overrides = (overrides ?? Array.Empty<MaterialOverride>()).Select(x => x == null ? null
                : new MaterialOverride { pcMaterial = x.pcMaterial, questMaterial = x.questMaterial }).ToArray()
        };
    }

    [FilePath("Library/FynnMultiUploaderSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge.Editor", "Fynn.QuestBridge.Editor", "MultiUploaderPreferences")]
    public sealed class MultiUploaderPreferences : ScriptableSingleton<MultiUploaderPreferences>
    {
        public UploadPlatforms platforms = UploadPlatforms.PCAndQuest;
        public QuestUploadOptions quest = new QuestUploadOptions();
        public bool restoreStartingPlatform = true;
        public bool continueAfterFailure;
        [SerializeField] int opaquePreferenceRevision;

        [InitializeOnLoadMethod]
        static void InitializePreferences() => EditorApplication.delayCall += () => instance.EnsureOpaquePreference();

        public void EnsureOpaquePreference()
        {
            if (opaquePreferenceRevision >= 1) return;
            if (quest == null) quest = new QuestUploadOptions();
            quest.approximateTransparencyAsOpaque = true;
            opaquePreferenceRevision = 1;
            Persist();
        }
        public void Persist() => Save(true);
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge.Editor", "Fynn.QuestBridge.Editor", "UploadJob")]
    public sealed class UploadJob
    {
        public string avatarGlobalId;
        public string avatarName;
        public string blueprintId;
        public BuildTarget platform;
        public UploadJobStatus status;
        public string detail;
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge.Editor", "Fynn.QuestBridge.Editor", "QueueAvatar")]
    public sealed class QueueAvatar
    {
        public string globalId;
        public string name;
        public string blueprintId;
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge.Editor", "Fynn.QuestBridge.Editor", "UploadQueueData")]
    public sealed class UploadQueueData
    {
        public QueuePhase phase;
        public bool active;
        public bool cancelRequested;
        public bool restoreStartingPlatform;
        public bool continueAfterFailure;
        public UploadOperation operation;
        public BuildTarget initialPlatform;
        public BuildTarget expectedPlatform;
        public QuestUploadOptions quest = new QuestUploadOptions();
        public List<UploadJob> jobs = new List<UploadJob>();
        public int cursor;
        public string sessionToken;
        public string status = "Ready.";
        public string finishMessage;
        public UploadJob Current => cursor >= 0 && cursor < jobs.Count ? jobs[cursor] : null;
        public int Completed => jobs.Count(j => j.status == UploadJobStatus.Succeeded);

        public static UploadQueueData Create(IEnumerable<QueueAvatar> avatars, UploadPlatforms platforms,
            UploadOperation operation, BuildTarget initial, QuestUploadOptions options,
            bool restore, bool continueAfterFailure, string sessionToken)
        {
            var roots = avatars.ToArray();
            var targets = platforms == UploadPlatforms.PC ? new[] { BuildTarget.StandaloneWindows64 }
                : platforms == UploadPlatforms.Quest ? new[] { BuildTarget.Android }
                : new[] { BuildTarget.StandaloneWindows64, BuildTarget.Android };
            var result = new UploadQueueData
            {
                active = true, phase = QueuePhase.Ready, operation = operation,
                initialPlatform = initial, quest = options.Snapshot(), restoreStartingPlatform = restore,
                continueAfterFailure = continueAfterFailure, sessionToken = sessionToken,
                status = "Preparing queue."
            };
            foreach (var target in targets)
                foreach (var avatar in roots)
                    result.jobs.Add(new UploadJob
                    {
                        avatarGlobalId = avatar.globalId, avatarName = avatar.name,
                        blueprintId = avatar.blueprintId, platform = target,
                        status = operation == UploadOperation.Publish && string.IsNullOrWhiteSpace(avatar.blueprintId)
                            ? UploadJobStatus.Skipped : UploadJobStatus.Pending,
                        detail = operation == UploadOperation.Publish && string.IsNullOrWhiteSpace(avatar.blueprintId)
                            ? "Upload once in the SDK to assign a blueprint ID." : "Waiting"
                    });
            return result;
        }

        public void AdvancePastFinished()
        {
            while (Current != null && Current.status != UploadJobStatus.Pending) cursor++;
        }

        public void Interrupt(string reason)
        {
            if (Current != null && Current.status == UploadJobStatus.Running)
            {
                Current.status = UploadJobStatus.Unknown;
                Current.detail = "Interrupted. Verify the avatar listing before retrying; the upload may have completed.";
            }
            active = false;
            phase = QueuePhase.Interrupted;
            status = reason;
        }
    }

    [FilePath("Library/FynnMultiUploaderQueue.asset", FilePathAttribute.Location.ProjectFolder)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Fynn.QuestBridge.Editor", "Fynn.QuestBridge.Editor", "MultiUploaderSession")]
    public sealed class MultiUploaderSession : ScriptableSingleton<MultiUploaderSession>
    {
        public UploadQueueData queue = new UploadQueueData();
        public void Persist() => Save(true);
    }

    public sealed class QueueConversionScope : IDisposable
    {
        public static QuestUploadOptions Current { get; private set; }
        public QueueConversionScope(QuestUploadOptions options)
        {
            if (Current != null) throw new InvalidOperationException("An uploader build is already running.");
            Current = options;
        }
        public void Dispose() => Current = null;
    }

    public static class QuestBuildSettings
    {
        public static MultiUploaderSettings Prepare(GameObject buildCopy, BuildTarget platform, bool playing)
        {
            if (platform != BuildTarget.Android || playing) return null;
            var settings = buildCopy.GetComponent<MultiUploaderSettings>();
            var queueOptions = QueueConversionScope.Current;
            if (queueOptions != null)
            {
                if (!queueOptions.convertQuest) return null;
                if (settings == null) settings = buildCopy.AddComponent<MultiUploaderSettings>();
                queueOptions.Apply(settings);
            }
            return settings != null && settings.enabled && settings.convertOnAndroid ? settings : null;
        }
    }
}
