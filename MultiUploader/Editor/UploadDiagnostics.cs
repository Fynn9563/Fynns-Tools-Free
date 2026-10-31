using System.Text.RegularExpressions;

namespace FynnsTools.MultiUploader.Editor
{
    internal static class UploadDiagnostics
    {
        public static string Sanitize(string message)
        {
            string text = Regex.Replace(message ?? "", @"https?://[^\s""'<>]+", match =>
            {
                int query = match.Value.IndexOf('?');
                return query < 0 ? match.Value : match.Value.Substring(0, query) + "?[redacted]";
            }, RegexOptions.IgnoreCase);
            return Regex.Replace(text, @"(?im)\b(Authorization|Cookie|Set-Cookie)\s*:\s*[^\r\n]+", "$1: [redacted]");
        }
    }
}
