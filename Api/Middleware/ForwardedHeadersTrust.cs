using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using AspNetIPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace Api.Middleware
{
    /// <summary>
    /// Decides which peers may set X-Forwarded-For / X-Forwarded-Proto.
    /// Before this, every peer was trusted, so anyone who could reach the API port
    /// directly could pick their own client IP and dodge every per-IP rate limit.
    /// Now only loopback and private (Docker / host reverse-proxy) addresses are trusted.
    ///
    /// Configuration (all optional):
    ///   ForwardedHeaders:TrustedNetworks  extra CIDR ranges, e.g. ["203.0.113.0/24"]
    ///   ForwardedHeaders:TrustAnyProxy    true restores the old trust-everyone behaviour
    ///                                     (env ForwardedHeaders__TrustAnyProxy=true; emergency rollback only)
    /// </summary>
    public static class ForwardedHeadersTrust
    {
        private static readonly string[] PrivateNetworks =
        {
            "127.0.0.0/8",
            "::1/128",
            "10.0.0.0/8",
            "172.16.0.0/12",
            "192.168.0.0/16",
            "fc00::/7"
        };

        public static void Apply(ForwardedHeadersOptions options, IConfiguration configuration)
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            if (configuration.GetValue<bool>("ForwardedHeaders:TrustAnyProxy"))
            {
                return;
            }

            var networks = PrivateNetworks
                .Concat(configuration.GetSection("ForwardedHeaders:TrustedNetworks").Get<string[]>() ?? Array.Empty<string>());

            foreach (var network in networks)
            {
                if (TryParseNetwork(network, out var parsed))
                {
                    options.KnownNetworks.Add(parsed);
                }
            }
        }

        private static bool TryParseNetwork(string value, out AspNetIPNetwork network)
        {
            network = default!;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var parts = value.Trim().Split('/', 2);
            if (!IPAddress.TryParse(parts[0], out var prefix))
            {
                return false;
            }

            var maxLength = prefix.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? 128 : 32;
            var prefixLength = maxLength;
            if (parts.Length == 2 && (!int.TryParse(parts[1], out prefixLength) || prefixLength < 0 || prefixLength > maxLength))
            {
                return false;
            }

            network = new AspNetIPNetwork(prefix, prefixLength);
            return true;
        }
    }
}
