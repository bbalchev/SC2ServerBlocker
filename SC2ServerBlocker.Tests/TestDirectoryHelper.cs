using System;
using System.Collections.Generic;
using System.IO;

namespace SC2ServerBlocker.Tests
{
    internal static class TestDirectoryHelper
    {
        public static string CreateServersDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "SC2ServerBlockerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        public static void WriteIniFile(string directory, string regionName, params string[] ipLines)
        {
            var lines = new List<string>
            {
                "; test file",
                ""
            };
            lines.AddRange(ipLines);

            File.WriteAllLines(Path.Combine(directory, regionName + ".ini"), lines);
        }
    }
}
