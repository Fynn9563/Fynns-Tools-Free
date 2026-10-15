using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace FynnsTools.PackageImporter
{
    internal sealed class PackageInspection
    {
        internal int Assets;
        internal bool HasExisting;
        internal bool AllIdentical = true;
        internal readonly List<string> Paths = new List<string>();
        internal readonly List<string> Guids = new List<string>();
    }

    internal static class PackageArchive
    {
        private sealed class Entry
        {
            internal string Path;
            internal byte[] AssetHash;
            internal byte[] MetaHash;
            internal string MetaGuid;
            internal bool Folder;
        }

        internal static PackageInspection Inspect(string package, string projectRoot)
        {
            var entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            var buffer = new byte[65536];
            var header = new byte[512];
            long total = 0;
            using (var file = File.OpenRead(package))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            {
                while (true)
                {
                    int count = Read(gzip, header, 512);
                    if (count != 512) throw new InvalidDataException("Incomplete package archive.");
                    if (header.All(b => b == 0)) break;
                    long checksum = Number(header, 148, 8);
                    long sum = 0;
                    for (int i = 0; i < 512; i++) sum += i >= 148 && i < 156 ? 32 : header[i];
                    if (sum != checksum) throw new InvalidDataException("Invalid package archive checksum.");
                    long size = Number(header, 124, 12);
                    total = checked(total + size);
                    if (size > 8L * 1024 * 1024 * 1024 || total > 64L * 1024 * 1024 * 1024)
                        throw new InvalidDataException("Package is too large to inspect.");
                    string name = Text(header, 0, 100);
                    string prefix = Text(header, 345, 155);
                    if (!string.IsNullOrEmpty(prefix)) name = prefix + "/" + name;
                    var parts = name.Split('/');
                    Entry entry = null;
                    string kind = "";
                    if (parts.Length == 2 && parts[0].Length == 32 && parts[0].All(Uri.IsHexDigit))
                    {
                        kind = parts[1];
                        if (!entries.TryGetValue(parts[0], out entry))
                        {
                            if (entries.Count >= 100000) throw new InvalidDataException("Too many package entries.");
                            entries.Add(parts[0], entry = new Entry());
                        }
                    }
                    if (entry != null && kind == "pathname")
                    {
                        if (header[156] != 0 && header[156] != '0') throw new InvalidDataException("Unsupported pathname entry.");
                        if (size == 0 || size > 4096 || entry.Path != null) throw new InvalidDataException("Invalid asset pathname.");
                        var pathBytes = new byte[(int)size];
                        if (Read(gzip, pathBytes, (int)size) != size) throw new EndOfStreamException();
                        entry.Path = new UTF8Encoding(false, true).GetString(pathBytes).TrimEnd('\0', '\r', '\n');
                        ValidatePath(entry.Path, projectRoot);
                    }
                    else
                    {
                        bool hash = entry != null && (kind == "asset" || kind == "asset.meta");
                        if (hash && header[156] != 0 && header[156] != '0') throw new InvalidDataException("Unsupported asset archive entry.");
                        using (var sha = hash ? SHA256.Create() : null)
                        using (var metadata = entry != null && kind == "asset.meta" ? new MemoryStream() : null)
                        {
                            long remaining = size;
                            while (remaining > 0)
                            {
                                int length = (int)Math.Min(buffer.Length, remaining);
                                int got = Read(gzip, buffer, length);
                                if (got != length) throw new EndOfStreamException();
                                if (sha != null) sha.TransformBlock(buffer, 0, got, buffer, 0);
                                if (metadata != null && metadata.Length < 65536)
                                    metadata.Write(buffer, 0, (int)Math.Min(got, 65536 - metadata.Length));
                                remaining -= got;
                            }
                            if (sha != null)
                            {
                                sha.TransformFinalBlock(buffer, 0, 0);
                                if (kind == "asset")
                                {
                                    if (entry.AssetHash != null) throw new InvalidDataException("Duplicate asset data.");
                                    entry.AssetHash = sha.Hash;
                                }
                                else
                                {
                                    if (entry.MetaHash != null) throw new InvalidDataException("Duplicate asset metadata.");
                                    entry.MetaHash = sha.Hash;
                                    string text = Encoding.UTF8.GetString(metadata.ToArray());
                                    entry.MetaGuid = Regex.Match(text, @"(?m)^guid:\s*([a-fA-F0-9]{32})\s*$").Groups[1].Value;
                                    entry.Folder = Regex.IsMatch(text, @"(?m)^folderAsset:\s*yes\s*$");
                                }
                            }
                        }
                    }
                    int padding = (int)((512 - size % 512) % 512);
                    if (Read(gzip, buffer, padding) != padding) throw new EndOfStreamException();
                }
                // Read the gzip trailer before accepting an unchanged archive.
                while (gzip.Read(buffer, 0, buffer.Length) > 0) { }
            }
            var result = new PackageInspection();
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in entries)
            {
                var entry = pair.Value;
                if (entry.Path == null || entry.MetaHash == null) throw new InvalidDataException("Missing asset path or metadata.");
                if (!string.Equals(entry.MetaGuid, pair.Key, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Asset GUID does not match its metadata.");
                if (!entry.Folder && entry.AssetHash == null) throw new InvalidDataException("Missing asset payload.");
                if (!paths.Add(entry.Path)) throw new InvalidDataException("Duplicate asset pathname.");
                string local = Path.Combine(projectRoot, entry.Path);
                bool exists = File.Exists(local) || Directory.Exists(local);
                string guidPath = AssetDatabase.GUIDToAssetPath(pair.Key);
                bool moved = !string.IsNullOrEmpty(guidPath) && !string.Equals(guidPath, entry.Path, StringComparison.Ordinal);
                result.HasExisting |= exists || moved;
                bool same = exists && !moved && HashMatches(local + ".meta", entry.MetaHash);
                if (entry.Folder) same &= Directory.Exists(local);
                else same &= HashMatches(local, entry.AssetHash);
                result.AllIdentical &= same;
                result.Assets++;
                result.Paths.Add(entry.Path);
                result.Guids.Add(pair.Key);
            }
            if (result.Assets == 0) throw new InvalidDataException("No supported Assets entries found in the package.");
            return result;
        }

        private static void ValidatePath(string path, string root)
        {
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.IndexOf('\\') >= 0 || path.IndexOf(':') >= 0
                || path.Split('/').Any(p => p == ".." || p == "." || p.Length == 0))
                throw new InvalidDataException("Unsupported asset path: " + path);
            string full = Path.GetFullPath(Path.Combine(root, path));
            string assets = Path.GetFullPath(Path.Combine(root, "Assets")) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(assets, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Asset path leaves Assets.");
        }

        private static bool HashMatches(string path, byte[] expected)
        {
            if (!File.Exists(path)) return false;
            using (var file = File.OpenRead(path))
            using (var sha = SHA256.Create()) return sha.ComputeHash(file).SequenceEqual(expected);
        }

        private static int Read(Stream stream, byte[] bytes, int length)
        {
            int offset = 0;
            while (offset < length)
            {
                int got = stream.Read(bytes, offset, length - offset);
                if (got == 0) break;
                offset += got;
            }
            return offset;
        }

        private static string Text(byte[] bytes, int offset, int length)
        {
            int end = offset;
            while (end < offset + length && bytes[end] != 0) end++;
            return Encoding.UTF8.GetString(bytes, offset, end - offset);
        }

        private static long Number(byte[] bytes, int offset, int length)
        {
            long result = 0;
            foreach (byte b in bytes.Skip(offset).Take(length))
            {
                if (b == 0 || b == 32) continue;
                if (b < '0' || b > '7') throw new InvalidDataException("Invalid archive size.");
                result = checked(result * 8 + b - '0');
            }
            return result;
        }
    }
}
