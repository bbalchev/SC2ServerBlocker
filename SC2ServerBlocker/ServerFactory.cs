using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC2ServerBlocker
{
    public class ServerFactory
    {
        private ServerFactory()
        {

        }

        public static List<Server> GetServers()
        {
            var servers = new List<Server>();

            // Australia
            List<String> australianServers = new List<string>(new string[]
                { "103.4.114.128/25", "37.244.40.128/25" });
            servers.Add(new Server("Australia", australianServers));

            // Brazil
            List<String> brazileanServers = new List<string>(new string[]
                { "52.67.103.10", "52.67.109.237", "52.67.159.199" });
            servers.Add(new Server("Brazil", brazileanServers));

            // Singapore
            List<String> singaporeanServers = new List<string>(new string[]
                {   "13.228.101.219", "13.228.112.153", "13.228.191.11",
                    "13.228.206.125", "13.229.23.192", "13.229.26.161",
                    "52.221.101.120", "52.221.47.252" });
            servers.Add(new Server("Singapore", singaporeanServers));

            // US East
            List<String> usEastServers = new List<string>(new string[]
                {  "107.23.122.166", "34.193.133.210", "34.195.101.118",
                "34.235.72.119", "34.238.213.23", "34.238.30.52",
                "34.239.26.221", "34.239.54.208", "52.23.20.87",
                "52.45.62.231", "52.71.51.54", "54.227.157.150" });
            servers.Add(new Server("US East", usEastServers));

            // US Central
            List<String> usCentralServers = new List<string>(new string[]
                {  "24.105.50.0/24", "24.105.51.0/24" });
            servers.Add(new Server("US Central", usCentralServers));

            // US West
            List<String> usWestServers = new List<string>(new string[]
                {  "24.105.48.0/24", "24.105.49.0/24" });
            servers.Add(new Server("US West", usWestServers));

            // Korea
            List<String> koreanServers = new List<string>(new string[]
                {  "117.52.36.0/24" });
            servers.Add(new Server("Korea", koreanServers));

            // Taiwan
            List<String> taiwaneseServers = new List<string>(new string[]
                {  "203.69.111.0/24", "210.59.128.0/24" });
            servers.Add(new Server("Taiwan", taiwaneseServers));

            return servers;
        }
    }
}
