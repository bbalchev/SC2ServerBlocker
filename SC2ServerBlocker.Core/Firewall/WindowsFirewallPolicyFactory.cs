using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NetFwTypeLib;

namespace SC2ServerBlocker.Firewall
{
    public sealed class WindowsFirewallPolicyFactory : IFirewallPolicyFactory
    {
        public IFirewallPolicy CreatePolicy()
        {
            try
            {
                return new ComFirewallPolicy(NetFwComFactory.CreatePolicy());
            }
            catch (Exception ex) when (ex is COMException || ex is TypeLoadException)
            {
                throw new InvalidOperationException("Unable to create Windows Firewall policy.", ex);
            }
        }
    }

    internal sealed class ComFirewallPolicy : IFirewallPolicy
    {
        private readonly INetFwPolicy2 _policy;

        public ComFirewallPolicy(INetFwPolicy2 policy)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Rules = new ComFirewallRules(_policy.Rules);
        }

        public int CurrentProfileTypes
        {
            get { return _policy.CurrentProfileTypes; }
        }

        public bool GetFirewallEnabled(int profileType)
        {
            return _policy.get_FirewallEnabled((NET_FW_PROFILE_TYPE2_)profileType);
        }

        public IFirewallRules Rules { get; private set; }
    }

    internal sealed class ComFirewallRules : IFirewallRules
    {
        private readonly INetFwRules _rules;

        public ComFirewallRules(INetFwRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public int Count
        {
            get { return _rules.Count; }
        }

        public IFirewallRule GetRule(string name)
        {
            return new ComFirewallRule(_rules.Item(name));
        }

        public IEnumerable<string> EnumerateRuleNames()
        {
            foreach (INetFwRule rule in _rules)
            {
                if (rule?.Name != null)
                {
                    yield return rule.Name;
                }
            }
        }

        public void Add(IFirewallRule rule)
        {
            var comRule = NetFwComFactory.CreateRule();
            comRule.Name = rule.Name;
            comRule.Description = rule.Description;
            comRule.Direction = (NET_FW_RULE_DIRECTION_)rule.Direction;
            comRule.Action = (NET_FW_ACTION_)rule.Action;
            comRule.Enabled = rule.Enabled;
            comRule.Profiles = rule.Profiles;
            comRule.RemoteAddresses = rule.RemoteAddresses;
            _rules.Add(comRule);
        }

        public void Remove(string name)
        {
            _rules.Remove(name);
        }
    }

    internal sealed class ComFirewallRule : IFirewallRule
    {
        private readonly INetFwRule _rule;

        public ComFirewallRule(INetFwRule rule)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public string Name
        {
            get { return _rule.Name; }
            set { _rule.Name = value; }
        }

        public string Description
        {
            get { return _rule.Description; }
            set { _rule.Description = value; }
        }

        public int Direction
        {
            get { return (int)_rule.Direction; }
            set { _rule.Direction = (NET_FW_RULE_DIRECTION_)value; }
        }

        public int Action
        {
            get { return (int)_rule.Action; }
            set { _rule.Action = (NET_FW_ACTION_)value; }
        }

        public bool Enabled
        {
            get { return _rule.Enabled; }
            set { _rule.Enabled = value; }
        }

        public int Profiles
        {
            get { return _rule.Profiles; }
            set { _rule.Profiles = value; }
        }

        public string RemoteAddresses
        {
            get { return _rule.RemoteAddresses; }
            set { _rule.RemoteAddresses = value; }
        }
    }
}
