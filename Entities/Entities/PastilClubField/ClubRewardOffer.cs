using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities.PastilClubField
{
    public class ClubRewardOffer : Id_Field
    {
        public long UserId { get; set; }
        public long RewardTemplateId { get; set; }
        public ClubRewardOfferSourceEnum SourceType { get; set; }
        public long? AutomationRuleId { get; set; }
        public ClubRewardOfferStatusEnum Status { get; set; }
        public long PointCostSnapshot { get; set; }
        public DateTimeOffset GeneratedDate { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
        public DateTimeOffset? RejectedDate { get; set; }
        public long? ApprovedByAdminId { get; set; }
        public long? RejectedByAdminId { get; set; }
        public string RejectReason { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RedeemedDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public User User { get; set; }
        public ClubRewardTemplate RewardTemplate { get; set; }
        public ClubRewardRedemption Redemption { get; set; }
    }
}
