using Entities.Entities.CommonField;
using Entities.Entities.CompanionField;
using Entities.Entities.PansionField;
using Entities.Entities.Security;
using Entities.Entities.PastilAIField;
using System;

namespace Entities.Entities
{
    public class Wallet : Name_Field
    {
        public bool IsIncrease { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public long UserId { get; set; }
        public long? PaymentId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public long? PastilAiSubscriptionId { get; set; }
        public bool Painding { get; set; }
        public bool Deleted { get; set; }
        public User User { get; set; }
        public Payment Payment { get; set; }
        public ProductOrder ProductOrder { get; set; }
        public CompanionReserve CompanionReserve { get; set; }
        public PansionReserve PansionReserve { get; set; }
        public Trip Trip { get; set; }
        public Cargo Cargo { get; set; }
        public CompanionInsurancePackageSale CompanionInsurancePackageSale { get; set; }
        public PastilAiSubscription PastilAiSubscription { get; set; }
    }
}
