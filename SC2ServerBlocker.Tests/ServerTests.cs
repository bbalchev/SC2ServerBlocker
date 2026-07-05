using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public void DisplayName_ShowsBlockedSuffix_WhenBlocked()
        {
            var server = new Server("US East", new System.Collections.Generic.List<string> { "1.2.3.4" });

            Assert.AreEqual("US East", server.DisplayName);

            server.IsBlocked = true;

            Assert.AreEqual("US East (blocked)", server.DisplayName);
        }

        [TestMethod]
        public void ReplaceIpAddresses_UpdatesCount()
        {
            var server = new Server("US East", new System.Collections.Generic.List<string> { "1.2.3.4" });

            server.ReplaceIpAddresses(new[] { "5.6.7.8", "9.9.9.9" });

            Assert.AreEqual(2, server.IpCount);
            CollectionAssert.AreEqual(new[] { "5.6.7.8", "9.9.9.9" }, server.IpAddressList);
        }
    }
}
