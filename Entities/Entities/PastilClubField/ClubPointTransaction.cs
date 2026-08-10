using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilClubField
{
    public class ClubPointTransaction : Id_Field
    {
        public long UserId { get; set; }
        public long PointAccountId { get; set; }
        public ClubPointTransactionTypeEnum TransactionType { get; set; }
        public long Amount { get; set; }
        public long AvailableBefore { get; set; }
        public long AvailableAfter { get; set; }
        public long DebtBefore { get; set; }
        public long DebtAfter { get; set; }
        public ClubPointSourceTypeEnum SourceType { get; set; }
        public long? SourceId { get; set; }
        public long? PointRuleId { get; set; }
        public long? ReferralId { get; set; }
        public long? RewardRedemptionId { get; set; }
        public long? ParentTransactionId { get; set; }
        public string Description { get; set; }
        public string IdempotencyKey { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedByUserId { get; set; }
        public long? CreatedByAdminId { get; set; }

        public User User { get; set; }
        public ClubPointAccount PointAccount { get; set; }
        public ClubPointRule PointRule { get; set; }
        public ClubPointTransaction ParentTransaction { get; set; }
    }
}
