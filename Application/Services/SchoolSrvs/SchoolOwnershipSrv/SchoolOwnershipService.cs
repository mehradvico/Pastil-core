using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolOwnershipSrv
{
    /// <summary>
    /// مالکیت مدرسه/دوره/جلسه/ثبت‌نام برای پنل/اپ ارائه‌دهنده (۲۰۲۶-۱۰-۲۶): مالک مدرسه «کاربرِ ارائه‌دهنده‌ی» آن است (School.ProviderId)؛
    /// برای مدرسه‌های قدیمی مالکِ نمایندگی (School.CompanionId → Companion.OwnerId) هم مثل قبل مالک حساب می‌شود.
    /// همیشه از دیتابیس سنجیده می‌شود، نه از توکن. هر متد شناسه‌ی «مدرسه‌ی» صاحب منبع را برمی‌گرداند (یا null = مالک نیست).
    /// approvedOnly: عملیات نوشتن فقط برای ارائه‌دهنده/نمایندگی فعال و تأییدشده؛ خواندن مال خود بدون آن هم مجاز است.
    /// </summary>
    public interface ISchoolOwnershipService
    {
        Task<List<long>> OwnedCompanionIdsAsync(long userId, bool approvedOnly = true);
        /// <summary>
        /// مالک مدرسه‌ی جدید: ارائه‌دهنده‌ی فعال/تأییدشده با نقش تأییدشده‌ی School (نمایندگی اختیاری)، وگرنه نمایندگیِ قدیمیِ کاربر.
        /// Error = متن پیام وقتی هیچ‌کدام نیست.
        /// </summary>
        Task<NewSchoolOwner> ResolveOwnerForNewSchoolAsync(long userId, long? requestedCompanionId);
        Task<long?> SchoolOfSchoolAsync(long schoolId, long userId, bool approvedOnly = true);
        Task<long?> SchoolOfCourseAsync(long courseId, long userId, bool approvedOnly = true);
        Task<long?> SchoolOfSessionAsync(long sessionId, long userId, bool approvedOnly = true);
        Task<long?> SchoolOfVideoAsync(long videoId, long userId, bool approvedOnly = true);
        Task<long?> SchoolOfLiveSessionAsync(long liveSessionId, long userId, bool approvedOnly = true);
        Task<long?> SchoolOfReserveAsync(long reserveId, long userId, bool approvedOnly = true);
        Task<long?> SchoolOfCoursePetAsync(long coursePetId, long userId, bool approvedOnly = true);
    }

    public sealed record NewSchoolOwner(long? ProviderId, long? CompanionId, string Error)
    {
        public bool Ok => Error == null;
    }

    public class SchoolOwnershipService : ISchoolOwnershipService
    {
        private readonly IDataBaseContext _context;

        public SchoolOwnershipService(IDataBaseContext context) => _context = context;

        public Task<List<long>> OwnedCompanionIdsAsync(long userId, bool approvedOnly = true) =>
            _context.Companions.AsNoTracking()
                .Where(c => c.OwnerId == userId && !c.Deleted && (!approvedOnly || (c.Active && c.Approved)))
                .Select(c => c.Id)
                .ToListAsync();

        public async Task<NewSchoolOwner> ResolveOwnerForNewSchoolAsync(long userId, long? requestedCompanionId)
        {
            var companionIds = await OwnedCompanionIdsAsync(userId);
            if (requestedCompanionId is > 0 && !companionIds.Contains(requestedCompanionId.Value))
                return new NewSchoolOwner(null, null, Resource.Notification.AccessDenied);
            long? companionId = requestedCompanionId is > 0 ? requestedCompanionId : companionIds.Count == 1 ? companionIds[0] : null;

            var providerId = await _context.Providers.AsNoTracking()
                .Where(p => p.UserId == userId && !p.Deleted && p.Active && p.Approved)
                .Where(p => _context.ProviderRoleAssignments.Any(a => a.ProviderId == p.Id && a.Approved && !a.Deleted
                    && a.ProviderRole.Label == ProviderRoleLabels.School && a.ProviderRole.Active && !a.ProviderRole.Deleted))
                .Select(p => (long?)p.Id)
                .FirstOrDefaultAsync();
            if (providerId.HasValue)
                return new NewSchoolOwner(providerId, companionId, null);

            // بدون نقش مدرسه: مسیر قدیمی (مدرسه زیر نمایندگی)؛ ارائه‌دهنده‌ی همان نمایندگی هم ثبت می‌شود تا تسویه درست برود
            if (companionId.HasValue)
            {
                var companionProviderId = await _context.Companions.AsNoTracking()
                    .Where(c => c.Id == companionId.Value).Select(c => c.ProviderId).FirstOrDefaultAsync();
                return new NewSchoolOwner(companionProviderId, companionId, null);
            }
            return new NewSchoolOwner(null, null, companionIds.Count > 1
                ? Resource.Notification.SchoolChooseCompanion
                : Resource.Notification.SchoolNeedsProviderRole);
        }

        // مالکیت روی خودِ مدرسه؛ همه‌ی متدهای زیر همین شرط را روی مدرسه‌ی منبع اعمال می‌کنند
        private IQueryable<long> OwnedSchoolIds(long userId, bool approvedOnly) =>
            _context.Schools.AsNoTracking()
                .Where(s => !s.Deleted && (
                    (s.Provider != null && s.Provider.UserId == userId && !s.Provider.Deleted
                        && (!approvedOnly || (s.Provider.Active && s.Provider.Approved)))
                    || (s.Companion != null && s.Companion.OwnerId == userId && !s.Companion.Deleted
                        && (!approvedOnly || (s.Companion.Active && s.Companion.Approved)))))
                .Select(s => s.Id);

        public Task<long?> SchoolOfSchoolAsync(long schoolId, long userId, bool approvedOnly = true) =>
            OwnedSchoolIds(userId, approvedOnly).Where(id => id == schoolId).Select(id => (long?)id).FirstOrDefaultAsync();

        public Task<long?> SchoolOfCourseAsync(long courseId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourses.AsNoTracking()
                .Where(c => c.Id == courseId && OwnedSchoolIds(userId, approvedOnly).Contains(c.SchoolId))
                .Select(c => (long?)c.SchoolId).FirstOrDefaultAsync();

        public Task<long?> SchoolOfSessionAsync(long sessionId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourseSessions.AsNoTracking()
                .Where(s => s.Id == sessionId && OwnedSchoolIds(userId, approvedOnly).Contains(s.SchoolCourse.SchoolId))
                .Select(s => (long?)s.SchoolCourse.SchoolId).FirstOrDefaultAsync();

        public Task<long?> SchoolOfVideoAsync(long videoId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourseVideos.AsNoTracking()
                .Where(v => v.Id == videoId && OwnedSchoolIds(userId, approvedOnly).Contains(v.SchoolCourse.SchoolId))
                .Select(v => (long?)v.SchoolCourse.SchoolId).FirstOrDefaultAsync();

        public Task<long?> SchoolOfLiveSessionAsync(long liveSessionId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourseLiveSessions.AsNoTracking()
                .Where(l => l.Id == liveSessionId && OwnedSchoolIds(userId, approvedOnly).Contains(l.SchoolCourseSession.SchoolCourse.SchoolId))
                .Select(l => (long?)l.SchoolCourseSession.SchoolCourse.SchoolId).FirstOrDefaultAsync();

        public Task<long?> SchoolOfReserveAsync(long reserveId, long userId, bool approvedOnly = true) =>
            _context.SchoolReserves.AsNoTracking()
                .Where(r => r.Id == reserveId && OwnedSchoolIds(userId, approvedOnly).Contains(r.SchoolCourse.SchoolId))
                .Select(r => (long?)r.SchoolCourse.SchoolId).FirstOrDefaultAsync();

        public Task<long?> SchoolOfCoursePetAsync(long coursePetId, long userId, bool approvedOnly = true) =>
            _context.SchoolCoursePets.AsNoTracking()
                .Where(p => p.Id == coursePetId && OwnedSchoolIds(userId, approvedOnly).Contains(p.SchoolCourse.SchoolId))
                .Select(p => (long?)p.SchoolCourse.SchoolId).FirstOrDefaultAsync();
    }
}
