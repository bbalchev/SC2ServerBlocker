using System;
using System.Collections.Generic;
using System.Linq;

namespace SC2ServerBlocker
{
    public sealed class RegionActionStates
    {
        public bool CanBlock { get; set; }

        public bool CanUnblock { get; set; }

        public bool CanUnblockAll { get; set; }

        public bool CanEditIps { get; set; }
    }

    public sealed class OperationResult
    {
        private OperationResult(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Message = message;
        }

        public bool Succeeded { get; private set; }

        public string Message { get; private set; }

        public static OperationResult Success(string message)
        {
            return new OperationResult(true, message);
        }

        public static OperationResult Failure(string message)
        {
            return new OperationResult(false, message);
        }
    }

    public class RegionBlockingService
    {
        private readonly IFirewallManager _firewallManager;
        private readonly ServerRepository _serverRepository;

        public RegionBlockingService(IFirewallManager firewallManager, ServerRepository serverRepository)
        {
            _firewallManager = firewallManager;
            _serverRepository = serverRepository;
        }

        public StartupValidationResult ValidateEnvironment()
        {
            return _firewallManager.ValidateEnvironment();
        }

        public List<Server> LoadServers()
        {
            return _serverRepository.GetServers();
        }

        public string GetServersDirectory()
        {
            return _serverRepository.ServersDirectory;
        }

        public BlockedRegionsQueryResult RefreshBlockedState(IEnumerable<Server> servers)
        {
            var serverList = servers == null ? new List<Server>() : servers.ToList();
            var query = _firewallManager.GetBlockedRegionNames(serverList);
            if (query.Succeeded)
            {
                ApplyBlockedState(serverList, query.BlockedRegionNames);
            }

            return query;
        }

        public BlockedRegionsQueryResult QueryBlockedRegionNames(IEnumerable<Server> servers)
        {
            var serverList = servers == null ? new List<Server>() : servers.ToList();
            return _firewallManager.GetBlockedRegionNames(serverList);
        }

        public void ApplyBlockedState(IEnumerable<Server> servers, HashSet<string> blockedNames)
        {
            var blocked = blockedNames ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var server in servers ?? Enumerable.Empty<Server>())
            {
                if (server != null)
                {
                    server.IsBlocked = blocked.Contains(server.Name);
                }
            }
        }

        public RegionActionStates GetActionStates(
            Server selectedServer,
            IEnumerable<Server> servers,
            StartupValidationResult validationResult)
        {
            var serverList = servers == null ? new List<Server>() : servers.ToList();
            var canModifyFirewall = validationResult != null && validationResult.AllowsBlocking;

            return new RegionActionStates
            {
                CanBlock = canModifyFirewall && selectedServer != null && !selectedServer.IsBlocked,
                CanUnblock = canModifyFirewall && selectedServer != null && selectedServer.IsBlocked,
                CanUnblockAll = canModifyFirewall && serverList.Any(server => server.IsBlocked),
                CanEditIps = selectedServer != null
            };
        }

        public OperationResult BlockSelectedServer(Server server)
        {
            if (server == null)
            {
                return OperationResult.Failure("No region selected.");
            }

            try
            {
                var serverToBlock = _serverRepository.LoadServerForBlock(server.Name) ?? server;
                server.ReplaceIpAddresses(serverToBlock.IpAddressList);
                _firewallManager.BlockServer(serverToBlock);

                return OperationResult.Success(
                    string.Format("Blocked {0} ({1} IP addresses).", server.Name, serverToBlock.IpCount));
            }
            catch (FirewallOperationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult UnblockSelectedServer(Server server)
        {
            if (server == null)
            {
                return OperationResult.Failure("No region selected.");
            }

            try
            {
                _firewallManager.UnblockServer(server);

                return OperationResult.Success(string.Format("Unblocked {0}.", server.Name));
            }
            catch (FirewallOperationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult UnblockAllRegions(IEnumerable<Server> servers)
        {
            var serverList = servers == null ? new List<Server>() : servers.ToList();

            if (!serverList.Any(server => server.IsBlocked))
            {
                return OperationResult.Success("No regions are currently blocked.");
            }

            try
            {
                _firewallManager.UnblockAll(serverList);
                return OperationResult.Success("All regions unblocked.");
            }
            catch (FirewallOperationException ex)
            {
                return OperationResult.Failure(ex.Message);
            }
        }

        public OperationResult SaveServerAddresses(Server server, IEnumerable<string> ipAddresses)
        {
            if (server == null)
            {
                return OperationResult.Failure("No region selected.");
            }

            return SaveServerAddressesForRegion(server.Name, new[] { server }, ipAddresses);
        }

        public OperationResult SaveServerAddressesForRegion(
            string regionName,
            IEnumerable<Server> servers,
            IEnumerable<string> ipAddresses)
        {
            var server = FindServer(servers, regionName);
            if (server == null)
            {
                return OperationResult.Failure("Region '" + regionName + "' was not found.");
            }

            return SaveServerAddressesInternal(server, ipAddresses);
        }

        public static Server FindServer(IEnumerable<Server> servers, string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName) || servers == null)
            {
                return null;
            }

            return servers.FirstOrDefault(server =>
                server != null &&
                string.Equals(server.Name, regionName, StringComparison.OrdinalIgnoreCase));
        }

        private OperationResult SaveServerAddressesInternal(Server server, IEnumerable<string> ipAddresses)
        {
            List<string> addresses;
            string validationError;
            if (!IpAddressParser.TryNormalizeAddresses(ipAddresses, out addresses, out validationError))
            {
                return OperationResult.Failure(validationError);
            }

            if (addresses.Count == 0)
            {
                return OperationResult.Failure("Add at least one IP address before saving.");
            }

            var previousAddresses = server.IpAddressList.ToList();
            bool hasFirewallRule;
            try
            {
                hasFirewallRule = _firewallManager.IsServerBlocked(server);
            }
            catch (Exception ex)
            {
                return OperationResult.Failure("Unable to check firewall state. " + ex.Message);
            }

            try
            {
                _serverRepository.SaveServerAddresses(server.Name, addresses);
            }
            catch (Exception ex)
            {
                return OperationResult.Failure("Failed to save server addresses. " + ex.Message);
            }

            if (hasFirewallRule)
            {
                try
                {
                    _firewallManager.BlockServer(new Server(server.Name, new List<string>(addresses)));
                }
                catch (FirewallOperationException ex)
                {
                    try
                    {
                        _serverRepository.SaveServerAddresses(server.Name, previousAddresses);
                    }
                    catch (Exception rollbackEx)
                    {
                        return OperationResult.Failure(
                            "Firewall update failed and restoring the previous IP list also failed. "
                            + ex.Message
                            + " Restore error: "
                            + rollbackEx.Message);
                    }

                    return OperationResult.Failure(
                        "Saved IP addresses to file, but updating the firewall rule failed. "
                        + "The previous IP list was restored. "
                        + ex.Message);
                }
            }

            server.ReplaceIpAddresses(addresses);

            if (!hasFirewallRule)
            {
                return OperationResult.Success(
                    string.Format("Saved {0} IP address(es) for {1}.", server.IpCount, server.Name));
            }

            return OperationResult.Success(
                string.Format(
                    "Updated block rule for {0} ({1} IP addresses).",
                    server.Name,
                    server.IpCount));
        }
    }
}

