using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Threading.Tasks;

namespace Api.Hubs
{
    /// <summary>
    /// ثبت مدت واقعی تماس درون‌برنامه‌ای (صوتی/تصویری) جلسه‌ی آنلاین برای گزارش ادمین: فقط لحظه‌هایی که <b>هر دو طرف</b> به هم وصل بودند.
    /// یک جلسه‌ی مشاوره ممکن است چند بار قطع و وصل شود؛ هر قطعه‌ی تماس به CallSeconds اضافه می‌شود.
    /// Singleton است چون هم هاب (scope هر درخواست) و هم تایمر پایان پنجره (بدون scope) از آن استفاده می‌کنند.
    /// </summary>
    public class CallDurationRecorder
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CallDurationRecorder> _logger;

        public CallDurationRecorder(IServiceScopeFactory scopeFactory, ILogger<CallDurationRecorder> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // هر دو طرف وصل شدند: فقط اولین بار CallStartDate ثبت می‌شود
        public async Task RecordConnectedAsync(long sessionId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IDataBaseContext>();
                var now = DateTime.Now;
                await context.OnlineSessions
                    .Where(s => s.Id == sessionId && s.CallStartDate == null)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.CallStartDate, (DateTime?)now));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record call start for online session {SessionId}.", sessionId);
            }
        }

        // یک قطعه‌ی تماس تمام شد؛ seconds = مدتی که هر دو طرف وصل بودند (صفر یعنی هیچ قطعه‌ای اندازه‌گیری نشد)
        public async Task RecordSegmentAsync(long sessionId, int seconds)
        {
            if (seconds <= 0)
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IDataBaseContext>();
                var now = DateTime.Now;
                await context.OnlineSessions
                    .Where(s => s.Id == sessionId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.CallSeconds, s => s.CallSeconds + seconds)
                        .SetProperty(s => s.CallEndDate, (DateTime?)now));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record call duration for online session {SessionId}.", sessionId);
            }
        }
    }
}
