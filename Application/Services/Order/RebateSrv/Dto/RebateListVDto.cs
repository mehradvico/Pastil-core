using Application.Common.Dto.Field;
using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.Order.RebateSrv.Dto
{
    /// <summary>ردیف لیست کدهای تخفیف یک فروشگاه (با تعداد استفاده و مشخصات کاربرِ مالک کد).</summary>
    public class RebateListVDto : Name_FieldDto
    {
        public string CodeValue { get; set; }
        public double PriceValue { get; set; }
        public bool IsPriceRebate { get; set; }
        public double MinCartPrice { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public bool Active { get; set; }
        public int UseCount { get; set; }
        public int UsedCount { get; set; }
        public int MaxUsePerUser { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public string UserMobile { get; set; }
        public ClubRewardTargetTypeEnum? TargetType { get; set; }
        public long? TargetId { get; set; }
        public long TypeId { get; set; }
    }
}
