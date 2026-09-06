using System.Collections.Concurrent;

namespace Api.Hubs
{
    /// <summary>
    /// نگه‌داری وضعیت درون‌حافظه‌ای شرکت‌کنندگان هر تماس درون‌برنامه‌ای (برای هر نمونه از سرویس API - در صورت
    /// افزایش تعداد نمونه‌ها در آینده باید به یک بک‌اند مشترک مثل Redis منتقل شود).
    /// </summary>
    public class CallSessionTracker
    {
        private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, long>> _participantsByReserve = new();

        public int Join(long reserveId, string connectionId, long userId)
        {
            var group = _participantsByReserve.GetOrAdd(reserveId, _ => new ConcurrentDictionary<string, long>());
            group[connectionId] = userId;
            return group.Count;
        }

        public (long ReserveId, int Remaining)? Leave(string connectionId)
        {
            foreach (var pair in _participantsByReserve)
            {
                if (pair.Value.TryRemove(connectionId, out _))
                {
                    return (pair.Key, pair.Value.Count);
                }
            }
            return null;
        }
    }
}
