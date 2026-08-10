using Microsoft.Extensions.Configuration;
using System;

namespace Application.Common.Configuration
{
    public static class SecretConfiguration
    {
        public static void Apply(
            IConfiguration configuration,
            string connectionEnvironmentVariable,
            bool includeVapidKeys = false)
        {
            Override(configuration, "connection", connectionEnvironmentVariable);
            Override(configuration, "JWtConfig:key", "PASTIL_JWT_KEY");
            Override(configuration, "Search:Hybrid:Enabled", "PASTIL_SEARCH_HYBRID_ENABLED");
            Override(configuration, "Search:Hybrid:Endpoint", "PASTIL_SEARCH_HYBRID_ENDPOINT");
            Override(configuration, "Search:Hybrid:ApiKey", "PASTIL_SEARCH_HYBRID_API_KEY");
            Override(configuration, "Search:Hybrid:SemanticWeight", "PASTIL_SEARCH_HYBRID_WEIGHT");
            Override(configuration, "Security:MerchantEncryptionKey", "PASTIL_MERCHANT_ENCRYPTION_KEY");

            if (includeVapidKeys)
            {
                Override(configuration, "VapidKeys:PublicKey", "PASTIL_VAPID_PUBLIC_KEY");
                Override(configuration, "VapidKeys:PrivateKey", "PASTIL_VAPID_PRIVATE_KEY");
            }
        }

        private static void Override(
            IConfiguration configuration,
            string configurationKey,
            string environmentVariable)
        {
            var value = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrWhiteSpace(value))
            {
                configuration[configurationKey] = value.Trim();
            }
        }
    }
}
