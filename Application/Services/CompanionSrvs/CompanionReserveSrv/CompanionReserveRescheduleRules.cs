using Application.Common.Helpers;
using Entities.Entities;
using System;
using System.Globalization;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv
{
    // قواعد خالص تغییر زمان رزرو و محاسبه‌ی زمان شروع واقعی (بدون دیتابیس) تا تست‌پذیر باشد.
    public static class CompanionReserveRescheduleRules
    {
        public const int ActorCompanion = 1;
        public const int ActorAdmin = 2;

        /// <summary>زمان جدید باید دست‌کم این‌قدر از «الان» جلوتر باشد.</summary>
        public static readonly TimeSpan MinLead = TimeSpan.FromMinutes(30);

        /// <summary>سقف طول دلیل (برای پیامک و ذخیره).</summary>
        public const int MaxReasonLength = 200;

        // CompanionReserve.DoDate فقط «روز» رزرو است (۰۰:۰۰)؛ ساعت از بازه‌ی انتخابی (CompanionTime، یا CompanionAssistanceTime در رزروهای قدیمی) می‌آید.
        // استفاده‌ی مستقیم از DoDate برای «زمان شروع» ساعت را نیمه‌شب حساب می‌کرد.
        public static DateTime? StartAt(DateTime doDate, string startTime)
            => ReservationScheduleValidator.TryGetServiceStartDateTime(doDate, startTime, out var startAt) ? startAt : null;

        public static DateTime? StartAt(CompanionReserve reserve)
        {
            if (reserve == null) return null;
            var startTime = reserve.CompanionTime?.StartTime ?? reserve.CompanionAssistanceTime?.StartTime;
            return StartAt(reserve.DoDate, startTime) ?? (reserve.DoDate.TimeOfDay == TimeSpan.Zero ? (DateTime?)null : reserve.DoDate);
        }

        // فقط رزرو پرداخت‌شده، لغو‌نشده، انجام‌نشده و غیرفوری قابل تغییر زمان است
        public static bool CanReschedule(bool isReserved, bool isCancel, DateTime? doneDate, bool isComplete, bool isInstantOnline) =>
            isReserved && !isCancel && doneDate == null && !isComplete && !isInstantOnline;

        public static bool IsFarEnough(DateTime newStartAt, DateTime now) => newStartAt - now >= MinLead;

        public static bool IsSameSlot(DateTime oldDoDate, long? oldTimeId, DateTime newDoDate, long newTimeId) =>
            oldDoDate.Date == newDoDate.Date && oldTimeId == newTimeId;

        public static bool HasCapacity(int capacity, int reservedByOthers) => reservedByOthers < (capacity > 0 ? capacity : 1);

        public static string NormalizeReason(string reason)
        {
            var clean = string.Join(' ', (reason ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            return clean.Length > MaxReasonLength ? clean.Substring(0, MaxReasonLength) : clean;
        }

        // زمان حرکت سفر پت‌رسانِ متصل = شروع رزرو منهای فاصله‌ی انتخابی کاربر (۶۰ یا ۱۲۰ دقیقه)
        public static DateTime DepartureFor(DateTime startAt, int? leadMinutes) => startAt.AddMinutes(-(leadMinutes ?? 60));

        // تاریخ شمسی «۱۴۰۵/۰۷/۲۰» با ارقام لاتین (کاوه‌نگار توکن را با ارقام لاتین بی‌مشکل می‌پذیرد)
        public static string JalaliDate(DateTime date)
        {
            var calendar = new PersianCalendar();
            return string.Create(CultureInfo.InvariantCulture,
                $"{calendar.GetYear(date)}/{calendar.GetMonth(date):D2}/{calendar.GetDayOfMonth(date):D2}");
        }

        public static string Time(DateTime date) => date.ToString("HH:mm", CultureInfo.InvariantCulture);

        // ساعت برای توکن پیامک کاوه‌نگار: بدون «:» (۴۳۱ «ساختار کد صحیح نمی‌باشد» برای HH:mm) → «14.00»
        public static string SmsTime(DateTime date) => date.ToString("HH.mm", CultureInfo.InvariantCulture);
    }
}
