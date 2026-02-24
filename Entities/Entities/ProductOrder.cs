using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class ProductOrder
    {
        public string Id { get; set; }
        public long UserId { get; set; }
        public long? AddressId { get; set; }
        public long PaymentTypeId { get; set; }
        public long ProductOrderStatusId { get; set; }
        public long ProductOrderStateId { get; set; }
        public string BonusCode { get; set; }
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public double DiscountPrice { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public double SiteShare {  get; set; }
        public double StoreShare { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string UserDescription { get; set; }
        public string AdminDescription { get; set; }
        public string StateDescription { get; set; }
        public bool IsPaid { get; set; }
        public bool Deleted { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public long? DeliveryTypeId { get; set; }
        public double DeliveryPrice { get; set; }
        public string TrackingCode { get; set; }
        public DateTime? CancelRequestDate { get; set; }
        public string ParentOrderId { get; set; }
        public string ChildOrderId { get; set; }
        public bool Permitted { get; set; }

        public DateTime? ReserveDate { get; set; }
        public Rebate Rebate { get; set; }
        public User User { get; set; }
        public Address Address { get; set; }
        public Code DeliveryType { get; set; }
        public Code PaymentType { get; set; }
        public Code ProductOrderStatus { get; set; }
        public Code ProductOrderState { get; set; }
        public ICollection<ProductOrderStore> ProductOrderStores { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public Wallet Wallet { get; set; }

    }
}
