using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolOwnershipSrv
{
    /// <summary>
    /// مالکیت مدرسه/دوره/جلسه/ثبت‌نام برای پنل نماینده: همیشه با «مالکِ نمایندگیِ همان مدرسه» (Companion.OwnerId) از دیتابیس سنجیده می‌شود،
    /// نه با CompanionId داخل توکن — آن فقط اولین نمایندگیِ فعال و تأییدشده است و تا ورود دوباره (بعد از تأیید ادمین) خالی/کهنه می‌ماند و
    /// برای مالک چند نمایندگی هم فقط یکی را می‌دید. هر متد شناسه‌ی نمایندگیِ صاحب منبع را برمی‌گرداند (یا null = مالک نیست).
    /// approvedOnly: عملیات نوشتن فقط برای نمایندگی فعال و تأییدشده؛ خواندن مال خود می‌تواند بدون آن باشد.
    /// </summary>
    public interface ISchoolOwnershipService
    {
        Task<List<long>> OwnedCompanionIdsAsync(long userId, bool approvedOnly = true);
        /// <summary>نمایندگی هدف برای ساخت مدرسه‌ی جدید: درخواستی (باید مال خودش باشد) یا تنها نمایندگیِ فعالش.</summary>
        Task<long?> ResolveCompanionForNewSchoolAsync(long userId, long? requestedCompanionId);
        Task<long?> CompanionOfSchoolAsync(long schoolId, long userId, bool approvedOnly = true);
        Task<long?> CompanionOfCourseAsync(long courseId, long userId, bool approvedOnly = true);
        Task<long?> CompanionOfSessionAsync(long sessionId, long userId, bool approvedOnly = true);
        Task<long?> CompanionOfVideoAsync(long videoId, long userId, bool approvedOnly = true);
        Task<long?> CompanionOfLiveSessionAsync(long liveSessionId, long userId, bool approvedOnly = true);
        Task<long?> CompanionOfReserveAsync(long reserveId, long userId, bool approvedOnly = true);
        Task<long?> CompanionOfCoursePetAsync(long coursePetId, long userId, bool approvedOnly = true);
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

        public async Task<long?> ResolveCompanionForNewSchoolAsync(long userId, long? requestedCompanionId)
        {
            var ids = await OwnedCompanionIdsAsync(userId);
            if (requestedCompanionId is > 0)
                return ids.Contains(requestedCompanionId.Value) ? requestedCompanionId : null;
            return ids.Count == 1 ? ids[0] : null;
        }

        public Task<long?> CompanionOfSchoolAsync(long schoolId, long userId, bool approvedOnly = true) =>
            _context.Schools.AsNoTracking()
                .Where(s => s.Id == schoolId && !s.Deleted && s.Companion.OwnerId == userId && !s.Companion.Deleted
                            && (!approvedOnly || (s.Companion.Active && s.Companion.Approved)))
                .Select(s => (long?)s.CompanionId).FirstOrDefaultAsync();

        public Task<long?> CompanionOfCourseAsync(long courseId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourses.AsNoTracking()
                .Where(c => c.Id == courseId && !c.School.Deleted && c.School.Companion.OwnerId == userId && !c.School.Companion.Deleted
                            && (!approvedOnly || (c.School.Companion.Active && c.School.Companion.Approved)))
                .Select(c => (long?)c.School.CompanionId).FirstOrDefaultAsync();

        public Task<long?> CompanionOfSessionAsync(long sessionId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourseSessions.AsNoTracking()
                .Where(s => s.Id == sessionId && s.SchoolCourse.School.Companion.OwnerId == userId && !s.SchoolCourse.School.Companion.Deleted
                            && (!approvedOnly || (s.SchoolCourse.School.Companion.Active && s.SchoolCourse.School.Companion.Approved)))
                .Select(s => (long?)s.SchoolCourse.School.CompanionId).FirstOrDefaultAsync();

        public Task<long?> CompanionOfVideoAsync(long videoId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourseVideos.AsNoTracking()
                .Where(v => v.Id == videoId && v.SchoolCourse.School.Companion.OwnerId == userId && !v.SchoolCourse.School.Companion.Deleted
                            && (!approvedOnly || (v.SchoolCourse.School.Companion.Active && v.SchoolCourse.School.Companion.Approved)))
                .Select(v => (long?)v.SchoolCourse.School.CompanionId).FirstOrDefaultAsync();

        public Task<long?> CompanionOfLiveSessionAsync(long liveSessionId, long userId, bool approvedOnly = true) =>
            _context.SchoolCourseLiveSessions.AsNoTracking()
                .Where(l => l.Id == liveSessionId && l.SchoolCourseSession.SchoolCourse.School.Companion.OwnerId == userId
                            && !l.SchoolCourseSession.SchoolCourse.School.Companion.Deleted
                            && (!approvedOnly || (l.SchoolCourseSession.SchoolCourse.School.Companion.Active && l.SchoolCourseSession.SchoolCourse.School.Companion.Approved)))
                .Select(l => (long?)l.SchoolCourseSession.SchoolCourse.School.CompanionId).FirstOrDefaultAsync();

        public Task<long?> CompanionOfReserveAsync(long reserveId, long userId, bool approvedOnly = true) =>
            _context.SchoolReserves.AsNoTracking()
                .Where(r => r.Id == reserveId && r.SchoolCourse.School.Companion.OwnerId == userId && !r.SchoolCourse.School.Companion.Deleted
                            && (!approvedOnly || (r.SchoolCourse.School.Companion.Active && r.SchoolCourse.School.Companion.Approved)))
                .Select(r => (long?)r.SchoolCourse.School.CompanionId).FirstOrDefaultAsync();

        public Task<long?> CompanionOfCoursePetAsync(long coursePetId, long userId, bool approvedOnly = true) =>
            _context.SchoolCoursePets.AsNoTracking()
                .Where(p => p.Id == coursePetId && p.SchoolCourse.School.Companion.OwnerId == userId && !p.SchoolCourse.School.Companion.Deleted
                            && (!approvedOnly || (p.SchoolCourse.School.Companion.Active && p.SchoolCourse.School.Companion.Approved)))
                .Select(p => (long?)p.SchoolCourse.School.CompanionId).FirstOrDefaultAsync();
    }
}
