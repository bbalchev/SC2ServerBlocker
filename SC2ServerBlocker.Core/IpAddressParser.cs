using System;
using System.Collections.Generic;
using System.Linq;

namespace SC2ServerBlocker
{
    public static class IpAddressParser
    {
        public static string ParseLine(string line)
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

        public static bool IsValidIpOrCidr(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmed = value.Trim();
            var slashIndex = trimmed.IndexOf('/');

            if (slashIndex >= 0)
            {
                var addressPart = trimmed.Substring(0, slashIndex);
                var prefixPart = trimmed.Substring(slashIndex + 1);
                int prefixLength;

                if (!Int32.TryParse(prefixPart, out prefixLength) || prefixLength < 0 || prefixLength > 128)
                {
                    return false;
                }

                System.Net.IPAddress parsedAddress;
                return System.Net.IPAddress.TryParse(addressPart, out parsedAddress);
            }

            System.Net.IPAddress address;
            return System.Net.IPAddress.TryParse(trimmed, out address);
        }

        public static bool TryNormalizeAddresses(
            IEnumerable<string> addresses,
            out List<string> normalized,
            out string errorMessage)
        {
            normalized = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var ipAddress in addresses ?? Enumerable.Empty<string>())
            {
                var trimmed = ipAddress == null ? null : ipAddress.Trim();
                if (String.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (!IsValidIpOrCidr(trimmed))
                {
                    normalized = null;
                    errorMessage = "Invalid IP address or CIDR range: " + trimmed;
                    return false;
                }

                if (seen.Add(trimmed))
                {
                    normalized.Add(trimmed);
                }
            }

            errorMessage = null;
            return true;
        }
    }
}
