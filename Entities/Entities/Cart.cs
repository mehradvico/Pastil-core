using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Cart : Id_Field
    {
        public string UniqueId { get; set; }
        public long? AddressId { get; set; }
        public long? MerchantId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int ItemCount { get; set; }
        public double BasePrice { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public double PaymentPrice { get; set; }
        public long? UserId { get; set; }
        public long? RebateId { get; set; }
        public string ReferralCode { get; set; }
        public double RebatePrice { get; set; }
        public long? DeliveryId { get; set; }
        public double DeliveryPrice { get; set; }
        public bool Changed { get; set; }
        public long? ClubFreeDeliveryBenefitId { get; set; }
        public double ClubDeliveryDiscount { get; set; }

        public Rebate Rebate { get; set; }
        public User User { get; set; }
        public Address Address { get; set; }
        public Merchant Merchant { get; set; }
        public Delivery Delivery { get; set; }
        public ICollection<CartStore> CartStores { get; set; }
        public PastilClubField.ClubFreeDeliveryBenefit ClubFreeDeliveryBenefit { get; set; }
        //public ICollection<CartItem> CartItems { get; set; }

    }
}
