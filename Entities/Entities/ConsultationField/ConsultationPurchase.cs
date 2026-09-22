using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    // خرید یک پکیج مشاوره توسط کاربر. قیمت/کانال/مدت در لحظه‌ی خرید کپی می‌شوند (تغییر بعدی پکیج اثری ندارد).
    // وضعیت: ConsultationPurchaseStatusEnum. پنجره‌ی زمانی از لحظه‌ی «شروع» نماینده (StartDate) تا ExpireDate است.
    public class ConsultationPurchase : Id_Field
    {
        public string PurchaseCode { get; set; }
        public long UserId { get; set; }
        public long ConsultationPackageId { get; set; }
        public long CompanionId { get; set; }

        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        // قیمت پکیج در لحظه‌ی خرید (قبل از تخفیف)
        public double Price { get; set; }

        public int Status { get; set; }

        // ---- پرداخت (الگوی Insurance/PastilAI)
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public double Discount { get; set; }
        public long? PaymentId { get; set; }
        public DateTime? PaidDate { get; set; }

        // ---- سهم درآمد (فعلاً کارمزد ۰٪: همه‌ی مبلغ سهم کلینیک)
        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }

        // ---- شروع و پنجره‌ی زمانی
        public DateTime? StartDeadline { get; set; }
        public long? AgentUserId { get; set; }
        public long? OnlineSessionId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }

        // ---- لغو / بازپرداخت
        public DateTime? CancelDate { get; set; }
        public string CancelReason { get; set; }
        public DateTime? RefundDate { get; set; }

        // ---- نظر کاربر (فقط بعد از Status=Completed، یک‌بار)
        public int? Rate { get; set; }
        public string ReviewText { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime CreateDate { get; set; }

        public User User { get; set; }
        public User AgentUser { get; set; }
        public Companion Companion { get; set; }
        public ConsultationPackage ConsultationPackage { get; set; }
        public OnlineSession OnlineSession { get; set; }
        public Rebate Rebate { get; set; }
    }
}
