using Application.Common.Dto.Field;
using Application.Services.Order.ProductOrderItemSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.Order.ProductOrderStoreSrv.Dto
{
    public class ProductOrderStoreDto : Id_FieldDto
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
        public Entities.Entities.ShippingField.ShippingProviderEnum? ShippingProvider { get; set; }
        public Entities.Entities.ShippingField.ShippingPaymentModeEnum? ShippingPaymentMode { get; set; }
        public double ShippingQuotedPrice { get; set; }
        public double PaymentPrice { get; set; }
        public List<ProductOrderItemDto> ProductOrderItems { get; set; }



    }
}
