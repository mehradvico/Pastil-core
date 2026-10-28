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
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolTeachingSrv
{
    // ---- DTOها

    public class SchoolInstructorVDto
    {
        public long Id { get; set; }
        public long SchoolCourseId { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public bool IsLead { get; set; }
    }

    public class SchoolInstructorAddDto
    {
        public long SchoolCourseId { get; set; }
        // کاربر با موبایل پیدا می‌شود (باید قبلاً در پاستیل ثبت‌نام کرده باشد)
        public string Mobile { get; set; }
        public bool IsLead { get; set; }
    }

    public class SchoolRosterItemVDto
    {
        public long SchoolReserveId { get; set; }
        public string ReserveCode { get; set; }
        public long UserPetId { get; set; }
        public string PetName { get; set; }
        public string OwnerFullName { get; set; }
        // null = هنوز ثبت نشده
        public bool? Present { get; set; }
        public string Note { get; set; }
    }

    public class SchoolAttendanceItemDto
    {
        public long SchoolReserveId { get; set; }
        public bool Present { get; set; }
        public string Note { get; set; }
    }

    public class SchoolAttendanceSaveDto
    {
        public long SchoolCourseSessionId { get; set; }
        public List<SchoolAttendanceItemDto> Items { get; set; } = new();
    }

    public class SchoolSkillDto
    {
        public long Id { get; set; }
        public long SchoolCourseId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
    }

    public class SchoolProgressItemDto
    {
        public long SchoolCourseSkillId { get; set; }
        public int Score { get; set; }
        public string Note { get; set; }
    }

    public class SchoolProgressSaveDto
    {
        public long SchoolReserveId { get; set; }
        public long? SchoolCourseSessionId { get; set; }
        public List<SchoolProgressItemDto> Items { get; set; } = new();
    }

    public class SchoolProgressEntryVDto
    {
        public int Score { get; set; }
        public string Note { get; set; }
        public long? SchoolCourseSessionId { get; set; }
        public DateTime? SessionDate { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class SchoolSkillProgressVDto
    {
        public long SkillId { get; set; }
        public string SkillName { get; set; }
        // آخرین نمره (۱ تا ۵) یا null
        public int? LatestScore { get; set; }
        public List<SchoolProgressEntryVDto> History { get; set; } = new();
    }

    public class SchoolAttendanceVDto
    {
        public long SchoolCourseSessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public string StartTime { get; set; }
        public bool? Present { get; set; }
        public string Note { get; set; }
    }

    public class SchoolCertificateVDto
    {
        public string Code { get; set; }
        public DateTime IssueDate { get; set; }
        public string CourseName { get; set; }
        public int CourseLevel { get; set; }
        public string SchoolName { get; set; }
        public string PetName { get; set; }
        public string OwnerFullName { get; set; }
        public int SessionCount { get; set; }
        public int AttendedCount { get; set; }
    }

    /// <summary>کارنامه‌ی یک ثبت‌نام: حضور و غیاب، مهارت‌ها و گواهی (برای مالک پت و مدرس)</summary>
    public class SchoolReportVDto
    {
        public long SchoolReserveId { get; set; }
        public string CourseName { get; set; }
        public string PetName { get; set; }
        public int StatusId { get; set; }
        public int SessionCount { get; set; }
        public int AttendedCount { get; set; }
        public int AbsentCount { get; set; }
        // درصد حضور از جلسه‌هایی که حضورشان ثبت شده
        public int AttendancePercent { get; set; }
        public List<SchoolAttendanceVDto> Attendance { get; set; } = new();
        public List<SchoolSkillProgressVDto> Skills { get; set; } = new();
        public SchoolCertificateVDto Certificate { get; set; }
    }

    public interface ISchoolTeachingService
    {
        Task<bool> CanTeachCourseAsync(long courseId, long userId);
        Task<long?> CourseOfSessionAsync(long sessionId);
        Task<long?> CourseOfReserveAsync(long reserveId);

        Task<BaseResultDto<List<SchoolInstructorVDto>>> InstructorsAsync(long courseId);
        Task<BaseResultDto<SchoolInstructorVDto>> AddInstructorAsync(SchoolInstructorAddDto dto);
        Task<BaseResultDto> RemoveInstructorAsync(long courseId, long id);
        /// <summary>دوره‌هایی که کاربر مدرسشان است (برای اپ ارائه‌دهنده/مدرس)</summary>
        Task<List<long>> CoursesOfInstructorAsync(long userId);

        Task<BaseResultDto<List<SchoolRosterItemVDto>>> RosterAsync(long sessionId);
        Task<BaseResultDto> SaveAttendanceAsync(SchoolAttendanceSaveDto dto, long userId);

        Task<BaseResultDto<List<SchoolSkillDto>>> SkillsAsync(long courseId);
        Task<BaseResultDto<SchoolSkillDto>> UpsertSkillAsync(SchoolSkillDto dto);
        Task<BaseResultDto> DeleteSkillAsync(long courseId, long id);
        Task<BaseResultDto> SaveProgressAsync(SchoolProgressSaveDto dto, long userId);

        Task<BaseResultDto<SchoolReportVDto>> ReportAsync(long reserveId);
        Task<BaseResultDto<SchoolCertificateVDto>> IssueCertificateAsync(long reserveId, long userId);
        /// <summary>بعد از «کامل‌شده»: اگر دوره گواهی دارد خودکار صادر می‌شود (خطا را بالا نمی‌برد).</summary>
        Task IssueCertificateIfEnabledAsync(long reserveId, long userId);
        Task<BaseResultDto<SchoolCertificateVDto>> VerifyCertificateAsync(string code);
        /// <summary>Job ساعتی: یادآوری «فردا کلاس دارید» برای جلسه‌هایی که ۲۰ تا ۲۸ ساعت دیگر شروع می‌شوند (یک‌بار).</summary>
        Task SendDayBeforeRemindersAsync(System.Threading.CancellationToken cancellationToken);
    }

    public class SchoolTeachingService : ISchoolTeachingService
    {
        public const int MinScore = 1;
        public const int MaxScore = 5;
        private const int MaxNote = 500;

        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _push;

        public SchoolTeachingService(IDataBaseContext context, IPushNotificationService push)
        {
            _context = context;
            _push = push;
        }

        // ---------------------------------------------------------------- دسترسی

        /// <summary>مالکِ مدرسه (ارائه‌دهنده/نمایندگی فعال و تأییدشده) یا مدرس فعالِ همین دوره.</summary>
        public async Task<bool> CanTeachCourseAsync(long courseId, long userId)
        {
            if (await _context.SchoolCourseInstructors.AnyAsync(i => i.SchoolCourseId == courseId && i.UserId == userId && !i.Deleted && !i.SchoolCourse.Deleted))
                return true;
            return await _context.SchoolCourses.AnyAsync(c => c.Id == courseId && !c.Deleted && !c.School.Deleted && (
                (c.School.Provider != null && c.School.Provider.UserId == userId && !c.School.Provider.Deleted && c.School.Provider.Active && c.School.Provider.Approved)
                || (c.School.Companion != null && c.School.Companion.OwnerId == userId && !c.School.Companion.Deleted && c.School.Companion.Active && c.School.Companion.Approved)));
        }

        public Task<long?> CourseOfSessionAsync(long sessionId) =>
            _context.SchoolCourseSessions.AsNoTracking().Where(s => s.Id == sessionId && !s.Deleted).Select(s => (long?)s.SchoolCourseId).FirstOrDefaultAsync();

        public Task<long?> CourseOfReserveAsync(long reserveId) =>
            _context.SchoolReserves.AsNoTracking().Where(r => r.Id == reserveId).Select(r => (long?)r.SchoolCourseId).FirstOrDefaultAsync();

        // ---------------------------------------------------------------- مدرس‌ها

        public async Task<BaseResultDto<List<SchoolInstructorVDto>>> InstructorsAsync(long courseId) =>
            new(true, await _context.SchoolCourseInstructors.AsNoTracking()
                .Where(i => i.SchoolCourseId == courseId && !i.Deleted)
                .OrderByDescending(i => i.IsLead).ThenBy(i => i.Id)
                .Select(i => new SchoolInstructorVDto
                {
                    Id = i.Id, SchoolCourseId = i.SchoolCourseId, UserId = i.UserId, IsLead = i.IsLead,
                    FullName = ((i.User.FirstName ?? "") + " " + (i.User.LastName ?? "")).Trim(),
                    Mobile = i.User.Mobile,
                }).ToListAsync());

        public async Task<BaseResultDto<SchoolInstructorVDto>> AddInstructorAsync(SchoolInstructorAddDto dto)
        {
            var mobile = dto?.Mobile?.Trim();
            if (string.IsNullOrEmpty(mobile) || dto.SchoolCourseId <= 0)
                return new BaseResultDto<SchoolInstructorVDto>(false, Resource.Notification.InvalidData, null);
            var userId = await _context.Users.AsNoTracking().Where(u => u.Mobile == mobile).Select(u => (long?)u.Id).FirstOrDefaultAsync();
            if (!userId.HasValue)
                return new BaseResultDto<SchoolInstructorVDto>(false, Resource.Notification.SchoolInstructorUserNotFound, null);
            if (await _context.SchoolCourseInstructors.AnyAsync(i => i.SchoolCourseId == dto.SchoolCourseId && i.UserId == userId.Value && !i.Deleted))
                return new BaseResultDto<SchoolInstructorVDto>(false, Resource.Notification.SchoolInstructorAlreadyAdded, null);

            // فقط یک مدرس اصلی: مدرس اصلی جدید، قبلی را کمک‌مدرس می‌کند
            if (dto.IsLead)
                await _context.SchoolCourseInstructors.Where(i => i.SchoolCourseId == dto.SchoolCourseId && i.IsLead && !i.Deleted)
                    .ExecuteUpdateAsync(x => x.SetProperty(i => i.IsLead, false));

            var row = new SchoolCourseInstructor { SchoolCourseId = dto.SchoolCourseId, UserId = userId.Value, IsLead = dto.IsLead, CreateDate = DateTime.Now };
            await _context.SchoolCourseInstructors.AddAsync(row);
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolInstructorVDto>(true, (await InstructorsAsync(dto.SchoolCourseId)).Data.FirstOrDefault(i => i.Id == row.Id));
        }

        public async Task<BaseResultDto> RemoveInstructorAsync(long courseId, long id)
        {
            var affected = await _context.SchoolCourseInstructors
                .Where(i => i.Id == id && i.SchoolCourseId == courseId && !i.Deleted)
                .ExecuteUpdateAsync(x => x.SetProperty(i => i.Deleted, true));
            return affected == 0 ? new BaseResultDto(false, Resource.Notification.NothingFound) : new BaseResultDto(true, Resource.Notification.Success);
        }

        public Task<List<long>> CoursesOfInstructorAsync(long userId) =>
            _context.SchoolCourseInstructors.AsNoTracking()
                .Where(i => i.UserId == userId && !i.Deleted && !i.SchoolCourse.Deleted)
                .Select(i => i.SchoolCourseId).Distinct().ToListAsync();

        // ---------------------------------------------------------------- حضور و غیاب

        // ثبت‌نام‌های فعال دوره (پرداخت‌شده یا کامل‌شده، لغونشده)
        private IQueryable<SchoolReserve> ActiveReserves(long courseId)
        {
            var paid = (int)SchoolReserveStatusEnum.Paid;
            var complete = (int)SchoolReserveStatusEnum.Complete;
            return _context.SchoolReserves.AsNoTracking()
                .Where(r => r.SchoolCourseId == courseId && r.IsReserved && !r.IsCancel && (r.StatusId == paid || r.StatusId == complete));
        }

        public async Task<BaseResultDto<List<SchoolRosterItemVDto>>> RosterAsync(long sessionId)
        {
            var courseId = await CourseOfSessionAsync(sessionId);
            if (!courseId.HasValue)
                return new BaseResultDto<List<SchoolRosterItemVDto>>(false, Resource.Notification.NothingFound, null);
            var marks = await _context.SchoolAttendances.AsNoTracking()
                .Where(a => a.SchoolCourseSessionId == sessionId)
                .ToDictionaryAsync(a => a.SchoolReserveId, a => new { a.Present, a.Note });
            var roster = await ActiveReserves(courseId.Value)
                .OrderBy(r => r.Id)
                .Select(r => new SchoolRosterItemVDto
                {
                    SchoolReserveId = r.Id, ReserveCode = r.ReserveCode, UserPetId = r.UserPetId, PetName = r.UserPet.Name,
                    OwnerFullName = ((r.Booker.FirstName ?? "") + " " + (r.Booker.LastName ?? "")).Trim(),
                }).ToListAsync();
            foreach (var item in roster)
                if (marks.TryGetValue(item.SchoolReserveId, out var m))
                {
                    item.Present = m.Present;
                    item.Note = m.Note;
                }
            return new BaseResultDto<List<SchoolRosterItemVDto>>(true, roster);
        }

        public async Task<BaseResultDto> SaveAttendanceAsync(SchoolAttendanceSaveDto dto, long userId)
        {
            if (dto == null || dto.Items == null || dto.Items.Count == 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var session = await _context.SchoolCourseSessions.AsNoTracking()
                .Where(s => s.Id == dto.SchoolCourseSessionId && !s.Deleted)
                .Select(s => new { s.Id, s.SchoolCourseId, CourseName = s.SchoolCourse.Name })
                .FirstOrDefaultAsync();
            if (session == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (dto.Items.Any(i => (i.Note?.Length ?? 0) > MaxNote))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var reserveIds = dto.Items.Select(i => i.SchoolReserveId).Distinct().ToList();
            var valid = await ActiveReserves(session.SchoolCourseId).Where(r => reserveIds.Contains(r.Id))
                .Select(r => new { r.Id, r.BookerId }).ToListAsync();
            if (valid.Count != reserveIds.Count)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var existing = await _context.SchoolAttendances.AsTracking()
                .Where(a => a.SchoolCourseSessionId == session.Id && reserveIds.Contains(a.SchoolReserveId))
                .ToDictionaryAsync(a => a.SchoolReserveId);
            var now = DateTime.Now;
            foreach (var item in dto.Items)
            {
                var note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim();
                if (existing.TryGetValue(item.SchoolReserveId, out var row))
                {
                    row.Present = item.Present;
                    row.Note = note;
                    row.RecordedByUserId = userId;
                    row.UpdateDate = now;
                }
                else
                {
                    row = new SchoolAttendance { SchoolCourseSessionId = session.Id, SchoolReserveId = item.SchoolReserveId, Present = item.Present, Note = note, RecordedByUserId = userId, CreateDate = now };
                    await _context.SchoolAttendances.AddAsync(row);
                    existing[item.SchoolReserveId] = row;
                }
            }
            await _context.SaveChangesAsync();

            // غایب‌ها پیام «جلسه را از دست دادی» می‌گیرند (یک‌بار برای هر جلسه)
            var bookers = valid.ToDictionary(v => v.Id, v => v.BookerId);
            foreach (var row in existing.Values.Where(r => !r.Present && r.MissedPushSentDate == null))
            {
                await _push.SendPushAsync(PushTypeEnum.PushSchoolSessionMissed, bookers[row.SchoolReserveId], token1: session.CourseName, token2: session.SchoolCourseId.ToString());
                row.MissedPushSentDate = now;
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        // ---------------------------------------------------------------- مهارت‌ها و کارنامه

        public async Task<BaseResultDto<List<SchoolSkillDto>>> SkillsAsync(long courseId) =>
            new(true, await _context.SchoolCourseSkills.AsNoTracking()
                .Where(s => s.SchoolCourseId == courseId && !s.Deleted)
                .OrderBy(s => s.SortOrder).ThenBy(s => s.Id)
                .Select(s => new SchoolSkillDto { Id = s.Id, SchoolCourseId = s.SchoolCourseId, Name = s.Name, SortOrder = s.SortOrder })
                .ToListAsync());

        public async Task<BaseResultDto<SchoolSkillDto>> UpsertSkillAsync(SchoolSkillDto dto)
        {
            var name = dto?.Name?.Trim();
            if (string.IsNullOrEmpty(name) || name.Length > 100)
                return new BaseResultDto<SchoolSkillDto>(false, Resource.Notification.PleaseEnterTheName, dto);
            SchoolCourseSkill row;
            if (dto.Id > 0)
            {
                row = await _context.SchoolCourseSkills.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.SchoolCourseId == dto.SchoolCourseId && !s.Deleted);
                if (row == null)
                    return new BaseResultDto<SchoolSkillDto>(false, Resource.Notification.NothingFound, dto);
                row.Name = name;
                row.SortOrder = dto.SortOrder;
            }
            else
            {
                row = new SchoolCourseSkill { SchoolCourseId = dto.SchoolCourseId, Name = name, SortOrder = dto.SortOrder };
                await _context.SchoolCourseSkills.AddAsync(row);
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolSkillDto>(true, new SchoolSkillDto { Id = row.Id, SchoolCourseId = row.SchoolCourseId, Name = row.Name, SortOrder = row.SortOrder });
        }

        public async Task<BaseResultDto> DeleteSkillAsync(long courseId, long id)
        {
            var affected = await _context.SchoolCourseSkills.Where(s => s.Id == id && s.SchoolCourseId == courseId && !s.Deleted)
                .ExecuteUpdateAsync(x => x.SetProperty(s => s.Deleted, true));
            return affected == 0 ? new BaseResultDto(false, Resource.Notification.NothingFound) : new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SaveProgressAsync(SchoolProgressSaveDto dto, long userId)
        {
            if (dto == null || dto.Items == null || dto.Items.Count == 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            if (dto.Items.Any(i => i.Score < MinScore || i.Score > MaxScore))
                return new BaseResultDto(false, Resource.Notification.SchoolProgressScoreInvalid);
            if (dto.Items.Any(i => (i.Note?.Length ?? 0) > MaxNote))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var courseId = await CourseOfReserveAsync(dto.SchoolReserveId);
            if (!courseId.HasValue || !await ActiveReserves(courseId.Value).AnyAsync(r => r.Id == dto.SchoolReserveId))
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var skillIds = dto.Items.Select(i => i.SchoolCourseSkillId).Distinct().ToList();
            var validSkills = await _context.SchoolCourseSkills.CountAsync(s => skillIds.Contains(s.Id) && s.SchoolCourseId == courseId.Value && !s.Deleted);
            if (validSkills != skillIds.Count)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            if (dto.SchoolCourseSessionId.HasValue
                && !await _context.SchoolCourseSessions.AnyAsync(s => s.Id == dto.SchoolCourseSessionId.Value && s.SchoolCourseId == courseId.Value && !s.Deleted))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var now = DateTime.Now;
            await _context.SchoolProgresses.AddRangeAsync(dto.Items.Select(i => new SchoolProgress
            {
                SchoolReserveId = dto.SchoolReserveId,
                SchoolCourseSkillId = i.SchoolCourseSkillId,
                SchoolCourseSessionId = dto.SchoolCourseSessionId,
                Score = i.Score,
                Note = string.IsNullOrWhiteSpace(i.Note) ? null : i.Note.Trim(),
                RecordedByUserId = userId,
                CreateDate = now,
            }));
            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto<SchoolReportVDto>> ReportAsync(long reserveId)
        {
            var reserve = await _context.SchoolReserves.AsNoTracking()
                .Where(r => r.Id == reserveId && r.IsReserved)
                .Select(r => new { r.Id, r.SchoolCourseId, r.StatusId, CourseName = r.SchoolCourse.Name, PetName = r.UserPet.Name })
                .FirstOrDefaultAsync();
            if (reserve == null)
                return new BaseResultDto<SchoolReportVDto>(false, Resource.Notification.NothingFound, null);

            var sessions = await _context.SchoolCourseSessions.AsNoTracking()
                .Where(s => s.SchoolCourseId == reserve.SchoolCourseId && !s.Deleted)
                .OrderBy(s => s.SessionDate).ThenBy(s => s.StartTime)
                .Select(s => new { s.Id, s.SessionDate, s.StartTime }).ToListAsync();
            var marks = await _context.SchoolAttendances.AsNoTracking()
                .Where(a => a.SchoolReserveId == reserveId)
                .ToDictionaryAsync(a => a.SchoolCourseSessionId, a => new { a.Present, a.Note });

            var report = new SchoolReportVDto
            {
                SchoolReserveId = reserve.Id,
                CourseName = reserve.CourseName,
                PetName = reserve.PetName,
                StatusId = reserve.StatusId,
                SessionCount = sessions.Count,
                Attendance = sessions.Select(s => new SchoolAttendanceVDto
                {
                    SchoolCourseSessionId = s.Id, SessionDate = s.SessionDate, StartTime = s.StartTime,
                    Present = marks.TryGetValue(s.Id, out var m) ? m.Present : null,
                    Note = marks.TryGetValue(s.Id, out var n) ? n.Note : null,
                }).ToList(),
            };
            report.AttendedCount = report.Attendance.Count(a => a.Present == true);
            report.AbsentCount = report.Attendance.Count(a => a.Present == false);
            var recorded = report.AttendedCount + report.AbsentCount;
            report.AttendancePercent = recorded == 0 ? 0 : (int)Math.Round(100.0 * report.AttendedCount / recorded);

            var skills = await _context.SchoolCourseSkills.AsNoTracking()
                .Where(s => s.SchoolCourseId == reserve.SchoolCourseId && !s.Deleted)
                .OrderBy(s => s.SortOrder).ThenBy(s => s.Id)
                .Select(s => new { s.Id, s.Name }).ToListAsync();
            var entries = await _context.SchoolProgresses.AsNoTracking()
                .Where(p => p.SchoolReserveId == reserveId)
                .OrderBy(p => p.CreateDate).ThenBy(p => p.Id)
                .Select(p => new { p.SchoolCourseSkillId, p.Score, p.Note, p.SchoolCourseSessionId, SessionDate = (DateTime?)p.SchoolCourseSession.SessionDate, p.CreateDate })
                .ToListAsync();
            report.Skills = skills.Select(s =>
            {
                var history = entries.Where(e => e.SchoolCourseSkillId == s.Id)
                    .Select(e => new SchoolProgressEntryVDto { Score = e.Score, Note = e.Note, SchoolCourseSessionId = e.SchoolCourseSessionId, SessionDate = e.SessionDate, CreateDate = e.CreateDate })
                    .ToList();
                return new SchoolSkillProgressVDto { SkillId = s.Id, SkillName = s.Name, History = history, LatestScore = history.LastOrDefault()?.Score };
            }).ToList();

            report.Certificate = await CertificateQuery(c => c.SchoolReserveId == reserveId).FirstOrDefaultAsync();
            return new BaseResultDto<SchoolReportVDto>(true, report);
        }

        // ---------------------------------------------------------------- گواهی

        private IQueryable<SchoolCertificateVDto> CertificateQuery(System.Linq.Expressions.Expression<Func<SchoolCertificate, bool>> filter) =>
            _context.SchoolCertificates.AsNoTracking().Where(filter).Select(c => new SchoolCertificateVDto
            {
                Code = c.Code,
                IssueDate = c.IssueDate,
                CourseName = c.SchoolReserve.SchoolCourse.Name,
                CourseLevel = c.SchoolReserve.SchoolCourse.Level,
                SchoolName = c.SchoolReserve.SchoolCourse.School.Name,
                PetName = c.SchoolReserve.UserPet.Name,
                OwnerFullName = ((c.SchoolReserve.Booker.FirstName ?? "") + " " + (c.SchoolReserve.Booker.LastName ?? "")).Trim(),
                SessionCount = _context.SchoolCourseSessions.Count(s => s.SchoolCourseId == c.SchoolReserve.SchoolCourseId && !s.Deleted),
                AttendedCount = _context.SchoolAttendances.Count(a => a.SchoolReserveId == c.SchoolReserveId && a.Present),
            });

        public async Task<BaseResultDto<SchoolCertificateVDto>> IssueCertificateAsync(long reserveId, long userId)
        {
            var complete = (int)SchoolReserveStatusEnum.Complete;
            var reserve = await _context.SchoolReserves.AsNoTracking()
                .Where(r => r.Id == reserveId && r.IsReserved && !r.IsCancel)
                .Select(r => new { r.Id, r.StatusId, r.BookerId, r.SchoolCourse.CertificateEnabled, CourseName = r.SchoolCourse.Name, PetName = r.UserPet.Name })
                .FirstOrDefaultAsync();
            if (reserve == null)
                return new BaseResultDto<SchoolCertificateVDto>(false, Resource.Notification.NothingFound, null);
            if (!reserve.CertificateEnabled || reserve.StatusId != complete)
                return new BaseResultDto<SchoolCertificateVDto>(false, Resource.Notification.SchoolCertificateNotAllowed, null);

            if (!await _context.SchoolCertificates.AnyAsync(c => c.SchoolReserveId == reserveId))
            {
                var code = await NewCodeAsync();
                await _context.SchoolCertificates.AddAsync(new SchoolCertificate { SchoolReserveId = reserveId, Code = code, IssueDate = DateTime.Now, IssuedByUserId = userId });
                await _context.SaveChangesAsync();
                await _push.SendPushAsync(PushTypeEnum.PushSchoolCertificateIssued, reserve.BookerId, token1: reserve.CourseName, token2: reserve.PetName, token3: code);
            }
            return new BaseResultDto<SchoolCertificateVDto>(true, await CertificateQuery(c => c.SchoolReserveId == reserveId).FirstOrDefaultAsync());
        }

        public async Task IssueCertificateIfEnabledAsync(long reserveId, long userId)
        {
            if (await _context.SchoolReserves.AnyAsync(r => r.Id == reserveId && r.SchoolCourse.CertificateEnabled))
                await IssueCertificateAsync(reserveId, userId);
        }

        public async Task<BaseResultDto<SchoolCertificateVDto>> VerifyCertificateAsync(string code)
        {
            var normalized = code?.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(normalized))
                return new BaseResultDto<SchoolCertificateVDto>(false, Resource.Notification.NothingFound, null);
            var item = await CertificateQuery(c => c.Code == normalized).FirstOrDefaultAsync();
            return item == null
                ? new BaseResultDto<SchoolCertificateVDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<SchoolCertificateVDto>(true, item);
        }

        public async Task SendDayBeforeRemindersAsync(System.Threading.CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var from = now.AddHours(20);
            var to = now.AddHours(28);
            var sessions = await _context.SchoolCourseSessions.AsTracking()
                .Include(s => s.SchoolCourse)
                .Where(s => !s.Deleted && s.Active && s.DayReminderPushSentDate == null && !s.SchoolCourse.Deleted
                            && s.SessionDate >= from.Date && s.SessionDate <= to.Date)
                .ToListAsync(cancellationToken);
            var paid = (int)SchoolReserveStatusEnum.Paid;
            foreach (var session in sessions)
            {
                var start = SchoolCourseRules.SessionStart(session.SessionDate, session.StartTime);
                if (start <= from || start > to)
                    continue;
                var bookers = await _context.SchoolReserves.AsNoTracking()
                    .Where(r => r.SchoolCourseId == session.SchoolCourseId && r.IsReserved && !r.IsCancel && r.StatusId == paid)
                    .Select(r => r.BookerId).Distinct().ToListAsync(cancellationToken);
                foreach (var bookerId in bookers)
                    await _push.SendPushAsync(PushTypeEnum.PushSchoolClassReminderDayBefore, bookerId,
                        token1: session.SchoolCourse.Name, token2: session.StartTime, token3: session.SchoolCourseId.ToString());
                session.DayReminderPushSentDate = now;
            }
            if (sessions.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }

        // کد ۸ نویسه‌ای بدون نویسه‌های گیج‌کننده (O/0، I/1) با پیشوند PSC-
        private async Task<string> NewCodeAsync()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            while (true)
            {
                var chars = new char[8];
                for (var i = 0; i < chars.Length; i++)
                    chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
                var code = "PSC-" + new string(chars);
                if (!await _context.SchoolCertificates.AnyAsync(c => c.Code == code))
                    return code;
            }
        }
    }
}
