using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>
    /// یک بسته‌ی انتخابی داخل رزرو آرایشگاه (رزرو چندخدمتی: مثلاً شست‌وشو + کوتاهی ناخن در یک نوبت).
    /// قیمت و مدت در لحظه‌ی رزرو اسنپ‌شات می‌شوند؛ سهم‌ها بعد از پرداخت از روی درصد کمیسیون همان خدمت پر می‌شوند.
    /// </summary>
    public class GroomerReserveItem : Id_Field
    {
        public long GroomerReserveId { get; set; }
        public long GroomerServiceId { get; set; }
        public long GroomerServicePackageId { get; set; }
        /// <summary>قیمت بسته (اسنپ‌شات).</summary>
        public double PackagePrice { get; set; }
        /// <summary>سهم این بسته از مبلغ آنلاین (پیش‌پرداخت بسته، یا کل قیمت اگر پیش‌پرداخت ندارد).</summary>
        public double OnlineAmount { get; set; }
        public int DurationMinutes { get; set; }
        /// <summary>درصد کمیسیون سایت از این بسته (بعد از پرداخت).</summary>
        public decimal CommissionPercent { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }

        public GroomerReserve GroomerReserve { get; set; }
        public GroomerService GroomerService { get; set; }
        public GroomerServicePackage GroomerServicePackage { get; set; }
    }
}
