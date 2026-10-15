using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FynnsTools.AssetOrganization
{
    public enum OrganizeAction { Skip, Move }
    public enum AssetCategory { Animation, Controller, Texture, Material, Model, Prefab, Audio, Mask, Scene, Preset, VRC, Shader, Script, Other }
    public enum AssetSort { AlphabeticalPath, AlphabeticalAsset, AssetType }

    [Serializable]
    public sealed class FolderRule
    {
        public string name;
        public bool enabled = true;
        public OrganizeAction action;
        public FolderRule(string name) { this.name = name; }
    }

    [Serializable]
    public sealed class OrganizerSettings
    {
        public List<FolderRule> folders = new List<FolderRule>();
        public OrganizeAction[] types;
        public bool deleteEmptyFolders = true;
        public AssetSort sort;

        public static OrganizerSettings Defaults()
        {
            var settings = new OrganizerSettings();
            foreach (var name in new[] { "VRCSDK", "_PoiyomiShaders", "VRLabs", "PumkinsAvatarTools", "FynnToolKit", "Packages", "Plugins", "Editor" })
                settings.folders.Add(new FolderRule(name));
            settings.types = Enum.GetValues(typeof(AssetCategory)).Cast<AssetCategory>()
                .Select(c => c == AssetCategory.Preset || c == AssetCategory.Shader || c == AssetCategory.Script || c == AssetCategory.Other ? OrganizeAction.Skip : OrganizeAction.Move).ToArray();
            return settings;
        }

        public void Validate()
        {
            var defaults = Defaults();
            if (folders == null) folders = defaults.folders;
            folders.RemoveAll(f => f == null);
            if (types == null || types.Length != defaults.types.Length)
            {
                if (types != null) Array.Copy(types, defaults.types, Math.Min(types.Length, defaults.types.Length));
                types = defaults.types;
            }
            for (int i = 0; i < types.Length; i++)
                if (!Enum.IsDefined(typeof(OrganizeAction), types[i])) types[i] = OrganizeAction.Skip;
        }
    }

    [Serializable]
    public sealed class AssetEntry
    {
        public string path;
        public string guid;
        public string name;
        public string typeName;
        public AssetCategory category;
        public OrganizeAction action;
        public string blockedReason;
        public bool automaticallySkipped;
    }

    [Serializable]
    public sealed class AssetMove
    {
        public string guid;
        public string source;
        public string destination;
    }

    [Serializable]
    public sealed class RemovedFolder
    {
        public string path;
        public string meta;
    }

    [Serializable]
    public sealed class MoveReport
    {
        public List<AssetMove> moved = new List<AssetMove>();
        public List<RemovedFolder> removedFolders = new List<RemovedFolder>();
        public List<string> errors = new List<string>();
        public int skipped;
    }

    public static class AssetOrganizerOperations
    {
        public static List<AssetEntry> Scan(Object source, OrganizerSettings settings)
        {
            if (!source) throw new ArgumentException("Choose an avatar, asset or project folder.");
            settings.Validate();
            string sourcePath = AssetDatabase.GetAssetPath(source);
            var avatar = source as GameObject;
            if (string.IsNullOrEmpty(sourcePath) && !avatar) throw new ArgumentException("Choose an avatar, asset or project folder.");
            var paths = avatar && !EditorUtility.IsPersistent(avatar) ? AvatarDependencies(avatar)
                : AssetDatabase.IsValidFolder(sourcePath)
                ? AssetDatabase.FindAssets("", new[] { sourcePath }).Select(AssetDatabase.GUIDToAssetPath)
                : AssetDatabase.GetDependencies(sourcePath, true).AsEnumerable();
            var entries = new List<AssetEntry>();
            foreach (var path in paths.Distinct().Where(p => !AssetDatabase.IsValidFolder(p)))
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                var type = asset ? asset.GetType() : null;
                var category = Classify(type, path);
                string blocked = GetBlockedReason(path);
                var action = settings.types[(int)category];
                string[] segments = (Path.GetDirectoryName(path) ?? "").Replace('\\', '/').Split('/');
                var rule = settings.folders.FirstOrDefault(f => f.enabled && !string.IsNullOrEmpty(f.name) && segments.Contains(f.name));
                if (rule != null) action = rule.action;
                if (!asset) blocked = "Asset could not be loaded.";
                entries.Add(new AssetEntry
                {
                    path = path, guid = AssetDatabase.AssetPathToGUID(path), name = asset ? asset.name : Path.GetFileName(path),
                    typeName = type?.Name ?? "Missing", category = category, action = blocked == null ? action : OrganizeAction.Skip,
                    blockedReason = blocked
                });
            }
            return (settings.sort == AssetSort.AlphabeticalAsset ? entries.OrderBy(e => e.name, StringComparer.OrdinalIgnoreCase)
                : settings.sort == AssetSort.AssetType ? entries.OrderBy(e => e.typeName, StringComparer.OrdinalIgnoreCase)
                : entries.OrderBy(e => e.path, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        private static IEnumerable<string> AvatarDependencies(GameObject avatar)
        {
            var objects = avatar.GetComponentsInChildren<Transform>(true).Select(t => (Object)t.gameObject)
                .Concat(avatar.GetComponentsInChildren<Component>(true).Where(c => c).Cast<Object>()).ToArray();
            var paths = EditorUtility.CollectDependencies(objects).Select(AssetDatabase.GetAssetPath)
                .Where(p => !string.IsNullOrEmpty(p) && !p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)).Distinct().ToArray();
            return paths.Concat(paths.Length == 0 ? Array.Empty<string>() : AssetDatabase.GetDependencies(paths, true))
                .Where(p => !p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)).Distinct();
        }

        public static AssetCategory Classify(Type type, string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (new[] { ".fbx", ".obj", ".dae", ".3ds", ".dxf" }.Contains(extension) || Is(type, typeof(Mesh))) return AssetCategory.Model;
            if (Is(type, typeof(AnimationClip)) || Is(type, typeof(BlendTree))) return AssetCategory.Animation;
            if (Is(type, typeof(AnimatorController)) || Is(type, typeof(AnimatorOverrideController))) return AssetCategory.Controller;
            if (Is(type, typeof(Texture))) return AssetCategory.Texture;
            if (Is(type, typeof(Material))) return AssetCategory.Material;
            if (extension == ".prefab") return AssetCategory.Prefab;
            if (Is(type, typeof(AudioClip))) return AssetCategory.Audio;
            if (Is(type, typeof(AvatarMask))) return AssetCategory.Mask;
            if (Is(type, typeof(SceneAsset))) return AssetCategory.Scene;
            if (Is(type, typeof(Preset))) return AssetCategory.Preset;
            for (var current = type; current != null; current = current.BaseType)
                if (current.FullName == "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters" || current.FullName == "VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu") return AssetCategory.VRC;
            if (Is(type, typeof(Shader)) || new[] { ".shader", ".shadergraph", ".shadersubgraph", ".compute", ".cginc", ".hlsl" }.Contains(extension)) return AssetCategory.Shader;
            if (Is(type, typeof(MonoScript)) || extension == ".dll" || extension == ".cs") return AssetCategory.Script;
            return AssetCategory.Other;
        }

        private static bool Is(Type type, Type parent) => type != null && parent.IsAssignableFrom(type);

        public static bool TryFolder(string input, out string path, out string error)
        {
            path = (input ?? "").Trim().Replace('\\', '/').TrimEnd('/');
            var segments = path.Split('/');
            if ((path != "Assets" && !path.StartsWith("Assets/", StringComparison.Ordinal)) ||
                segments.Any(s => string.IsNullOrEmpty(s) || s == "." || s == ".." || s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            {
                error = "Choose a folder under Assets.";
                return false;
            }
            error = null;
            return true;
        }

        public static string GetBlockedReason(string path)
        {
            if (!TryFolder(path, out _, out _)) return "Built-in and package assets cannot be moved.";
            if (!File.Exists(path) || string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path))) return "Asset is missing.";
            if ((File.GetAttributes(path) & FileAttributes.ReadOnly) != 0 ||
                (File.Exists(path + ".meta") && (File.GetAttributes(path + ".meta") & FileAttributes.ReadOnly) != 0) ||
                !AssetDatabase.IsOpenForEdit(path)) return "Asset or metadata is read-only.";
            return null;
        }

        public static bool TryPlan(IEnumerable<AssetEntry> entries, string destination, out List<AssetMove> moves, out string error)
        {
            moves = new List<AssetMove>();
            if (!TryFolder(destination, out destination, out error)) return false;
            string directory = destination;
            while (directory != "Assets")
            {
                if (File.Exists(directory)) { error = directory + " is a file, not a folder."; return false; }
                directory = Path.GetDirectoryName(directory).Replace('\\', '/');
            }
            var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries ?? Enumerable.Empty<AssetEntry>())
            {
                if (entry == null || entry.action == OrganizeAction.Skip) continue;
                error = GetBlockedReason(entry.path);
                if (error != null) { error = entry.path + ": " + error; return false; }
                if (!Enum.IsDefined(typeof(AssetCategory), entry.category) || !Enum.IsDefined(typeof(OrganizeAction), entry.action))
                { error = "An asset rule is invalid. Get Assets again."; return false; }
                if (AssetDatabase.GUIDToAssetPath(entry.guid) != entry.path)
                { error = "An asset has moved since the scan. Get Assets again."; return false; }
                if (!sources.Add(entry.path)) continue;
                string target = destination + "/" + entry.category + "/" + Path.GetFileName(entry.path);
                if (string.Equals(target, entry.path, StringComparison.OrdinalIgnoreCase)) continue;
                string candidate = target;
                int number = 1;
                while (reserved.Contains(candidate) || File.Exists(candidate) || Directory.Exists(candidate) || File.Exists(candidate + ".meta"))
                {
                    if (number > 100000) { error = "Could not find a free filename for " + target; return false; }
                    candidate = Path.GetDirectoryName(target).Replace('\\', '/') + "/" + Path.GetFileNameWithoutExtension(target) + " " + number++ + Path.GetExtension(target);
                }
                reserved.Add(candidate);
                moves.Add(new AssetMove { guid = entry.guid, source = entry.path, destination = candidate });
            }
            error = null;
            return true;
        }

        public static MoveReport Execute(IEnumerable<AssetEntry> entries, string destination, bool deleteEmptyFolders)
        {
            var rows = (entries ?? Enumerable.Empty<AssetEntry>()).ToArray();
            var report = new MoveReport();
            if (!TryPlan(rows, destination, out var moves, out string error)) { report.errors.Add(error); return report; }
            report.skipped = rows.Length - moves.Count;
            if (moves.Count == 0) return report;
            var createdFolders = new List<string>();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var move in moves)
                {
                    try
                    {
                        EnsureFolder(Path.GetDirectoryName(move.destination).Replace('\\', '/'), createdFolders);
                        error = AssetDatabase.ValidateMoveAsset(move.source, move.destination);
                        if (string.IsNullOrEmpty(error)) error = AssetDatabase.MoveAsset(move.source, move.destination);
                        if (!string.IsNullOrEmpty(error)) report.errors.Add(move.source + ": " + error);
                        else report.moved.Add(move);
                    }
                    catch (Exception exception) { report.errors.Add(move.source + ": " + exception.Message); }
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            if (deleteEmptyFolders)
                foreach (var folder in report.moved.Select(m => Path.GetDirectoryName(m.source).Replace('\\', '/')).Concat(createdFolders).Distinct().OrderByDescending(p => p.Length))
                    RemoveEmptyFolder(folder, report);
            return report;
        }

        public static MoveReport Revert(MoveReport previous)
        {
            var result = new MoveReport();
            if (previous == null || previous.moved.Count == 0) return result;
            // Validate all original slots before reverting moves.
            foreach (var move in previous.moved)
            {
                if (AssetDatabase.GUIDToAssetPath(move.guid) != move.destination || GetBlockedReason(move.destination) != null ||
                    File.Exists(move.source) || Directory.Exists(move.source) || !TryFolder(move.source, out _, out _))
                    result.errors.Add("Cannot safely restore " + move.source + ". Its location or original slot has changed.");
            }
            if (result.errors.Count > 0) return result;
            try
            {
                foreach (var removed in previous.removedFolders.OrderBy(f => f.path.Length))
                {
                    if (AssetDatabase.IsValidFolder(removed.path)) continue;
                    if (!TryFolder(removed.path, out _, out _) || File.Exists(removed.path + ".meta"))
                        throw new InvalidOperationException("Cannot restore folder metadata for " + removed.path);
                    Directory.CreateDirectory(removed.path);
                    if (!string.IsNullOrEmpty(removed.meta)) File.WriteAllText(removed.path + ".meta", removed.meta);
                    AssetDatabase.ImportAsset(removed.path);
                }
            }
            catch (Exception exception) { result.errors.Add(exception.Message); return result; }
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var move in previous.moved.AsEnumerable().Reverse())
                {
                    EnsureFolder(Path.GetDirectoryName(move.source).Replace('\\', '/'), new List<string>());
                    string error = AssetDatabase.MoveAsset(move.destination, move.source);
                    if (!string.IsNullOrEmpty(error)) result.errors.Add(move.source + ": " + error);
                    else result.moved.Add(move);
                }
            }
            catch (Exception exception) { result.errors.Add(exception.Message); }
            finally { AssetDatabase.StopAssetEditing(); }
            return result;
        }

        private static void EnsureFolder(string path, List<string> created)
        {
            if (!TryFolder(path, out path, out string error)) throw new ArgumentException(error);
            string current = "Assets";
            foreach (string segment in path.Split('/').Skip(1))
            {
                string next = current + "/" + segment;
                if (!AssetDatabase.IsValidFolder(next))
                {
                    if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(current, segment))) throw new IOException("Could not create " + next);
                    created.Add(next);
                }
                current = next;
            }
        }

        private static void RemoveEmptyFolder(string folder, MoveReport report)
        {
            try
            {
                if (folder == "Assets" || !TryFolder(folder, out _, out _) || !AssetDatabase.IsValidFolder(folder) || Directory.EnumerateFileSystemEntries(folder).Any()) return;
                string meta = File.Exists(folder + ".meta") ? File.ReadAllText(folder + ".meta") : null;
                if (AssetDatabase.DeleteAsset(folder)) report.removedFolders.Add(new RemovedFolder { path = folder, meta = meta });
                else report.errors.Add("Could not remove empty folder " + folder);
            }
            catch (Exception exception) { report.errors.Add(folder + ": " + exception.Message); }
        }
    }
}
