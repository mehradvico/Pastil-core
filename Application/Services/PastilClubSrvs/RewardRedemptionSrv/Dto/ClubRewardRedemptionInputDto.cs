using Application.Common.Dto.Input;
using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto
{
    public class ClubRewardRedemptionInputDto : BaseInputDto
    {
        public long? UserId { get; set; }
        public long? RewardTemplateId { get; set; }
        public ClubRewardRedemptionStatusEnum? Status { get; set; }
        public ClubRewardTypeEnum? RewardType { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
