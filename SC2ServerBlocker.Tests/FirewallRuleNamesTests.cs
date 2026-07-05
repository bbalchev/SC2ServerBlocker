using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC2ServerBlocker.Firewall;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class FirewallRuleNamesTests
    {
        [TestMethod]
        public void GetRuleName_PrefixesRegionName()
        {
            var server = new Server("US East", new System.Collections.Generic.List<string>());

            Assert.AreEqual("ScUS East", FirewallRuleNames.GetRuleName(server));
            Assert.AreEqual("ScUS East", FirewallRuleNames.GetRuleName("US East"));
        }

        [TestMethod]
        public void IsManagedRuleName_MatchesExactRuleNameOnly()
        {
            Assert.IsTrue(FirewallRuleNames.IsManagedRuleName("ScUS East", "US East"));
            Assert.IsFalse(FirewallRuleNames.IsManagedRuleName("ScanSomething", "US East"));
            Assert.IsFalse(FirewallRuleNames.IsManagedRuleName("ScUS East", "Korea"));
            Assert.IsFalse(FirewallRuleNames.IsManagedRuleName("OtherRule", "US East"));
            Assert.IsFalse(FirewallRuleNames.IsManagedRuleName(null, "US East"));
        }
    }
}
