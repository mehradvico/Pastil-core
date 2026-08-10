using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilClubField
{
    public class ClubRewardCostTransaction : Id_Field
    {
        public long RewardRedemptionId { get; set; }
        public long UserId { get; set; }
        public ClubRewardTargetTypeEnum BusinessType { get; set; }
        public long? BusinessId { get; set; }
        public ClubRewardTypeEnum RewardType { get; set; }
        public decimal GrossValue { get; set; }
        public decimal PastilFundedValue { get; set; }
        public string OrderId { get; set; }
        public long? ReservationId { get; set; }
        public long? PaymentId { get; set; }
        public DateTime CreateDate { get; set; }

        public ClubRewardRedemption RewardRedemption { get; set; }
        public User User { get; set; }
        public Payment Payment { get; set; }
    }
}
