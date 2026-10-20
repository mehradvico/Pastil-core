namespace Application.Services.Order.PaymentGatewaySrv
{
    public sealed class PaymentTestModeOptions
    {
        public const string SectionName = "PaymentTestMode";

        public bool Enabled { get; set; }
        public bool AllowResultOverride { get; set; } = true;
        public string DefaultResult { get; set; } = "Success";
        // حالت تست همه‌ی پرداخت‌ها را بدون بانک موفق می‌کند؛ روی محیط Production فقط با این تأیید صریح دوم روشن می‌شود
        public bool AllowInProduction { get; set; }
    }
}
