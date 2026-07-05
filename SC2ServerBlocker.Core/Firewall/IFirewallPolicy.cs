using System.Collections.Generic;

namespace SC2ServerBlocker.Firewall
{
    public interface IFirewallPolicy
    {
        int CurrentProfileTypes { get; }

        bool GetFirewallEnabled(int profileType);

        IFirewallRules Rules { get; }
    }

    public interface IFirewallRules
    {
        int Count { get; }

        IFirewallRule GetRule(string name);

        IEnumerable<string> EnumerateRuleNames();

        void Add(IFirewallRule rule);

        void Remove(string name);
    }

    public interface IFirewallRule
    {
        string Name { get; set; }

        string Description { get; set; }

        int Direction { get; set; }

        int Action { get; set; }

        bool Enabled { get; set; }

        int Profiles { get; set; }

        string RemoteAddresses { get; set; }
    }
}
