using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using Entities.Entities.ShippingField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Delivery : Id_Field
    {

        public long DeliveryTypeId { get; set; }
        public double BasePrice { get; set; }
        public double MinPriceForFree { get; set; }
        public int MinCountForFree { get; set; }
        public int MaxDays { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public bool AfterRent { get; set; }
        public ShippingProviderEnum ShippingProvider { get; set; }
        public bool LivePricing { get; set; }
        public bool AllowPrepaid { get; set; } = true;
        public bool AllowReceiverPay { get; set; }
        public long? StoreId { get; set; }
        public City City { get; set; }
        public Code DeliveryType { get; set; }
        public State State { get; set; }
        public Store Store { get; set; }
        public ICollection<DeliveryDistance> DeliveryDistance { get; set; }
        public ICollection<Cart> Cart { get; set; }

    }
}
