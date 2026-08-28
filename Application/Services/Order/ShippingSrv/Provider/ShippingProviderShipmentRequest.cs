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

        // فیلدهای زیر برای ارائه‌دهنده‌هایی مثل میاره لازم است که علاوه بر آدرس متنی،
        // به مختصات دقیق مبدا/مقصد و مشخصات فروشگاه (به‌عنوان محل تحویل‌گیری) نیاز دارند.
        public string PickupName { get; set; }
        public string PickupPhone { get; set; }
        public double? OriginLatitude { get; set; }
        public double? OriginLongitude { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
    }
}
