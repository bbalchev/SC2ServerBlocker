namespace SC2ServerBlocker.Firewall
{
    public static class FirewallRuleNames
    {
        public const string Prefix = "Sc";

        public static string GetRuleName(Server server)
        {
            return GetRuleName(server.Name);
        }

        public static string GetRuleName(string regionName)
        {
            return Prefix + regionName;
        }

        public static bool IsManagedRuleName(string ruleName, string regionName)
        {
            return ruleName != null &&
                   regionName != null &&
                   string.Equals(ruleName, GetRuleName(regionName), System.StringComparison.Ordinal);
        }
    }
}
