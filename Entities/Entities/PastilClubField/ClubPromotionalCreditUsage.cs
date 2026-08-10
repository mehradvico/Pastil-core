using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.PastilClubField
{
    public class ClubPromotionalCreditUsage : Id_Field
    {
        public long PromotionalCreditId { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public ClubRewardApplicationMethodEnum ApplicationMethod { get; set; }
        public string ReferenceKey { get; set; }
        public DateTime CreateDate { get; set; }

        public ClubPromotionalWalletCredit PromotionalCredit { get; set; }
    }
}
