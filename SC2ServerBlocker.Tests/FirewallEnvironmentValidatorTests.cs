using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SC2ServerBlocker.Firewall;
using SC2ServerBlocker.Tests.Fakes;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class FirewallEnvironmentValidatorTests
    {
        [TestMethod]
        public void Validate_ReturnsError_WhenNotAdministrator()
        {
            var validator = CreateValidator(isAdministrator: false, firewallEnabled: true);

            var result = validator.Validate();

            Assert.AreEqual(StartupValidationSeverity.Error, result.Severity);
            StringAssert.Contains(result.Message, "Administrator");
        }

        [TestMethod]
        public void Validate_ReturnsError_WhenPolicyFactoryFails()
        {
            var factory = new FakeFirewallPolicyFactory { ShouldThrowOnCreate = true };
            var validator = new FirewallEnvironmentValidator(
                new FakeAdministratorChecker(),
                factory);

            var result = validator.Validate();

            Assert.AreEqual(StartupValidationSeverity.Error, result.Severity);
            StringAssert.Contains(result.Message, "Unable to access Windows Firewall");
        }

        [TestMethod]
        public void Validate_ReturnsWarning_WhenFirewallDisabled()
        {
            var factory = new FakeFirewallPolicyFactory();
            factory.CreatePolicy();
            factory.Policy.FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_PUBLIC] = false;

            var validator = new FirewallEnvironmentValidator(
                new FakeAdministratorChecker(),
                factory);

            var result = validator.Validate();

            Assert.AreEqual(StartupValidationSeverity.Warning, result.Severity);
            StringAssert.Contains(result.Message, "turned off");
        }

        [TestMethod]
        public void Validate_ReturnsOk_WhenEnvironmentIsHealthy()
        {
            var result = CreateValidator(isAdministrator: true, firewallEnabled: true).Validate();

            Assert.AreEqual(StartupValidationSeverity.None, result.Severity);
        }

        private static FirewallEnvironmentValidator CreateValidator(bool isAdministrator, bool firewallEnabled)
        {
            var factory = new FakeFirewallPolicyFactory();
            factory.CreatePolicy();
            factory.Policy.FirewallEnabledByProfile[NetFwInterop.NET_FW_PROFILE2_PUBLIC] = firewallEnabled;

            return new FirewallEnvironmentValidator(
                new FakeAdministratorChecker { IsAdministrator = isAdministrator },
                factory);
        }
    }
}
