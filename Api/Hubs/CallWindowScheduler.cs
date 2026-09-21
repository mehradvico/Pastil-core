using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Hubs
{
    /// <summary>
    /// پایان خودکار تماسِ جلسه‌ی مشاوره‌ی مدت‌دار: وقتی به ExpireDate می‌رسد به هر دو طرف «callEnded» می‌رود و تماس دیگر سیگنال رله نمی‌کند.
    /// (به‌ازای هر جلسه حداکثر یک تایمر؛ درون‌حافظه‌ای مثل CallSessionTracker.) بدون آن، تماسی که قبل از پایان پنجره شروع شده
    /// تا وقتی یکی قطع کند ادامه می‌یافت. اتصال جدید بعد از پایان پنجره را خودِ JoinSessionCall رد می‌کند.
    /// </summary>
    public class CallWindowScheduler
    {
        private readonly IHubContext<CallHub> _hubContext;
        private readonly CallSessionTracker _tracker;
        private readonly ConcurrentDictionary<long, CancellationTokenSource> _timers = new();

        public CallWindowScheduler(IHubContext<CallHub> hubContext, CallSessionTracker tracker)
        {
            _hubContext = hubContext;
            _tracker = tracker;
        }

        public void Schedule(long callKey, DateTime expireDate)
        {
            var delay = expireDate - DateTime.Now;
            if (delay <= TimeSpan.Zero)
                return;

            var cts = new CancellationTokenSource();
            if (!_timers.TryAdd(callKey, cts))
            {
                cts.Dispose();
                return; // برای این جلسه تایمر از قبل هست
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cts.Token);
                    await _hubContext.Clients.Group($"call-{callKey}").SendAsync("callEnded", cts.Token);
                    _tracker.RemoveCall(callKey);
                }
                catch (OperationCanceledException)
                {
                    // لغو تایمر
                }
                finally
                {
                    _timers.TryRemove(callKey, out _);
                    cts.Dispose();
                }
            });
        }
    }
}
