using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolSrv
{
    public class SchoolService : CommonSrv<School, SchoolDto>, ISchoolService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<SchoolService> _logger;

        public SchoolService(IDataBaseContext _context, IMapper mapper, IPushNotificationService pushNotificationService, ILogger<SchoolService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._pushNotificationService = pushNotificationService;
            this._logger = logger;
        }

        public async Task<BaseResultDto<SchoolVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Schools
                .Include(s => s.Picture)
                .Include(s => s.Companion).ThenInclude(s => s.Owner)
                .Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.SchoolCourses.Where(c => !c.Deleted))
                .FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<SchoolVDto>(true, mapper.Map<SchoolVDto>(item));
            return new BaseResultDto<SchoolVDto>(false, mapper.Map<SchoolVDto>(item));
        }

        public SchoolSearchDto Search(SchoolInputDto baseSearchDto)
        {
            var model = _context.Schools
                .Include(s => s.Picture)
                .Include(s => s.Companion)
                .Include(s => s.City).ThenInclude(s => s.State)
                .AsQueryable();

            if (baseSearchDto.CompanionId.HasValue)
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            if (baseSearchDto.Approve.HasValue)
                model = model.Where(s => s.Approve == baseSearchDto.Approve.Value);
            if (baseSearchDto.ShowToSite.HasValue)
                model = model.Where(s => s.ShowToSite == baseSearchDto.ShowToSite.Value);
            if (baseSearchDto.StateId.HasValue)
                model = model.Where(s => s.StateId == baseSearchDto.StateId.Value);
            if (baseSearchDto.CityId.HasValue)
                model = model.Where(s => s.CityId == baseSearchDto.CityId.Value);
            if (baseSearchDto.Suggested.HasValue)
                model = model.Where(s => s.Suggested == baseSearchDto.Suggested.Value);
            if (baseSearchDto.Available.HasValue)
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q));

            model = baseSearchDto.SortBy switch
            {
                Common.Enumerable.SortEnum.Old => model.OrderBy(s => s.Id),
                Common.Enumerable.SortEnum.MoreVisit => model.OrderByDescending(s => s.RateAvg),
                Common.Enumerable.SortEnum.LessVisit => model.OrderBy(s => s.RateAvg),
                _ => model.OrderByDescending(s => s.Id),
            };

            return new SchoolSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<SchoolDto>> InsertAsyncDto(SchoolDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto<SchoolDto>(false, Resource.Notification.PleaseEnterTheName, dto);

            var companion = await _context.Companions.FirstOrDefaultAsync(s => s.Id == dto.CompanionId && !s.Deleted && s.Active && s.Approved);
            if (companion == null)
                return new BaseResultDto<SchoolDto>(false, Resource.Notification.NothingFound, dto);

            var item = mapper.Map<School>(dto);
            item.Active = false;
            item.Approve = false;
            item.ApprovalValue = null;
            item.ShowToSite = false;
            item.Suggested = false;
            item.CommentCount = 0;
            item.RateAvg = 0;
            item.RateCount = 0;

            await _context.Schools.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<SchoolDto>(true, mapper.Map<SchoolDto>(item));
        }

        public BaseResultDto UpdateSchoolActiveDto(SchoolActiveDto dto, long? companionId = null)
        {
            var item = _context.Schools.FirstOrDefault(s => s.Id == dto.Id);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (companionId.HasValue && item.CompanionId != companionId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            item.Active = dto.Active;
            _context.Schools.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateSchoolApproveAsyncDto(SchoolApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            var item = await _context.Schools
                .Include(s => s.Companion)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item?.Companion == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            item.Approve = dto.Approve;
            item.Active = dto.Approve;
            item.ApprovalValue = dto.Approve ? null : dto.ApprovalValue.Trim();
            // مدرسه‌ی تأییدشده خودکار در سایت/اپ نمایش داده می‌شود؛ رد شدن آن را پنهان می‌کند
            item.ShowToSite = dto.Approve;
            await _context.SaveChangesAsync();

            return new BaseResultDto(true);
        }

        // نمایش/عدم‌نمایش مدرسه توی سایت - تا قبل از این متد هیچ راهی برای true شدن
        // ShowToSite وجود نداشت (نه در Insert، نه در Approve)، یعنی مدرسه‌ها حتی بعد از
        // تأیید ادمین هم هیچ‌وقت توی صفحه‌ی اصلی/عمومی سایت نمایش داده نمی‌شدن. الگوی
        // این متد دقیقاً همون چیزیه که Companion/Pansion/Store/Assistance از قبل دارن.
        public async Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite)
        {
            var affectedRows = await _context.Schools
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ShowToSite, showToSite));

            if (affectedRows == 0)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            return new BaseResultDto(true);
        }
    }
}
