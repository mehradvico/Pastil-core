using Application.Services.Dto;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Order.ProductOrderStoreSrv.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Order.ProductOrderSrv.Dto
{
    public class ProductOrderVDto
    {
        public string Id { get; set; }

        public long UserId { get; set; }
        public long? AddressId { get; set; }
        public long PaymentTypeId { get; set; }
        public long ProductOrderStatusId { get; set; }
        public long ProductOrderStateId { get; set; }
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public double DiscountPrice { get; set; }
        public double PaymentPrice { get; set; }
        public DateTime CreateDate { get; set; }
        public string BonusCode { get; set; }
        public string UserDescription { get; set; }
        public string AdminDescription { get; set; }
        public string StateDescription { get; set; }
        public bool IsPaid { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public long? DeliveryTypeId { get; set; }
        public double DeliveryPrice { get; set; }
        public long? ClubFreeDeliveryBenefitId { get; set; }
        public double ClubDeliveryDiscount { get; set; }
        public string TrackingCode { get; set; }
        public DateTime? HasCancelRequestDate { get; set; }
        public double WalletPrice { get; set; }
        public double SiteShare { get; set; }
        public double StoreShare { get; set; }
        public RebateVDto Rebate { get; set; }
        public UserVDto User { get; set; }
        public AddressVDto Address { get; set; }
        public CodeVDto PaymentType { get; set; }
        public CodeVDto ProductOrderStatus { get; set; }
        public CodeVDto ProductOrderState { get; set; }
        public CodeVDto DeliveryType { get; set; }
        public List<ProductOrderStoreVDto> ProductOrderStores { get; set; }
    }
}
