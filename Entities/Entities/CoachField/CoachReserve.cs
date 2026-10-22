using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.CoachField
{
    /// <summary>
    /// رزرو نوبت مربی (جایگزین CompanionReserve برای مربی). یک پت، یک بسته‌ی قیمتی.
    /// مبلغ آنلاین (OnlineAmount) = پیش‌پرداخت بسته (یا کل قیمت اگر پیش‌پرداخت ندارد) به‌ازای نحوه‌ی ارائه‌ی انتخاب‌شده؛
    /// باقی‌مانده (Price − OnlineAmount) حضوری در مربی پرداخت می‌شود و در حسابداری سایت نمی‌آید.
    /// </summary>
    public class CoachReserve : Id_Field
    {
        public string ReserveCode { get; set; }
        public long CoachId { get; set; }
        public long CoachServiceId { get; set; }
        public long CoachServicePackageId { get; set; }
        public long BookerId { get; set; }
        public long UserPetId { get; set; }
        /// <summary>پزشک/پرسنل انتخابی (از CoachServiceUser)؛ خالی = مربی انتخاب می‌کند.</summary>
        public long? AssigneeUserId { get; set; }
        /// <summary>ردیف ساعت کاری که این نوبت داخل آن افتاده (برای ظرفیت).</summary>
        public long? CoachTimeId { get; set; }

        public DateTime DoDate { get; set; }
        public int DurationMinutes { get; set; }
        /// <summary>DoDate + DurationMinutes؛ ذخیره می‌شود تا بررسی هم‌پوشانی با یک کوئری ساده انجام شود.</summary>
        public DateTime EndDate { get; set; }

        /// <summary>CoachServiceModeEnum: ۳۷ آنلاین، ۳۸ حضوری در مربی، ۳۹ در محل مشتری.</summary>
        public int Mode { get; set; }
        /// <summary>نشانی مشتری؛ فقط برای Mode=InPlace.</summary>
        public long? AddressId { get; set; }
        public string BookerNote { get; set; }

        /// <summary>قیمت بسته در لحظه‌ی رزرو (اسنپ‌شات).</summary>
        public double PackagePrice { get; set; }
        /// <summary>کل مبلغ خدمت = جمع قیمت بسته‌ها.</summary>
        public double Price { get; set; }
        /// <summary>سقف مبلغ قابل پرداخت آنلاین قبل از تخفیف.</summary>
        public double OnlineAmount { get; set; }

        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        /// <summary>مبلغ نهایی پرداخت آنلاین = OnlineAmount − RebatePrice (حداقل صفر).</summary>
        public double PaymentPrice { get; set; }

        /// <summary>درصد کمیسیون سایت در لحظه‌ی پرداخت (اسنپ‌شات از خدمت/مربی).</summary>
        public decimal CommissionPercent { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        /// <summary>true = در یک تسویه‌ی ارائه‌دهنده پرداخت شده (یا پرداخت تستی).</summary>
        public bool Permitted { get; set; }

        /// <summary>CoachReserveStatusEnum: ۰ ثبت‌شده، ۱ پرداخت‌شده، ۲ تکمیل‌شده.</summary>
        public int Status { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public DateTime? CancelDate { get; set; }
        public bool CancelledByBooker { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? DoneDate { get; set; }
        /// <summary>تعداد دفعات تغییر زمان توسط مربی/ادمین؛ زمان اولیه در OriginalDoDate می‌ماند.</summary>
        public int RescheduleCount { get; set; }
        public DateTime? OriginalDoDate { get; set; }
        public DateTime? LastRescheduleDate { get; set; }
        /// <summary>snapshot مدت اقامت رزرو = بیشینه‌ی StayDays بسته‌های انتخاب‌شده (در صورت وجود).</summary>
        public int? StayDays { get; set; }
        /// <summary>snapshot جمع تعداد جلسات بسته‌های انتخاب‌شده (در صورت وجود).</summary>
        public int? SessionCount { get; set; }
        /// <summary>تماس تصویری/صوتی درون‌برنامه‌ای (فقط Mode=آنلاین): لحظه‌ی وصل‌شدن هر دو طرف و لحظه‌ی پایان.</summary>
        public DateTime? CallStartDate { get; set; }
        public DateTime? CallEndDate { get; set; }
        /// <summary>CoachReserveOwnerDecisionEnum: ۰ نیاز نیست، ۱ منتظر پاسخ، ۲ تأیید، ۳ رد، ۴ بی‌پاسخ (لغو خودکار).</summary>
        public int OwnerDecision { get; set; }
        public DateTime? OwnerDecisionDate { get; set; }
        public string OwnerDecisionReason { get; set; }
        public DateTime? OwnerApprovalDeadline { get; set; }

        public Coach Coach { get; set; }
        public CoachService CoachService { get; set; }
        public CoachServicePackage CoachServicePackage { get; set; }
        public User Booker { get; set; }
        public UserPet UserPet { get; set; }
        public User Assignee { get; set; }
        public CoachTime CoachTime { get; set; }
        public Address Address { get; set; }
        public Rebate Rebate { get; set; }
        public System.Collections.Generic.ICollection<CoachReserveItem> Items { get; set; }
    }
}
