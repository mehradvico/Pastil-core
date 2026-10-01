using Application.Common.Enumerable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Application.Services.ConsultationSrvs.ConsultationBookingSrv
{
    // قواعد خالص (بدون دیتابیس) رزرو ساعت‌دار مشاوره: روزهفته، بازه‌ها، اسلات‌ها، ظرفیت، لغو.
    // طراحی: backend/Docs/ONLINE_CONSULTATION_BOOKING_FA.md
    public static class ConsultationBookingRules
    {
        public readonly record struct Window(int StartMinute, int EndMinute, int Capacity);

        public enum SlotProblem
        {
            None,
            NotOnGrid,
            TooSoon,
            TooFar,
            OutsideAvailability,
            Full
        }

        // WeekDayId پروژه: ۱ شنبه … ۷ جمعه (مثل CompanionAssistanceTime)
        public static int WeekDayId(DateTime date) => (((int)date.DayOfWeek + 1) % 7) + 1;

        // «HH:mm» ⇒ دقیقه از نیمه‌شب؛ نامعتبر ⇒ null. «24:00» فقط برای پایان بازه مجاز است.
        public static int? ParseTime(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            var parts = text.Trim().Split(':');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var h) ||
                !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var m))
                return null;
            if (m < 0 || m > 59 || h < 0 || h > 24 || (h == 24 && m != 0))
                return null;
            return h * 60 + m;
        }

        public static string FormatTime(int minute) =>
            string.Create(CultureInfo.InvariantCulture, $"{minute / 60:D2}:{minute % 60:D2}");

        public enum WindowProblem
        {
            None,
            BadWeekDay,
            BadTime,
            EndNotAfterStart,
            BadCapacity,
            Overlap
        }

        // اعتبارسنجی یک بازه‌ی کاری؛ شبکه‌ی ۱۵ دقیقه‌ای اجباری است تا اسلات‌ها با مرز بازه بخوانند
        public static WindowProblem ValidateWindow(int weekDayId, int startMinute, int endMinute, int capacity)
        {
            if (weekDayId < 1 || weekDayId > 7)
                return WindowProblem.BadWeekDay;
            var step = ConsultationRules.BookingSlotStepMinutes;
            if (startMinute < 0 || endMinute > 24 * 60 || startMinute % step != 0 || endMinute % step != 0)
                return WindowProblem.BadTime;
            if (endMinute <= startMinute)
                return WindowProblem.EndNotAfterStart;
            if (capacity < 1 || capacity > ConsultationRules.BookingMaxCapacity)
                return WindowProblem.BadCapacity;
            return WindowProblem.None;
        }

        // بازه‌های هم‌روز نباید هم‌پوشانی داشته باشند (لبه‌به‌لبه مجاز)
        public static bool HasOverlap(IEnumerable<(int WeekDayId, int StartMinute, int EndMinute)> windows) =>
            windows.GroupBy(w => w.WeekDayId).Any(g =>
            {
                var ordered = g.OrderBy(w => w.StartMinute).ToList();
                for (var i = 1; i < ordered.Count; i++)
                    if (ordered[i].StartMinute < ordered[i - 1].EndMinute)
                        return true;
                return false;
            });

        // ظرفیتی که یک بازه برای یک ساعت شروع (طول duration) می‌دهد؛ ۰ یعنی اسلات داخل هیچ بازه‌ای نیست.
        // کل مدت مشاوره باید داخل «یک» بازه بماند (مشاوره از وسط تعطیلی رد نمی‌شود).
        public static int CapacityFor(IEnumerable<Window> windowsOfDay, int startMinute, int durationMinutes)
        {
            var end = startMinute + durationMinutes;
            foreach (var w in windowsOfDay)
                if (startMinute >= w.StartMinute && end <= w.EndMinute)
                    return w.Capacity;
            return 0;
        }

        // تعداد رزروهای هم‌پوشان با [start, end) ؛ هر عنصر یک رزرو موجود (Paid/Active/نگه‌داشته‌ی پرداخت)
        public static int CountOverlaps(IEnumerable<(DateTime Start, DateTime End)> existing, DateTime start, DateTime end) =>
            existing.Count(e => e.Start < end && e.End > start);

        public static bool OnGrid(DateTime start) =>
            start.Second == 0 && start.Millisecond == 0 &&
            (start.Hour * 60 + start.Minute) % ConsultationRules.BookingSlotStepMinutes == 0;

        public static SlotProblem CheckSlot(DateTime start, int durationMinutes, DateTime now,
            IEnumerable<Window> windowsOfDay, int overlaps)
        {
            if (!OnGrid(start))
                return SlotProblem.NotOnGrid;
            if (start < now + ConsultationRules.BookingMinLead)
                return SlotProblem.TooSoon;
            if (start.Date > now.Date.AddDays(ConsultationRules.BookingMaxDaysAhead))
                return SlotProblem.TooFar;
            var capacity = CapacityFor(windowsOfDay, start.Hour * 60 + start.Minute, durationMinutes);
            if (capacity <= 0)
                return SlotProblem.OutsideAvailability;
            if (overlaps >= capacity)
                return SlotProblem.Full;
            return SlotProblem.None;
        }

        public readonly record struct Slot(DateTime Start, DateTime End, bool Available);

        // همه‌ی ساعت‌های شروع ممکن یک روز؛ existing = رزروهای هم‌پوشان آن روز. اسلات‌های قبل از «الان + حداقل فاصله» نمایش داده نمی‌شوند.
        public static List<Slot> BuildSlots(DateTime date, int durationMinutes, DateTime now,
            IReadOnlyList<Window> windowsOfDay, IReadOnlyList<(DateTime Start, DateTime End)> existing)
        {
            var result = new List<Slot>();
            var day = date.Date;
            if (day > now.Date.AddDays(ConsultationRules.BookingMaxDaysAhead))
                return result;

            var step = ConsultationRules.BookingSlotStepMinutes;
            foreach (var w in windowsOfDay.OrderBy(x => x.StartMinute))
            {
                for (var minute = w.StartMinute; minute + durationMinutes <= w.EndMinute; minute += step)
                {
                    var start = day.AddMinutes(minute);
                    var end = start.AddMinutes(durationMinutes);
                    if (start < now + ConsultationRules.BookingMinLead)
                        continue;
                    var full = CountOverlaps(existing, start, end) >= w.Capacity;
                    result.Add(new Slot(start, end, !full));
                }
            }
            return result;
        }

        // ---- زمان‌بندی چرخه‌ی خرید

        // مهلت شروع نماینده برای رزرو ساعت‌دار: ساعت رزرو + مهلت دیرکرد
        public static DateTime StartDeadline(DateTime scheduledStart) => scheduledStart + ConsultationRules.BookingStartGrace;

        // نماینده از ۱۰ دقیقه قبل از ساعت رزرو می‌تواند شروع کند (خرید فوری: scheduledStart = null ⇒ همیشه)
        public static bool IsStartAllowed(DateTime? scheduledStart, DateTime now) =>
            !scheduledStart.HasValue || now >= scheduledStart.Value - ConsultationRules.BookingEarlyStart;

        // کاربر تا ۲ ساعت قبل از ساعت رزرو آزادانه لغو می‌کند؛ خرید فوری (null) همیشه قابل لغو قبل از شروع است
        public static bool IsUserCancelAllowed(DateTime? scheduledStart, DateTime now) =>
            !scheduledStart.HasValue || now <= scheduledStart.Value - ConsultationRules.BookingFreeCancelBefore;

        // یادآوری: از ۳۰ دقیقه قبل از ساعت رزرو تا خودِ ساعت رزرو (بعدش پوش «شروع» می‌رود)
        public static bool IsReminderDue(DateTime? scheduledStart, DateTime now) =>
            scheduledStart.HasValue && now >= scheduledStart.Value - ConsultationRules.BookingReminderLead && now < scheduledStart.Value;

        // رزروِ در انتظار پرداخت فقط تا مدت مشخص ظرفیت را نگه می‌دارد
        public static bool PendingStillHolds(DateTime createDate, DateTime now) =>
            now - createDate <= ConsultationRules.BookingPendingHold;
    }
}
