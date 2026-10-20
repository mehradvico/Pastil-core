using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>
    /// یک بسته‌ی انتخابی داخل رزرو مربی (رزرو چندخدمتی در یک نوبت).
    /// قیمت و مدت در لحظه‌ی رزرو اسنپ‌شات می‌شوند؛ سهم‌ها بعد از پرداخت از روی درصد کمیسیون همان خدمت پر می‌شوند.
    /// </summary>
    public class CoachReserveItem : Id_Field
    {
        public long CoachReserveId { get; set; }
        public long CoachServiceId { get; set; }
        public long CoachServicePackageId { get; set; }
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
        public System.DateTime CreateDate { get; set; }

        public CoachReserve CoachReserve { get; set; }
        public CoachService CoachService { get; set; }
        public CoachServicePackage CoachServicePackage { get; set; }
    }
}
