using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Service;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv
{
    public class SchoolCourseService : CommonSrv<SchoolCourse, SchoolCourseDto>, ISchoolCourseService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public SchoolCourseService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        private async Task<int?> RemainingCapacityAsync(long courseId, int? capacity)
        {
            if (!capacity.HasValue)
                return null;
            var takenCount = await SchoolCourseRules.SeatTakers(_context.SchoolReserves.Where(r => r.SchoolCourseId == courseId), DateTime.Now).CountAsync();
            return capacity.Value - takenCount;
        }

        // خلاصه‌ی «چند جلسه در هفته / کل دوره چند هفته طول می‌کشد» را از روی تاریخ واقعی
        // جلسات (SchoolCourseSessions) حساب می‌کند، نه یک الگوی هفتگی فرضی - چون یک دوره
        // می‌تواند نامنظم هم زمان‌بندی شده باشد. دوره‌ی تک‌جلسه‌ای (SessionCount==1) هفته/بازه ندارد.
        private static void ApplySessionSummary(SchoolCourseVDto vdto, IEnumerable<SchoolCourseSession> sessions)
        {
            vdto.IsSingleSession = vdto.SessionCount <= 1;
            var liveSessions = sessions.Where(s => !s.Deleted).ToList();

            // جلسات همیشه به ترتیب تاریخ و ساعت برمی‌گردند (قبلاً به ترتیب ثبت، پس «جلسه ۲» می‌توانست قبل از «جلسه ۱» باشد)
            vdto.SchoolCourseSessions = vdto.SchoolCourseSessions?
                .OrderBy(s => SchoolCourseRules.SessionStart(s.SessionDate, s.StartTime))
                .ThenBy(s => s.Id)
                .ToList();

            // باز بودن و قیمت ثبت‌نام همان قاعده‌ی ثبت‌نام واقعی (ثبت‌نام دیرهنگام: بسته / قیمت کامل / قیمت نسبی)
            var enrollment = SchoolCourseRules.Enrollment(vdto.Price, vdto.LateEnrollmentMode,
                liveSessions.Select(s => SchoolCourseRules.SessionStart(s.SessionDate, s.StartTime)).ToList(), DateTime.Now);
            vdto.EnrollPrice = enrollment.Price;
            vdto.RemainingSessions = enrollment.RemainingSessions;
            vdto.IsRegistrationOpen = vdto.Active && enrollment.Open &&
                                      (!vdto.RemainingCapacity.HasValue || vdto.RemainingCapacity.Value > 0);

            var dates = liveSessions.Select(s => s.SessionDate).OrderBy(d => d).ToList();
            if (dates.Count == 0)
                return;

            vdto.FirstSessionDate = dates.First();
            vdto.LastSessionDate = dates.Last();
            var (sessionsPerWeek, totalWeeks) = SchoolCourseRules.WeeklySummary(dates);
            vdto.SessionsPerWeek = sessionsPerWeek;
            vdto.TotalDurationWeeks = totalWeeks;
        }

        public async Task<BaseResultDto<SchoolCourseVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.SchoolCourses
                .Include(c => c.Pet)
                .Include(c => c.PetBreed)
                .Include(c => c.School)
                .Include(c => c.PrerequisiteCourse)
                .Include(c => c.SchoolCourseSessions.Where(s => !s.Deleted))
                .Include(c => c.SchoolCourseVideos.Where(v => !v.Deleted))
                .FirstOrDefaultAsync(c => c.Id == id && !c.Deleted);

            if (item == null)
                return new BaseResultDto<SchoolCourseVDto>(false, mapper.Map<SchoolCourseVDto>(item));

            var vdto = mapper.Map<SchoolCourseVDto>(item);
            vdto.RemainingCapacity = await RemainingCapacityAsync(item.Id, item.Capacity);
            ApplySessionSummary(vdto, item.SchoolCourseSessions ?? new List<SchoolCourseSession>());

            var acceptedPets = await _context.SchoolCoursePets
                .Include(p => p.Pet).Include(p => p.PetBreed)
                .Where(p => p.SchoolCourseId == id && !p.Deleted)
                .ToListAsync();
            vdto.AcceptedPets = mapper.Map<List<Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto.SchoolCoursePetVDto>>(acceptedPets);

            return new BaseResultDto<SchoolCourseVDto>(true, vdto);
        }

        public SchoolCourseSearchDto Search(SchoolCourseInputDto baseSearchDto)
        {
            var model = _context.SchoolCourses
                .Include(c => c.Pet)
                .Include(c => c.PetBreed)
                .Include(c => c.SchoolCourseSessions.Where(s => !s.Deleted))
                .Include(c => c.SchoolCourseVideos.Where(v => !v.Deleted))
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (baseSearchDto.SchoolId.HasValue)
                model = model.Where(c => c.SchoolId == baseSearchDto.SchoolId.Value);
            if (baseSearchDto.CompanionId.HasValue)
                model = model.Where(c => c.School.CompanionId == baseSearchDto.CompanionId.Value);
            if (baseSearchDto.OwnerUserId.HasValue)
                model = model.Where(c => ((c.School.Provider != null && c.School.Provider.UserId == baseSearchDto.OwnerUserId.Value && !c.School.Provider.Deleted) || (c.School.Companion != null && c.School.Companion.OwnerId == baseSearchDto.OwnerUserId.Value && !c.School.Companion.Deleted)));
            if (baseSearchDto.CourseTypeId.HasValue)
                model = model.Where(c => c.CourseTypeId == baseSearchDto.CourseTypeId.Value);
            if (baseSearchDto.PetId.HasValue)
                model = model.Where(c => c.PetId == null || c.PetId == baseSearchDto.PetId.Value);
            if (baseSearchDto.Available.HasValue)
                model = model.Where(c => c.Active == baseSearchDto.Available.Value);
            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
                model = model.Where(c => c.Name.Contains(baseSearchDto.Q));

            model = baseSearchDto.SortBy switch
            {
                Common.Enumerable.SortEnum.Old => model.OrderBy(c => c.Id),
                Common.Enumerable.SortEnum.Expensive => model.OrderByDescending(c => c.Price),
                Common.Enumerable.SortEnum.Inexpensive => model.OrderBy(c => c.Price),
                _ => model.OrderByDescending(c => c.Id),
            };

            var result = new SchoolCourseSearchDto(baseSearchDto, model, mapper);

            var courseIds = result.List.Select(c => c.Id).ToList();
            var takenCounts = SchoolCourseRules.SeatTakers(_context.SchoolReserves.Where(r => courseIds.Contains(r.SchoolCourseId)), DateTime.Now)
                .GroupBy(r => r.SchoolCourseId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);

            var courseEntities = model.Where(c => courseIds.Contains(c.Id)).ToList();
            var acceptedPetsByCourse = _context.SchoolCoursePets
                .Include(p => p.Pet).Include(p => p.PetBreed)
                .Where(p => courseIds.Contains(p.SchoolCourseId) && !p.Deleted)
                .ToList()
                .GroupBy(p => p.SchoolCourseId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in result.List)
            {
                takenCounts.TryGetValue(item.Id, out var taken);
                item.RemainingCapacity = item.Capacity.HasValue ? item.Capacity.Value - taken : (int?)null;
                var entity = courseEntities.FirstOrDefault(c => c.Id == item.Id);
                ApplySessionSummary(item, entity?.SchoolCourseSessions ?? new List<SchoolCourseSession>());

                item.AcceptedPets = acceptedPetsByCourse.TryGetValue(item.Id, out var acceptedPets)
                    ? mapper.Map<List<Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto.SchoolCoursePetVDto>>(acceptedPets)
                    : new List<Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto.SchoolCoursePetVDto>();
            }

            return result;
        }

        public override async Task<BaseResultDto<SchoolCourseDto>> InsertAsyncDto(SchoolCourseDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.PleaseEnterTheName, dto);
            if (!await _context.Schools.AnyAsync(s => s.Id == dto.SchoolId && !s.Deleted))
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.NothingFound, dto);
            if (dto.Price <= 0)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);
            if (dto.SessionCount <= 0)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);
            // Capacity=null یعنی نامحدود - مجاز است؛ فقط مقدار صفر/منفیِ صریح رد می‌شود
            if (dto.Capacity.HasValue && dto.Capacity <= 0)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);
            if (dto.CourseTypeId != (int)SchoolCourseTypeEnum.Video &&
                dto.CourseTypeId != (int)SchoolCourseTypeEnum.Live &&
                dto.CourseTypeId != (int)SchoolCourseTypeEnum.InPerson)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);
            // بخش «آنلاین» مدرسه فعلاً غیرفعال است (Application.Common.FeatureFlags.SchoolOnlineCoursesEnabled) -
            // کد کامل است و حذف نشده، فقط ساخت دوره‌ی جدید از این نوع مسدود است.
            if (dto.CourseTypeId == (int)SchoolCourseTypeEnum.Live && !Application.Common.FeatureFlags.SchoolOnlineCoursesEnabled)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.FeatureTemporarilyDisabled, dto);

            var policyError = await ValidateExtrasAsync(dto);
            if (policyError != null)
                return new BaseResultDto<SchoolCourseDto>(false, policyError, dto);

            var item = mapper.Map<SchoolCourse>(dto);
            item.Active = true;
            item.Deleted = false;
            item.FullRefundHoursBeforeStart = dto.FullRefundHoursBeforeStart ?? 24;
            item.LateCancelRefundPercent = dto.LateCancelRefundPercent ?? 50;

            await _context.SchoolCourses.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolCourseDto>(true, mapper.Map<SchoolCourseDto>(item));
        }

        public override BaseResultDto UpdateDto(SchoolCourseDto dto)
        {
            if (dto == null || dto.SessionCount <= 0 || dto.Price <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            // «تعداد جلسات» نمی‌تواند از جلسات تعریف‌شده کمتر شود (دوره‌ی «۴ جلسه» با فقط ۲ جلسه هم با همین عدد در صفحه دیده می‌شود)
            var definedCount = _context.SchoolCourseSessions.Count(s => s.SchoolCourseId == dto.Id && !s.Deleted);
            if (dto.SessionCount < definedCount)
                return new BaseResultDto(false, Resource.Notification.SchoolCourseSessionCountBelowDefined);
            var policyError = ValidateExtrasAsync(dto).GetAwaiter().GetResult();
            if (policyError != null)
                return new BaseResultDto(false, policyError);
            // سیاست لغو نیامده = مقدار ذخیره‌شده (کلاینت قدیمی این فیلدها را نمی‌فرستد)؛ امتیاز دوره فقط از نظرها
            var stored = _context.SchoolCourses.AsNoTracking().Where(c => c.Id == dto.Id)
                .Select(c => new { c.FullRefundHoursBeforeStart, c.LateCancelRefundPercent }).FirstOrDefault();
            dto.FullRefundHoursBeforeStart ??= stored?.FullRefundHoursBeforeStart ?? 24;
            dto.LateCancelRefundPercent ??= stored?.LateCancelRefundPercent ?? 50;
            return base.UpdateDto(dto);
        }

        // سطح، پیش‌نیاز (دوره‌ی دیگرِ همین مدرسه، نه خودش)، حالت ثبت‌نام دیرهنگام و سیاست لغو
        private async Task<string> ValidateExtrasAsync(SchoolCourseDto dto)
        {
            if (!Enum.IsDefined(typeof(SchoolCourseLevelEnum), dto.Level) || !Enum.IsDefined(typeof(SchoolLateEnrollmentEnum), dto.LateEnrollmentMode))
                return Resource.Notification.InvalidData;
            if (dto.FullRefundHoursBeforeStart is < 0 or > 720 || dto.LateCancelRefundPercent is < 0 or > 100)
                return Resource.Notification.InvalidData;
            if (dto.PrerequisiteCourseId.HasValue)
            {
                if (dto.PrerequisiteCourseId.Value == dto.Id)
                    return Resource.Notification.InvalidData;
                var schoolId = dto.SchoolId > 0 ? dto.SchoolId
                    : await _context.SchoolCourses.AsNoTracking().Where(c => c.Id == dto.Id).Select(c => c.SchoolId).FirstOrDefaultAsync();
                if (!await _context.SchoolCourses.AnyAsync(c => c.Id == dto.PrerequisiteCourseId.Value && c.SchoolId == schoolId && !c.Deleted))
                    return Resource.Notification.NothingFound;
            }
            return null;
        }

        /// <summary>
        /// «این دوره n جلسه است و روزهایش این‌ها»: همه‌ی جلسه‌های آینده‌ی دوره با لیست ارسالی جایگزین می‌شوند.
        /// جلسه‌های شروع‌شده/گذشته و جلسه‌هایی که حضور و غیاب یا کلاس زنده دارند دست نمی‌خورند و جزو شمارش‌اند؛
        /// جمعشان باید دقیقاً برابر «تعداد جلسات» دوره باشد. هر روز ساعت خودش را دارد.
        /// </summary>
        public async Task<BaseResultDto<List<SchoolCourseSessionDto>>> SetScheduleAsync(SchoolCourseScheduleDto dto, long? ownedSchoolId = null)
        {
            if (dto == null || dto.Sessions == null || dto.Sessions.Count == 0)
                return new BaseResultDto<List<SchoolCourseSessionDto>>(false, Resource.Notification.InvalidData, null);
            if (!await CanManageCourseAsync(dto.SchoolCourseId, ownedSchoolId))
                return new BaseResultDto<List<SchoolCourseSessionDto>>(false, Resource.Notification.AccessDenied, null);
            var course = await _context.SchoolCourses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.SchoolCourseId && !c.Deleted);
            if (course == null)
                return new BaseResultDto<List<SchoolCourseSessionDto>>(false, Resource.Notification.NothingFound, null);

            var now = DateTime.Now;
            var slots = new List<(DateTime Date, string Start, string End, DateTime StartAt, DateTime EndAt)>();
            foreach (var s in dto.Sessions)
            {
                var start = s.StartTime?.Trim();
                var end = s.EndTime?.Trim();
                if (!ReservationScheduleValidator.TryGetServiceTimeRange(start, end, out _, out _))
                    return new BaseResultDto<List<SchoolCourseSessionDto>>(false, Resource.Notification.ToTimeMustBeBiggerThanFromTime, null);
                var startAt = SchoolCourseRules.SessionStart(s.SessionDate, start);
                if (startAt <= now)
                    return new BaseResultDto<List<SchoolCourseSessionDto>>(false, Resource.Notification.SchoolSessionMustBeInFuture, null);
                slots.Add((s.SessionDate.Date, start, end, startAt, SchoolCourseRules.SessionStart(s.SessionDate, end)));
            }
            // هم‌پوشانی دو جلسه‌ی یک روز (یا تکرار همان زمان)
            var ordered = slots.OrderBy(x => x.StartAt).ToList();
            for (var i = 1; i < ordered.Count; i++)
                if (ordered[i].StartAt < ordered[i - 1].EndAt)
                    return new BaseResultDto<List<SchoolCourseSessionDto>>(false, Resource.Notification.SchoolSessionOverlap, null);

            var existing = await _context.SchoolCourseSessions.AsTracking()
                .Where(s => s.SchoolCourseId == course.Id && !s.Deleted).ToListAsync();
            var lockedIds = (await _context.SchoolAttendances.AsNoTracking()
                    .Where(a => a.SchoolCourseSession.SchoolCourseId == course.Id).Select(a => a.SchoolCourseSessionId)
                    .Concat(_context.SchoolCourseLiveSessions.AsNoTracking()
                        .Where(l => l.SchoolCourseSession.SchoolCourseId == course.Id).Select(l => l.SchoolCourseSessionId))
                    .ToListAsync()).ToHashSet();
            var kept = existing.Where(s => SchoolCourseRules.SessionStart(s.SessionDate, s.StartTime) <= now || lockedIds.Contains(s.Id)).ToList();
            if (kept.Count + slots.Count != course.SessionCount)
                return new BaseResultDto<List<SchoolCourseSessionDto>>(false,
                    string.Format(Resource.Notification.SchoolSessionCountMismatch, course.SessionCount, course.SessionCount - kept.Count), null);

            foreach (var s in existing.Except(kept))
                s.Deleted = true;
            var meetingUrl = existing.Select(s => s.MeetingUrl).FirstOrDefault(u => !string.IsNullOrEmpty(u));
            foreach (var slot in ordered)
                await _context.SchoolCourseSessions.AddAsync(new SchoolCourseSession
                {
                    SchoolCourseId = course.Id, SessionDate = slot.Date, StartTime = slot.Start, EndTime = slot.End,
                    MeetingUrl = meetingUrl, Active = true, Deleted = false,
                });
            await _context.SaveChangesAsync();

            var all = await _context.SchoolCourseSessions.AsNoTracking()
                .Where(s => s.SchoolCourseId == course.Id && !s.Deleted).ToListAsync();
            return new BaseResultDto<List<SchoolCourseSessionDto>>(true, all
                .OrderBy(s => SchoolCourseRules.SessionStart(s.SessionDate, s.StartTime))
                .Select(s => mapper.Map<SchoolCourseSessionDto>(s)).ToList());
        }

        public async Task<bool> CanReadLessonFileAsync(long fileId, long userId, long? companionId, bool isAdmin)
        {
            // ویدیوی حذف‌شده هم محافظت‌شده می‌ماند (شناسه‌اش قبلاً در پاسخ عمومی دوره بوده)
            var courses = await _context.SchoolCourseVideos.AsNoTracking()
                .Where(v => v.FileId == fileId)
                .Select(v => new { v.SchoolCourseId, v.SchoolCourse.School.CompanionId, ProviderUserId = v.SchoolCourse.School.Provider != null && !v.SchoolCourse.School.Provider.Deleted ? (long?)v.SchoolCourse.School.Provider.UserId : null })
                .ToListAsync();
            if (courses.Count == 0)
                return true;
            if (isAdmin)
                return true;
            if (companionId.HasValue && courses.Any(c => c.CompanionId == companionId.Value))
                return true;
            // مالکِ ارائه‌دهنده‌ی مدرسه (مدرسه‌های بدون نمایندگی)
            if (courses.Any(c => c.ProviderUserId == userId))
                return true;
            var courseIds = courses.Select(c => c.SchoolCourseId).ToList();
            return await _context.SchoolReserves.AsNoTracking().AnyAsync(r =>
                courseIds.Contains(r.SchoolCourseId) && r.BookerId == userId && r.IsReserved && !r.IsCancel);
        }

        public async Task<BaseResultDto> UpdateActiveAsync(long id, bool active, long? ownedSchoolId = null)
        {
            var item = await _context.SchoolCourses.Include(c => c.School).AsTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownedSchoolId.HasValue && item.SchoolId != ownedSchoolId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            item.Active = active;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private async Task<bool> CanManageCourseAsync(long courseId, long? ownedSchoolId)
        {
            if (!ownedSchoolId.HasValue) return true;
            return await _context.SchoolCourses.Include(c => c.School)
                .AnyAsync(c => c.Id == courseId && c.SchoolId == ownedSchoolId.Value);
        }

        public async Task<BaseResultDto<SchoolCourseSessionDto>> UpsertSessionAsync(SchoolCourseSessionDto dto, long? ownedSchoolId = null)
        {
            if (!await CanManageCourseAsync(dto.SchoolCourseId, ownedSchoolId))
                return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.AccessDenied, dto);

            // لینک کلاس در پنل ادمین/وب‌اپ به‌صورت href رندر می‌شود؛ `javascript:` و امثال آن (XSS ذخیره‌شده علیه ادمین) رد می‌شود.
            dto.MeetingUrl = string.IsNullOrWhiteSpace(dto.MeetingUrl) ? null : dto.MeetingUrl.Trim();
            if (dto.MeetingUrl != null &&
                !(System.Uri.TryCreate(dto.MeetingUrl, System.UriKind.Absolute, out var meetingUri) &&
                  (meetingUri.Scheme == System.Uri.UriSchemeHttps || meetingUri.Scheme == System.Uri.UriSchemeHttp)))
                return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.InvalidData, dto);

            dto.StartTime = dto.StartTime?.Trim();
            dto.EndTime = dto.EndTime?.Trim();
            if (!ReservationScheduleValidator.TryGetServiceTimeRange(dto.StartTime, dto.EndTime, out _, out _))
                return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.ToTimeMustBeBiggerThanFromTime, dto);

            var course = await _context.SchoolCourses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.SchoolCourseId && !c.Deleted);
            if (course == null)
                return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.NothingFound, dto);

            SchoolCourseSession item;
            if (dto.Id > 0)
            {
                item = await _context.SchoolCourseSessions.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.SchoolCourseId == dto.SchoolCourseId);
                if (item == null)
                    return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.NothingFound, dto);
                // جلسه‌ی گذشته را فقط بدون جابه‌جایی زمانش می‌شود ویرایش کرد (مثلاً لینک)؛ زمان جدید باید در آینده باشد
                var timeChanged = item.SessionDate.Date != dto.SessionDate.Date || item.StartTime != dto.StartTime || item.EndTime != dto.EndTime;
                if (timeChanged && SchoolCourseRules.SessionStart(dto.SessionDate, dto.StartTime) <= DateTime.Now)
                    return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.SchoolSessionMustBeInFuture, dto);
                item.SessionDate = dto.SessionDate;
                item.StartTime = dto.StartTime;
                item.EndTime = dto.EndTime;
                item.MeetingUrl = dto.MeetingUrl;
                item.Active = dto.Active;
            }
            else
            {
                if (SchoolCourseRules.SessionStart(dto.SessionDate, dto.StartTime) <= DateTime.Now)
                    return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.SchoolSessionMustBeInFuture, dto);
                // تعداد جلسات تعریف‌شده نباید از «تعداد جلسات» دوره بیشتر شود (دوره‌ی «۴ جلسه» با ۵ جلسه)
                var definedCount = await _context.SchoolCourseSessions.CountAsync(s => s.SchoolCourseId == dto.SchoolCourseId && !s.Deleted);
                if (definedCount >= course.SessionCount)
                    return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.SchoolSessionCountExceeded, dto);

                item = mapper.Map<SchoolCourseSession>(dto);
                item.Deleted = false;
                await _context.SchoolCourseSessions.AddAsync(item);
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolCourseSessionDto>(true, mapper.Map<SchoolCourseSessionDto>(item));
        }

        public async Task<BaseResultDto> DeleteSessionAsync(long id, long? ownedSchoolId = null)
        {
            var item = await _context.SchoolCourseSessions.Include(s => s.SchoolCourse).ThenInclude(c => c.School).AsTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownedSchoolId.HasValue && item.SchoolCourse.SchoolId != ownedSchoolId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            item.Deleted = true;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto<SchoolCourseVideoDto>> UpsertVideoAsync(SchoolCourseVideoDto dto, long? ownedSchoolId = null)
        {
            if (!await CanManageCourseAsync(dto.SchoolCourseId, ownedSchoolId))
                return new BaseResultDto<SchoolCourseVideoDto>(false, Resource.Notification.AccessDenied, dto);
            if (dto.FileId <= 0)
                return new BaseResultDto<SchoolCourseVideoDto>(false, Resource.Notification.InvalidData, dto);

            SchoolCourseVideo item;
            if (dto.Id > 0)
            {
                item = await _context.SchoolCourseVideos.AsTracking().FirstOrDefaultAsync(v => v.Id == dto.Id && v.SchoolCourseId == dto.SchoolCourseId);
                if (item == null)
                    return new BaseResultDto<SchoolCourseVideoDto>(false, Resource.Notification.NothingFound, dto);
                item.Name = dto.Name;
                item.FileId = dto.FileId;
                item.SortOrder = dto.SortOrder;
                item.DurationSeconds = dto.DurationSeconds;
                item.Active = dto.Active;
            }
            else
            {
                item = mapper.Map<SchoolCourseVideo>(dto);
                item.Deleted = false;
                await _context.SchoolCourseVideos.AddAsync(item);
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolCourseVideoDto>(true, mapper.Map<SchoolCourseVideoDto>(item));
        }

        public async Task<BaseResultDto> DeleteVideoAsync(long id, long? ownedSchoolId = null)
        {
            var item = await _context.SchoolCourseVideos.Include(v => v.SchoolCourse).ThenInclude(c => c.School).AsTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownedSchoolId.HasValue && item.SchoolCourse.SchoolId != ownedSchoolId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            item.Deleted = true;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<List<SearchSchoolCourseDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<SchoolCourse>(request.SearchTerms,
                item => item.Name, item => item.Discription, item => item.School.Name);
            var query = _context.SchoolCourses.AsNoTracking()
                .Where(c => c.Active && !c.Deleted && !c.School.Deleted && c.School.Active && c.School.Approve && c.School.ShowToSite);
            return await query.Where(predicate)
                .Take(SearchQueryHelper.CandidateCount(request.SchoolCourseCount))
                .Select(c => new SearchSchoolCourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    SchoolId = c.SchoolId,
                    SchoolName = c.School.Name,
                    Price = c.Price,
                    Description = c.Discription,
                    Picture = c.School.Picture == null ? null : new PictureVDto { Id = c.School.Picture.Id, Url = c.School.Picture.Url, OrginalName = c.School.Picture.OrginalName, GuidName = c.School.Picture.GuidName, Extension = c.School.Picture.Extension }
                })
                .ToListAsync();
        }
    }
}
