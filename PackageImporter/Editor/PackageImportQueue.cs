using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.PackageImporter
{
    internal enum ImportStatus { Pending, Importing, Imported, Skipped, Cancelled, Failed, NeedsReview }

    [Serializable]
    internal sealed class ImportItem
    {
        public string path;
        public bool selected = true;
        public ImportStatus status;
        public string detail = "";
    }

    [Serializable]
    internal sealed class QueueState
    {
        public List<ImportItem> items = new List<ImportItem>();
        public bool running;
        public bool stopRequested;
        public int active = -1;
        public bool sawStart;
        public bool uncertain;
        public bool resumeOnResult;
        public bool automaticImport;
        public List<string> expectedPaths = new List<string>();
        public List<string> expectedGuids = new List<string>();
        public string message = "";
        public bool reviewAll;
        public string folder = "";

        internal ImportItem Current { get { return active >= 0 && active < items.Count ? items[active] : null; } }
        internal bool Busy { get { return running || Current != null; } }

        internal bool Matches(string name)
        {
            if (Current == null || string.IsNullOrEmpty(name)) return false;
            string actual = Path.GetFileName(name.Replace('\\', '/'));
            string expected = Path.GetFileName(Current.path);
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
                || string.Equals(actual, Path.GetFileNameWithoutExtension(expected), StringComparison.OrdinalIgnoreCase);
        }

        internal void Interrupted()
        {
            if (Current == null) return;
            resumeOnResult |= running && !stopRequested;
            Current.status = ImportStatus.NeedsReview;
            Current.detail = "Unity reloaded before the import result was recorded. Check this package before continuing.";
            running = false;
            uncertain = true;
            message = Current.detail;
        }

        internal bool Complete(string name, ImportStatus status, string detail)
        {
            if (!Matches(name) || !sawStart) return false;
            running |= resumeOnResult;
            resumeOnResult = false;
            Current.status = status;
            Current.detail = detail;
            active = -1;
            sawStart = false;
            uncertain = false;
            if (stopRequested || status == ImportStatus.Failed || status == ImportStatus.Cancelled)
            {
                running = false;
                message = status == ImportStatus.Failed ? "Import failed. Remaining packages are paused."
                    : status == ImportStatus.Cancelled ? "Import cancelled. Remaining packages are paused." : "Queue stopped.";
            }
            stopRequested = false;
            if (running) message = "";
            return true;
        }
    }

    [FilePath("Library/FynnsTools/PackageImporter.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class PackageImportStore : ScriptableSingleton<PackageImportStore>
    {
        [SerializeField] internal QueueState state = new QueueState();
        internal void Persist() { Save(true); }
    }

    [InitializeOnLoad]
    internal static class PackageImportQueue
    {
        internal static event Action Changed;
        internal static QueueState State { get { return PackageImportStore.instance.state; } }
        private static double startedAt;
        private static double nextAt;
        private static bool foreignImport;
        private static string foreignName;
        private static bool inspecting;
        internal static bool ExternalImport { get { return foreignImport; } }
        private static string Root { get { return Directory.GetParent(Application.dataPath).FullName; } }

        static PackageImportQueue()
        {
            State.Interrupted();
            PackageImportStore.instance.Persist();
            AssetDatabase.importPackageStarted += OnStarted;
            AssetDatabase.importPackageCompleted += OnCompleted;
            AssetDatabase.importPackageCancelled += OnCancelled;
            AssetDatabase.importPackageFailed += OnFailed;
            EditorApplication.update += Update;
            nextAt = EditorApplication.timeSinceStartup + 1;
        }

        internal static void Save()
        {
            PackageImportStore.instance.Persist();
            if (Changed != null) Changed();
        }

        internal static void Add(IEnumerable<string> paths)
        {
            if (State.Busy) return;
            string warning = "";
            foreach (string path in paths.OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
            {
                string full = Path.GetFullPath(path);
                if (!File.Exists(full) || !full.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase)) continue;
                if (State.items.Any(i => string.Equals(i.path, full, StringComparison.OrdinalIgnoreCase))) continue;
                // Ambiguous package names cannot share a queue.
                if (State.items.Any(i => string.Equals(Path.GetFileName(i.path), Path.GetFileName(full), StringComparison.OrdinalIgnoreCase)))
                { warning = "A package named " + Path.GetFileName(full) + " is already queued. Import these packages in separate queues."; continue; }
                State.items.Add(new ImportItem { path = full });
            }
            State.message = warning;
            Save();
        }

        internal static void Start()
        {
            if (State.Busy || foreignImport || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating || EditorApplication.isCompiling) return;
            if (!State.items.Any(i => i.selected && i.status == ImportStatus.Pending)) return;
            State.running = true;
            State.stopRequested = false;
            State.message = "";
            nextAt = EditorApplication.timeSinceStartup + 0.1;
            Save();
        }

        internal static void Stop()
        {
            State.running = false;
            State.resumeOnResult = false;
            State.stopRequested = State.Current != null;
            State.message = State.Current == null ? "Queue stopped." : "Stopping after this package. Use Unity's import dialog to cancel the current import.";
            Save();
        }

        internal static void RetrySelected()
        {
            if (State.Busy) return;
            foreach (var item in State.items.Where(i => i.selected))
            { item.status = ImportStatus.Pending; item.detail = ""; }
            State.message = "";
            Save();
        }

        internal static void CheckResult()
        {
            if (State.Current == null) return;
            State.running = false;
            State.resumeOnResult = false;
            State.uncertain = true;
            State.Current.status = ImportStatus.NeedsReview;
            State.message = "Finish or close Unity's import dialog, then check the assets and record this package's result.";
            Save();
        }

        internal static void Resolve(ImportStatus status)
        {
            // Timeouts do not prove a package imported successfully.
            if (State.Current == null || !State.uncertain || foreignImport || EditorApplication.isUpdating || EditorApplication.isCompiling) return;
            State.Current.status = status;
            State.Current.detail = "Manually marked " + status.ToString().ToLowerInvariant() + ".";
            State.active = -1;
            State.sawStart = false;
            State.uncertain = false;
            State.resumeOnResult = false;
            State.running = false;
            State.stopRequested = false;
            State.message = "Result recorded. Continue when ready.";
            Save();
        }

        private static void Update()
        {
            if (inspecting) return;
            var state = State;
            if (state.Current != null)
            {
                if (!state.uncertain && EditorApplication.timeSinceStartup - startedAt > 120)
                {
                    state.uncertain = true;
                    state.resumeOnResult = false;
                    state.running = false;
                    state.Current.status = ImportStatus.NeedsReview;
                    state.Current.detail = "Still waiting for Unity's result. Finish or close its import dialog; then check the package if no result arrives.";
                    state.message = state.Current.detail;
                    Save();
                }
                return;
            }
            if (!state.running || foreignImport || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup < nextAt) return;
            int index = state.items.FindIndex(i => i.selected && i.status == ImportStatus.Pending);
            if (index < 0)
            {
                state.running = false;
                state.message = "Queue complete.";
                Save();
                return;
            }
            var item = state.items[index];
            try
            {
                inspecting = true;
                item.detail = "Checking package…";
                if (Changed != null) Changed();
                var inspection = PackageArchive.Inspect(item.path, Root);
                if (inspection.AllIdentical)
                {
                    item.status = ImportStatus.Skipped;
                    item.detail = "Asset and metadata bytes already match the package.";
                    nextAt = EditorApplication.timeSinceStartup + 0.2;
                    Save();
                    return;
                }
                state.active = index;
                state.sawStart = false;
                state.uncertain = false;
                startedAt = EditorApplication.timeSinceStartup;
                bool review = state.reviewAll || inspection.HasExisting;
                state.automaticImport = !review;
                state.expectedPaths = inspection.Paths;
                state.expectedGuids = inspection.Guids;
                item.status = ImportStatus.Importing;
                item.detail = review ? "Review this package in Unity's import dialog." : "Importing new assets…";
                Save();
                AssetDatabase.ImportPackage(item.path, review);
            }
            catch (Exception e)
            {
                item.status = ImportStatus.Failed;
                item.detail = e.Message;
                state.active = -1;
                state.sawStart = false;
                state.uncertain = false;
                state.running = false;
                state.message = "Package failed. Remaining packages are paused.";
                Save();
            }
            finally { inspecting = false; }
        }

        private static void OnStarted(string name)
        {
            if (State.Matches(name) && !foreignImport && !State.sawStart)
            {
                State.sawStart = true;
                Save();
            }
            else
            {
                if (State.Matches(name)) State.sawStart = false;
                foreignImport = true;
                foreignName = name;
                State.running = false;
                State.resumeOnResult = false;
                State.message = "Another package import started. Finish it before continuing this queue.";
                if (State.Current != null)
                {
                    State.uncertain = true;
                    State.running = false;
                    State.Current.status = ImportStatus.NeedsReview;
                    State.message = "Another package import started. Check the current package before continuing.";
                }
                Save();
            }
        }

        private static void Finished(string name, ImportStatus status, string detail)
        {
            if (status == ImportStatus.Imported && State.Matches(name) && State.sawStart && State.automaticImport)
            {
                for (int i = 0; i < State.expectedPaths.Count; i++)
                {
                    string path = State.expectedPaths[i];
                    string local = Path.Combine(Root, path);
                    if ((!File.Exists(local) && !Directory.Exists(local))
                        || !string.Equals(AssetDatabase.AssetPathToGUID(path), State.expectedGuids[i], StringComparison.OrdinalIgnoreCase))
                    {
                        status = ImportStatus.Failed;
                        detail = "Unity finished, but an expected asset was not imported: " + path;
                        break;
                    }
                }
            }
            if (State.Complete(name, status, detail))
            {
                nextAt = EditorApplication.timeSinceStartup + 1;
                Save();
            }
            else if (string.Equals(EventName(foreignName), EventName(name), StringComparison.OrdinalIgnoreCase))
            { foreignImport = false; foreignName = null; if (Changed != null) Changed(); }
        }

        private static string EventName(string name)
        {
            return string.IsNullOrEmpty(name) ? "" : Path.GetFileNameWithoutExtension(name.Replace('\\', '/'));
        }

        private static void OnCompleted(string name) { Finished(name, ImportStatus.Imported, "Unity reported import complete."); }
        private static void OnCancelled(string name) { Finished(name, ImportStatus.Cancelled, "Cancelled in Unity's import dialog."); }
        private static void OnFailed(string name, string error) { Finished(name, ImportStatus.Failed, error); }
    }
}
