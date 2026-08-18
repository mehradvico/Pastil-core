using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.ShippingField
{
    public class ShippingQuote : Id_Field
    {
        public Guid Token { get; set; }
        public long UserId { get; set; }
        public long? CartStoreId { get; set; }
        public long AddressId { get; set; }
        public long DeliveryId { get; set; }
        public ShippingProviderEnum Provider { get; set; }
        public ShippingPaymentModeEnum PaymentMode { get; set; }
        public ShippingQuoteStatusEnum Status { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string ExternalQuoteId { get; set; }
        public string RequestFingerprint { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? SelectedAtUtc { get; set; }
        public DateTime? UsedAtUtc { get; set; }

        public User User { get; set; }
        public CartStore CartStore { get; set; }
        public Address Address { get; set; }
        public Delivery Delivery { get; set; }
    }
}
