using Application.Common.Enumerable;
using System;

namespace Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Dto
{
    // ورودی خرید: قیمت را کلاینت نمی‌فرستد؛ فقط پکیج، روش پرداخت و (اختیاری) کد تخفیف
    public class ConsultationPurchaseCreateDto
    {
        public long PackageId { get; set; }
        public bool FromWallet { get; set; }
        public long? MerchantId { get; set; }
        public string RebateCode { get; set; }
    }

    public class ConsultationPurchaseVDto
    {
        public long Id { get; set; }
        public string PurchaseCode { get; set; }
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public double RebatePrice { get; set; }
        public double PaymentPrice { get; set; }
        public double WalletPrice { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? StartDeadline { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public long? OnlineSessionId { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? RefundDate { get; set; }
        // ساعت سرور هنگام پاسخ؛ کلاینت شمارش معکوس را با آن هم‌گام می‌کند (نه ساعت دستگاه)
        public DateTime ServerNow { get; set; }
        public int? Rate { get; set; }
        public string ReviewText { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public class ConsultationPurchaseReviewDto
    {
        public int Rate { get; set; }
        public string Text { get; set; }
    }
}
