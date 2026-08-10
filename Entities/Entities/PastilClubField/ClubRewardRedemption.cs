using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilClubField
{
    public class ClubRewardRedemption : Id_Field
    {
        public long UserId { get; set; }
        public long RewardOfferId { get; set; }
        public long RewardTemplateId { get; set; }
        public long PointTransactionId { get; set; }
        public ClubRewardBenefitTypeEnum BenefitType { get; set; }
        public long? BenefitReferenceId { get; set; }
        public long PointSpent { get; set; }
        public DateTimeOffset RedeemedDate { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public ClubRewardRedemptionStatusEnum Status { get; set; }
        public string IdempotencyKey { get; set; }

        public User User { get; set; }
        public ClubRewardOffer RewardOffer { get; set; }
        public ClubRewardTemplate RewardTemplate { get; set; }
        public ClubPointTransaction PointTransaction { get; set; }
    }
}
