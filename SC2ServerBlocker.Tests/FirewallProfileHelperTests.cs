using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC2ServerBlocker.Firewall;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class FirewallProfileHelperTests
    {
        [TestMethod]
        public void GetActiveProfiles_ReturnsSetBits()
        {
            var profiles = FirewallProfileHelper.GetActiveProfiles(
                NetFwInterop.NET_FW_PROFILE2_PRIVATE | NetFwInterop.NET_FW_PROFILE2_PUBLIC).ToList();

            CollectionAssert.AreEquivalent(
                new[]
                {
                    NetFwInterop.NET_FW_PROFILE2_PRIVATE,
                    NetFwInterop.NET_FW_PROFILE2_PUBLIC
                },
                profiles);
        }

        [TestMethod]
        public void IsFirewallEnabledForActiveProfiles_UsesPublicProfile_WhenNoActiveProfiles()
        {
            var policy = new Fakes.FakeFirewallPolicy
            {
                CurrentProfileTypes = 0
            };
            policy.FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_PUBLIC] = false;

            Assert.IsFalse(FirewallProfileHelper.IsFirewallEnabledForActiveProfiles(policy));
        }

        [TestMethod]
        public void IsFirewallEnabledForActiveProfiles_RequiresAllActiveProfilesEnabled()
        {
            var policy = new Fakes.FakeFirewallPolicy
            {
                CurrentProfileTypes = NetFwInterop.NET_FW_PROFILE2_PRIVATE | NetFwInterop.NET_FW_PROFILE2_PUBLIC
            };
            policy.FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_PRIVATE] = true;
            policy.FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_PUBLIC] = false;

            Assert.IsFalse(FirewallProfileHelper.IsFirewallEnabledForActiveProfiles(policy));
        }
    }
}
