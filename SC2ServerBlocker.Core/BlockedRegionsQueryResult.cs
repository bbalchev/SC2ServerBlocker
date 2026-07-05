using System;
using System.Collections.Generic;

namespace SC2ServerBlocker
{
    public sealed class BlockedRegionsQueryResult
    {
        private BlockedRegionsQueryResult(bool succeeded, HashSet<string> blockedRegionNames, string errorMessage)
        {
            Succeeded = succeeded;
            BlockedRegionNames = blockedRegionNames ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; private set; }

        public HashSet<string> BlockedRegionNames { get; private set; }

        public string ErrorMessage { get; private set; }

        public static BlockedRegionsQueryResult Success(HashSet<string> blockedRegionNames)
        {
            return new BlockedRegionsQueryResult(true, blockedRegionNames, null);
        }

        public static BlockedRegionsQueryResult Failure(string errorMessage)
        {
            return new BlockedRegionsQueryResult(false, null, errorMessage);
        }
    }
}
