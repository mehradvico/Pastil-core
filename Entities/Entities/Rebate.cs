using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Entities
{
    public class Rebate : Name_Field
    {
        public long? UserId { get; set; }
        public long TypeId { get; set; }
        public string CodeValue { get; set; }
        public double PriceValue { get; set; }
        public double MinCartPrice { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public bool IsPriceRebate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public int UseCount { get; set; }
        public int UsedCount { get; set; }
        public int MaxUsePerUser { get; set; }
        public long? ProductId { get; set; }
        public long? ClubRewardId { get; set; }
        // محدودیت کد به یک ارائه‌دهنده (Store، Coach، Clinic، Groomer، Pansion، School). null = همه
        public PastilClubField.ClubRewardTargetTypeEnum? TargetType { get; set; }
        public long? TargetId { get; set; }
        // null در کدهای قدیمی یعنی ۰٪ پاستیل و ۱۰۰٪ مرکز برای کد اختصاصی مرکز.
        [Column(TypeName = "decimal(5,2)")]
        public decimal? SiteFundedPercent { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ProviderFundedPercent { get; set; }
        public User User { get; set; }
        public Product Product { get; set; }
        public Code Type { get; set; }
        public PastilClubField.ClubCoupon ClubCoupon { get; set; }
    }
}
