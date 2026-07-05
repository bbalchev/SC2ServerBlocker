using System.Collections.Generic;
using System.Linq;

namespace SC2ServerBlocker
{
    public static class BlockedRegionsFormatter
    {
        public static string FormatSummary(IEnumerable<string> blockedRegionNames)
        {
            var blockedNames = blockedRegionNames == null
                ? new List<string>()
                : blockedRegionNames.OrderBy(name => name).ToList();

            if (blockedNames.Count == 0)
            {
                return "Blocked regions: none";
            }

            return "Blocked regions: " + string.Join(", ", blockedNames);
        }
    }
}
