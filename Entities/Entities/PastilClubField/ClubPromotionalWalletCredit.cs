using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilClubField
{
    public class ClubPromotionalWalletCredit : Id_Field
    {
        public long UserId { get; set; }
        public long RewardRedemptionId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public ClubRewardTargetTypeEnum ServiceScopeType { get; set; }
        public long? ServiceScopeId { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public ClubPromotionalCreditStatusEnum Status { get; set; }
        public DateTime CreateDate { get; set; }
        public byte[] RowVersion { get; set; }

        public User User { get; set; }
        public ClubRewardRedemption RewardRedemption { get; set; }
    }
}
