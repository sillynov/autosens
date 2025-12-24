using System;
using System.IO;

namespace autosens
{
    public static class ConfigSearcher
    {
        public static string FindConfigPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string[] pathParts = path.Split(new string[] { "[UNKNOWN]" }, StringSplitOptions.None);

            if (pathParts.Length < 2)
            {
                return path;
            }

            string part1 = pathParts[0];
            string part2 = pathParts[1];

            string baseDir = Path.GetDirectoryName(part1);
            if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
            {
                return path;
            }

            foreach (string dir in Directory.GetDirectories(baseDir))
            {
                string cleanedPart2 = part2.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string possiblePath = Path.Combine(dir, cleanedPart2);
                if (File.Exists(possiblePath))
                {
                    return possiblePath;
                }
            }

            return path;
        }
    }
}
