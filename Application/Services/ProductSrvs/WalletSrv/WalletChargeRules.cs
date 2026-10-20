using System;

namespace Application.Services.ProductSrvs.WalletSrv
{
    // شارژ کیف پول توسط کاربر (تومان): فقط عدد صحیح بین حداقل و حداکثر. قبلاً منفی/صفر بی‌صدا ۱۰٬۰۰۰ می‌شد و عدد ۱۵ رقمی
    // تا درگاه می‌رفت (Convert.ToInt32(amount * 10) آن‌جا سرریز می‌کرد).
    public static class WalletChargeRules
    {
        public const double MinAmount = 10_000;
        public const double MaxAmount = 10_000_000;

        public static bool IsValid(double amount) =>
            !double.IsNaN(amount) && !double.IsInfinity(amount) &&
            amount == Math.Floor(amount) &&
            amount >= MinAmount && amount <= MaxAmount;
    }
}
