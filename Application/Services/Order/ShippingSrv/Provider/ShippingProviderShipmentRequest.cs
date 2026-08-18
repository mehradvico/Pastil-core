using Entities.Entities.ShippingField;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingProviderShipmentRequest
    {
        public string OrderId { get; set; }
        public long StoreId { get; set; }
        public ShippingProviderEnum Provider { get; set; }
        public ShippingPaymentModeEnum PaymentMode { get; set; }
        public string ExternalQuoteId { get; set; }
        public string RecipientName { get; set; }
        public string RecipientMobile { get; set; }
        public string RecipientAddress { get; set; }
    }
}
