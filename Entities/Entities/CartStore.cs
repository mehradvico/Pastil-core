using Entities.Entities.CommonField;
using Entities.Entities.ShippingField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class CartStore : Id_Field
    {
        public long CartId { get; set; }
        public int ItemCount { get; set; }
        public double BasePrice { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public long StoreId { get; set; }
        public long? DeliveryId { get; set; }
        public double DeliveryPrice { get; set; }
        public long? ShippingQuoteId { get; set; }
        public ShippingProviderEnum? ShippingProvider { get; set; }
        public ShippingPaymentModeEnum? ShippingPaymentMode { get; set; }
        public double ShippingQuotedPrice { get; set; }
        public double PaymentPrice { get; set; }
        public bool Active { get; set; }
        public Store Store { get; set; }
        public Cart Cart { get; set; }
        public Delivery Delivery { get; set; }
        public ShippingQuote ShippingQuote { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}
