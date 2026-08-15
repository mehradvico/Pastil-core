using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv
{
    public class PansionService : CommonSrv<Pansion, PansionDto>, IPansionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly INoticeService _noticeService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PansionService> _logger;
        public PansionService(IDataBaseContext _context, IMapper mapper, INoticeService noticeService, IPushNotificationService pushNotificationService, ILogger<PansionService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._noticeService = noticeService;
            this._pushNotificationService = pushNotificationService;
            this._logger = logger;
        }

        public async Task<BaseResultDto<PansionVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Pansions.Include(s => s.Picture).Include(s => s.Companion).ThenInclude(s => s.Owner).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.PansionPets).ThenInclude(s => s.Pet).Include(s => s.PansionComments).Include(s => s.PansionPictures).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<PansionVDto>(true, mapper.Map<PansionVDto>(item));
            }
            return new BaseResultDto<PansionVDto>(false, mapper.Map<PansionVDto>(item));
        }

        public async Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite)
        {
            var affectedRows = await _context.Pansions
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ShowToSite, showToSite));

            if (affectedRows == 0)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            return new BaseResultDto(true);
        }

        public PansionSearchDto Search(PansionInputDto baseSearchDto)
        {
            var model = _context.Pansions.Include(s => s.Picture).Include(s => s.Companion).ThenInclude(s => s.Owner).Include(s => s.Companion).ThenInclude(s => s.Neighborhood).Include(s => s.City).ThenInclude(s => s.State).Include(s => s.PansionComments)
                .Include(s => s.PansionPictures).ThenInclude(s => s.Picture).AsQueryable();

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.ShowToSite.HasValue)
            {
                model = model.Where(s => s.ShowToSite == baseSearchDto.ShowToSite.Value);
            }
            if (baseSearchDto.IsSchool.HasValue)
            {
                var isSchool = baseSearchDto.IsSchool.Value;
                model = model.Where(s => s.IsSchool == null || s.IsSchool == isSchool);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.Approve.HasValue)
            {
                model = model.Where(s => s.Approve == baseSearchDto.Approve.Value);
            }
            if (baseSearchDto.StateId.HasValue)
            {
                model = model.Where(s => s.StateId == baseSearchDto.StateId.Value);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.CityId == baseSearchDto.CityId.Value);
            }
            if (baseSearchDto.NeighborhoodIds != null && baseSearchDto.NeighborhoodIds.Any())
            {
                model = model.Where(s => s.Companion.NeighborhoodId != null &&
                    baseSearchDto.NeighborhoodIds.Contains(s.Companion.Neighborhood.Id)
                );
            }

            if (baseSearchDto.Suggested.HasValue)
            {
                model = model.Where(s => s.Suggested == baseSearchDto.Suggested.Value);
            }
            if (baseSearchDto.PetId.HasValue)
            {
                model = model.Where(s => s.PansionPets.Any(s => s.PetId == baseSearchDto.PetId.Value));
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.MoreVisit:
                    {
                        model = model.OrderByDescending(s => s.RateAvg);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessVisit:
                    {
                        model = model.OrderBy(s => s.RateAvg);
                        break;
                    }
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        model = model.OrderByDescending(s =>
                            s.IsSchool == true
                                ? s.SchoolPrice
                                : s.PansionPrice
                        );
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        model = model.OrderBy(s =>
                            s.IsSchool == true
                                ? s.SchoolPrice
                                : s.PansionPrice
                        );
                        break;
                    }
                default:
                    break;
            }
            return new PansionSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<PansionDto>> InsertAsyncDto(PansionDto dto)
        {
            try
            {
                var modelCheker = await ValidateRequestAsync(dto);
                if (!modelCheker.IsSuccess)
                {
                    return new BaseResultDto<PansionDto>(false, dto);
                }
                else
                {
                    var item = mapper.Map<Pansion>(dto);
                    item.Active = false;
                    item.Approve = false;
                    item.ApprovalValue = null;
                    item.ShowToSite = false;
                    item.Suggested = false;
                    item.CommentCount = 0;
                    item.RateAvg = 0;
                    item.RateCount = 0;
                    item.DailyCommissionPercent = 0;
                    item.HourlyCommissionPercent = 0;
                    var companionId = dto.CompanionId;                  
                    var model = await _context.Companions.Include(s => s.Pansions).FirstOrDefaultAsync(s => s.Id == companionId && !s.Deleted && s.Active && s.Approved);
                    if (model == null)
                    {
                        return new BaseResultDto<PansionDto>(false, Resource.Notification.NothingFound, dto);
                    }
                    await _context.Pansions.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await TryCreateNoticeAsync(item, NoticeTypeLabels.PansionSubmitted, model.OwnerId);
                    return new BaseResultDto<PansionDto>(true, mapper.Map<PansionDto>(item));

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating pansion request for companion {CompanionId} failed.", dto?.CompanionId);
                return new BaseResultDto<PansionDto>(false, "خطا در ثبت درخواست پانسیون. اطلاعات فرم را بررسی کرده و دوباره تلاش کنید.", dto);
            }
        }
        private async Task<BaseResultDto> ValidateRequestAsync(PansionDto dto)
        {
            var errors = new List<Tuple<string, string>>();

            if (dto == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            dto.Name = dto.Name?.Trim();
            dto.AddressValue = dto.AddressValue?.Trim();
            dto.OpenHour = dto.OpenHour?.Trim();
            dto.CloseHour = dto.CloseHour?.Trim();

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            }
            if (!dto.IsSchool.HasValue)
                errors.Add(Tuple.Create("نوع مرکز (پانسیون یا مدرسه) را انتخاب کنید.", nameof(dto.IsSchool)));
            if (dto.StateId <= 0 || !await _context.States.AnyAsync(s => s.Id == dto.StateId))
                errors.Add(Tuple.Create("استان انتخاب‌شده معتبر نیست.", nameof(dto.StateId)));
            if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(s => s.Id == dto.CityId && s.StateId == dto.StateId))
                errors.Add(Tuple.Create("شهر انتخاب‌شده متعلق به استان انتخاب‌شده نیست.", nameof(dto.CityId)));
            if (string.IsNullOrWhiteSpace(dto.AddressValue))
                errors.Add(Tuple.Create("آدرس پانسیون را وارد کنید.", nameof(dto.AddressValue)));
            if (string.IsNullOrWhiteSpace(dto.OpenHour))
                errors.Add(Tuple.Create("ساعت شروع فعالیت را وارد کنید.", nameof(dto.OpenHour)));
            if (string.IsNullOrWhiteSpace(dto.CloseHour))
                errors.Add(Tuple.Create("ساعت پایان فعالیت را وارد کنید.", nameof(dto.CloseHour)));
            if (dto.IsSchool == true && dto.SchoolPrice <= 0)
                errors.Add(Tuple.Create("هزینه مدرسه باید بیشتر از صفر باشد.", nameof(dto.SchoolPrice)));
            if (dto.IsSchool == false && dto.PansionPrice <= 0)
                errors.Add(Tuple.Create("هزینه پانسیون باید بیشتر از صفر باشد.", nameof(dto.PansionPrice)));
            if (dto.PictureId.HasValue && dto.PictureId.Value > 0 &&
                !await _context.Pictures.AnyAsync(s => s.Id == dto.PictureId.Value))
                errors.Add(Tuple.Create("تصویر پانسیون معتبر نیست.", nameof(dto.PictureId)));
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            return new BaseResultDto(true);
        }
        public BaseResultDto UpdatePansionActiveDto(PansionActiveDto dto)
        {
            var item = _context.Pansions.FirstOrDefault(s => s.Id == dto.Id);
            item.Active = dto.Active;
            _context.Pansions.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }

        public async Task<BaseResultDto> ResubmitAsyncDto(PansionDto dto, long companionId, long ownerId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, "شناسه درخواست پانسیون معتبر نیست.");

            var companionIsValid = await _context.Companions.AnyAsync(s =>
                s.Id == companionId &&
                s.OwnerId == ownerId &&
                s.Active &&
                s.Approved &&
                !s.Deleted);
            if (!companionIsValid)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            var checker = await ValidateRequestAsync(dto);
            if (!checker.IsSuccess)
                return checker;

            var item = await _context.Pansions
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && s.CompanionId == companionId);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            if (item.Approve)
                return new BaseResultDto(false, "درخواست پانسیون تأیید شده است و از مسیر ارسال مجدد قابل تغییر نیست.");

            dto.CompanionId = companionId;
            dto.Active = false;
            dto.Approve = false;
            dto.ApprovalValue = null;
            dto.ShowToSite = false;
            var suggested = item.Suggested;
            var commentCount = item.CommentCount;
            var rateAvg = item.RateAvg;
            var rateCount = item.RateCount;
            var dailyCommissionPercent = item.DailyCommissionPercent;
            var hourlyCommissionPercent = item.HourlyCommissionPercent;
            mapper.Map(dto, item);
            item.Suggested = suggested;
            item.CommentCount = commentCount;
            item.RateAvg = rateAvg;
            item.RateCount = rateCount;
            item.DailyCommissionPercent = dailyCommissionPercent;
            item.HourlyCommissionPercent = hourlyCommissionPercent;
            await _context.SaveChangesAsync();
            await TryCreateNoticeAsync(item, NoticeTypeLabels.PansionUpdated, ownerId);

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdatePansionApproveAsyncDto(PansionApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            var item = await _context.Pansions
                .Include(s => s.Companion)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item?.Companion == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            item.Approve = dto.Approve;
            item.Active = dto.Approve;
            item.ApprovalValue = dto.Approve ? null : dto.ApprovalValue.Trim();
            item.ShowToSite = dto.Approve && item.ShowToSite;
            await _context.SaveChangesAsync();

            try
            {
                await _pushNotificationService.SendPushAsync(
                    dto.Approve ? PushTypeEnum.PushPansionRequestApproved : PushTypeEnum.PushPansionRequestRejected,
                    item.Companion.OwnerId,
                    token1: item.Name,
                    token2: item.ApprovalValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending pansion decision push for {PansionId} failed.", item.Id);
            }

            return new BaseResultDto(true);
        }

        private async Task TryCreateNoticeAsync(Pansion item, string label, long ownerId)
        {
            try
            {
                await _noticeService.CreateAsync(new NoticeCreateDto
                {
                    Label = label,
                    ActorUserId = ownerId,
                    ReferenceType = "Pansion",
                    ReferenceId = item.Id,
                    DeduplicationKey = $"{label}:{item.Id}:{DateTime.UtcNow.Ticks}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "pansionName", item.Name },
                        { "companionId", item.CompanionId.ToString() }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating admin notice {NoticeLabel} for pansion {PansionId} failed.", label, item.Id);
            }
        }

        public void UpdatePansionCommentCount(long pansionId)
        {
            var item = _context.Pansions.Include(s => s.PansionComments).ThenInclude(s => s.Status).AsTracking().FirstOrDefault(s => s.Id == pansionId);
            item.CommentCount = item.PansionComments.Count(c => c.Status.Label == CommentEnum.Comment_Accept.ToString());
            _context.Pansions.Update(item);
            _context.SaveChanges();
        }

        public async Task<List<SearchPansionDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<Pansion>(request.SearchTerms, item => item.Name, item => item.Discription, item => item.AddressValue, item => item.City.Name, item => item.State.Name);
            var query = _context.Pansions.Where(p => p.Active && p.Approve);
            return await query.Where(predicate).OrderByDescending(p => p.RateAvg)
                .Take(SearchQueryHelper.CandidateCount(request.PansionCount))
                .ProjectTo<SearchPansionDto>(mapper.ConfigurationProvider).ToListAsync();
        }

    }
}
