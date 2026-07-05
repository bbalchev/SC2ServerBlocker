using System;
using System.Collections.Generic;
using System.Linq;

namespace SC2ServerBlocker
{
    public sealed class ServerIpEditorSession
    {
        private readonly Dictionary<string, Server> _serversByName;
        private List<string> _ipAddresses;

        public ServerIpEditorSession(IEnumerable<Server> servers, string initialRegionName)
        {
            if (servers == null)
            {
                throw new ArgumentNullException(nameof(servers));
            }

            _serversByName = servers
                .Where(server => server != null && !string.IsNullOrWhiteSpace(server.Name))
                .GroupBy(server => server.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            if (_serversByName.Count == 0)
            {
                throw new ArgumentException("At least one region is required.", nameof(servers));
            }

            if (!TrySelectRegion(initialRegionName, out var error))
            {
                throw new ArgumentException(error, nameof(initialRegionName));
            }
        }

        public IReadOnlyList<string> AvailableRegions
        {
            get
            {
                return _serversByName.Keys
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        public string SelectedRegionName { get; private set; }

        public IReadOnlyList<string> IpAddresses
        {
            get { return _ipAddresses; }
        }

        public bool TrySelectRegion(string regionName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(regionName))
            {
                errorMessage = "Select a region.";
                return false;
            }

            Server server;
            if (!_serversByName.TryGetValue(regionName, out server))
            {
                errorMessage = "Region '" + regionName + "' was not found.";
                return false;
            }

            SelectedRegionName = server.Name;
            _ipAddresses = server.IpAddressList.ToList();
            errorMessage = null;
            return true;
        }

        public bool TryAddAddress(string value, out string errorMessage)
        {
            var trimmed = value == null ? string.Empty : value.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                errorMessage = null;
                return false;
            }

            if (!IpAddressParser.IsValidIpOrCidr(trimmed))
            {
                errorMessage = "Enter a valid IPv4/IPv6 address or CIDR range (for example 192.168.0.0/24).";
                return false;
            }

            if (_ipAddresses.Any(existing => string.Equals(existing, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                errorMessage = "That address is already in the list.";
                return false;
            }

            _ipAddresses.Add(trimmed);
            errorMessage = null;
            return true;
        }

        public bool TryRemoveAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return _ipAddresses.RemoveAll(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)) > 0;
        }

        public bool CanSave()
        {
            return _ipAddresses.Count > 0;
        }

        public string GetDescriptionText()
        {
            return string.Format(
                "Editing IP addresses for {0}. One IP or CIDR range per entry.",
                SelectedRegionName);
        }
    }
}
