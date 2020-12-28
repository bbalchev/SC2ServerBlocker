using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC2ServerBlocker
{
    public class Server
    {
        public Server(String name, List<String> ipAddresses)
        {
            Name = name;
            IpAddressList = ipAddresses;
        }

        public String Name { get; private set; }
        public List<String> IpAddressList { get; private set; }
    }
}
