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

        // ثبت‌نام و شروع پرداخت هر دو با همین چک رد می‌شوند (نه فقط مخفی کردن دکمه در فرانت)
        public static async Task<bool> IsRegistrationOpenAsync(IDataBaseContext context, long courseId, DateTime now)
        {
            var sessions = await context.SchoolCourseSessions.AsNoTracking()
                .Where(s => s.SchoolCourseId == courseId && !s.Deleted)
                .Select(s => new { s.SessionDate, s.StartTime })
                .ToListAsync();
            return IsRegistrationOpenByDate(FirstSessionStart(sessions.Select(s => (s.SessionDate, s.StartTime))), now);
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
