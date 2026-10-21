using System;

namespace Application.Services.Order.ShippingSrv
{
    // واحد مبلغ‌های API میاره (برآورد قیمت /estimate/price/، مبلغ سطح‌های Boost، و delivery_cost وب‌هوک).
    //
    // قرارداد فعلی محصول: عدد خام میاره همان مبلغ تومانی مشتری است؛ مثلاً ۳۲۰٬۰۰۰ خام = ۳۲۰٬۰۰۰ تومان.
    // مقدار قدیمی Shipping:Miare:AmountUnit ممکن است در env باقی مانده باشد، ولی دیگر نباید مبلغ مشتری را تقسیم بر ۱۰ کند.
    // ProviderCost همچنان در پایگاه داده به ریال ذخیره می‌شود.
    public static class MiareAmountRules
    {
        public static bool IsRial(string unit)
        {
            var value = (unit ?? string.Empty).Trim();
            return value.Equals("Rial", StringComparison.OrdinalIgnoreCase)
                   || value.Equals("IRR", StringComparison.OrdinalIgnoreCase)
                   || value.Equals("ریال", StringComparison.Ordinal);
        }

        /// <summary>عدد خام میاره را بدون تغییر به‌عنوان تومان استفاده می‌کند.</summary>
        public static double ToToman(double raw, string unit) => raw;

        /// <summary>مبلغ تومانی میاره را برای ProviderCost به ریال تبدیل می‌کند.</summary>
        public static double ToRial(double raw, string unit) => raw * 10.0;
    }
}
