using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.GroomerField
{
    /// <summary>
    /// رزرو نوبت آرایشگاه (جایگزین CompanionReserve برای آرایشگاه). یک پت، یک بسته‌ی قیمتی.
    /// مبلغ آنلاین (OnlineAmount) = پیش‌پرداخت بسته (یا کل قیمت اگر پیش‌پرداخت ندارد) + هزینه‌ی دریافت/تحویل؛
    /// باقی‌مانده (Price − OnlineAmount) حضوری در آرایشگاه پرداخت می‌شود و در حسابداری سایت نمی‌آید.
    /// </summary>
    public class GroomerReserve : Id_Field
    {
        public string ReserveCode { get; set; }
        public long GroomerId { get; set; }
        public long GroomerServiceId { get; set; }
        public long GroomerServicePackageId { get; set; }
        public long BookerId { get; set; }
        public long UserPetId { get; set; }
        /// <summary>گرومر انتخابی (از GroomerServiceUser)؛ خالی = آرایشگاه انتخاب می‌کند.</summary>
        public long? AssigneeUserId { get; set; }
        /// <summary>ردیف ساعت کاری که این نوبت داخل آن افتاده (برای ظرفیت).</summary>
        public long? GroomerTimeId { get; set; }

        public DateTime DoDate { get; set; }
        public int DurationMinutes { get; set; }
        /// <summary>DoDate + DurationMinutes؛ ذخیره می‌شود تا بررسی هم‌پوشانی با یک کوئری ساده انجام شود.</summary>
        public DateTime EndDate { get; set; }

        public bool IsHomeService { get; set; }
        public long? AddressId { get; set; }
        public bool PickupRequested { get; set; }
        public string BookerNote { get; set; }

        /// <summary>قیمت بسته در لحظه‌ی رزرو (اسنپ‌شات).</summary>
        public double PackagePrice { get; set; }
        public double PickupFee { get; set; }
        /// <summary>کل مبلغ خدمت = PackagePrice + PickupFee.</summary>
        public double Price { get; set; }
        /// <summary>سقف مبلغ قابل پرداخت آنلاین قبل از تخفیف.</summary>
        public double OnlineAmount { get; set; }

        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        /// <summary>مبلغ نهایی پرداخت آنلاین = OnlineAmount − RebatePrice (حداقل صفر).</summary>
        public double PaymentPrice { get; set; }

        /// <summary>درصد کمیسیون سایت در لحظه‌ی پرداخت (اسنپ‌شات از خدمت/آرایشگاه).</summary>
        public decimal CommissionPercent { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        /// <summary>true = در یک تسویه‌ی ارائه‌دهنده پرداخت شده (یا پرداخت تستی).</summary>
        public bool Permitted { get; set; }

        /// <summary>GroomerReserveStatusEnum: ۰ ثبت‌شده، ۱ پرداخت‌شده، ۲ تکمیل‌شده.</summary>
        public int Status { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public DateTime? CancelDate { get; set; }
        public bool CancelledByBooker { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? DoneDate { get; set; }
        /// <summary>تعداد دفعات تغییر زمان توسط آرایشگاه/ادمین؛ زمان اولیه در OriginalDoDate می‌ماند.</summary>
        public int RescheduleCount { get; set; }
        public DateTime? OriginalDoDate { get; set; }
        public DateTime? LastRescheduleDate { get; set; }

        /// <summary>GroomerReserveOwnerDecisionEnum: ۰ نیاز نیست، ۱ منتظر پاسخ، ۲ تأیید، ۳ رد، ۴ بی‌پاسخ (لغو خودکار).</summary>
        public int OwnerDecision { get; set; }
        public DateTime? OwnerDecisionDate { get; set; }
        public string OwnerDecisionReason { get; set; }
        public DateTime? OwnerApprovalDeadline { get; set; }

        public Groomer Groomer { get; set; }
        public GroomerService GroomerService { get; set; }
        public GroomerServicePackage GroomerServicePackage { get; set; }
        public User Booker { get; set; }
        public UserPet UserPet { get; set; }
        public User Assignee { get; set; }
        public GroomerTime GroomerTime { get; set; }
        public Address Address { get; set; }
        public Rebate Rebate { get; set; }
        public System.Collections.Generic.ICollection<GroomerReserveItem> Items { get; set; }
    }
}
