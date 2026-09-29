using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Service;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
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

        private async Task<int> RemainingCapacityAsync(long courseId, int capacity)
        {
            var takenCount = await _context.SchoolReserves.CountAsync(r => r.SchoolCourseId == courseId && !r.IsCancel);
            return capacity - takenCount;
        }

        // خلاصه‌ی «چند جلسه در هفته / کل دوره چند هفته طول می‌کشد» را از روی تاریخ واقعی
        // جلسات (SchoolCourseSessions) حساب می‌کند، نه یک الگوی هفتگی فرضی - چون یک دوره
        // می‌تواند نامنظم هم زمان‌بندی شده باشد. دوره‌ی تک‌جلسه‌ای (SessionCount==1) هفته/بازه ندارد.
        private static void ApplySessionSummary(SchoolCourseVDto vdto, IEnumerable<SchoolCourseSession> sessions)
        {
            vdto.IsSingleSession = vdto.SessionCount <= 1;
            var dates = sessions.Where(s => !s.Deleted).Select(s => s.SessionDate).OrderBy(d => d).ToList();
            if (dates.Count == 0)
                return;

            vdto.FirstSessionDate = dates.First();
            vdto.LastSessionDate = dates.Last();

            if (dates.Count == 1)
            {
                vdto.TotalDurationWeeks = 0;
                vdto.SessionsPerWeek = dates.Count;
                return;
            }

            var spanDays = (dates.Last() - dates.First()).TotalDays;
            var spanWeeks = spanDays / 7.0;
            vdto.TotalDurationWeeks = System.Math.Round(spanWeeks, 1);
            vdto.SessionsPerWeek = spanWeeks > 0 ? System.Math.Round(dates.Count / spanWeeks, 1) : dates.Count;
        }

        public async Task<BaseResultDto<SchoolCourseVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.SchoolCourses
                .Include(c => c.Pet)
                .Include(c => c.PetBreed)
                .Include(c => c.School)
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
            var takenCounts = _context.SchoolReserves
                .Where(r => courseIds.Contains(r.SchoolCourseId) && !r.IsCancel)
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
                item.RemainingCapacity = item.Capacity - taken;
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
            if (!await _context.Schools.AnyAsync(s => s.Id == dto.SchoolId))
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.NothingFound, dto);
            if (dto.Price <= 0)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);
            if (dto.SessionCount <= 0)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);
            if (dto.Capacity <= 0)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);
            if (dto.CourseTypeId != (int)SchoolCourseTypeEnum.Video &&
                dto.CourseTypeId != (int)SchoolCourseTypeEnum.Live &&
                dto.CourseTypeId != (int)SchoolCourseTypeEnum.InPerson)
                return new BaseResultDto<SchoolCourseDto>(false, Resource.Notification.InvalidData, dto);

            var item = mapper.Map<SchoolCourse>(dto);
            item.Active = true;
            item.Deleted = false;

            await _context.SchoolCourses.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolCourseDto>(true, mapper.Map<SchoolCourseDto>(item));
        }

        public async Task<BaseResultDto> UpdateActiveAsync(long id, bool active, long? companionId = null)
        {
            var item = await _context.SchoolCourses.Include(c => c.School).AsTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (companionId.HasValue && item.School.CompanionId != companionId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            item.Active = active;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private async Task<bool> CanManageCourseAsync(long courseId, long? companionId)
        {
            if (!companionId.HasValue) return true;
            return await _context.SchoolCourses.Include(c => c.School)
                .AnyAsync(c => c.Id == courseId && c.School.CompanionId == companionId.Value);
        }

        public async Task<BaseResultDto<SchoolCourseSessionDto>> UpsertSessionAsync(SchoolCourseSessionDto dto, long? companionId = null)
        {
            if (!await CanManageCourseAsync(dto.SchoolCourseId, companionId))
                return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.AccessDenied, dto);

            // لینک کلاس در پنل ادمین/وب‌اپ به‌صورت href رندر می‌شود؛ `javascript:` و امثال آن (XSS ذخیره‌شده علیه ادمین) رد می‌شود.
            dto.MeetingUrl = string.IsNullOrWhiteSpace(dto.MeetingUrl) ? null : dto.MeetingUrl.Trim();
            if (dto.MeetingUrl != null &&
                !(System.Uri.TryCreate(dto.MeetingUrl, System.UriKind.Absolute, out var meetingUri) &&
                  (meetingUri.Scheme == System.Uri.UriSchemeHttps || meetingUri.Scheme == System.Uri.UriSchemeHttp)))
                return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.InvalidData, dto);

            SchoolCourseSession item;
            if (dto.Id > 0)
            {
                item = await _context.SchoolCourseSessions.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && s.SchoolCourseId == dto.SchoolCourseId);
                if (item == null)
                    return new BaseResultDto<SchoolCourseSessionDto>(false, Resource.Notification.NothingFound, dto);
                item.SessionDate = dto.SessionDate;
                item.StartTime = dto.StartTime;
                item.EndTime = dto.EndTime;
                item.MeetingUrl = dto.MeetingUrl;
                item.Active = dto.Active;
            }
            else
            {
                item = mapper.Map<SchoolCourseSession>(dto);
                item.Deleted = false;
                await _context.SchoolCourseSessions.AddAsync(item);
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolCourseSessionDto>(true, mapper.Map<SchoolCourseSessionDto>(item));
        }

        public async Task<BaseResultDto> DeleteSessionAsync(long id, long? companionId = null)
        {
            var item = await _context.SchoolCourseSessions.Include(s => s.SchoolCourse).ThenInclude(c => c.School).AsTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (companionId.HasValue && item.SchoolCourse.School.CompanionId != companionId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            item.Deleted = true;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto<SchoolCourseVideoDto>> UpsertVideoAsync(SchoolCourseVideoDto dto, long? companionId = null)
        {
            if (!await CanManageCourseAsync(dto.SchoolCourseId, companionId))
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

        public async Task<BaseResultDto> DeleteVideoAsync(long id, long? companionId = null)
        {
            var item = await _context.SchoolCourseVideos.Include(v => v.SchoolCourse).ThenInclude(c => c.School).AsTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (companionId.HasValue && item.SchoolCourse.School.CompanionId != companionId.Value)
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
                .Where(c => c.Active && !c.Deleted && c.School.Active && c.School.Approve && c.School.ShowToSite);
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
