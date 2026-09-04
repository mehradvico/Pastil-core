using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.ShippingField
{
    public class Shipment : Id_Field
    {
        public long ProductOrderStoreId { get; set; }
        public long? ShippingQuoteId { get; set; }
        public ShippingProviderEnum Provider { get; set; }
        public ShippingPaymentModeEnum PaymentMode { get; set; }
        public ShipmentStatusEnum Status { get; set; }
        public double QuotedPrice { get; set; }
        public double ChargedPrice { get; set; }
        public double? ProviderCost { get; set; }
        public string ExternalShipmentId { get; set; }
        public string TrackingCode { get; set; }
        public string FailureReason { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? RequestedAtUtc { get; set; }
        public DateTime? DeliveredAtUtc { get; set; }

        public ProductOrderStore ProductOrderStore { get; set; }
        public ShippingQuote ShippingQuote { get; set; }
    }
}
