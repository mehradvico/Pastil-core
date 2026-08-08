using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Order.MerchantSrv.Dto;

namespace Application.Services.Order.PaymentSrv.Dto
{
    public class PaymentVDto
    {
        public string Id { get; set; }
        public long? MerchantId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? PasnionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public string RefNumber { get; set; }
        public double Amount { get; set; }
        public string BonusCode { get; set; }
        public System.DateTime Datetime { get; set; }
        public string Description { get; set; }
        public bool? IsSuccess { get; set; }
        public bool IsOnline { get; set; }
        public int? FileId { get; set; }
        public string CallBackTypeLabel { get; set; }
        public string CallBackId { get; set; }
        public MerchantVDto Merchant { get; set; }
        public FileVDto File { get; set; }

    }
}
