using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SC2ServerBlocker.Tests.Fakes;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class RegionBlockingServiceTests
    {
        [TestMethod]
        public void RefreshBlockedState_UpdatesServerFlags()
        {
            var firewall = new Mock<IFirewallManager>();
            var repository = new ServerRepository(TestDirectoryHelper.CreateServersDirectory());
            var service = new RegionBlockingService(firewall.Object, repository);
            var servers = new List<Server>
            {
                new Server("US East", new List<string> { "1.2.3.4" }),
                new Server("Korea", new List<string> { "5.6.7.8" })
            };

            firewall.Setup(f => f.GetBlockedRegionNames(servers))
                .Returns(BlockedRegionsQueryResult.Success(new HashSet<string> { "US East" }));

            var query = service.RefreshBlockedState(servers);

            Assert.IsTrue(query.Succeeded);
            Assert.IsTrue(servers[0].IsBlocked);
            Assert.IsFalse(servers[1].IsBlocked);
            CollectionAssert.AreEquivalent(new[] { "US East" }, query.BlockedRegionNames.ToArray());
        }

        [TestMethod]
        public void RefreshBlockedState_PreservesFlags_WhenQueryFails()
        {
            var firewall = new Mock<IFirewallManager>();
            var repository = new ServerRepository(TestDirectoryHelper.CreateServersDirectory());
            var service = new RegionBlockingService(firewall.Object, repository);
            var servers = new List<Server>
            {
                new Server("US East", new List<string> { "1.2.3.4" }) { IsBlocked = true },
                new Server("Korea", new List<string> { "5.6.7.8" })
            };

            firewall.Setup(f => f.GetBlockedRegionNames(servers))
                .Returns(BlockedRegionsQueryResult.Failure("Firewall unavailable."));

            var query = service.RefreshBlockedState(servers);

            Assert.IsFalse(query.Succeeded);
            Assert.IsTrue(servers[0].IsBlocked);
            Assert.IsFalse(servers[1].IsBlocked);
        }

        [TestMethod]
        public void GetActionStates_RespectsValidationAndSelection()
        {
            var firewall = new Mock<IFirewallManager>();
            var repository = new ServerRepository(TestDirectoryHelper.CreateServersDirectory());
            var service = new RegionBlockingService(firewall.Object, repository);
            var server = new Server("US East", new List<string> { "1.2.3.4" }) { IsBlocked = false };

            var allowed = service.GetActionStates(server, new[] { server }, StartupValidationResult.Ok());
            var denied = service.GetActionStates(server, new[] { server }, StartupValidationResult.Error("nope"));

            Assert.IsTrue(allowed.CanBlock);
            Assert.IsFalse(allowed.CanUnblock);
            Assert.IsFalse(denied.CanBlock);
            Assert.IsTrue(allowed.CanEditIps);
        }

        [TestMethod]
        public void BlockSelectedServer_UsesRepositoryAndFirewall()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            TestDirectoryHelper.WriteIniFile(directory, "US East", "9.9.9.9");

            var factory = new FakeFirewallPolicyFactory();
            var firewall = new SC2FirewallManager(factory, new FakeAdministratorChecker());
            var service = new RegionBlockingService(firewall, new ServerRepository(directory));
            var server = new Server("US East", new List<string> { "1.2.3.4" });

            var result = service.BlockSelectedServer(server);

            Assert.IsTrue(result.Succeeded);
            StringAssert.Contains(result.Message, "Blocked US East");
            Assert.AreEqual("9.9.9.9", server.IpAddressList.Single());
            Assert.AreEqual("9.9.9.9", factory.Policy.Rules.GetAllRules().Single().RemoteAddresses);
        }

        [TestMethod]
        public void SaveServerAddresses_UpdatesBlockedRule_WhenRegionBlocked()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            var factory = new FakeFirewallPolicyFactory();
            var firewall = new SC2FirewallManager(factory, new FakeAdministratorChecker());
            var service = new RegionBlockingService(firewall, new ServerRepository(directory));
            var server = new Server("US East", new List<string> { "1.2.3.4" }) { IsBlocked = true };

            factory.CreatePolicy();
            factory.Policy.Rules.Add(new SC2ServerBlocker.Firewall.MutableFirewallRule
            {
                Name = "ScUS East",
                RemoteAddresses = "1.2.3.4"
            });

            var result = service.SaveServerAddresses(server, new[] { "8.8.8.8" });

            Assert.IsTrue(result.Succeeded);
            StringAssert.Contains(result.Message, "Updated block rule");
            Assert.AreEqual("8.8.8.8", factory.Policy.Rules.GetAllRules().Single().RemoteAddresses);
            Assert.AreEqual("8.8.8.8", server.IpAddressList.Single());
        }

        [TestMethod]
        public void SaveServerAddresses_DoesNotWriteIni_WhenBlockedFirewallUpdateFails()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            TestDirectoryHelper.WriteIniFile(directory, "US East", "1.2.3.4");

            var factory = new FakeFirewallPolicyFactory();
            var firewall = new SC2FirewallManager(factory, new FakeAdministratorChecker());
            var service = new RegionBlockingService(firewall, new ServerRepository(directory));
            var server = new Server("US East", new List<string> { "1.2.3.4" }) { IsBlocked = true };

            factory.CreatePolicy();
            factory.Policy.Rules.Add(new SC2ServerBlocker.Firewall.MutableFirewallRule
            {
                Name = "ScUS East",
                RemoteAddresses = "1.2.3.4"
            });
            factory.Policy.Rules.FailNextAdd = true;

            var result = service.SaveServerAddresses(server, new[] { "8.8.8.8" });

            Assert.IsFalse(result.Succeeded);
            CollectionAssert.AreEqual(new[] { "1.2.3.4" }, server.IpAddressList);
            var reloaded = new ServerRepository(directory).LoadServer("US East");
            CollectionAssert.AreEqual(new[] { "1.2.3.4" }, reloaded.IpAddressList);
        }

        [TestMethod]
        public void SaveServerAddresses_UpdatesFirewall_WhenRuleExistsButFlagIsFalse()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            var factory = new FakeFirewallPolicyFactory();
            var firewall = new SC2FirewallManager(factory, new FakeAdministratorChecker());
            var service = new RegionBlockingService(firewall, new ServerRepository(directory));
            var server = new Server("US East", new List<string> { "1.2.3.4" }) { IsBlocked = false };

            factory.CreatePolicy();
            factory.Policy.Rules.Add(new SC2ServerBlocker.Firewall.MutableFirewallRule
            {
                Name = "ScUS East",
                RemoteAddresses = "1.2.3.4"
            });

            var result = service.SaveServerAddresses(server, new[] { "8.8.8.8" });

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("8.8.8.8", factory.Policy.Rules.GetAllRules().Single().RemoteAddresses);
            Assert.AreEqual("8.8.8.8", server.IpAddressList.Single());
        }

        [TestMethod]
        public void SaveServerAddressesForRegion_UpdatesNamedRegion_NotMainSelection()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            TestDirectoryHelper.WriteIniFile(directory, "US East", "1.2.3.4");
            TestDirectoryHelper.WriteIniFile(directory, "Korea", "5.6.7.8");

            var factory = new FakeFirewallPolicyFactory();
            var firewall = new SC2FirewallManager(factory, new FakeAdministratorChecker());
            var service = new RegionBlockingService(firewall, new ServerRepository(directory));
            var servers = new List<Server>
            {
                new Server("US East", new List<string> { "1.2.3.4" }),
                new Server("Korea", new List<string> { "5.6.7.8" }) { IsBlocked = true }
            };

            factory.CreatePolicy();
            factory.Policy.Rules.Add(new SC2ServerBlocker.Firewall.MutableFirewallRule
            {
                Name = "ScKorea",
                RemoteAddresses = "5.6.7.8"
            });

            var result = service.SaveServerAddressesForRegion(
                "Korea",
                servers,
                new[] { "9.9.9.9" });

            Assert.IsTrue(result.Succeeded);
            StringAssert.Contains(result.Message, "Korea");
            CollectionAssert.AreEqual(new[] { "1.2.3.4" }, servers[0].IpAddressList);
            CollectionAssert.AreEqual(new[] { "9.9.9.9" }, servers[1].IpAddressList);
            Assert.AreEqual("9.9.9.9", factory.Policy.Rules.GetAllRules().Single().RemoteAddresses);

            var koreaIni = new ServerRepository(directory).LoadServer("Korea");
            CollectionAssert.AreEqual(new[] { "9.9.9.9" }, koreaIni.IpAddressList);

            var usEastIni = new ServerRepository(directory).LoadServer("US East");
            CollectionAssert.AreEqual(new[] { "1.2.3.4" }, usEastIni.IpAddressList);
        }

        [TestMethod]
        public void FindServer_MatchesRegionCaseInsensitively()
        {
            var servers = new[]
            {
                new Server("US East", new List<string> { "1.2.3.4" })
            };

            var server = RegionBlockingService.FindServer(servers, "us east");

            Assert.IsNotNull(server);
            Assert.AreEqual("US East", server.Name);
        }

        [TestMethod]
        public void UnblockAllRegions_ReturnsMessage_WhenNothingBlocked()
        {
            var service = new RegionBlockingService(
                new Mock<IFirewallManager>().Object,
                new ServerRepository(TestDirectoryHelper.CreateServersDirectory()));

            var result = service.UnblockAllRegions(new[]
            {
                new Server("US East", new List<string> { "1.2.3.4" })
            });

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("No regions are currently blocked.", result.Message);
        }
    }
}
