using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class BlockedRegionsFormatterTests
    {
        [TestMethod]
        public void FormatSummary_ReturnsNoneMessage_WhenEmpty()
        {
            Assert.AreEqual("Blocked regions: none", BlockedRegionsFormatter.FormatSummary(new string[0]));
            Assert.AreEqual("Blocked regions: none", BlockedRegionsFormatter.FormatSummary(null));
        }

        [TestMethod]
        public void FormatSummary_OrdersRegionNames()
        {
            var summary = BlockedRegionsFormatter.FormatSummary(new[] { "US West", "Korea", "US East" });

            Assert.AreEqual("Blocked regions: Korea, US East, US West", summary);
        }
    }
}
