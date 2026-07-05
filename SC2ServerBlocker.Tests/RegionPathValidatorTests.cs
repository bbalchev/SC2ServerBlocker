using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class RegionPathValidatorTests
    {
        [TestMethod]
        public void IsValidRegionName_RejectsPathTraversalNames()
        {
            Assert.IsFalse(RegionPathValidator.IsValidRegionName(null));
            Assert.IsFalse(RegionPathValidator.IsValidRegionName(".."));
            Assert.IsFalse(RegionPathValidator.IsValidRegionName("."));
            Assert.IsFalse(RegionPathValidator.IsValidRegionName(@"US\East"));
        }

        [TestMethod]
        public void ResolveIniFilePath_RejectsNamesThatEscapeServersDirectory()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();

            try
            {
                RegionPathValidator.ResolveIniFilePath(directory, "..");
                Assert.Fail("Expected ArgumentException.");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void GetIniFilePath_KeepsResolvedPathInsideServersDirectory()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            var repository = new ServerRepository(directory);

            var iniPath = repository.GetIniFilePath("US East");
            var serversRoot = System.IO.Path.GetFullPath(directory);

            Assert.IsTrue(iniPath.StartsWith(serversRoot, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void GetServers_IgnoresInvalidIniFileNames()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            TestDirectoryHelper.WriteIniFile(directory, "..", "1.2.3.4");

            var repository = new ServerRepository(directory);
            var servers = repository.GetServers();

            Assert.IsFalse(servers.Exists(server => server.Name == ".."));
        }
    }
}
