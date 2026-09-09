using System.Collections.Concurrent;

namespace Api.Hubs
{
    /// <summary>
    /// نگه‌داری وضعیت درون‌حافظه‌ای شرکت‌کنندگان هر تماس درون‌برنامه‌ای (برای هر نمونه از سرویس API - در صورت
    /// افزایش تعداد نمونه‌ها در آینده باید به یک بک‌اند مشترک مثل Redis منتقل شود).
    /// </summary>
    public class CallSessionTracker
    {
        public record WaitingCallInfo(long BookerId, string CallerName);

        private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, long>> _participantsByReserve = new();

        // فقط تماس‌هایی که دقیقاً یک نفر (غیر از رزروکننده) در آن‌ها منتظر است اینجا نگه داشته می‌شوند؛
        // با پیوستن نفر دوم یا خروج کامل از تماس، حذف می‌شوند. برای این‌که اگر کاربر پوش را از دست داد،
        // با باز کردن اپ هم بشود زنگ خوردن تماس را کشف کرد (نگاه کنید به CallPendingController).
        private readonly ConcurrentDictionary<long, WaitingCallInfo> _waitingByReserve = new();

        public int Join(long reserveId, string connectionId, long userId, long? bookerId = null, string callerName = null)
        {
            var group = _participantsByReserve.GetOrAdd(reserveId, _ => new ConcurrentDictionary<string, long>());
            group[connectionId] = userId;
            var count = group.Count;

            if (count == 1 && bookerId.HasValue && userId != bookerId.Value)
            {
                _waitingByReserve[reserveId] = new WaitingCallInfo(bookerId.Value, callerName);
            }
            else if (count > 1)
            {
                _waitingByReserve.TryRemove(reserveId, out _);
            }

            return count;
        }

        public (long ReserveId, int Remaining)? Leave(string connectionId)
        {
            foreach (var pair in _participantsByReserve)
            {
                if (pair.Value.TryRemove(connectionId, out _))
                {
                    if (pair.Value.IsEmpty)
                    {
                        _waitingByReserve.TryRemove(pair.Key, out _);
                    }
                    return (pair.Key, pair.Value.Count);
                }
            }
            return null;
        }

        public (long ReserveId, string CallerName)? FindPendingCallForBooker(long bookerUserId)
        {
            foreach (var pair in _waitingByReserve)
            {
                if (pair.Value.BookerId == bookerUserId)
                {
                    return (pair.Key, pair.Value.CallerName);
                }
            }
            return null;
        }
    }
}
