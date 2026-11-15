using Application.Common.Dto.Input;
using Entities.Entities.PastilClubField;

namespace Application.Services.Order.RebateSrv.Dto
{
    /// <summary>جستجوی کدهای تخفیف بر اساس هدف (مثلاً یک فروشگاه) و/یا کاربرِ مالک کد.</summary>
    public class RebateInputDto : BaseInputDto
    {
        public ClubRewardTargetTypeEnum? TargetType { get; set; }
        public long? TargetId { get; set; }
        public long? UserId { get; set; }
    }
}
