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
