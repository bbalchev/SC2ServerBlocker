using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SC2ServerBlocker
{
    public class ServerFactory
    {
        private const string ServersFolderName = "Servers";

        private ServerFactory()
        {
        }

        public static List<Server> GetServers()
        {
            var servers = new Dictionary<String, Server>(StringComparer.OrdinalIgnoreCase);

            foreach (var regionName in DefaultServers.RegionNames)
            {
                var server = LoadServer(regionName);

                if (server == null)
                {
                    List<String> defaultAddresses;
                    if (DefaultServers.TryGetAddresses(regionName, out defaultAddresses))
                    {
                        server = new Server(regionName, new List<String>(defaultAddresses));
                    }
                }

                if (server != null)
                {
                    servers[regionName] = server;
                }
            }

            var serversDirectory = GetServersDirectory();
            if (Directory.Exists(serversDirectory))
            {
                foreach (var iniFile in Directory.GetFiles(serversDirectory, "*.ini"))
                {
                    var regionName = Path.GetFileNameWithoutExtension(iniFile);

                    if (!servers.ContainsKey(regionName))
                    {
                        var server = LoadServerFromFile(iniFile);

                        if (server != null)
                        {
                            servers[regionName] = server;
                        }
                    }
                }
            }

            return servers.Values.OrderBy(server => server.Name).ToList();
        }

        public static Server LoadServer(String regionName)
        {
            var iniFilePath = GetIniFilePath(regionName);

            if (!File.Exists(iniFilePath))
            {
                return null;
            }

            return LoadServerFromFile(iniFilePath);
        }

        public static Server LoadServerForBlock(String regionName)
        {
            var iniFilePath = GetIniFilePath(regionName);

            if (!File.Exists(iniFilePath))
            {
                DefaultServers.WriteIniFile(regionName, iniFilePath);
            }

            var server = LoadServerFromFile(iniFilePath);

            if (server != null)
            {
                return server;
            }

            List<String> defaultAddresses;
            if (DefaultServers.TryGetAddresses(regionName, out defaultAddresses))
            {
                return new Server(regionName, new List<String>(defaultAddresses));
            }

            return null;
        }

        private static String GetServersDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServersFolderName);
        }

        private static String GetIniFilePath(String regionName)
        {
            return Path.Combine(GetServersDirectory(), regionName + ".ini");
        }

        private static Server LoadServerFromFile(String iniFilePath)
        {
            var regionName = Path.GetFileNameWithoutExtension(iniFilePath);
            var ipAddresses = LoadIpAddresses(iniFilePath);

            if (ipAddresses.Count == 0)
            {
                return null;
            }

            return new Server(regionName, ipAddresses);
        }

        private static List<String> LoadIpAddresses(String iniFilePath)
        {
            var ipAddresses = new List<String>();
            var seen = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadAllLines(iniFilePath))
            {
                var ipAddress = ParseIpAddressLine(line);

                if (ipAddress != null && seen.Add(ipAddress))
                {
                    ipAddresses.Add(ipAddress);
                }
            }

            return ipAddresses;
        }

        private static String ParseIpAddressLine(String line)
        {
            if (String.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var trimmed = line.Trim();

            if (trimmed.StartsWith(";") || trimmed.StartsWith("#"))
            {
                return null;
            }

            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                return null;
            }

            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex >= 0)
            {
                trimmed = trimmed.Substring(equalsIndex + 1).Trim();
            }

            return String.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}
