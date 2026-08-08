using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilAIField
{
    public class PastilAiSubscription : Id_Field
    {
        public long UserId { get; set; }
        public long PlanId { get; set; }
        public long? PaymentId { get; set; }
        public long? RebateId { get; set; }
        public PastilAiSubscriptionStatus Status { get; set; }
        public decimal PriceSnapshot { get; set; }
        public decimal RebatePrice { get; set; }
        public bool FromWallet { get; set; }
        public decimal WalletPrice { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public User User { get; set; }
        public PastilAiPlan Plan { get; set; }
        public Payment Payment { get; set; }
        public Rebate Rebate { get; set; }
        public Wallet Wallet { get; set; }
    }
}
