using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SC2ServerBlocker
{
    public static class DefaultServers
    {
        private static readonly Dictionary<String, List<String>> Addresses =
            new Dictionary<String, List<String>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "Australia", new List<String>
                    {
                        "103.4.114.128/25",
                        "37.244.40.128/25"
                    }
                },
                {
                    "Brazil", new List<String>
                    {
                        "52.67.103.10",
                        "52.67.109.237",
                        "52.67.159.199",
                        "18.228.90.183",
                        "54.207.104.0/24",
                        "15.229.96.172",
                        "15.229.96.179",
                        "54.67.132.106",
                        "54.233.165.118",
                        "18.229.151.97",
                        "56.125.127.216",
                        "4.193.243.54",
                        "4.201.180.63",
                        "4.191.193.89",
                        "4.193.225.27",
                        "4.201.115.95",
                        "4.201.192.180",
                        "104.41.44.179"
                    }
                },
                {
                    "Singapore", new List<String>
                    {
                        "13.228.101.219",
                        "13.228.112.153",
                        "13.228.191.11",
                        "13.228.206.125",
                        "13.229.23.192",
                        "13.229.26.161",
                        "52.221.101.120",
                        "52.221.47.252"
                    }
                },
                {
                    "US East", new List<String>
                    {
                        "107.23.122.166",
                        "34.193.133.210",
                        "34.195.101.118",
                        "34.235.72.119",
                        "34.238.213.23",
                        "34.238.30.52",
                        "34.239.26.221",
                        "34.239.54.208",
                        "52.23.20.87",
                        "52.45.62.231",
                        "52.71.51.54",
                        "54.227.157.150"
                    }
                },
                {
                    "US Central", new List<String>
                    {
                        "24.105.50.0/24",
                        "24.105.51.0/24"
                    }
                },
                {
                    "US West", new List<String>
                    {
                        "24.105.48.0/24",
                        "24.105.49.0/24",
                        "37.244.3.144",
                        "37.244.3.146",
                        "37.244.3.148",
                        "37.244.3.151",
                        "37.244.3.202",
                        "37.244.3.208",
                        "37.244.3.211",
                        "37.244.3.212",
                        "37.244.3.214"
                    }
                },
                {
                    "Korea", new List<String>
                    {
                        "117.52.36.0/24"
                    }
                },
                {
                    "Taiwan", new List<String>
                    {
                        "203.69.111.0/24",
                        "210.59.128.0/24"
                    }
                }
            };

        public static IEnumerable<String> RegionNames
        {
            get { return Addresses.Keys.OrderBy(name => name); }
        }

        public static bool TryGetAddresses(String regionName, out List<String> addresses)
        {
            return Addresses.TryGetValue(regionName, out addresses);
        }

        public static void WriteIniFile(String regionName, String iniFilePath)
        {
            List<String> addresses;
            if (!TryGetAddresses(regionName, out addresses))
            {
                return;
            }

            var lines = new List<String>
            {
                "; StarCraft 2 server IPs for " + regionName,
                "; One IP or CIDR range per line. Lines starting with ; or # are ignored.",
                ""
            };
            lines.AddRange(addresses);

            Directory.CreateDirectory(Path.GetDirectoryName(iniFilePath));
            File.WriteAllLines(iniFilePath, lines);
        }
    }
}
