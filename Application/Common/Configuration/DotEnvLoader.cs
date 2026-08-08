using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Application.Common.Configuration
{
    public static class DotEnvLoader
    {
        private const string EnvironmentFileVariable = "PASTIL_ENV_FILE";
        private const string DefaultFileName = ".env";

        public static void Load()
        {
            var path = ResolvePath();
            if (path == null)
            {
                return;
            }

            foreach (var rawLine in File.ReadLines(path))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                {
                    line = line[7..].TrimStart();
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = line[..separatorIndex].Trim();
                if (!IsValidKey(key) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    continue;
                }

                var value = Unquote(line[(separatorIndex + 1)..].Trim());
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        private static string ResolvePath()
        {
            var explicitPath = Environment.GetEnvironmentVariable(EnvironmentFileVariable);
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                var fullPath = Path.GetFullPath(explicitPath);
                return File.Exists(fullPath) ? fullPath : null;
            }

            var roots = new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };

            return roots
                .SelectMany(GetCandidatePaths)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(File.Exists);
        }

        private static IEnumerable<string> GetCandidatePaths(string startPath)
        {
            var directory = new DirectoryInfo(startPath);
            for (var level = 0; directory != null && level < 8; level++)
            {
                yield return Path.Combine(directory.FullName, DefaultFileName);
                directory = directory.Parent;
            }
        }

        private static bool IsValidKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                !(char.IsLetter(key[0]) || key[0] == '_'))
            {
                return false;
            }

            return key.All(character =>
                char.IsLetterOrDigit(character) || character == '_');
        }

        private static string Unquote(string value)
        {
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
            {
                return value[1..^1];
            }

            return value;
        }
    }
}
