using Application.Services.Dto;
using Application.Services.Order.MerchantSrv.Dto;

namespace Application.Services.Order.PaymentSrv.Dto
{
    public class PaymentStartDto
    {
        public bool IsOnline { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public long PaymentId { get; set; }
        public long? MerchantId { get; set; }
        public long? RebateId { get; set; }
        public double Amount { get; set; }
        public double GrossAmount { get; set; }
        public double RebateAmount { get; set; }
        public double WalletAmount { get; set; }
        public MerchantVDto Merchant { get; set; }
        public string PaymentUrl { get; set; }
        public bool PaymentIsLink { get; set; }
        public bool IsTestMode { get; set; }
        public string TestSuccessUrl { get; set; }
        public string TestFailureUrl { get; set; }
        public long? UserId { get; set; }
        public long TypeId { get; set; }
        public string CallBackTypeLabel { get; set; }
        public string CallBackId { get; set; }
        public string CallbackUrl { get; set; }
        public UserMinVDto User { get; set; }
    }
}
