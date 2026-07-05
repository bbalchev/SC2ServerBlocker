using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SC2ServerBlocker
{
    public static class ServerFactory
    {
        private static readonly ServerRepository DefaultRepository =
            new ServerRepository(GetDefaultServersDirectory());

        public static List<Server> GetServers()
        {
            return DefaultRepository.GetServers();
        }

        public static Server LoadServer(string regionName)
        {
            return DefaultRepository.LoadServer(regionName);
        }

        public static Server LoadServerForBlock(string regionName)
        {
            return DefaultRepository.LoadServerForBlock(regionName);
        }

        public static string GetServersDirectory()
        {
            return DefaultRepository.ServersDirectory;
        }

        public static string GetIniFilePath(string regionName)
        {
            return DefaultRepository.GetIniFilePath(regionName);
        }

        public static void SaveServerAddresses(string regionName, IEnumerable<string> ipAddresses)
        {
            DefaultRepository.SaveServerAddresses(regionName, ipAddresses);
        }

        public static bool IsValidIpOrCidr(string value)
        {
            return IpAddressParser.IsValidIpOrCidr(value);
        }

        private static string GetDefaultServersDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Servers");
        }
    }
}
