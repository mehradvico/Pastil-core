using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.SchoolSrvs.SchoolCourseSrv;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolWaitlistSrv
{
    public class SchoolWaitlistVDto
    {
        public long Id { get; set; }
        public long SchoolCourseId { get; set; }
        public string CourseName { get; set; }
        public string SchoolName { get; set; }
        public long UserPetId { get; set; }
        public string PetName { get; set; }
        public DateTime CreateDate { get; set; }
        // جایگاه در صف (۱ = اول)
        public int Position { get; set; }
        public DateTime? NotifiedDate { get; set; }
    }

    public class SchoolWaitlistJoinDto
    {
        public long SchoolCourseId { get; set; }
        public long UserPetId { get; set; }
    }

    public interface ISchoolWaitlistService
    {
        Task<BaseResultDto<SchoolWaitlistVDto>> JoinAsync(long userId, SchoolWaitlistJoinDto dto);
        Task<BaseResultDto> LeaveAsync(long userId, long id);
        Task<BaseResultDto<List<SchoolWaitlistVDto>>> MineAsync(long userId);
        /// <summary>برای مالک/مدرس دوره: صف انتظار یک دوره</summary>
        Task<BaseResultDto<List<SchoolWaitlistVDto>>> CourseAsync(long courseId);
    }

    /// <summary>
    /// لیست انتظار دوره‌ی پر (۲۰۲۶-۱۰-۲۸): فقط وقتی دوره پر است می‌شود عضو شد. با خالی شدن جا (لغو)، به همه‌ی افراد صف
    /// پوش می‌رود و هر کس زودتر ثبت‌نام کند جا را می‌گیرد (رزرو جا برای نفر اول نداریم تا ظرفیت قفل نماند).
    /// </summary>
    public class SchoolWaitlistService : ISchoolWaitlistService
    {
        private readonly IDataBaseContext _context;

        public SchoolWaitlistService(IDataBaseContext context) => _context = context;

        public async Task<BaseResultDto<SchoolWaitlistVDto>> JoinAsync(long userId, SchoolWaitlistJoinDto dto)
        {
            if (dto == null || dto.SchoolCourseId <= 0 || dto.UserPetId <= 0)
                return new BaseResultDto<SchoolWaitlistVDto>(false, Resource.Notification.InvalidData, null);
            var course = await _context.SchoolCourses.AsNoTracking()
                .Where(c => c.Id == dto.SchoolCourseId && !c.Deleted && c.Active && !c.School.Deleted)
                .Select(c => new { c.Id, c.Capacity })
                .FirstOrDefaultAsync();
            if (course == null)
                return new BaseResultDto<SchoolWaitlistVDto>(false, Resource.Notification.NothingFound, null);
            if (!await _context.UserPets.AnyAsync(p => p.Id == dto.UserPetId && p.UserId == userId))
                return new BaseResultDto<SchoolWaitlistVDto>(false, Resource.Notification.InvalidData, null);
            if (await _context.SchoolReserves.AnyAsync(r => r.SchoolCourseId == dto.SchoolCourseId && r.UserPetId == dto.UserPetId && !r.IsCancel && r.IsReserved))
                return new BaseResultDto<SchoolWaitlistVDto>(false, Resource.Notification.PaymentReserveAlreadyPaid, null);
            if (!await IsFullAsync(_context, course.Id, course.Capacity, DateTime.Now))
                return new BaseResultDto<SchoolWaitlistVDto>(false, Resource.Notification.SchoolWaitlistOnlyWhenFull, null);
            if (await _context.SchoolCourseWaitlists.AnyAsync(w => w.SchoolCourseId == dto.SchoolCourseId && w.UserPetId == dto.UserPetId && !w.Deleted))
                return new BaseResultDto<SchoolWaitlistVDto>(false, Resource.Notification.SchoolWaitlistAlreadyJoined, null);

            var row = new SchoolCourseWaitlist { SchoolCourseId = dto.SchoolCourseId, UserId = userId, UserPetId = dto.UserPetId, CreateDate = DateTime.Now };
            await _context.SchoolCourseWaitlists.AddAsync(row);
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolWaitlistVDto>(true, (await LoadAsync(w => w.Id == row.Id)).FirstOrDefault());
        }

        public async Task<BaseResultDto> LeaveAsync(long userId, long id)
        {
            var affected = await _context.SchoolCourseWaitlists
                .Where(w => w.Id == id && w.UserId == userId && !w.Deleted)
                .ExecuteUpdateAsync(x => x.SetProperty(w => w.Deleted, true));
            return affected == 0 ? new BaseResultDto(false, Resource.Notification.NothingFound) : new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto<List<SchoolWaitlistVDto>>> MineAsync(long userId) =>
            new(true, await LoadAsync(w => w.UserId == userId));

        public async Task<BaseResultDto<List<SchoolWaitlistVDto>>> CourseAsync(long courseId) =>
            new(true, await LoadAsync(w => w.SchoolCourseId == courseId));

        private async Task<List<SchoolWaitlistVDto>> LoadAsync(System.Linq.Expressions.Expression<Func<SchoolCourseWaitlist, bool>> filter)
        {
            var rows = await _context.SchoolCourseWaitlists.AsNoTracking()
                .Where(w => !w.Deleted).Where(filter)
                .OrderBy(w => w.CreateDate).ThenBy(w => w.Id)
                .Select(w => new SchoolWaitlistVDto
                {
                    Id = w.Id,
                    SchoolCourseId = w.SchoolCourseId,
                    CourseName = w.SchoolCourse.Name,
                    SchoolName = w.SchoolCourse.School.Name,
                    UserPetId = w.UserPetId,
                    PetName = w.UserPet.Name,
                    CreateDate = w.CreateDate,
                    NotifiedDate = w.NotifiedDate,
                    // جایگاه = تعداد ردیف‌های فعالِ زودترِ همان دوره + ۱
                    Position = _context.SchoolCourseWaitlists.Count(o => o.SchoolCourseId == w.SchoolCourseId && !o.Deleted
                        && (o.CreateDate < w.CreateDate || (o.CreateDate == w.CreateDate && o.Id < w.Id))) + 1,
                })
                .ToListAsync();
            return rows;
        }

        public static async Task<bool> IsFullAsync(IDataBaseContext context, long courseId, int? capacity, DateTime now)
        {
            if (!capacity.HasValue)
                return false;
            var taken = await SchoolCourseRules.SeatTakers(context.SchoolReserves.Where(r => r.SchoolCourseId == courseId), now).CountAsync();
            return taken >= capacity.Value;
        }
    }

    public static class SchoolWaitlistNotifier
    {
        /// <summary>
        /// بعد از خالی شدن یک جا: اگر دوره واقعاً جا دارد و ثبت‌نامش باز است، به همه‌ی افراد صف پوش «جا باز شد» می‌رود.
        /// NotifiedDate ثبت می‌شود (برای نمایش)، ولی دوباره خالی شدن جا دوباره خبر می‌دهد.
        /// </summary>
        public static async Task NotifySeatOpenAsync(IDataBaseContext context, IPushNotificationService push, long courseId, DateTime now)
        {
            var course = await context.SchoolCourses.AsNoTracking()
                .Where(c => c.Id == courseId && !c.Deleted && c.Active)
                .Select(c => new { c.Id, c.Name, c.Capacity })
                .FirstOrDefaultAsync();
            if (course == null)
                return;
            if (await SchoolWaitlistService.IsFullAsync(context, courseId, course.Capacity, now))
                return;
            if (!await SchoolCourseRules.IsRegistrationOpenAsync(context, courseId, now))
                return;

            var users = await context.SchoolCourseWaitlists.AsNoTracking()
                .Where(w => w.SchoolCourseId == courseId && !w.Deleted)
                .Select(w => w.UserId).Distinct().ToListAsync();
            foreach (var userId in users)
                await push.SendPushAsync(PushTypeEnum.PushSchoolWaitlistSeatOpen, userId, token1: course.Name, token2: course.Id.ToString());

            await context.SchoolCourseWaitlists
                .Where(w => w.SchoolCourseId == courseId && !w.Deleted)
                .ExecuteUpdateAsync(x => x.SetProperty(w => w.NotifiedDate, (DateTime?)now));
        }
    }
}
