using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class ServerIpEditorSessionTests
    {
        [TestMethod]
        public void Constructor_LoadsInitialRegionAddresses()
        {
            var session = CreateSession("US East");

            CollectionAssert.AreEqual(new[] { "1.2.3.4" }, new List<string>(session.IpAddresses));
            Assert.AreEqual("US East", session.SelectedRegionName);
        }

        [TestMethod]
        public void TrySelectRegion_RefreshesAddressesForDifferentRegion()
        {
            var session = CreateSession("US East");

            string errorMessage;
            Assert.IsTrue(session.TrySelectRegion("Korea", out errorMessage));

            CollectionAssert.AreEqual(new[] { "5.6.7.8" }, new List<string>(session.IpAddresses));
            Assert.AreEqual("Korea", session.SelectedRegionName);
            Assert.IsNull(errorMessage);
        }

        [TestMethod]
        public void TrySelectRegion_RejectsUnknownRegion()
        {
            var session = CreateSession("US East");

            string errorMessage;
            Assert.IsFalse(session.TrySelectRegion("Atlantis", out errorMessage));

            CollectionAssert.AreEqual(new[] { "1.2.3.4" }, new List<string>(session.IpAddresses));
            Assert.AreEqual("US East", session.SelectedRegionName);
            StringAssert.Contains(errorMessage, "Atlantis");
        }

        [TestMethod]
        public void TryAddAddress_AddsValidAddressToCurrentRegion()
        {
            var session = CreateSession("US East");

            string errorMessage;
            Assert.IsTrue(session.TryAddAddress("8.8.8.8", out errorMessage));

            CollectionAssert.AreEqual(new[] { "1.2.3.4", "8.8.8.8" }, new List<string>(session.IpAddresses));
        }

        [TestMethod]
        public void TryAddAddress_RejectsDuplicateAddress()
        {
            var session = CreateSession("US East");

            string errorMessage;
            Assert.IsFalse(session.TryAddAddress("1.2.3.4", out errorMessage));

            StringAssert.Contains(errorMessage, "already");
        }

        [TestMethod]
        public void TrySelectRegion_DoesNotCarryUnsavedEditsToOtherRegion()
        {
            var session = CreateSession("US East");

            string errorMessage;
            session.TryAddAddress("8.8.8.8", out errorMessage);
            Assert.IsTrue(session.TrySelectRegion("Korea", out errorMessage));

            CollectionAssert.AreEqual(new[] { "5.6.7.8" }, new List<string>(session.IpAddresses));
        }

        private static ServerIpEditorSession CreateSession(string initialRegionName)
        {
            return new ServerIpEditorSession(
                new[]
                {
                    new Server("US East", new List<string> { "1.2.3.4" }),
                    new Server("Korea", new List<string> { "5.6.7.8" })
                },
                initialRegionName);
        }
    }
}
