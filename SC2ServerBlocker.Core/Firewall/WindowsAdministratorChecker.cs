using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SC2ServerBlocker.Firewall
{
    public sealed class WindowsAdministratorChecker : IAdministratorChecker
    {
        public bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
