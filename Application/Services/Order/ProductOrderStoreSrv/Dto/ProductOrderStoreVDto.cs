using Application.Common.Dto.Field;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.Order.ProductOrderItemSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.Order.ProductOrderStoreSrv.Dto
{
    public class ProductOrderStoreVDto : Id_FieldDto
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
        public long? ShippingSlotId { get; set; }
        public System.DateTime? ShippingSlotDate { get; set; }
        public Entities.Entities.ShippingField.ShippingProviderEnum? ShippingProvider { get; set; }
        public Entities.Entities.ShippingField.ShippingPaymentModeEnum? ShippingPaymentMode { get; set; }
        public double ShippingQuotedPrice { get; set; }
        public double PaymentPrice { get; set; }
        public DeliveryVDto Delivery { get; set; }

        // مرسوله (AutoMapper از Shipment.* پر می‌کند؛ فقط وقتی Shipment همراه سفارش لود شده باشد)
        public Entities.Entities.ShippingField.ShipmentStatusEnum? ShipmentStatus { get; set; }
        public System.DateTime? ShipmentSlotStartUtc { get; set; }
        public System.DateTime? ShipmentSlotEndUtc { get; set; }
        public System.DateTime? ShipmentPickupDeadlineUtc { get; set; }
        public string ShipmentTrackingCode { get; set; }
        /// <summary>کد ۵ رقمی تحویل؛ فقط در پاسخِ خودِ مشتری پر می‌شود و برای فروشنده/ادمین همیشه خالی است.</summary>
        public string ShipmentDeliveryCode { get; set; }

        public StoreMinVDto Store { get; set; }
        public List<ProductOrderItemVDto> ProductOrderItems { get; set; }

    }

}
