using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolInsightSrv
{
    public class SchoolCourseRateDto
    {
        public long SchoolReserveId { get; set; }
        // ۱ تا ۵
        public int Rate { get; set; }
        public string Text { get; set; }
    }

    public class SchoolCourseRateVDto
    {
        public long Id { get; set; }
        public long SchoolCourseId { get; set; }
        public int? Rate { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public string UserFullName { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class SchoolCourseStatVDto
    {
        public long CourseId { get; set; }
        public string CourseName { get; set; }
        public int CourseTypeId { get; set; }
        public int Level { get; set; }
        public bool Active { get; set; }
        public int? Capacity { get; set; }
        public int Enrolled { get; set; }
        // درصد پر شدن ظرفیت (دوره‌ی بدون سقف: null)
        public int? FillPercent { get; set; }
        public int Waiting { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public double Revenue { get; set; }
        public double ProviderShare { get; set; }
        // درصد حضور از حضور و غیاب‌های ثبت‌شده
        public int? AttendancePercent { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public DateTime? FirstSessionDate { get; set; }
        public DateTime? LastSessionDate { get; set; }
    }

    public class SchoolDashboardVDto
    {
        public long SchoolId { get; set; }
        public string SchoolName { get; set; }
        public int CourseCount { get; set; }
        public int ActiveEnrollments { get; set; }
        public int WaitingCount { get; set; }
        public double Revenue { get; set; }
        public double ProviderShare { get; set; }
        public int? AttendancePercent { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public List<SchoolCourseStatVDto> Courses { get; set; } = new();
    }

    public interface ISchoolInsightService
    {
        Task<BaseResultDto<SchoolCourseRateVDto>> RateAsync(long userId, SchoolCourseRateDto dto);
        Task<BaseResultDto<List<SchoolCourseRateVDto>>> CourseRatesAsync(long courseId, int take = 50);
        Task<BaseResultDto> SetRateStatusAsync(long commentId, bool accept);
        Task<BaseResultDto<SchoolDashboardVDto>> DashboardAsync(long schoolId);
    }

    /// <summary>امتیاز دوره (۲۰۲۶-۱۰-۲۸) و داشبورد مدرسه برای ارائه‌دهنده.</summary>
    public class SchoolInsightService : ISchoolInsightService
    {
        private readonly IDataBaseContext _context;

        public SchoolInsightService(IDataBaseContext context) => _context = context;

        private Task<long?> StatusIdAsync(CommentEnum status) =>
            _context.Codes.AsNoTracking().Where(c => c.Label == status.ToString()).Select(c => (long?)c.Id).FirstOrDefaultAsync();

        /// <summary>
        /// فقط صاحب ثبت‌نامِ «کامل‌شده» می‌تواند به دوره امتیاز بدهد؛ برای هر ثبت‌نام یک نظر (ارسال دوباره = ویرایش).
        /// امتیاز همان لحظه در میانگین می‌آید؛ ادمین با رد کردن نظر آن را از میانگین و فهرست بیرون می‌برد.
        /// </summary>
        public async Task<BaseResultDto<SchoolCourseRateVDto>> RateAsync(long userId, SchoolCourseRateDto dto)
        {
            if (dto == null || dto.Rate < 1 || dto.Rate > 5)
                return new BaseResultDto<SchoolCourseRateVDto>(false, Resource.Notification.TheRangeEnteredIsNotCorrect, null);
            var text = string.IsNullOrWhiteSpace(dto.Text) ? null : dto.Text.Trim();
            if (text?.Length > 1000)
                return new BaseResultDto<SchoolCourseRateVDto>(false, Resource.Notification.InvalidData, null);
            var complete = (int)SchoolReserveStatusEnum.Complete;
            var reserve = await _context.SchoolReserves.AsNoTracking()
                .Where(r => r.Id == dto.SchoolReserveId && r.BookerId == userId && r.IsReserved && !r.IsCancel && r.StatusId == complete)
                .Select(r => new { r.Id, r.SchoolCourseId, r.SchoolCourse.SchoolId })
                .FirstOrDefaultAsync();
            if (reserve == null)
                return new BaseResultDto<SchoolCourseRateVDto>(false, Resource.Notification.NothingFound, null);
            var notChecked = await StatusIdAsync(CommentEnum.Comment_NotChecked);
            if (!notChecked.HasValue)
                return new BaseResultDto<SchoolCourseRateVDto>(false, Resource.Notification.Unsuccess, null);

            var row = await _context.SchoolComments.AsTracking().FirstOrDefaultAsync(c => c.SchoolReserveId == reserve.Id && c.UserId == userId);
            if (row == null)
            {
                row = new SchoolComment
                {
                    SchoolId = reserve.SchoolId, SchoolCourseId = reserve.SchoolCourseId, SchoolReserveId = reserve.Id, IsReserved = true,
                    UserId = userId, CreateDate = DateTime.Now,
                };
                await _context.SchoolComments.AddAsync(row);
            }
            row.Rate = dto.Rate;
            row.Text = text;
            row.StatusId = notChecked.Value;
            await _context.SaveChangesAsync();
            await RecomputeAsync(reserve.SchoolCourseId, reserve.SchoolId);
            return new BaseResultDto<SchoolCourseRateVDto>(true, (await Rates(c => c.Id == row.Id, 1)).FirstOrDefault());
        }

        public async Task<BaseResultDto<List<SchoolCourseRateVDto>>> CourseRatesAsync(long courseId, int take = 50) =>
            new(true, await Rates(c => c.SchoolCourseId == courseId, Math.Clamp(take, 1, 200)));

        public async Task<BaseResultDto> SetRateStatusAsync(long commentId, bool accept)
        {
            var status = await StatusIdAsync(accept ? CommentEnum.Comment_Accept : CommentEnum.Comment_Reject);
            var row = await _context.SchoolComments.AsTracking().FirstOrDefaultAsync(c => c.Id == commentId);
            if (row == null || !status.HasValue)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            row.StatusId = status.Value;
            await _context.SaveChangesAsync();
            if (row.SchoolCourseId.HasValue)
                await RecomputeAsync(row.SchoolCourseId.Value, row.SchoolId);
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        // نظرهای ردنشده (رد = پنهان و بیرون از میانگین)
        private async Task<List<SchoolCourseRateVDto>> Rates(System.Linq.Expressions.Expression<Func<SchoolComment, bool>> filter, int take)
        {
            var reject = await StatusIdAsync(CommentEnum.Comment_Reject) ?? -1;
            return await _context.SchoolComments.AsNoTracking()
                .Where(filter).Where(c => c.SchoolCourseId != null && c.StatusId != reject)
                .OrderByDescending(c => c.CreateDate).Take(take)
                .Select(c => new SchoolCourseRateVDto
                {
                    Id = c.Id, SchoolCourseId = c.SchoolCourseId.Value, Rate = c.Rate, Text = c.Text, Answer = c.Answer, CreateDate = c.CreateDate,
                    UserFullName = c.User == null ? null : ((c.User.FirstName ?? "") + " " + (c.User.LastName ?? "")).Trim(),
                }).ToListAsync();
        }

        // میانگین امتیاز دوره و مدرسه از نظرهای امتیازدارِ ردنشده
        private async Task RecomputeAsync(long courseId, long schoolId)
        {
            var reject = await StatusIdAsync(CommentEnum.Comment_Reject) ?? -1;
            var course = await _context.SchoolComments.AsNoTracking()
                .Where(c => c.SchoolCourseId == courseId && c.Rate != null && c.StatusId != reject)
                .GroupBy(c => 1).Select(g => new { Avg = g.Average(c => (double)c.Rate.Value), Count = g.Count() }).FirstOrDefaultAsync();
            double courseAvg = Math.Round(course?.Avg ?? 0, 1);
            int courseCount = course?.Count ?? 0;
            await _context.SchoolCourses.Where(c => c.Id == courseId).ExecuteUpdateAsync(x => x
                .SetProperty(c => c.RateAvg, courseAvg)
                .SetProperty(c => c.RateCount, courseCount));
            var school = await _context.SchoolComments.AsNoTracking()
                .Where(c => c.SchoolId == schoolId && c.Rate != null && c.StatusId != reject)
                .GroupBy(c => 1).Select(g => new { Avg = g.Average(c => (double)c.Rate.Value), Count = g.Count() }).FirstOrDefaultAsync();
            double schoolAvg = Math.Round(school?.Avg ?? 0, 1);
            int schoolCount = school?.Count ?? 0;
            await _context.Schools.Where(s => s.Id == schoolId).ExecuteUpdateAsync(x => x
                .SetProperty(s => s.RateAvg, schoolAvg)
                .SetProperty(s => s.RateCount, schoolCount)
                .SetProperty(s => s.CommentCount, schoolCount));
        }

        public async Task<BaseResultDto<SchoolDashboardVDto>> DashboardAsync(long schoolId)
        {
            var school = await _context.Schools.AsNoTracking().Where(s => s.Id == schoolId)
                .Select(s => new { s.Id, s.Name, s.RateAvg, s.RateCount }).FirstOrDefaultAsync();
            if (school == null)
                return new BaseResultDto<SchoolDashboardVDto>(false, Resource.Notification.NothingFound, null);

            var paid = (int)SchoolReserveStatusEnum.Paid;
            var complete = (int)SchoolReserveStatusEnum.Complete;
            var courses = await _context.SchoolCourses.AsNoTracking()
                .Where(c => c.SchoolId == schoolId && !c.Deleted)
                .OrderByDescending(c => c.Active).ThenByDescending(c => c.Id)
                .Select(c => new SchoolCourseStatVDto
                {
                    CourseId = c.Id, CourseName = c.Name, CourseTypeId = c.CourseTypeId, Level = c.Level, Active = c.Active, Capacity = c.Capacity,
                    RateAvg = c.RateAvg, RateCount = c.RateCount,
                    Enrolled = c.SchoolReserves.Count(r => r.IsReserved && !r.IsCancel && (r.StatusId == paid || r.StatusId == complete)),
                    Completed = c.SchoolReserves.Count(r => r.IsReserved && !r.IsCancel && r.StatusId == complete),
                    Cancelled = c.SchoolReserves.Count(r => r.IsReserved && r.IsCancel),
                    // درآمد = پرداختی‌های لغونشده + بخشِ نگه‌داشته‌ی لغوشده‌ها (سهم‌ها)
                    Revenue = c.SchoolReserves.Where(r => r.IsReserved && !r.IsCancel).Sum(r => (double?)r.PaymentPrice) ?? 0,
                    ProviderShare = c.SchoolReserves.Where(r => r.IsReserved && (!r.IsCancel || r.CompanionShare > 0)).Sum(r => (double?)r.CompanionShare) ?? 0,
                    Waiting = _context.SchoolCourseWaitlists.Count(w => w.SchoolCourseId == c.Id && !w.Deleted),
                    FirstSessionDate = c.SchoolCourseSessions.Where(s => !s.Deleted).Min(s => (DateTime?)s.SessionDate),
                    LastSessionDate = c.SchoolCourseSessions.Where(s => !s.Deleted).Max(s => (DateTime?)s.SessionDate),
                }).ToListAsync();

            var courseIds = courses.Select(c => c.CourseId).ToList();
            var attendance = await _context.SchoolAttendances.AsNoTracking()
                .Where(a => courseIds.Contains(a.SchoolCourseSession.SchoolCourseId))
                .GroupBy(a => a.SchoolCourseSession.SchoolCourseId)
                .Select(g => new { CourseId = g.Key, Total = g.Count(), Present = g.Count(a => a.Present) })
                .ToListAsync();
            foreach (var c in courses)
            {
                c.FillPercent = c.Capacity is > 0 ? (int)Math.Round(100.0 * c.Enrolled / c.Capacity.Value) : null;
                var a = attendance.FirstOrDefault(x => x.CourseId == c.CourseId);
                c.AttendancePercent = a == null || a.Total == 0 ? null : (int)Math.Round(100.0 * a.Present / a.Total);
            }

            var totalMarks = attendance.Sum(a => a.Total);
            return new BaseResultDto<SchoolDashboardVDto>(true, new SchoolDashboardVDto
            {
                SchoolId = school.Id,
                SchoolName = school.Name,
                CourseCount = courses.Count,
                ActiveEnrollments = courses.Sum(c => c.Enrolled - c.Completed),
                WaitingCount = courses.Sum(c => c.Waiting),
                Revenue = courses.Sum(c => c.Revenue),
                ProviderShare = courses.Sum(c => c.ProviderShare),
                AttendancePercent = totalMarks == 0 ? null : (int)Math.Round(100.0 * attendance.Sum(a => a.Present) / totalMarks),
                RateAvg = school.RateAvg,
                RateCount = school.RateCount,
                Courses = courses,
            });
        }
    }
}
