namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingProviderShipmentResult
    {
        public bool IsSuccess { get; set; }
        public string ExternalShipmentId { get; set; }
        public string TrackingCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
