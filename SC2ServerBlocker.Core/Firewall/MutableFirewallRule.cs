namespace SC2ServerBlocker.Firewall
{
    public sealed class MutableFirewallRule : IFirewallRule
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int Direction { get; set; }

        public int Action { get; set; }

        public bool Enabled { get; set; }

        public int Profiles { get; set; }

        public string RemoteAddresses { get; set; }
    }
}
