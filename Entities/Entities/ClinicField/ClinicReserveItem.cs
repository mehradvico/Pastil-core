using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>
    /// یک بسته‌ی انتخابی داخل رزرو کلینیک (رزرو چندخدمتی: مثلاً شست‌وشو + کوتاهی ناخن در یک نوبت).
    /// قیمت و مدت در لحظه‌ی رزرو اسنپ‌شات می‌شوند؛ سهم‌ها بعد از پرداخت از روی درصد کمیسیون همان خدمت پر می‌شوند.
    /// </summary>
    public class ClinicReserveItem : Id_Field
    {
        public long ClinicReserveId { get; set; }
        public long ClinicServiceId { get; set; }
        public long ClinicServicePackageId { get; set; }
        /// <summary>قیمت بسته (اسنپ‌شات).</summary>
        public double PackagePrice { get; set; }
        /// <summary>سهم این بسته از مبلغ آنلاین (پیش‌پرداخت بسته، یا کل قیمت اگر پیش‌پرداخت ندارد).</summary>
        public double OnlineAmount { get; set; }
        public int DurationMinutes { get; set; }
        /// <summary>درصد کمیسیون سایت از این بسته (بعد از پرداخت).</summary>
        public decimal CommissionPercent { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }

        /// <summary>نام بسته در لحظه‌ی ثبت (برای تاریخچه).</summary>
        public string PackageName { get; set; }
        /// <summary>ClinicReserveItemStatusEnum: ۱ در انتظار تأیید، ۲ تأییدشده، ۳ لغوشده.</summary>
        public int Status { get; set; } = 1;
        public string StatusReason { get; set; }
        public System.DateTime? StatusChangedDate { get; set; }
        public long? StatusChangedByUserId { get; set; }
        public bool StatusChangedByAdmin { get; set; }
        /// <summary>مبلغی که با لغو این بسته به کیف پول کاربر برگشت.</summary>
        public double RefundAmount { get; set; }
        public System.DateTime? RefundDate { get; set; }
        /// <summary>بسته‌ای که بعد از پرداخت اضافه شده؛ چیزی برایش پیش‌پرداخت نشده و حضوری حساب می‌شود.</summary>
        public bool AddedAfterPayment { get; set; }
        public System.DateTime CreateDate { get; set; }

        public ClinicReserve ClinicReserve { get; set; }
        public ClinicService ClinicService { get; set; }
        public ClinicServicePackage ClinicServicePackage { get; set; }
    }
}
