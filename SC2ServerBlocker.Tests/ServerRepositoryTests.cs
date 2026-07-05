using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class ServerRepositoryTests
    {
        [TestMethod]
        public void GetServers_LoadsCustomIniAndDefaults()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            TestDirectoryHelper.WriteIniFile(directory, "Custom Region", "10.0.0.1");

            var repository = new ServerRepository(directory);
            var servers = repository.GetServers();

            Assert.IsTrue(servers.Any(server => server.Name == "Custom Region"));
            Assert.IsTrue(servers.Any(server => server.Name == "US East"));
        }

        [TestMethod]
        public void LoadServer_ReturnsNull_WhenFileMissing()
        {
            var repository = new ServerRepository(TestDirectoryHelper.CreateServersDirectory());

            Assert.IsNull(repository.LoadServer("Missing Region"));
        }

        [TestMethod]
        public void LoadServerForBlock_CreatesIniFromDefaults()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            var repository = new ServerRepository(directory);

            var server = repository.LoadServerForBlock("US East");

            Assert.IsNotNull(server);
            Assert.IsTrue(File.Exists(repository.GetIniFilePath("US East")));
            Assert.IsTrue(server.IpAddressList.Count > 0);
        }

        [TestMethod]
        public void SaveServerAddresses_DeduplicatesAndWritesFile()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            var repository = new ServerRepository(directory);

            repository.SaveServerAddresses("US East", new[] { " 1.2.3.4 ", "1.2.3.4", "5.6.7.8" });

            var saved = repository.LoadServer("US East");
            CollectionAssert.AreEqual(new[] { "1.2.3.4", "5.6.7.8" }, saved.IpAddressList);
        }

        [TestMethod]
        public void GetServers_IgnoresCommentsAndDuplicateLines()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            TestDirectoryHelper.WriteIniFile(
                directory,
                "US East",
                "; comment",
                "1.2.3.4",
                "1.2.3.4",
                "Address=5.6.7.8");

            var repository = new ServerRepository(directory);
            var server = repository.LoadServer("US East");

            CollectionAssert.AreEqual(new[] { "1.2.3.4", "5.6.7.8" }, server.IpAddressList);
        }

        [TestMethod]
        public void GetServers_IgnoresInvalidIpLines()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            TestDirectoryHelper.WriteIniFile(
                directory,
                "US East",
                "1.2.3.4",
                "not-an-ip",
                "5.6.7.8");

            var repository = new ServerRepository(directory);
            var server = repository.LoadServer("US East");

            CollectionAssert.AreEqual(new[] { "1.2.3.4", "5.6.7.8" }, server.IpAddressList);
        }

        [TestMethod]
        public void SaveServerAddresses_RejectsInvalidAddress()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            var repository = new ServerRepository(directory);

            try
            {
                repository.SaveServerAddresses("US East", new[] { "1.2.3.4", "bad-value" });
                Assert.Fail("Expected ArgumentException.");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
