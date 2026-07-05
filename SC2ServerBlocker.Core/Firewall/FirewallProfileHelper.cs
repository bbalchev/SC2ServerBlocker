using System.Collections.Generic;

namespace SC2ServerBlocker.Firewall
{
    public static class FirewallProfileHelper
    {
        public static IEnumerable<int> GetActiveProfiles(int activeProfiles)
        {
            if ((activeProfiles & NetFwInterop.NET_FW_PROFILE2_DOMAIN) != 0)
            {
                yield return NetFwInterop.NET_FW_PROFILE2_DOMAIN;
            }

            if ((activeProfiles & NetFwInterop.NET_FW_PROFILE2_PRIVATE) != 0)
            {
                yield return NetFwInterop.NET_FW_PROFILE2_PRIVATE;
            }

            if ((activeProfiles & NetFwInterop.NET_FW_PROFILE2_PUBLIC) != 0)
            {
                yield return NetFwInterop.NET_FW_PROFILE2_PUBLIC;
            }
        }

        public static bool IsFirewallEnabledForActiveProfiles(IFirewallPolicy policy)
        {
            bool enabled;
            return TryIsFirewallEnabledForActiveProfiles(policy, out enabled) && enabled;
        }

        public static bool TryIsFirewallEnabledForActiveProfiles(IFirewallPolicy policy, out bool isEnabled)
        {
            isEnabled = true;

            if (policy == null)
            {
                return false;
            }

            try
            {
                var activeProfiles = policy.CurrentProfileTypes;

                if (activeProfiles == 0)
                {
                    isEnabled = policy.GetFirewallEnabled(NetFwInterop.NET_FW_PROFILE2_PUBLIC);
                    return true;
                }

                foreach (var profile in GetActiveProfiles(activeProfiles))
                {
                    if (!policy.GetFirewallEnabled(profile))
                    {
                        isEnabled = false;
                        return true;
                    }
                }

                isEnabled = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
