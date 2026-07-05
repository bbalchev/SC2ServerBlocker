namespace SC2ServerBlocker.Firewall
{
    public interface IFirewallPolicyFactory
    {
        IFirewallPolicy CreatePolicy();
    }
}
