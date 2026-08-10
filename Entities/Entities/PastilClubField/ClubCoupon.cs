using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilClubField
{
    public class ClubCoupon : Id_Field
    {
        public long RewardRedemptionId { get; set; }
        public long UserId { get; set; }
        public long RebateId { get; set; }
        public string Code { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTimeOffset? UsedDate { get; set; }
        public string OrderId { get; set; }
        public long? ReservationId { get; set; }
        public long? PaymentId { get; set; }
        public DateTime CreateDate { get; set; }

        public ClubRewardRedemption RewardRedemption { get; set; }
        public User User { get; set; }
        public Rebate Rebate { get; set; }
        public Payment Payment { get; set; }
    }
}
