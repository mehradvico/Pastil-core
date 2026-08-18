using Entities.Entities.ShippingField;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingProviderQuoteRequest
    {
        public long StoreId { get; set; }
        public ShippingProviderEnum Provider { get; set; }
        public double OriginLatitude { get; set; }
        public double OriginLongitude { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
        public int WeightGrams { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
        public double DeclaredValue { get; set; }
    }
}
