using Entities.Entities.ShippingField;
using System;

namespace Application.Services.Order.ShippingSrv.Dto
{
    public class ShippingQuoteVDto
    {
        public Guid QuoteToken { get; set; }
        public long DeliveryId { get; set; }
        public string DeliveryName { get; set; }
        public ShippingProviderEnum Provider { get; set; }
        public ShippingPaymentModeEnum PaymentMode { get; set; }
        public double QuotedPrice { get; set; }
        public double PayableDeliveryPrice { get; set; }
        public bool PayAtDestination { get; set; }
        public string Currency { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
