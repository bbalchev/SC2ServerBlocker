using System;
using System.IO;

namespace SC2ServerBlocker
{
    public static class RegionPathValidator
    {
        public static bool IsValidRegionName(string regionName)
        {
            if (String.IsNullOrWhiteSpace(regionName))
            {
                return false;
            }

            if (regionName.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0)
            {
                return false;
            }

            if (String.Equals(regionName, ".", StringComparison.Ordinal) ||
                String.Equals(regionName, "..", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public static void ValidateRegionName(string regionName)
        {
            if (!IsValidRegionName(regionName))
            {
                throw new ArgumentException("Invalid region name.", "regionName");
            }
        }

        public static string ResolveIniFilePath(string serversDirectory, string regionName)
        {
            ValidateRegionName(regionName);

            var serversRoot = Path.GetFullPath(serversDirectory);
            var iniFilePath = Path.GetFullPath(Path.Combine(serversRoot, regionName + ".ini"));
            var serversRootWithSeparator = serversRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!iniFilePath.StartsWith(serversRootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Region name resolves outside the servers directory.", "regionName");
            }

            return iniFilePath;
        }
    }
}
