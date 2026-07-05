using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SC2ServerBlocker
{
    public class ServerRepository
    {
        private readonly string _serversDirectory;

        public ServerRepository(string serversDirectory)
        {
            if (String.IsNullOrWhiteSpace(serversDirectory))
            {
                throw new ArgumentException("Servers directory is required.", "serversDirectory");
            }

            _serversDirectory = serversDirectory;
        }

        public string ServersDirectory
        {
            get { return _serversDirectory; }
        }

        public string GetIniFilePath(string regionName)
        {
            return RegionPathValidator.ResolveIniFilePath(_serversDirectory, regionName);
        }

        public List<Server> GetServers()
        {
            var servers = new Dictionary<string, Server>(StringComparer.OrdinalIgnoreCase);

            foreach (var regionName in DefaultServers.RegionNames)
            {
                if (!RegionPathValidator.IsValidRegionName(regionName))
                {
                    continue;
                }

                var server = LoadServer(regionName);

                if (server == null)
                {
                    List<string> defaultAddresses;
                    if (DefaultServers.TryGetAddresses(regionName, out defaultAddresses))
                    {
                        server = new Server(regionName, new List<string>(defaultAddresses));
                    }
                }

                if (server != null)
                {
                    servers[regionName] = server;
                }
            }

            if (Directory.Exists(_serversDirectory))
            {
                foreach (var iniFile in Directory.GetFiles(_serversDirectory, "*.ini"))
                {
                    var regionName = Path.GetFileNameWithoutExtension(iniFile);

                    if (!RegionPathValidator.IsValidRegionName(regionName) || servers.ContainsKey(regionName))
                    {
                        continue;
                    }

                    var server = LoadServerFromFile(iniFile);

                    if (server != null)
                    {
                        servers[regionName] = server;
                    }
                }
            }

            return servers.Values.OrderBy(server => server.Name).ToList();
        }

        public Server LoadServer(string regionName)
        {
            var iniFilePath = GetIniFilePath(regionName);

            if (!File.Exists(iniFilePath))
            {
                return null;
            }

            return LoadServerFromFile(iniFilePath);
        }

        public Server LoadServerForBlock(string regionName)
        {
            RegionPathValidator.ValidateRegionName(regionName);

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

            List<string> defaultAddresses;
            if (DefaultServers.TryGetAddresses(regionName, out defaultAddresses))
            {
                return new Server(regionName, new List<string>(defaultAddresses));
            }

            return null;
        }

        public void SaveServerAddresses(string regionName, IEnumerable<string> ipAddresses)
        {
            List<string> uniqueAddresses;
            string validationError;
            if (!IpAddressParser.TryNormalizeAddresses(ipAddresses, out uniqueAddresses, out validationError))
            {
                throw new ArgumentException(validationError, "ipAddresses");
            }

            if (uniqueAddresses.Count == 0)
            {
                throw new ArgumentException("At least one IP address is required.", "ipAddresses");
            }

            var iniFilePath = GetIniFilePath(regionName);
            var lines = new List<string>
            {
                "; StarCraft 2 server IPs for " + regionName,
                "; One IP or CIDR range per line. Lines starting with ; or # are ignored.",
                ""
            };
            lines.AddRange(uniqueAddresses);

            Directory.CreateDirectory(_serversDirectory);
            File.WriteAllLines(iniFilePath, lines);
        }

        private Server LoadServerFromFile(string iniFilePath)
        {
            var regionName = Path.GetFileNameWithoutExtension(iniFilePath);

            if (!RegionPathValidator.IsValidRegionName(regionName))
            {
                return null;
            }

            var ipAddresses = LoadIpAddresses(iniFilePath);

            if (ipAddresses.Count == 0)
            {
                return null;
            }

            return new Server(regionName, ipAddresses);
        }

        private List<string> LoadIpAddresses(string iniFilePath)
        {
            var ipAddresses = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadAllLines(iniFilePath))
            {
                var ipAddress = IpAddressParser.ParseLine(line);

                if (ipAddress != null &&
                    IpAddressParser.IsValidIpOrCidr(ipAddress) &&
                    seen.Add(ipAddress))
                {
                    ipAddresses.Add(ipAddress);
                }
            }

            return ipAddresses;
        }
    }
}
