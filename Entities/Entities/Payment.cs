using Entities.Entities.CommonField;
using Entities.Entities.Security;
using Entities.Entities.PastilAIField;
using System;

namespace Entities.Entities
{
    public class Payment : Id_Field
    {
        public string PaymentCode { get; set; }
        public string IdempotencyKey { get; set; }
        public long? MerchantId { get; set; }
        public long? RebateId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public string RefNumber { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public string Description { get; set; }
        public bool? IsSuccess { get; set; }
        public bool IsOnline { get; set; }
        public long? FileId { get; set; }
        public long UserId { get; set; }
        public long? ApprovedByUserId { get; set; }
        public string ApprovedIp { get; set; }
        public long TypeId { get; set; }
        public string CallBackTypeLabel { get; set; }
        public string CallBackId { get; set; }
        public string Token { get; set; }
        public string CallbackToken { get; set; }
        public string GatewayStatus { get; set; }
        public string PaymentUrl { get; set; }
        public bool PaymentIsLink { get; set; }
        public double GrossAmount { get; set; }
        public double RebateAmount { get; set; }
        public double WalletAmount { get; set; }
        public DateTime? AppliedDate { get; set; }
        public PastilAiSubscription PastilAiSubscription { get; set; }
        public Merchant Merchant { get; set; }
        public Rebate Rebate { get; set; }
        public File File { get; set; }
        public ProductOrder ProductOrder { get; set; }
        public Wallet Wallet { get; set; }
        public Code Type { get; set; }
        public User User { get; set; }

    }
}
