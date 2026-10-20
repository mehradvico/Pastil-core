using Entities.Entities.ShippingField;
using System;

namespace Application.Services.Order.ShippingSrv
{
    // تمدید دستی مهلت فروشنده توسط ادمین (مثلاً وقتی مشکل از سمت پاستیل/میاره بود و فروشنده نتوانست «درخواست پیک» بزند).
    public static class ShipmentDeadlineRules
    {
        public const string SellerConfirmExpiredReason = "SellerConfirmExpired";
        public const int MinExtendMinutes = 15;
        public const int MaxExtendMinutes = 480;
        public const int DefaultExtendMinutes = 60;

        public static bool IsValidMinutes(int minutes) => minutes >= MinExtendMinutes && minutes <= MaxExtendMinutes;

        // منتظر فروشنده، در حال آماده‌سازی (بازه‌ای)، یا مرسوله‌ای که فقط به‌خاطر پایان مهلت فروشنده ناموفق شده (احیا می‌شود)
        public static bool CanExtend(ShipmentStatusEnum status, string failureReason) =>
            status == ShipmentStatusEnum.AwaitingSellerConfirm ||
            status == ShipmentStatusEnum.Preparing ||
            (status == ShipmentStatusEnum.Failed && string.Equals(failureReason, SellerConfirmExpiredReason, StringComparison.Ordinal));

        // مهلت جدید: از دیرترِ «مهلت فعلی» و «الان» + دقیقه‌ها (مهلتِ گذشته از الان حساب می‌شود)
        public static DateTime NewDeadline(DateTime? current, DateTime nowUtc, int minutes) =>
            ((current.HasValue && current.Value > nowUtc) ? current.Value : nowUtc).AddMinutes(minutes);
    }
}
