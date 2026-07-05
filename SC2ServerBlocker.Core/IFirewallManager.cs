using System.Collections.Generic;

namespace SC2ServerBlocker
{
    public interface IFirewallManager
    {
        string RuleNamePrefix { get; }

        StartupValidationResult ValidateEnvironment();

        void BlockServer(Server server);

        void UnblockServer(Server server);

        void UnblockAll(IEnumerable<Server> servers);

        bool IsServerBlocked(Server server);

        BlockedRegionsQueryResult GetBlockedRegionNames(IEnumerable<Server> servers);
    }
}
