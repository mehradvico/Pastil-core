using Application.Common.Dto.Field;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;

namespace Application.Services.Order.PaymentSrv.Dto
{
    public class PaymentDto : Id_FieldDto
    {
        public long? MerchantId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? PasnionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public long? WalletId { get; set; }
        public string RefNumber { get; set; }
        public string BonusCode { get; set; }
        public double Amount { get; set; }
        public double GrossAmount { get; set; }
        public double RebateAmount { get; set; }
        public double WalletAmount { get; set; }
        public string Description { get; set; }
        public bool? IsSuccess { get; set; }
        public bool IsOnline { get; set; }
        public long TypeId { get; set; }
        public long UserId { get; set; }
        public long? FileId { get; set; }
        public string CallBackTypeLabel { get; set; }
        public string CallBackId { get; set; }
        public DateTime? AppliedDate { get; set; }
        public CodeVDto Type { get; set; }
        public MerchantVDto Merchant { get; set; }
    }
}
