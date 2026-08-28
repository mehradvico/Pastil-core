namespace Application.Services.Order.ShippingSrv
{
    public class ShippingOptions
    {
        public const string SectionName = "Shipping";

        public bool TestMode { get; set; } = true;
        public int QuoteTtlMinutes { get; set; } = 5;
        public int DefaultWeightGrams { get; set; } = 1000;
        public decimal DefaultLengthCm { get; set; } = 20;
        public decimal DefaultWidthCm { get; set; } = 20;
        public decimal DefaultHeightCm { get; set; } = 20;
        public ShippingProviderOptions AloPeyk { get; set; } = new();
        public ShippingProviderOptions Tipax { get; set; } = new();
        public ShippingProviderOptions SnappBox { get; set; } = new();
        public ShippingProviderOptions Miare { get; set; } = new();
    }

    public class ShippingProviderOptions
    {
        public bool Enabled { get; set; } = true;
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
