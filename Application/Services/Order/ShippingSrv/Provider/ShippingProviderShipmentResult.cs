namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingProviderShipmentResult
    {
        public bool IsSuccess { get; set; }
        public string ExternalShipmentId { get; set; }
        public string TrackingCode { get; set; }
        public string ErrorMessage { get; set; }
        /// <summary>boost واقعاً روی سفر اعمال شد؟ (اگر درخواست boost رد شد، سفر بدون boost ساخته می‌شود و false برمی‌گردد)</summary>
        public bool BoostApplied { get; set; }
    }
}
