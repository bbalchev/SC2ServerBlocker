using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SC2ServerBlocker
{
    public class SC2FirewallManager
    {
        private readonly String FirewallPrefixName = "Sc";

        public SC2FirewallManager()
        {
        }

        public void BlockServer(Server server)
        {
            // first delete previous entries if any
            UnblockServer(server);

            String delimiter = ",";
            String serverListAsString = ConvertServerListToString(server, delimiter);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = String.Format("/C netsh advfirewall firewall add rule name=\"{0}\" Dir=Out Action=Block RemoteIP={1}",
                AddFirewallPrefixToServerName(server),
                serverListAsString);
            process.StartInfo = startInfo;
            process.Start();
        }

        public void UnblockServer(Server server)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = String.Format("/C netsh advfirewall firewall delete rule name=\"{0}\"",
                AddFirewallPrefixToServerName(server));
            process.StartInfo = startInfo;
            process.Start();
        }

        private String AddFirewallPrefixToServerName(Server server)
        {
            return FirewallPrefixName + server.Name;
        }

        private String ConvertServerListToString(Server server, String delimiter = ",")
        {
            var result = String.Join(delimiter, server.IpAddressList);

            return result;
        }
    }
}
