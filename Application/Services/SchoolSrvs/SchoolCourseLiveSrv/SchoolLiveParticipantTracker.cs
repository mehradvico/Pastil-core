using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv
{
    /// <summary>
    /// حضور درون‌حافظه‌ای بینندگان هر پخش زنده‌ی مدرسه (کدام UserPet الان واقعاً در لایو است) - برای
    /// نمایش «حاضر/غایب» به مربی. مثل CallSessionTracker، در صورت افزایش تعداد نمونه‌های API باید به
    /// Redis منتقل شود. در لایه‌ی Application است (نه Api/Hubs) چون هم هاب و هم سرویس به آن نیاز دارند.
    /// </summary>
    public class SchoolLiveParticipantTracker
    {
        public record Participant(long UserId, long UserPetId);

        private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, Participant>> _byLiveSession = new();

        public int Join(long liveSessionId, string connectionId, long userId, long userPetId)
        {
            var group = _byLiveSession.GetOrAdd(liveSessionId, _ => new ConcurrentDictionary<string, Participant>());
            group[connectionId] = new Participant(userId, userPetId);
            return group.Count;
        }

        public (long LiveSessionId, int Remaining)? Leave(string connectionId)
        {
            foreach (var pair in _byLiveSession)
            {
                if (pair.Value.TryRemove(connectionId, out _))
                    return (pair.Key, pair.Value.Count);
            }
            return null;
        }

        public HashSet<long> GetPresentUserPetIds(long liveSessionId)
        {
            if (!_byLiveSession.TryGetValue(liveSessionId, out var group))
                return new HashSet<long>();
            var set = new HashSet<long>();
            foreach (var p in group.Values)
                set.Add(p.UserPetId);
            return set;
        }

        public void RemoveLiveSession(long liveSessionId) => _byLiveSession.TryRemove(liveSessionId, out _);
    }
}
