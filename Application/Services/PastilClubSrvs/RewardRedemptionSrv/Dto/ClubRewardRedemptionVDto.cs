using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto
{
    public class ClubRewardRedemptionVDto
    {
        public long Id { get; set; }
        public long RewardOfferId { get; set; }
        public long RewardTemplateId { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public string RewardTitle { get; set; }
        public ClubRewardTypeEnum RewardType { get; set; }
        public long PointTransactionId { get; set; }
        public long PointSpent { get; set; }
        public long RemainingPoint { get; set; }
        public ClubRewardBenefitTypeEnum BenefitType { get; set; }
        public long? BenefitReferenceId { get; set; }
        public DateTimeOffset RedeemedDate { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public ClubRewardRedemptionStatusEnum Status { get; set; }
    }
}
