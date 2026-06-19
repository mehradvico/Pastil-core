using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Order.CartStoreSrv.Dto;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.RebateSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Order.CartSrv.Dto
{
    public class CartVDto : Id_FieldDto
    {
        public string UniqueId { get; set; }
        public long? AddressId { get; set; }
        public long? MerchantId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string ReferralCode { get; set; }
        public int ItemCount { get; set; }
        public double BasePrice { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public double PaymentPrice { get; set; }
        public long? UserId { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public long? DeliveryId { get; set; }
        public double DeliveryPrice { get; set; }
        public bool Reserve { get; set; }
        public string ParentOrderId { get; set; }

        public RebateVDto Rebate { get; set; }
        public UserVDto User { get; set; }
        public AddressVDto Address { get; set; }
        public MerchantVDto Merchant { get; set; }
        public DeliveryVDto Delivery { get; set; }
        public List<CartStoreVDto> CartStores { get; set; }
    }

}
