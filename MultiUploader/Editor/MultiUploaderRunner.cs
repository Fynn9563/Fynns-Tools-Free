using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3A.Editor;
using VRC.SDKBase;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace FynnsTools.MultiUploader.Editor
{
    [InitializeOnLoad]
    public static class MultiUploaderRunner
    {
        const string SessionKey = "Fynn.MultiUploader.EditorSession";
        static bool executing;
        static CancellationTokenSource cancellation;
        static double nextAction;
        static double builderDeadline;
        static bool openedPanel;
        public static UploadQueueData Queue => MultiUploaderSession.instance.queue;
        public static bool Busy => Queue.active || executing;
        public static event Action Changed;

        static MultiUploaderRunner()
        {
            EditorApplication.delayCall += Recover;
            EditorApplication.update += Tick;
            EditorApplication.quitting += () =>
            {
                if (Queue.active) { Queue.Interrupt("Editor closed. Queue stopped; no uploads will restart automatically."); Persist(); }
            };
        }

        static string EditorSession
        {
            get
            {
                string value = SessionState.GetString(SessionKey, "");
                if (!string.IsNullOrEmpty(value)) return value;
                value = Guid.NewGuid().ToString("N");
                SessionState.SetString(SessionKey, value);
                return value;
            }
        }

        static void Recover()
        {
            if (executing) return;
            var q = Queue;
            if (!q.active) return;
            if (q.sessionToken != EditorSession)
                q.Interrupt("Previous editor session ended. Queue stopped; check the avatar listing before retrying.");
            else if (q.phase == QueuePhase.Executing)
                q.Interrupt("Scripts reloaded during an SDK operation. Queue stopped to avoid repeating a possibly successful upload.");
            // Platform switches can resume; an interrupted upload cannot.
            Persist();
        }

        public static GameObject[] FindAvatars() => Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>()
            .Where(a => a != null && a.gameObject.scene.IsValid() && a.gameObject.scene.isLoaded
                && a.gameObject.activeInHierarchy && !EditorUtility.IsPersistent(a)
                && (a.gameObject.hideFlags & HideFlags.DontSave) == 0
                && !EditorSceneManager.IsPreviewScene(a.gameObject.scene)
                && a.GetComponent<PipelineManager>() != null)
            .Select(a => a.gameObject).Distinct().OrderBy(a => a.name, StringComparer.Ordinal).ToArray();

        public static void Start(GameObject[] avatars, UploadOperation operation)
        {
            if (Busy) throw new InvalidOperationException("An upload queue is already running.");
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || Application.isPlaying)
                throw new InvalidOperationException("Wait until Unity finishes importing/compiling and leave Play Mode.");
            var roots = avatars.Where(a => a != null).Distinct().ToArray();
            if (roots.Length == 0) throw new InvalidOperationException("Select at least one avatar.");
            var preferences = MultiUploaderPreferences.instance;
            preferences.EnsureOpaquePreference();
            var targets = preferences.platforms == UploadPlatforms.PC ? new[] { BuildTarget.StandaloneWindows64 }
                : preferences.platforms == UploadPlatforms.Quest ? new[] { BuildTarget.Android }
                : new[] { BuildTarget.StandaloneWindows64, BuildTarget.Android };
            foreach (var target in targets)
                if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                    throw new InvalidOperationException($"Unity's {PlatformName(target)} build module is not installed.");

            if (roots.Any(a => !a.scene.IsValid() || string.IsNullOrEmpty(a.scene.path) || a.scene.isDirty))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                if (roots.Any(a => string.IsNullOrEmpty(a.scene.path) || a.scene.isDirty))
                    throw new InvalidOperationException("Save the selected avatars' scenes before starting a queue that switches platforms.");
            }
            var snapshots = roots.Select(a =>
            {
                var pipeline = a.GetComponent<PipelineManager>();
                if (pipeline == null || a.GetComponent<VRCAvatarDescriptor>() == null)
                    throw new InvalidOperationException(a.name + " is missing its avatar descriptor or PipelineManager.");
                return new QueueAvatar { globalId = GlobalObjectId.GetGlobalObjectIdSlow(a).ToString(), name = a.name, blueprintId = pipeline.blueprintId };
            }).ToArray();
            if (operation == UploadOperation.Publish)
            {
                var duplicate = snapshots.Where(a => !string.IsNullOrWhiteSpace(a.blueprintId))
                    .GroupBy(a => a.blueprintId).FirstOrDefault(g => g.Count() > 1);
                if (duplicate != null) throw new InvalidOperationException("Selected avatars share blueprint ID " + duplicate.Key + ". Select only one source avatar for each listing.");
                if (snapshots.All(a => string.IsNullOrWhiteSpace(a.blueprintId)))
                    throw new InvalidOperationException("These avatars have no blueprint IDs. Upload them once through the SDK first.");
                if (!EditorUtility.DisplayDialog("Publish selected avatars", VRCCopyrightAgreement.AgreementText
                    + $"\n\nThis queue will publish {snapshots.Count(a => !string.IsNullOrWhiteSpace(a.blueprintId))} avatar(s) to {preferences.platforms}.", "OK", "Cancel")) return;
            }
            MultiUploaderSession.instance.queue = UploadQueueData.Create(snapshots, preferences.platforms,
                operation, EditorUserBuildSettings.activeBuildTarget, preferences.quest,
                preferences.restoreStartingPlatform, preferences.continueAfterFailure, EditorSession);
            openedPanel = false;
            Persist();
        }

        public static void Cancel()
        {
            if (!Queue.active) return;
            Queue.cancelRequested = true;
            Queue.status = executing ? "Cancellation requested. Waiting for the current SDK build; upload cancellation has been signalled." : "Cancelling queue…";
            cancellation?.Cancel();
            Persist();
        }

        static void Tick()
        {
            if (executing || !Queue.active || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.timeSinceStartup < nextAction || Application.isPlaying) return;
            try
            {
                var q = Queue;
                if (q.phase == QueuePhase.Restoring)
                {
                    if (EditorUserBuildSettings.activeBuildTarget == q.initialPlatform) Complete();
                    else SwitchPlatform(q.initialPlatform, true);
                    return;
                }
                if (q.cancelRequested) { Finish("Cancelled. Completed jobs are listed below; remaining jobs were not started."); return; }
                q.AdvancePastFinished();
                if (q.Current == null) { Finish($"Finished: {q.Completed}/{q.jobs.Count} succeeded, {q.jobs.Count(j => j.status == UploadJobStatus.Failed)} failed, {q.jobs.Count(j => j.status == UploadJobStatus.Skipped)} skipped."); return; }
                if (EditorUserBuildSettings.activeBuildTarget != q.Current.platform)
                {
                    if (q.phase == QueuePhase.Switching) throw new InvalidOperationException("Unity did not switch to the requested platform. Queue stopped.");
                    SwitchPlatform(q.Current.platform, false);
                    return;
                }
                if (!PlatformReadiness.IsReady(q.Current.platform))
                {
                    q.status = "Waiting for " + PlatformName(q.Current.platform) + " editor scripts to finish reloading. " + PlatformReadiness.Describe();
                    nextAction = EditorApplication.timeSinceStartup + .5;
                    Changed?.Invoke(); return;
                }
                if (q.phase == QueuePhase.Switching) { q.phase = QueuePhase.Ready; Persist(); return; }
                if (!openedPanel)
                {
                    EditorApplication.ExecuteMenuItem("VRChat SDK/Show Control Panel");
                    openedPanel = true;
                    builderDeadline = EditorApplication.timeSinceStartup + 90;
                    nextAction = EditorApplication.timeSinceStartup + 1;
                    q.status = "Opening VRChat SDK builder…"; Persist(); return;
                }
                if (!VRCSdkControlPanel.TryGetBuilder(out IVRCSdkAvatarBuilderApi builder)
                    || (q.operation == UploadOperation.Publish && APIUser.CurrentUser == null))
                {
                    if (EditorApplication.timeSinceStartup > builderDeadline)
                        throw new InvalidOperationException("Open the SDK Control Panel and sign in, then start the queue again.");
                    q.status = "Waiting for the VRChat SDK builder / sign-in…";
                    nextAction = EditorApplication.timeSinceStartup + .5;
                    Changed?.Invoke(); return;
                }
                if (builder.BuildState == SdkBuildState.Building || builder.UploadState == SdkUploadState.Uploading)
                {
                    if (EditorApplication.timeSinceStartup > builderDeadline)
                        throw new InvalidOperationException("The VRChat SDK is busy with another operation. Queue stopped.");
                    nextAction = EditorApplication.timeSinceStartup + .5;
                    q.status = "Waiting for the current VRChat SDK operation…";
                    Changed?.Invoke(); return;
                }
                Execute(builder);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (Queue.phase == QueuePhase.Restoring)
                {
                    Queue.finishMessage += " Could not restore the starting platform: " + e.GetBaseException().Message + ". Switch it manually.";
                    Complete();
                }
                else Finish("Queue stopped: " + e.GetBaseException().Message);
            }
        }

        static void SwitchPlatform(BuildTarget target, bool restoring)
        {
            var q = Queue;
            q.phase = restoring ? QueuePhase.Restoring : QueuePhase.Switching;
            q.expectedPlatform = target;
            q.status = (restoring ? "Restoring " : "Switching to ") + PlatformName(target) + ". Unity may reload scripts…";
            Persist();
            openedPanel = false;
            nextAction = EditorApplication.timeSinceStartup + 2;
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target))
            {
                if (restoring) { q.finishMessage += " Could not restore the starting platform; switch it manually."; Complete(); }
                else Finish("Platform switch failed. Check that Unity's build module is installed.");
            }
        }

        static async void Execute(IVRCSdkAvatarBuilderApi builder)
        {
            var q = Queue;
            var job = q.Current;
            executing = true;
            cancellation = new CancellationTokenSource();
            q.phase = QueuePhase.Executing;
            job.status = UploadJobStatus.Running;
            q.status = job.avatarName + " · " + PlatformName(job.platform);
            Persist();
            EventHandler<string> progress = (_, message) => { q.status = job.avatarName + " · " + PlatformName(job.platform) + ": " + message; Changed?.Invoke(); };
            EventHandler<string> built = (_, message) => { q.status = job.avatarName + (q.operation == UploadOperation.Publish ? ": uploading…" : ": finishing build…"); Changed?.Invoke(); };
            builder.OnSdkBuildProgress += progress;
            builder.OnSdkBuildFinish += built;
            var sdkErrors = new List<string>();
            Application.LogCallback captureErrors = (message, stack, type) =>
            {
                if ((type != LogType.Error && type != LogType.Exception) || sdkErrors.Count >= 30) return;
                string safeMessage = UploadDiagnostics.Sanitize(message);
                if (safeMessage.Length > 2000) safeMessage = safeMessage.Substring(0, 2000);
                if (!sdkErrors.Contains(safeMessage)) sdkErrors.Add(safeMessage);
            };
            Application.logMessageReceived += captureErrors;
            try
            {
                if (!PlatformReadiness.IsReady(job.platform))
                    throw new InvalidOperationException("Refusing to build with mismatched Unity platform state: " + PlatformReadiness.Describe());
                Debug.Log("[MultiUploader] Starting " + job.avatarName + " · " + PlatformName(job.platform)
                    + " (" + PlatformReadiness.Describe() + "). Quest conversion=" + q.quest.convertQuest);
                if (!GlobalObjectId.TryParse(job.avatarGlobalId, out var id)) throw new InvalidOperationException("Avatar scene identity is invalid.");
                var root = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                if (root == null || !root.activeInHierarchy) throw new InvalidOperationException("Avatar is missing or inactive. Reopen its saved scene before starting a new queue.");
                var pipeline = root.GetComponent<PipelineManager>();
                if (pipeline == null || (pipeline.blueprintId ?? "") != (job.blueprintId ?? ""))
                    throw new InvalidOperationException("Avatar blueprint ID changed after the queue was prepared. Queue stopped.");
                cancellation.Token.ThrowIfCancellationRequested();
                builder.SelectAvatar(root);
                using (new QueueConversionScope(q.quest))
                {
                    if (q.operation == UploadOperation.Publish)
                    {
                        var avatar = await VRCApi.GetAvatar(job.blueprintId);
                        cancellation.Token.ThrowIfCancellationRequested();
                        if (avatar.ID != job.blueprintId) throw new InvalidOperationException("VRChat returned a different avatar ID.");
                        await RecordCopyrightConsent(job.blueprintId, cancellation.Token);
                        await builder.BuildAndUpload(root, avatar, cancellationToken: cancellation.Token);
                    }
                    else if (q.operation == UploadOperation.BuildAndTest) await builder.BuildAndTest(root);
                    else job.detail = await builder.Build(root);
                }
                job.status = UploadJobStatus.Succeeded;
                if (q.operation != UploadOperation.BuildOnly) job.detail = "Finished";
            }
            catch (OperationCanceledException)
            {
                job.status = UploadJobStatus.Cancelled;
                job.detail = "Cancelled. If uploading had started, verify the listing before retrying.";
                q.cancelRequested = true;
            }
            catch (Exception e)
            {
                job.status = UploadJobStatus.Failed;
                job.detail = UploadDiagnostics.Sanitize(e.GetBaseException().Message);
                if (sdkErrors.Count > 0) job.detail += "\nSDK details:\n" + string.Join("\n", sdkErrors.Take(8));
                Debug.LogError(UploadDiagnostics.Sanitize(e.ToString()));
            }
            finally
            {
                builder.OnSdkBuildProgress -= progress;
                builder.OnSdkBuildFinish -= built;
                Application.logMessageReceived -= captureErrors;
                cancellation.Dispose(); cancellation = null;
                executing = false;
                q.phase = QueuePhase.Ready;
                q.cursor++;
                builderDeadline = EditorApplication.timeSinceStartup + 90;
                Persist();
            }
            if (q.cancelRequested) Finish("Cancelled. The current job's result is listed below; no remaining jobs were started.");
            else if (job.status == UploadJobStatus.Failed && !q.continueAfterFailure)
                Finish("Stopped after " + job.avatarName + " · " + PlatformName(job.platform) + ": " + job.detail);
        }

        static async Task RecordCopyrightConsent(string blueprint, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var result = await VRCApi.ContentUploadConsent(new VRCAgreement
            {
                AgreementCode = "content.copyright.owned", AgreementFulltext = VRCCopyrightAgreement.AgreementText,
                ContentId = blueprint, Version = 1
            });
            token.ThrowIfCancellationRequested();
            if (result.ContentId != blueprint || result.Version != 1 || result.AgreementCode != "content.copyright.owned")
                throw new InvalidOperationException("VRChat did not record the copyright agreement.");
            const string key = "VRCSdkControlPanel.CopyrightAgreement.ContentList";
            var ids = new HashSet<string>(SessionState.GetString(key, "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            ids.Add(blueprint);
            SessionState.SetString(key, string.Join(";", ids));
        }

        static void Finish(string message)
        {
            var q = Queue;
            q.finishMessage = message;
            if (q.restoreStartingPlatform && EditorUserBuildSettings.activeBuildTarget != q.initialPlatform)
            {
                q.phase = QueuePhase.Restoring; q.status = "Restoring the starting platform…"; Persist();
            }
            else Complete();
        }

        static void Complete()
        {
            var q = Queue;
            q.active = false;
            q.phase = QueuePhase.Finished;
            q.status = q.finishMessage;
            if (q.restoreStartingPlatform && EditorUserBuildSettings.activeBuildTarget == q.initialPlatform)
                q.status += "\nReturned to starting platform: " + PlatformName(q.initialPlatform) + ".";
            Persist();
        }

        static void Persist() { MultiUploaderSession.instance.Persist(); Changed?.Invoke(); }
        public static string PlatformName(BuildTarget target) => target == BuildTarget.Android ? "Quest / Android"
            : target == BuildTarget.StandaloneWindows64 ? "PC / Windows" : target.ToString();
    }
}
