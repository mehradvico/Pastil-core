using Application.Services.Order.ProductOrderStoreSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Order.ProductOrderSrv.Dto
{
    public class ProductOrderDto
    {
        public string Id { get; set; }
        public string OrderCode { get; set; }

        public long UserId { get; set; }
        public long? AddressId { get; set; }
        public long PaymentTypeId { get; set; }
        public long ProductOrderStatusId { get; set; }
        public long ProductOrderStateId { get; set; }
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public double DiscountPrice { get; set; }
        public double WalletPrice { get; set; }
        public double FinalPrice { get; set; }
        public double PaymentPrice { get; set; }
        public string BonusCode { get; set; }
        public string UserDescription { get; set; }
        public string AdminDescription { get; set; }
        public string StateDescription { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public long? DeliveryTypeId { get; set; }
        public double DeliveryPrice { get; set; }
        public long? ClubFreeDeliveryBenefitId { get; set; }
        public double ClubDeliveryDiscount { get; set; }
        public string TrackingCode { get; set; }
        public DateTime? CancelRequest { get; set; }

        public List<ProductOrderStoreDto> ProductOrderStores { get; set; }
    }
}
