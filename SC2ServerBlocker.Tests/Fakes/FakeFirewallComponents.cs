using System;
using System.Collections.Generic;
using System.Linq;
using SC2ServerBlocker.Firewall;

namespace SC2ServerBlocker.Tests.Fakes
{
    internal sealed class FakeAdministratorChecker : IAdministratorChecker
    {
        public bool IsAdministrator { get; set; } = true;

        public bool IsRunningAsAdministrator()
        {
            return IsAdministrator;
        }
    }

    internal sealed class FakeFirewallPolicyFactory : IFirewallPolicyFactory
    {
        public FakeFirewallPolicy Policy { get; private set; }

        public bool ShouldThrowOnCreate { get; set; }

        public Exception CreateException { get; set; }

        public IFirewallPolicy CreatePolicy()
        {
            if (ShouldThrowOnCreate)
            {
                throw CreateException ?? new InvalidOperationException("Unable to create policy.");
            }

            if (Policy == null)
            {
                Policy = new FakeFirewallPolicy();
            }

            return Policy;
        }
    }

    internal sealed class FakeFirewallPolicy : IFirewallPolicy
    {
        public FakeFirewallPolicy()
        {
            Rules = new FakeFirewallRules();
            FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_DOMAIN] = true;
            FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_PRIVATE] = true;
            FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_PUBLIC] = true;
        }

        public int CurrentProfileTypes { get; set; } = NetFwInterop.NET_FW_PROFILE2_PUBLIC;

        public Dictionary<int, bool> FirewallEnabledByProfile { get; } = new Dictionary<int, bool>();

        public FakeFirewallRules Rules { get; private set; }

        IFirewallRules IFirewallPolicy.Rules
        {
            get { return Rules; }
        }

        public bool GetFirewallEnabled(int profileType)
        {
            bool enabled;
            return FirewallEnabledByProfile.TryGetValue(profileType, out enabled) && enabled;
        }
    }

    internal sealed class FakeFirewallRules : IFirewallRules
    {
        private readonly Dictionary<string, MutableFirewallRule> _rules =
            new Dictionary<string, MutableFirewallRule>(StringComparer.OrdinalIgnoreCase);

        private readonly List<string> _ruleOrder = new List<string>();

        public int Count
        {
            get { return _ruleOrder.Count; }
        }

        public IFirewallRule GetRule(string name)
        {
            MutableFirewallRule rule;
            if (!_rules.TryGetValue(name, out rule))
            {
                throw new KeyNotFoundException("Rule not found: " + name);
            }

            return CloneRule(rule);
        }

        public IEnumerable<string> EnumerateRuleNames()
        {
            return _ruleOrder.ToList();
        }

        public void Add(IFirewallRule rule)
        {
            if (FailNextAdd)
            {
                FailNextAdd = false;
                throw new InvalidOperationException("Simulated firewall add failure.");
            }

            var copy = CloneRule(rule);
            if (!_rules.ContainsKey(copy.Name))
            {
                _ruleOrder.Add(copy.Name);
            }

            _rules[copy.Name] = copy;
        }

        public void Remove(string name)
        {
            if (!_rules.ContainsKey(name))
            {
                throw new KeyNotFoundException("Rule not found: " + name);
            }

            _rules.Remove(name);
            _ruleOrder.RemoveAll(ruleName => string.Equals(ruleName, name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<MutableFirewallRule> GetAllRules()
        {
            return _ruleOrder.Select(name => _rules[name]).ToList();
        }

        public bool FailNextAdd { get; set; }

        private static MutableFirewallRule CloneRule(IFirewallRule rule)
        {
            return new MutableFirewallRule
            {
                Name = rule.Name,
                Description = rule.Description,
                Direction = rule.Direction,
                Action = rule.Action,
                Enabled = rule.Enabled,
                Profiles = rule.Profiles,
                RemoteAddresses = rule.RemoteAddresses
            };
        }
    }
}
