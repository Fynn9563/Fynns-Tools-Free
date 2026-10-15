using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FynnsTools.BuildSizes
{
    [Serializable]
    public sealed class BuildAssetSize
    {
        public string size;
        public double bytes;
        public double percent;
        public string path;
    }

    [Serializable]
    public sealed class SizeReport
    {
        public string context;
        public string compressedSize;
        public List<string> categories = new List<string>();
        public List<BuildAssetSize> assets = new List<BuildAssetSize>();
        public int startLine;
        public bool complete;
        public int malformedRows;
        public bool vrchat;
    }

    public sealed class LogResult
    {
        public SizeReport report;
        public string message;
        public bool warning;
    }

    public static class BuildSizeLog
    {
        public const int MaxLogBytes = 64 * 1024 * 1024;
        private static readonly Regex Row = new Regex(@"^\s*(?<size>\d+(?:[.,]\d+)?)\s*(?<unit>[kmgt]i?b|b)\s+(?<percent>\d+(?:[.,]\d+)?)\s*%\s+(?<path>.+?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
        private static readonly Regex Size = new Regex(@"^\d+(?:[.,]\d+)?\s*(?:[kmgt]i?b|b)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

        public static LogResult Read(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return Problem("Choose an Editor log file.");
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    long length = stream.Length;
                    int count = (int)Math.Min(length, MaxLogBytes);
                    bool tail = length > count;
                    stream.Seek(length - count, SeekOrigin.Begin);
                    var bytes = new byte[count];
                    int offset = 0;
                    while (offset < count)
                    {
                        int read = stream.Read(bytes, offset, count - offset);
                        if (read == 0) break;
                        offset += read;
                    }
                    using (var reader = new StreamReader(new MemoryStream(bytes, 0, offset), Encoding.UTF8, true))
                    {
                        if (tail) reader.ReadLine();
                        var result = Parse(reader);
                        if (tail) { result.warning = true; result.message += " Only the last 64 MB of this log were read; line numbers refer to that portion."; }
                        return result;
                    }
                }
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException || exception is NotSupportedException)
            { return Problem("Could not read the log: " + exception.Message); }
        }

        public static LogResult Parse(TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            SizeReport current = null;
            var completed = new List<SizeReport>();
            bool currentVrc = false;
            bool inAssets = false;
            bool inCategories = false;
            bool laterFailure = false;
            int failureLine = 0;
            string context = "Unity build report";
            int lineNumber = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                string trimmed = line.Trim();
                string lower = trimmed.ToLowerInvariant();
                if (inAssets && TryRow(line, out var asset)) { current.assets.Add(asset); continue; }
                bool failed = lower.Contains("build failed") || lower.Contains("error building player") ||
                    (lower.Contains("build completed") && lower.Contains("failed")) || lower.Contains("assetbundle build failed");
                bool marker = !inAssets && ((lower.Contains("scene-") && lower.Contains(".vrcw")) ||
                    (lower.Contains("avtr") && (lower.Contains(".prefab.unity3d") || lower.Contains(".vrca"))));
                if (failed)
                {
                    laterFailure = true;
                    failureLine = lineNumber;
                    if (current != null) completed.Remove(current);
                    current = null; inAssets = false; inCategories = false;
                    continue;
                }
                if (marker || lower.StartsWith("bundle name:") || trimmed.Equals("Build Report", StringComparison.OrdinalIgnoreCase) ||
                    (lower.StartsWith("uncompressed usage by category") && current != null && (current.complete || inAssets)))
                {
                    if (marker || lower.StartsWith("bundle name:")) { context = trimmed; currentVrc = marker; }
                    else if (current == null || current.complete) { context = "Unity build report"; currentVrc = false; }
                    current = new SizeReport { context = context, startLine = lineNumber, vrchat = currentVrc };
                    inAssets = false; inCategories = false;
                    if (marker || lower.StartsWith("bundle name:") || trimmed.Equals("Build Report", StringComparison.OrdinalIgnoreCase)) continue;
                }
                bool categories = lower.StartsWith("uncompressed usage by category");
                bool assets = lower.StartsWith("used assets and files from the resources folder") || lower.StartsWith("used assets, sorted by uncompressed size");
                bool compressed = lower.StartsWith("compressed size:") || lower.StartsWith("build compressed size:");
                if (categories || assets || compressed)
                {
                    if (current == null || (current.complete && (!compressed || !string.IsNullOrEmpty(current.compressedSize))))
                    { current = new SizeReport { context = context, startLine = lineNumber, vrchat = currentVrc }; }
                    if (compressed)
                    {
                        string value = trimmed.Substring(trimmed.IndexOf(':') + 1).Trim();
                        if (value.Length < 80 && Size.IsMatch(value)) current.compressedSize = value;
                    }
                    if (categories) { inCategories = true; inAssets = false; }
                    if (assets) { inAssets = true; inCategories = false; }
                    continue;
                }
                if (inAssets)
                {
                    bool separator = trimmed.Length >= 5 && trimmed.All(c => c == '-');
                    if (separator || lower.StartsWith("build completed") || lower.StartsWith("build succeeded"))
                    {
                        current.complete = true;
                        if (current.assets.Count > 0 || (currentVrc && current.malformedRows == 0))
                        {
                            completed.Add(current);
                            laterFailure = false;
                        }
                        inAssets = false;
                    }
                    else if (!string.IsNullOrWhiteSpace(trimmed)) current.malformedRows++;
                }
                else if (inCategories && trimmed.Length > 0)
                {
                    if ((trimmed.Contains("%") || lower.StartsWith("complete build size")) && trimmed.Length < 1000) current.categories.Add(trimmed);
                }
            }
            var latestComplete = completed.LastOrDefault(r => r.vrchat) ?? completed.LastOrDefault();
            if (latestComplete == null)
                return Problem(laterFailure ? "The build failed; no complete size report was found. Build successfully, then read the log again."
                    : current == null ? "No size report found. Build an avatar or world, then read its Editor log. You can also open a previous log."
                    : "The latest size report is incomplete. Wait for the build to finish, then read the log again.");
            laterFailure = failureLine > latestComplete.startLine;
            bool incomplete = current != null && !current.complete && (currentVrc || !latestComplete.vrchat);
            var result = new LogResult { report = latestComplete, message = "Loaded the latest complete size report (line " + latestComplete.startLine + ")." };
            if (incomplete || laterFailure)
            {
                result.warning = true;
                result.message += laterFailure ? " A later build failed; this is an earlier report." : " A later report is incomplete; this is an earlier report.";
            }
            if (latestComplete.malformedRows > 0)
            {
                result.warning = true;
                result.message += " Skipped " + latestComplete.malformedRows + " unrecognized asset rows.";
            }
            if (string.IsNullOrEmpty(latestComplete.compressedSize)) result.message += " Compressed size was not recorded.";
            return result;
        }

        public static bool TryRow(string line, out BuildAssetSize asset)
        {
            asset = null;
            if (string.IsNullOrEmpty(line) || line.Length > 32768) return false;
            var match = Row.Match(line);
            if (!match.Success || !Number(match.Groups["size"].Value, out double size) || !Number(match.Groups["percent"].Value, out double percent) || percent > 100) return false;
            string unit = match.Groups["unit"].Value.ToLowerInvariant();
            int power = "bkmgt".IndexOf(unit[0]);
            if (unit[0] == 'b') power = 0;
            asset = new BuildAssetSize { size = match.Groups["size"].Value + " " + match.Groups["unit"].Value,
                bytes = size * Math.Pow(1024, power), percent = percent, path = match.Groups["path"].Value.Replace('\\', '/') };
            return true;
        }

        private static bool Number(string text, out double value) => double.TryParse(text.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value) && !double.IsInfinity(value) && !double.IsNaN(value);
        private static LogResult Problem(string message) => new LogResult { message = message, warning = true };
    }
}
