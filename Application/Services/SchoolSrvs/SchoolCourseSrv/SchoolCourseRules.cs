using Application.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv
{
    // قواعد زمان‌بندی دوره‌ی مدرسه: باز بودن ثبت‌نام و خلاصه‌ی «چند جلسه در هفته»
    public static class SchoolCourseRules
    {
        /// <summary>ثبت‌نامِ پرداخت‌نشده فقط تا این مدت جا نگه می‌دارد؛ بعد از آن ظرفیت آزاد می‌شود (تا ساخت ثبت‌نام/تلاش دوباره).</summary>
        public static readonly TimeSpan UnpaidSeatHold = TimeSpan.FromMinutes(15);

        /// <summary>ثبت‌نام‌هایی که یک جای دوره را اشغال می‌کنند: پرداخت‌شده‌ی لغونشده، یا پرداخت‌نشده‌ی تازه (داخل UnpaidSeatHold).</summary>
        public static IQueryable<Entities.Entities.SchoolField.SchoolReserve> SeatTakers(
            IQueryable<Entities.Entities.SchoolField.SchoolReserve> reserves, DateTime now)
        {
            var holdSince = now - UnpaidSeatHold;
            return reserves.Where(r => !r.IsCancel && (r.IsReserved || r.CreateDate >= holdSince));
        }

        // شروع واقعی جلسه = تاریخ + ساعت شروع؛ ساعتِ نامعتبر = ابتدای همان روز
        public static DateTime SessionStart(DateTime sessionDate, string startTime) =>
            ReservationScheduleValidator.TryGetServiceStartDateTime(sessionDate, startTime, out var start)
                ? start
                : sessionDate.Date;

        public static DateTime? FirstSessionStart(IEnumerable<(DateTime Date, string StartTime)> sessions)
        {
            var starts = sessions?.Select(s => SessionStart(s.Date, s.StartTime)).ToList();
            return starts == null || starts.Count == 0 ? null : starts.Min();
        }

        // ثبت‌نام فقط تا قبل از شروع جلسه‌ی اول باز است (دوره‌ی ویدیویی بدون جلسه همیشه باز است)
        public static bool IsRegistrationOpenByDate(DateTime? firstSessionStart, DateTime now) =>
            !firstSessionStart.HasValue || firstSessionStart.Value > now;

        /// <summary>وضعیت ثبت‌نام دوره در لحظه‌ی now: باز است؟ و با چه قیمتی (ثبت‌نام دیرهنگامِ نسبی).</summary>
        public sealed record EnrollmentWindow(bool Open, double Price, int RemainingSessions, int TotalSessions);

        // قیمت نسبی: قیمت کامل × جلسه‌های باقی‌مانده / کل جلسه‌ها (گرد به تومان)
        public static double ProratedPrice(double price, int remaining, int total) =>
            total <= 0 || remaining >= total ? price : Math.Round(price * Math.Max(remaining, 0) / total);

        /// <summary>
        /// قاعده‌ی خالص: قبل از شروع جلسه‌ی اول همیشه باز با قیمت کامل. بعد از آن بر اساس LateEnrollmentMode:
        /// بسته (پیش‌فرض/رفتار قبلی)، باز با قیمت کامل، یا باز با قیمت جلسه‌های باقی‌مانده — هر دو فقط تا شروع جلسه‌ی آخر.
        /// </summary>
        public static EnrollmentWindow Enrollment(double price, int lateEnrollmentMode, IReadOnlyCollection<DateTime> sessionStarts, DateTime now)
        {
            var total = sessionStarts?.Count ?? 0;
            if (total == 0)
                return new EnrollmentWindow(true, price, 0, 0); // دوره‌ی ویدیویی بدون جلسه
            var remaining = sessionStarts.Count(s => s > now);
            if (sessionStarts.Min() > now)
                return new EnrollmentWindow(true, price, remaining, total);
            var mode = (Application.Common.Enumerable.Code.SchoolLateEnrollmentEnum)lateEnrollmentMode;
            if (remaining == 0 || mode == Application.Common.Enumerable.Code.SchoolLateEnrollmentEnum.Closed)
                return new EnrollmentWindow(false, price, remaining, total);
            return mode == Application.Common.Enumerable.Code.SchoolLateEnrollmentEnum.OpenProrated
                ? new EnrollmentWindow(true, ProratedPrice(price, remaining, total), remaining, total)
                : new EnrollmentWindow(true, price, remaining, total);
        }

        public static async Task<EnrollmentWindow> EnrollmentAsync(IDataBaseContext context, long courseId, DateTime now)
        {
            var course = await context.SchoolCourses.AsNoTracking()
                .Where(c => c.Id == courseId)
                .Select(c => new { c.Price, c.LateEnrollmentMode })
                .FirstOrDefaultAsync();
            if (course == null)
                return new EnrollmentWindow(false, 0, 0, 0);
            return Enrollment(course.Price, course.LateEnrollmentMode, await SessionStartsAsync(context, courseId), now);
        }

        public static async Task<List<DateTime>> SessionStartsAsync(IDataBaseContext context, long courseId)
        {
            var sessions = await context.SchoolCourseSessions.AsNoTracking()
                .Where(s => s.SchoolCourseId == courseId && !s.Deleted)
                .Select(s => new { s.SessionDate, s.StartTime })
                .ToListAsync();
            return sessions.Select(s => SessionStart(s.SessionDate, s.StartTime)).ToList();
        }

        // ثبت‌نام و شروع پرداخت هر دو با همین چک رد می‌شوند (نه فقط مخفی کردن دکمه در فرانت)
        public static async Task<bool> IsRegistrationOpenAsync(IDataBaseContext context, long courseId, DateTime now) =>
            (await EnrollmentAsync(context, courseId, now)).Open;

        /// <summary>جلسه‌ی اول شروع شده؟ (لغو توسط کاربر فقط قبل از آن)</summary>
        public static async Task<bool> HasStartedAsync(IDataBaseContext context, long courseId, DateTime now)
        {
            var starts = await SessionStartsAsync(context, courseId);
            return starts.Count > 0 && starts.Min() <= now;
        }

        /// <summary>
        /// درصد برگشت وجه لغو توسط کاربر (قبل از شروع): تا FullRefundHoursBeforeStart ساعت قبل از جلسه‌ی اول ۱۰۰٪،
        /// دیرتر LateCancelRefundPercent. دوره‌ی بدون جلسه ۱۰۰٪.
        /// </summary>
        public static int RefundPercent(DateTime? firstSessionStart, DateTime now, int fullRefundHoursBeforeStart, int lateCancelRefundPercent)
        {
            if (!firstSessionStart.HasValue)
                return 100;
            var hours = (firstSessionStart.Value - now).TotalHours;
            return hours >= Math.Max(fullRefundHoursBeforeStart, 0) ? 100 : Math.Clamp(lateCancelRefundPercent, 0, 100);
        }

        // «جلسه در هفته» بر اساس هفته‌های تقویمیِ بازه‌ی دوره: ۲ جلسه با ۲ روز فاصله = ۱ هفته و ۲ جلسه در هفته.
        // قبلاً تعداد بر کسرِ هفته (۲ روز = ۰٫۲۹ هفته) تقسیم می‌شد و «۷ جلسه در هفته» نشان داده می‌شد.
        public static (double SessionsPerWeek, double TotalWeeks) WeeklySummary(IReadOnlyCollection<DateTime> sessionDates)
        {
            if (sessionDates == null || sessionDates.Count == 0)
                return (0, 0);
            if (sessionDates.Count == 1)
                return (1, 0);

            var spanDays = (sessionDates.Max().Date - sessionDates.Min().Date).TotalDays;
            var weeks = Math.Floor(spanDays / 7.0) + 1;
            return (Math.Round(sessionDates.Count / weeks, 1), weeks);
        }
    }
}
