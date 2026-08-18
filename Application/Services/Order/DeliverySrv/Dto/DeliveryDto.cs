using Application.Common.Dto.Field;

namespace Application.Services.Order.DeliverySrv.Dto
{
    public class DeliveryDto : Id_FieldDto
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
        public long? StoreId { get; set; }
        public bool AfterRent { get; set; }
        public Entities.Entities.ShippingField.ShippingProviderEnum ShippingProvider { get; set; }
        public bool LivePricing { get; set; }
        public bool AllowPrepaid { get; set; } = true;
        public bool AllowReceiverPay { get; set; }
    }
}
