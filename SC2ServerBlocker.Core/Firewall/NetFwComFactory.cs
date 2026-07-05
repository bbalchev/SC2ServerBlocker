using System;
using System.Runtime.InteropServices;
using NetFwTypeLib;

namespace SC2ServerBlocker.Firewall
{
    internal static class NetFwComFactory
    {
        internal static INetFwPolicy2 CreatePolicy()
        {
            var policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2", throwOnError: true);
            return (INetFwPolicy2)Activator.CreateInstance(policyType);
        }

        internal static INetFwRule CreateRule()
        {
            var ruleType = Type.GetTypeFromProgID("HNetCfg.FwRule", throwOnError: true);
            return (INetFwRule)Activator.CreateInstance(ruleType);
        }
    }
}
