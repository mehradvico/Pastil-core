using System;

namespace Application.Services.TripSrv.TripSrv
{
    // سفر درخواست‌شده‌ای که راننده ندارد (فوری، رزرو، نوبت سرویس هفتگی):
    //  - ۱۰ دقیقه بعد از ثبت ⇒ اعلان پنل به ادمین که باید دستی راننده انتخاب کند (TripChooseDriver)؛
    //  - لغو خودکار اگر هنوز رسیدگی نشده: سفر فوری ۳۰ دقیقه بعد از ثبت؛ رزرو/سرویس ۳۰ دقیقه مانده به حرکت
    //    (ولی هیچ‌وقت زودتر از ۳۰ دقیقه بعد از ثبت، تا ادمین فرصت انتخاب داشته باشد). رزروِ چند روز بعد نیم‌ساعت بعد از ثبت لغو
    //    نمی‌شود و نوبت هفته‌ی بعد سرویس (که با لغو نوبت فعلی ساخته می‌شود) هم بلافاصله لغو نمی‌شود.
    public static class TripDriverTimeoutRules
    {
        public static readonly TimeSpan AdminNoticeDelay = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan CancelAfterRequest = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan CancelBeforeDeparture = TimeSpan.FromMinutes(30);

        public static bool IsInstant(DateTime? scheduledDepartureAt, long? serviceScheduleId) =>
            !scheduledDepartureAt.HasValue && serviceScheduleId == null;

        // زمان حرکت سفر رزروی/سرویس؛ برای سفر فوری null
        public static DateTime? Departure(DateTime? scheduledDepartureAt, long? serviceScheduleId, DateTime? tripStartDateTime) =>
            IsInstant(scheduledDepartureAt, serviceScheduleId) ? null : scheduledDepartureAt ?? tripStartDateTime;

        public static DateTime CancelAt(DateTime createDate, DateTime? departure)
        {
            var afterRequest = createDate + CancelAfterRequest;
            if (!departure.HasValue)
                return afterRequest;
            var beforeDeparture = departure.Value - CancelBeforeDeparture;
            return beforeDeparture > afterRequest ? beforeDeparture : afterRequest;
        }
    }
}
