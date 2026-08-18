using Application.Common.Dto.Field;
using Application.Services.Order.CartItemSrv.Dto;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.Order.CartStoreSrv.Dto
{
    public class CartStoreVDto : Id_FieldDto
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
        public Entities.Entities.ShippingField.ShippingProviderEnum? ShippingProvider { get; set; }
        public Entities.Entities.ShippingField.ShippingPaymentModeEnum? ShippingPaymentMode { get; set; }
        public double ShippingQuotedPrice { get; set; }
        public double PaymentPrice { get; set; }
        public bool Active { get; set; }
        public DeliveryVDto Delivery { get; set; }
        public StoreVDto Store { get; set; }
        public List<CartItemVDto> CartItems { get; set; }
    }
}
