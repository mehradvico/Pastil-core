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
        private readonly CallDurationRecorder _durationRecorder;

        public CallWindowScheduler(IHubContext<CallHub> hubContext, CallSessionTracker tracker, CallDurationRecorder durationRecorder)
        {
            _hubContext = hubContext;
            _tracker = tracker;
            _durationRecorder = durationRecorder;
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
                    // آخرین قطعه‌ی تماسِ در جریان تا پایان پنجره حساب می‌شود
                    var seconds = _tracker.TakeConnectedSeconds(callKey);
                    _tracker.RemoveCall(callKey);
                    if (callKey < 0)
                        await _durationRecorder.RecordSegmentAsync(-callKey, seconds);
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
