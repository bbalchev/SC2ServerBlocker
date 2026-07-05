using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class IpAddressParserTests
    {
        [TestMethod]
        public void ParseLine_ReturnsNull_ForBlankOrCommentLines()
        {
            Assert.IsNull(IpAddressParser.ParseLine(null));
            Assert.IsNull(IpAddressParser.ParseLine(string.Empty));
            Assert.IsNull(IpAddressParser.ParseLine("   "));
            Assert.IsNull(IpAddressParser.ParseLine("; comment"));
            Assert.IsNull(IpAddressParser.ParseLine("# comment"));
            Assert.IsNull(IpAddressParser.ParseLine("[Section]"));
        }

        [TestMethod]
        public void ParseLine_ReturnsIp_ForPlainAddress()
        {
            Assert.AreEqual("107.23.122.166", IpAddressParser.ParseLine("107.23.122.166"));
            Assert.AreEqual("107.23.122.166", IpAddressParser.ParseLine("  107.23.122.166  "));
        }

        [TestMethod]
        public void ParseLine_ReturnsValueAfterEquals_ForKeyValueLine()
        {
            Assert.AreEqual("24.105.50.0/24", IpAddressParser.ParseLine("Address=24.105.50.0/24"));
        }

        [TestMethod]
        public void IsValidIpOrCidr_AcceptsIpAndCidr()
        {
            Assert.IsTrue(IpAddressParser.IsValidIpOrCidr("107.23.122.166"));
            Assert.IsTrue(IpAddressParser.IsValidIpOrCidr("24.105.50.0/24"));
            Assert.IsTrue(IpAddressParser.IsValidIpOrCidr("::1"));
        }

        [TestMethod]
        public void IsValidIpOrCidr_RejectsInvalidValues()
        {
            Assert.IsFalse(IpAddressParser.IsValidIpOrCidr(null));
            Assert.IsFalse(IpAddressParser.IsValidIpOrCidr("not-an-ip"));
            Assert.IsFalse(IpAddressParser.IsValidIpOrCidr("1.2.3.4/999"));
            Assert.IsFalse(IpAddressParser.IsValidIpOrCidr("1.2.3.4/-1"));
        }

        [TestMethod]
        public void TryNormalizeAddresses_DeduplicatesAndValidates()
        {
            List<string> normalized;
            string errorMessage;

            Assert.IsTrue(IpAddressParser.TryNormalizeAddresses(
                new[] { " 1.2.3.4 ", "1.2.3.4", "5.6.7.8" },
                out normalized,
                out errorMessage));

            CollectionAssert.AreEqual(new[] { "1.2.3.4", "5.6.7.8" }, normalized);
            Assert.IsNull(errorMessage);
        }

        [TestMethod]
        public void TryNormalizeAddresses_RejectsInvalidValue()
        {
            List<string> normalized;
            string errorMessage;

            Assert.IsFalse(IpAddressParser.TryNormalizeAddresses(
                new[] { "1.2.3.4", "bad-value" },
                out normalized,
                out errorMessage));

            Assert.IsNull(normalized);
            StringAssert.Contains(errorMessage, "bad-value");
        }
    }
}
