using System;
using System.Collections.Generic;
using System.Linq;
using SC2ServerBlocker.Firewall;

namespace SC2ServerBlocker
{
    public class SC2FirewallManager : IFirewallManager
    {
        private readonly IFirewallPolicyFactory _policyFactory;
        private readonly FirewallEnvironmentValidator _environmentValidator;

        public SC2FirewallManager()
            : this(new WindowsFirewallPolicyFactory(), new WindowsAdministratorChecker())
        {
        }

        public SC2FirewallManager(
            IFirewallPolicyFactory policyFactory,
            IAdministratorChecker administratorChecker)
        {
            _policyFactory = policyFactory;
            _environmentValidator = new FirewallEnvironmentValidator(administratorChecker, policyFactory);
        }

        public string RuleNamePrefix
        {
            get { return FirewallRuleNames.Prefix; }
        }

        public StartupValidationResult ValidateEnvironment()
        {
            return _environmentValidator.Validate();
        }

        public void BlockServer(Server server)
        {
            var ruleName = FirewallRuleNames.GetRuleName(server);
            MutableFirewallRule previousRule;
            var hadPreviousRule = TryCopyExistingRule(ruleName, out previousRule);

            if (hadPreviousRule)
            {
                RemoveRuleIfExists(ruleName);
            }

            try
            {
                using (var scope = CreatePolicyScope())
                {
                    var rule = CreateBlockRule(server);
                    scope.Policy.Rules.Add(rule);
                }
            }
            catch (Exception ex)
            {
                if (hadPreviousRule && previousRule != null)
                {
                    RestoreRule(previousRule);
                }

                throw new FirewallOperationException(
                    "Failed to block " + server.Name + ". " + ex.Message, ex);
            }
        }

        public void UnblockServer(Server server)
        {
            RemoveRuleIfExists(FirewallRuleNames.GetRuleName(server));
        }

        public void UnblockAll(IEnumerable<Server> servers)
        {
            if (servers == null)
            {
                return;
            }

            try
            {
                using (var scope = CreatePolicyScope())
                {
                    foreach (var server in servers)
                    {
                        if (server == null)
                        {
                            continue;
                        }

                        var ruleName = FirewallRuleNames.GetRuleName(server);
                        if (RuleExists(scope.Policy, ruleName))
                        {
                            scope.Policy.Rules.Remove(ruleName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FirewallOperationException("Failed to unblock all regions. " + ex.Message, ex);
            }
        }

        public bool IsServerBlocked(Server server)
        {
            using (var scope = CreatePolicyScope())
            {
                return RuleExists(scope.Policy, FirewallRuleNames.GetRuleName(server));
            }
        }

        public BlockedRegionsQueryResult GetBlockedRegionNames(IEnumerable<Server> servers)
        {
            var blockedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (servers == null)
            {
                return BlockedRegionsQueryResult.Success(blockedNames);
            }

            var regionByRuleName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var server in servers)
            {
                if (server != null)
                {
                    regionByRuleName[FirewallRuleNames.GetRuleName(server)] = server.Name;
                }
            }

            if (regionByRuleName.Count == 0)
            {
                return BlockedRegionsQueryResult.Success(blockedNames);
            }

            try
            {
                using (var scope = CreatePolicyScope())
                {
                    foreach (var ruleName in scope.Policy.Rules.EnumerateRuleNames())
                    {
                        string regionName;
                        if (regionByRuleName.TryGetValue(ruleName, out regionName))
                        {
                            blockedNames.Add(regionName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BlockedRegionsQueryResult.Failure(
                    "Unable to read blocked regions from Windows Firewall. " + ex.Message);
            }

            return BlockedRegionsQueryResult.Success(blockedNames);
        }

        private static MutableFirewallRule CreateBlockRule(Server server)
        {
            return new MutableFirewallRule
            {
                Name = FirewallRuleNames.GetRuleName(server),
                Description = "StarCraft 2 server blocker outbound rule for " + server.Name,
                Direction = NetFwInterop.NET_FW_RULE_DIR_OUT,
                Action = NetFwInterop.NET_FW_ACTION_BLOCK,
                Enabled = true,
                Profiles = NetFwInterop.NET_FW_PROFILE2_ALL,
                RemoteAddresses = string.Join(",", server.IpAddressList)
            };
        }

        private PolicyScope CreatePolicyScope()
        {
            return new PolicyScope(_policyFactory.CreatePolicy());
        }

        private bool TryCopyExistingRule(string ruleName, out MutableFirewallRule rule)
        {
            rule = null;

            try
            {
                using (var scope = CreatePolicyScope())
                {
                    var existingRule = scope.Policy.Rules.GetRule(ruleName);
                    rule = new MutableFirewallRule
                    {
                        Name = existingRule.Name,
                        Description = existingRule.Description,
                        Direction = existingRule.Direction,
                        Action = existingRule.Action,
                        Enabled = existingRule.Enabled,
                        Profiles = existingRule.Profiles,
                        RemoteAddresses = existingRule.RemoteAddresses
                    };
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void RestoreRule(MutableFirewallRule rule)
        {
            try
            {
                using (var scope = CreatePolicyScope())
                {
                    scope.Policy.Rules.Add(rule);
                }
            }
            catch (Exception)
            {
            }
        }

        private static bool RuleExists(IFirewallPolicy policy, string ruleName)
        {
            try
            {
                policy.Rules.GetRule(ruleName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void RemoveRuleIfExists(string ruleName)
        {
            try
            {
                using (var scope = CreatePolicyScope())
                {
                    scope.Policy.Rules.Remove(ruleName);
                }
            }
            catch (Exception)
            {
            }
        }

        private sealed class PolicyScope : IDisposable
        {
            public PolicyScope(IFirewallPolicy policy)
            {
                Policy = policy;
            }

            public IFirewallPolicy Policy { get; private set; }

            public void Dispose()
            {
                Policy = null;
            }
        }
    }
}
