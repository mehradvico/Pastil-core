using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv
{
    /// <summary>
    /// هر چند دقیقه یک‌بار توسط Hangfire اجرا می‌شود؛ پیام‌های پوش دارای SendDate را
    /// بررسی و در صورت «سررسید بودن» با استفاده از همان مسیر ارسال دستی (IPushBroadcastService)
    /// ارسال می‌کند. یک‌باره‌ها (AutoSend=false) دقیقاً یک بار ارسال می‌شوند؛
    /// تکرارشونده‌ها (AutoSend=true) طبق RecurrenceType هر دوره یک بار.
    /// </summary>
    public class PushScheduleService : IPushScheduleService
    {
        private static readonly TimeZoneInfo TehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        private readonly IDataBaseContext _context;
        private readonly IPushBroadcastService _broadcastService;

        public PushScheduleService(IDataBaseContext context, IPushBroadcastService broadcastService)
        {
            _context = context;
            _broadcastService = broadcastService;
        }

        public async Task DispatchDueAsync(CancellationToken cancellationToken = default)
        {
            var tehranNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TehranTimeZone).DateTime;

            var messages = await _context.PushMessages
                .AsTracking()
                .Where(item => !item.Deleted && item.SendDate != null)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!IsDue(message, tehranNow, out var occurrence))
                    continue;

                await _broadcastService.BroadcastAsync(new PushBroadcastDto { PushMessageId = message.Id });

                message.LastSentDate = tehranNow;
                _context.PushMessages.Update(message);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        private static bool IsDue(PushMessage message, DateTime tehranNow, out DateTime occurrence)
        {
            occurrence = default;

            if (!message.SendDate.HasValue)
                return false;

            var anchor = message.SendDate.Value;

            if (!message.AutoSend)
            {
                // one-time: fires exactly once, the first time we notice it's due
                if (message.LastSentDate.HasValue || anchor > tehranNow)
                    return false;

                occurrence = anchor;
                return true;
            }

            var recurrence = (PushRecurrenceEnum)message.RecurrenceType;
            var mostRecent = GetMostRecentOccurrence(recurrence, anchor, tehranNow);
            if (mostRecent == null)
                return false;

            if (message.LastSentDate.HasValue && message.LastSentDate.Value >= mostRecent.Value)
                return false;

            occurrence = mostRecent.Value;
            return true;
        }

        /// <summary>
        /// آخرین لحظه‌ای که طبق الگوی تکرار، تا این‌لحظه (now) باید ارسال می‌شده - با
        /// حفظ ساعتِ anchor و (برحسب نوع) روز هفته/روز ماه/روز و ماه سال از anchor.
        /// </summary>
        private static DateTime? GetMostRecentOccurrence(PushRecurrenceEnum type, DateTime anchor, DateTime now)
        {
            var time = anchor.TimeOfDay;

            switch (type)
            {
                case PushRecurrenceEnum.Daily:
                {
                    var candidate = now.Date + time;
                    return candidate <= now ? candidate : candidate.AddDays(-1);
                }
                case PushRecurrenceEnum.Weekly:
                {
                    var daysSince = ((int)now.DayOfWeek - (int)anchor.DayOfWeek + 7) % 7;
                    var candidate = now.Date.AddDays(-daysSince) + time;
                    return candidate <= now ? candidate : candidate.AddDays(-7);
                }
                case PushRecurrenceEnum.Monthly:
                {
                    var day = Math.Min(anchor.Day, DateTime.DaysInMonth(now.Year, now.Month));
                    var candidate = new DateTime(now.Year, now.Month, day) + time;
                    if (candidate <= now)
                        return candidate;

                    var prevMonth = now.AddMonths(-1);
                    var prevDay = Math.Min(anchor.Day, DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));
                    return new DateTime(prevMonth.Year, prevMonth.Month, prevDay) + time;
                }
                case PushRecurrenceEnum.Yearly:
                {
                    var day = anchor.Month == 2 && anchor.Day == 29 && !DateTime.IsLeapYear(now.Year) ? 28 : anchor.Day;
                    var candidate = new DateTime(now.Year, anchor.Month, day) + time;
                    if (candidate <= now)
                        return candidate;

                    var prevYear = now.Year - 1;
                    var prevDay = anchor.Month == 2 && anchor.Day == 29 && !DateTime.IsLeapYear(prevYear) ? 28 : anchor.Day;
                    return new DateTime(prevYear, anchor.Month, prevDay) + time;
                }
                default:
                    return null;
            }
        }
    }
}
