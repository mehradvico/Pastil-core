using Entities.Entities.CommonField;
using Entities.Entities.ShippingField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class ProductOrderStore : Id_Field
    {
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public double DiscountPrice { get; set; }
        public long StoreId { get; set; }
        public string ProductOrderId { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public bool Edited { get; set; }
        public long? DeliveryId { get; set; }
        public double DeliveryPrice { get; set; }
        public long? ShippingQuoteId { get; set; }
        public ShippingProviderEnum? ShippingProvider { get; set; }
        public ShippingPaymentModeEnum? ShippingPaymentMode { get; set; }
        public double ShippingQuotedPrice { get; set; }
        public double PaymentPrice { get; set; }
        public Delivery Delivery { get; set; }
        public Store Store { get; set; }
        public ProductOrder ProductOrder { get; set; }
        public ShippingQuote ShippingQuote { get; set; }
        public Shipment Shipment { get; set; }
        public ICollection<ProductOrderItem> ProductOrderItems { get; set; }
    }
}
