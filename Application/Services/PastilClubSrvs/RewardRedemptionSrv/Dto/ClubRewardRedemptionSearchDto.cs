using Application.Common.Dto.Result;
using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto
{
    public class ClubRewardRedemptionSearchDto : BaseSearchDto<ClubRewardRedemptionVDto>
    {
        public ClubRewardRedemptionSearchDto(ClubRewardRedemptionInputDto dto) : base(dto)
        {
            UserId = dto.UserId;
            RewardTemplateId = dto.RewardTemplateId;
            Status = dto.Status;
            RewardType = dto.RewardType;
            FromDate = dto.FromDate;
            ToDate = dto.ToDate;
        }

        public long? UserId { get; set; }
        public long? RewardTemplateId { get; set; }
        public ClubRewardRedemptionStatusEnum? Status { get; set; }
        public ClubRewardTypeEnum? RewardType { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
