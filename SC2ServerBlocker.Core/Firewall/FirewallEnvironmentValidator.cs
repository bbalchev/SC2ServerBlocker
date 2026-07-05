using System;
using System.Runtime.InteropServices;

namespace SC2ServerBlocker.Firewall
{
    public sealed class FirewallEnvironmentValidator
    {
        private readonly IAdministratorChecker _administratorChecker;
        private readonly IFirewallPolicyFactory _policyFactory;

        public FirewallEnvironmentValidator(
            IAdministratorChecker administratorChecker,
            IFirewallPolicyFactory policyFactory)
        {
            _administratorChecker = administratorChecker;
            _policyFactory = policyFactory;
        }

        public StartupValidationResult Validate()
        {
            if (!_administratorChecker.IsRunningAsAdministrator())
            {
                return StartupValidationResult.Error(
                    "Administrator rights are required. Restart the app using Run as administrator.");
            }

            IFirewallPolicy policy;

            try
            {
                policy = _policyFactory.CreatePolicy();
            }
            catch (Exception ex)
            {
                return StartupValidationResult.Error(
                    "Unable to access Windows Firewall. " + GetFirewallErrorMessage(ex));
            }

            if (!FirewallProfileHelper.TryIsFirewallEnabledForActiveProfiles(policy, out var firewallEnabled))
            {
                return StartupValidationResult.Warning(
                    "Unable to verify Windows Firewall status. Blocking may still work if the firewall is enabled.");
            }

            if (!firewallEnabled)
            {
                return StartupValidationResult.Warning(
                    "Windows Firewall is turned off. Blocking will not take effect until the firewall is enabled.");
            }

            return StartupValidationResult.Ok();
        }

        internal static string GetFirewallErrorMessage(Exception ex)
        {
            if (ex is COMException comException)
            {
                return string.Format(
                    "COM error 0x{0:X8}. Ensure the Windows Firewall service is running.",
                    (uint)comException.ErrorCode);
            }

            if (ex.InnerException != null)
            {
                return GetFirewallErrorMessage(ex.InnerException);
            }

            return ex.Message;
        }
    }
}
