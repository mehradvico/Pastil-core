using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv;
using System;

namespace Application.Services.CompanionSrvs.CompanionDayScheduleSrv
{
    // قواعد خالص (بدون دیتابیس) برنامه‌ی روز نماینده: ساعت شروع/پایان آیتم، وضعیت، سن پت.
    public static class CompanionDayScheduleRules
    {
        // ساعت شروع/پایان رزرو خدمت: «HH:mm» بازه‌ی انتخاب‌شده روی تاریخ DoDate. اگر بازه نامعتبر/خالی بود، شروع همان DoDate و پایان نامشخص.
        public static (DateTime Start, DateTime? End) ServiceRange(DateTime doDate, string startTime, string endTime)
        {
            if (!string.IsNullOrWhiteSpace(startTime) && !string.IsNullOrWhiteSpace(endTime) &&
                ReservationScheduleValidator.TryGetServiceTimeRange(startTime, endTime, out var s, out var e))
                return (doDate.Date.Add(s), doDate.Date.Add(e));
            if (!string.IsNullOrWhiteSpace(startTime) &&
                ReservationScheduleValidator.TryGetServiceStartDateTime(doDate, startTime, out var start))
                return (start, null);
            return (doDate, null);
        }

        public static int ServiceStatus(bool isCancel, DateTime? doneDate, DateTime start, DateTime? end, DateTime now)
        {
            if (isCancel)
                return DayItemStatus.Cancelled;
            if (doneDate.HasValue)
                return DayItemStatus.Done;
            if (now < start)
                return DayItemStatus.Upcoming;
            // بازه‌ی پایان مشخص ⇒ تا پایان «الان»؛ بعدش «از دست رفته». بدون پایان: تا پایان همان روز «الان» حساب نمی‌کنیم، فقط از ساعت شروع.
            if (end.HasValue)
                return now < end.Value ? DayItemStatus.Now : DayItemStatus.Missed;
            return DayItemStatus.Now;
        }

        // وضعیت مشاوره‌ی رزروشده بر اساس وضعیت خرید (ConsultationPurchaseStatusEnum)
        public static int ConsultationStatus(int purchaseStatus, DateTime? startDeadline, DateTime now) => (ConsultationPurchaseStatusEnum)purchaseStatus switch
        {
            ConsultationPurchaseStatusEnum.Completed => DayItemStatus.Done,
            ConsultationPurchaseStatusEnum.Active => DayItemStatus.Now,
            ConsultationPurchaseStatusEnum.Paid => startDeadline.HasValue && now > startDeadline.Value ? DayItemStatus.Missed : DayItemStatus.Upcoming,
            ConsultationPurchaseStatusEnum.Expired => DayItemStatus.Missed,
            ConsultationPurchaseStatusEnum.Cancelled => DayItemStatus.Cancelled,
            ConsultationPurchaseStatusEnum.Refunded => DayItemStatus.Cancelled,
            _ => DayItemStatus.Upcoming
        };

        public static bool ConsultationCanStart(int purchaseStatus, DateTime? startDeadline, DateTime now, DateTime? scheduledStart) =>
            ConsultationPurchaseRules.CanStart(purchaseStatus, startDeadline, now, scheduledStart);

        // سن به ماه؛ تاریخ تولد خالی/آینده ⇒ ۰
        public static int AgeMonths(DateTime birthday, DateTime now)
        {
            if (birthday == default || birthday > now)
                return 0;
            var months = (now.Year - birthday.Year) * 12 + now.Month - birthday.Month;
            if (now.Day < birthday.Day)
                months--;
            return Math.Max(0, months);
        }

        public static string TimeText(DateTime? value) => value.HasValue ? value.Value.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture) : null;
    }
}
