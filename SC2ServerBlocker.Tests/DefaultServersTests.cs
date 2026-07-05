using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class DefaultServersTests
    {
        [TestMethod]
        public void RegionNames_ContainsKnownRegions()
        {
            var names = DefaultServers.RegionNames.ToList();

            CollectionAssert.Contains(names, "US East");
            CollectionAssert.Contains(names, "Korea");
            CollectionAssert.Contains(names, "Australia");
        }

        [TestMethod]
        public void TryGetAddresses_ReturnsAddresses_ForKnownRegion()
        {
            Assert.IsTrue(DefaultServers.TryGetAddresses("US East", out var addresses));
            Assert.IsTrue(addresses.Count > 0);
            CollectionAssert.Contains(addresses, "107.23.122.166");
        }

        [TestMethod]
        public void TryGetAddresses_ReturnsFalse_ForUnknownRegion()
        {
            Assert.IsFalse(DefaultServers.TryGetAddresses("Atlantis", out _));
        }

        [TestMethod]
        public void WriteIniFile_WritesCommentHeaderAndAddresses()
        {
            var directory = TestDirectoryHelper.CreateServersDirectory();
            var iniPath = System.IO.Path.Combine(directory, "US East.ini");

            DefaultServers.WriteIniFile("US East", iniPath);

            var lines = System.IO.File.ReadAllLines(iniPath);
            StringAssert.Contains(lines[0], "US East");
            CollectionAssert.Contains(lines, "107.23.122.166");
        }
    }
}
