using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC2ServerBlocker.Firewall;
using SC2ServerBlocker.Tests.Fakes;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class SC2FirewallManagerTests
    {
        [TestMethod]
        public void BlockServer_AddsOutboundBlockRule()
        {
            var factory = new FakeFirewallPolicyFactory();
            var manager = CreateManager(factory);
            var server = CreateServer("US East", "1.2.3.4", "5.6.7.8");

            manager.BlockServer(server);

            var rule = factory.Policy.Rules.GetAllRules().Single();
            Assert.AreEqual("ScUS East", rule.Name);
            Assert.AreEqual(NetFwInterop.NET_FW_RULE_DIR_OUT, rule.Direction);
            Assert.AreEqual(NetFwInterop.NET_FW_ACTION_BLOCK, rule.Action);
            Assert.AreEqual("1.2.3.4,5.6.7.8", rule.RemoteAddresses);
        }

        [TestMethod]
        public void BlockServer_ReplacesExistingRule()
        {
            var factory = new FakeFirewallPolicyFactory();
            var manager = CreateManager(factory);
            var server = CreateServer("US East", "1.2.3.4");

            manager.BlockServer(server);
            server.ReplaceIpAddresses(new[] { "9.9.9.9" });
            manager.BlockServer(server);

            Assert.AreEqual(1, factory.Policy.Rules.Count);
            Assert.AreEqual("9.9.9.9", factory.Policy.Rules.GetAllRules().Single().RemoteAddresses);
        }

        [TestMethod]
        public void BlockServer_RestoresPreviousRule_WhenAddFails()
        {
            var factory = new FakeFirewallPolicyFactory();
            var manager = CreateManager(factory);
            var server = CreateServer("US East", "1.2.3.4");

            manager.BlockServer(server);
            server.ReplaceIpAddresses(new[] { "9.9.9.9" });
            factory.Policy.Rules.FailNextAdd = true;

            try
            {
                manager.BlockServer(server);
                Assert.Fail("Expected FirewallOperationException.");
            }
            catch (FirewallOperationException)
            {
            }

            Assert.AreEqual(1, factory.Policy.Rules.Count);
            Assert.AreEqual("1.2.3.4", factory.Policy.Rules.GetAllRules().Single().RemoteAddresses);
        }

        [TestMethod]
        public void UnblockServer_RemovesRule()
        {
            var factory = new FakeFirewallPolicyFactory();
            var manager = CreateManager(factory);
            var server = CreateServer("US East", "1.2.3.4");

            manager.BlockServer(server);
            manager.UnblockServer(server);

            Assert.AreEqual(0, factory.Policy.Rules.Count);
            Assert.IsFalse(manager.IsServerBlocked(server));
        }

        [TestMethod]
        public void UnblockAll_RemovesOnlyKnownRegionRules()
        {
            var factory = new FakeFirewallPolicyFactory();
            factory.CreatePolicy();
            factory.Policy.Rules.Add(new MutableFirewallRule { Name = "ScUS East", RemoteAddresses = "1.1.1.1" });
            factory.Policy.Rules.Add(new MutableFirewallRule { Name = "ScanSomething", RemoteAddresses = "2.2.2.2" });
            factory.Policy.Rules.Add(new MutableFirewallRule { Name = "OtherRule", RemoteAddresses = "3.3.3.3" });

            var manager = CreateManager(factory);
            manager.UnblockAll(new[] { CreateServer("US East", "1.1.1.1") });

            var remaining = factory.Policy.Rules.GetAllRules().Select(rule => rule.Name).ToList();
            CollectionAssert.AreEquivalent(new[] { "ScanSomething", "OtherRule" }, remaining);
        }

        [TestMethod]
        public void GetBlockedRegionNames_ReturnsBlockedServers()
        {
            var factory = new FakeFirewallPolicyFactory();
            var manager = CreateManager(factory);
            var servers = new List<Server>
            {
                CreateServer("US East", "1.2.3.4"),
                CreateServer("Korea", "4.5.6.7")
            };

            manager.BlockServer(servers[0]);

            var result = manager.GetBlockedRegionNames(servers);

            Assert.IsTrue(result.Succeeded);
            CollectionAssert.AreEquivalent(new[] { "US East" }, result.BlockedRegionNames.ToArray());
        }

        [TestMethod]
        public void GetBlockedRegionNames_ReturnsFailure_WhenPolicyUnavailable()
        {
            var factory = new FakeFirewallPolicyFactory { ShouldThrowOnCreate = true };
            var manager = CreateManager(factory);

            var result = manager.GetBlockedRegionNames(new[] { CreateServer("US East", "1.2.3.4") });

            Assert.IsFalse(result.Succeeded);
            StringAssert.Contains(result.ErrorMessage, "Unable to read blocked regions");
        }

        [TestMethod]
        public void ValidateEnvironment_UsesInjectedDependencies()
        {
            var factory = new FakeFirewallPolicyFactory { ShouldThrowOnCreate = true };
            var manager = CreateManager(factory, isAdministrator: false);

            var result = manager.ValidateEnvironment();

            Assert.AreEqual(StartupValidationSeverity.Error, result.Severity);
        }

        private static SC2FirewallManager CreateManager(
            FakeFirewallPolicyFactory factory,
            bool isAdministrator = true)
        {
            return new SC2FirewallManager(
                factory,
                new FakeAdministratorChecker { IsAdministrator = isAdministrator });
        }

        private static Server CreateServer(string name, params string[] ips)
        {
            return new Server(name, new List<string>(ips));
        }
    }
}
