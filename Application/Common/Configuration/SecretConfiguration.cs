using Microsoft.Extensions.Configuration;
using System;
using System.Text;

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
            Override(configuration, "Security:BankCardEncryptionKey", "PASTIL_BANKCARD_ENCRYPTION_KEY");
            OverrideFcmServiceAccountJson(configuration);
            Override(configuration, "Security:PasswordPepper", "PASTIL_PASSWORD_PEPPER");
            Override(configuration, "Security:ClientIpAttestationKey", "PASTIL_CLIENT_IP_ATTESTATION_KEY");
            // فهرست Hostهای مجاز (جداشده با ;) به‌جای "*"؛ مثال در .env.example. بدون این متغیر همان مقدار appsettings (پیش‌فرض "*") می‌ماند.
            Override(configuration, "AllowedHosts", "PASTIL_ALLOWED_HOSTS");
            // حالت سیاست عضویت ناحیه‌های Seller/Companion/Driver: Audit (پیش‌فرض، فقط لاگ) یا Enforce (رد غیرعضو)
            Override(configuration, "Security:AreaPolicyMode", "PASTIL_AREA_POLICY_MODE");
            Override(configuration, "Security:OtpLength", "PASTIL_OTP_LENGTH");
            Application.Common.Helpers.GenerateHelper.ConfigureOtpLength(configuration["Security:OtpLength"]);
            // هشدار لحظه‌ای امنیتی (اختیاری): webhook عمومی یا ربات تلگرام؛ بدون آن‌ها هشدار فقط در لاگ (Critical, Security.Alert) می‌آید
            Override(configuration, "Security:AlertWebhookUrl", "PASTIL_SECURITY_ALERT_WEBHOOK_URL");
            Override(configuration, "Security:AlertTelegramBotToken", "PASTIL_SECURITY_ALERT_TELEGRAM_BOT_TOKEN");
            Override(configuration, "Security:AlertTelegramChatId", "PASTIL_SECURITY_ALERT_TELEGRAM_CHAT_ID");
            Override(configuration, "MapIr:ApiKey", "PASTIL_MAPIR_API_KEY");
            Override(configuration, "Shipping:TestMode", "PASTIL_SHIPPING_TEST_MODE");
            Override(configuration, "Shipping:AloPeyk:BaseUrl", "PASTIL_SHIPPING_ALOPEYK_BASE_URL");
            Override(configuration, "Shipping:AloPeyk:ApiKey", "PASTIL_SHIPPING_ALOPEYK_API_KEY");
            Override(configuration, "Shipping:Tipax:BaseUrl", "PASTIL_SHIPPING_TIPAX_BASE_URL");
            Override(configuration, "Shipping:Tipax:ApiKey", "PASTIL_SHIPPING_TIPAX_API_KEY");
            Override(configuration, "Shipping:SnappBox:BaseUrl", "PASTIL_SHIPPING_SNAPPBOX_BASE_URL");
            Override(configuration, "Shipping:SnappBox:ApiKey", "PASTIL_SHIPPING_SNAPPBOX_API_KEY");
            Override(configuration, "Shipping:Miare:BaseUrl", "PASTIL_SHIPPING_MIARE_BASE_URL");
            Override(configuration, "Shipping:Miare:ApiKey", "PASTIL_SHIPPING_MIARE_API_KEY");
            Override(configuration, "Shipping:Miare:AccountingBaseUrl", "PASTIL_SHIPPING_MIARE_ACCOUNTING_BASE_URL");

            // کلید رمزنگاری ستونی شماره کارت/شبا؛ بدون کلید معتبر مقدارهای جدید بدون رمز ذخیره می‌شوند (هشدار در استارتاپ)
            Persistence.Security.SensitiveDataProtector.Configure(configuration["Security:BankCardEncryptionKey"]);

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

        private static void OverrideFcmServiceAccountJson(IConfiguration configuration)
        {
            const string configurationKey = "Fcm:ServiceAccountJson";
            var encodedValue = Environment.GetEnvironmentVariable("PASTIL_FCM_SERVICE_ACCOUNT_JSON_BASE64");

            // Base64 is the dependable option for Docker/.env: the JSON private_key field itself
            // contains escaped line breaks, which some dotenv parsers can transform on the way
            // into a container. Keep the original variable as a backwards-compatible fallback.
            if (!string.IsNullOrWhiteSpace(encodedValue))
            {
                try
                {
                    var value = Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue.Trim()));
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        configuration[configurationKey] = value;
                        return;
                    }
                }
                catch (FormatException)
                {
                    // Fall back to the legacy JSON environment variable. FcmSender emits the
                    // user-facing configuration failure if neither value is usable.
                }
            }

            Override(configuration, configurationKey, "PASTIL_FCM_SERVICE_ACCOUNT_JSON");
        }
    }
}
