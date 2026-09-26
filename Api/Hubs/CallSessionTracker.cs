using System;
using System.Collections.Concurrent;

namespace Api.Hubs
{
    /// <summary>
    /// نگه‌داری وضعیت درون‌حافظه‌ای شرکت‌کنندگان هر تماس درون‌برنامه‌ای (برای هر نمونه از سرویس API - در صورت
    /// افزایش تعداد نمونه‌ها در آینده باید به یک بک‌اند مشترک مثل Redis منتقل شود).
    /// </summary>
    public class CallSessionTracker
    {
        public record WaitingCallInfo(long BookerId, string CallerName, bool IsVideo = false);

        private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, long>> _participantsByReserve = new();

        // فقط تماس‌هایی که دقیقاً یک نفر (غیر از رزروکننده) در آن‌ها منتظر است اینجا نگه داشته می‌شوند؛
        // با پیوستن نفر دوم یا خروج کامل از تماس، حذف می‌شوند. برای این‌که اگر کاربر پوش را از دست داد،
        // با باز کردن اپ هم بشود زنگ خوردن تماس را کشف کرد (نگاه کنید به CallPendingController).
        private readonly ConcurrentDictionary<long, WaitingCallInfo> _waitingByReserve = new();

        // لحظه‌ی وصل شدن هر دو طرف هر تماس (برای اندازه‌گیری مدت واقعی تماس)؛ فقط تا وقتی که هر دو وصل‌اند
        private readonly ConcurrentDictionary<long, DateTime> _connectedSince = new();

        // هر دو طرف وصل شدند؛ true فقط وقتی اولین‌بار این قطعه‌ی تماس شروع می‌شود
        public bool MarkConnected(long callKey) => _connectedSince.TryAdd(callKey, DateTime.Now);

        // پایان قطعه‌ی تماس (یکی خارج شد / پنجره تمام شد): ثانیه‌های وصل بودن را برمی‌گرداند و علامت را پاک می‌کند (تکرار صدا زدن ۰ می‌دهد)
        public int TakeConnectedSeconds(long callKey) =>
            _connectedSince.TryRemove(callKey, out var since)
                ? Math.Max(0, (int)Math.Round((DateTime.Now - since).TotalSeconds))
                : 0;

        public int Join(long reserveId, string connectionId, long userId, long? bookerId = null, string callerName = null, bool isVideo = false)
        {
            var group = _participantsByReserve.GetOrAdd(reserveId, _ => new ConcurrentDictionary<string, long>());
            group[connectionId] = userId;
            var count = group.Count;

            if (count == 1 && bookerId.HasValue && userId != bookerId.Value)
            {
                _waitingByReserve[reserveId] = new WaitingCallInfo(bookerId.Value, callerName, isVideo);
            }
            else if (count > 1)
            {
                _waitingByReserve.TryRemove(reserveId, out _);
            }

            return count;
        }

        public bool IsParticipant(long reserveId, string connectionId, long userId) =>
            _participantsByReserve.TryGetValue(reserveId, out var group) &&
            group.TryGetValue(connectionId, out var participantUserId) &&
            participantUserId == userId;

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

        // پایان اجباری تماس (پایان پنجره‌ی مشاوره): همه‌ی شرکت‌کنندگان از ردیاب حذف می‌شوند تا دیگر سیگنالی رله نشود
        public void RemoveCall(long reserveId)
        {
            _participantsByReserve.TryRemove(reserveId, out _);
            _waitingByReserve.TryRemove(reserveId, out _);
        }

        public (long ReserveId, string CallerName, bool IsVideo)? FindPendingCallForBooker(long bookerUserId)
        {
            foreach (var pair in _waitingByReserve)
            {
                if (pair.Value.BookerId == bookerUserId)
                {
                    return (pair.Key, pair.Value.CallerName, pair.Value.IsVideo);
                }
            }
            return null;
        }
    }
}
