using System;

namespace Application.Services.Order.ShippingSrv
{
    // واحد مبلغ‌های API میاره (برآورد قیمت /estimate/price/، مبلغ سطح‌های Boost، و delivery_cost وب‌هوک).
    //
    // ⚠️ میاره بین محیط‌ها واحد یکسان نمی‌دهد: Staging به «تومان» (۳۲٬۰۰۰ برای ≈۲٫۸ کیلومتر) ولی Production برای ≈۱۱ کیلومتر ۹۰۱٬۰۰۰ و ۸۵۳٬۰۰۰
    // برگرداند (۲۵ مهر ۱۴۰۵، لاگ Api) که ۱۰ برابر مقیاس Staging و در تومان غیرواقعی است؛ یعنی «ریال». کد قبلاً همه‌چیز را تومان فرض می‌کرد
    // و مشتری ۱۰ برابر قیمت واقعی می‌دید. واحد حالا با Shipping:Miare:AmountUnit (Toman پیش‌فرض | Rial) تعیین می‌شود.
    // سیستم پاستیل همه‌جا تومان نشان می‌دهد؛ ProviderCost (هزینه‌ی واقعی سفر) در پایگاه داده «ریال» ذخیره می‌شود.
    public static class MiareAmountRules
    {
        public static bool IsRial(string unit)
        {
            var value = (unit ?? string.Empty).Trim();
            return value.Equals("Rial", StringComparison.OrdinalIgnoreCase)
                   || value.Equals("IRR", StringComparison.OrdinalIgnoreCase)
                   || value.Equals("ریال", StringComparison.Ordinal);
        }

        /// <summary>مبلغ خام میاره را به تومان (واحد نمایش و پرداخت مشتری) تبدیل می‌کند؛ ریال ÷ ۱۰ و رُند به تومان کامل.</summary>
        public static double ToToman(double raw, string unit) =>
            IsRial(unit) ? Math.Round(raw / 10.0, MidpointRounding.AwayFromZero) : raw;

        /// <summary>مبلغ خام میاره را به ریال (واحد ذخیره‌ی ProviderCost) تبدیل می‌کند.</summary>
        public static double ToRial(double raw, string unit) => IsRial(unit) ? raw : raw * 10.0;
    }
}
