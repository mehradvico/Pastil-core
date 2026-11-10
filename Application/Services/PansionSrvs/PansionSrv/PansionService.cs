using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.PansionSrvs.PansionOwnershipSrv;
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
using Entities.Entities.ProviderField;
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
        private readonly Application.Services.Setting.MessageSenderSrv.IVenueRegistrationSms _registrationSms;

        public PansionService(IDataBaseContext _context, IMapper mapper, INoticeService noticeService, IPushNotificationService pushNotificationService, ILogger<PansionService> logger, Application.Services.Setting.MessageSenderSrv.IVenueRegistrationSms registrationSms = null) : base(_context, mapper)
        {
            _registrationSms = registrationSms;
            this._context = _context;
            this.mapper = mapper;
            this._noticeService = noticeService;
            this._pushNotificationService = pushNotificationService;
            this._logger = logger;
        }

        public BaseResultDto GetSiteMap()
        {
            var list = _context.Pansions.AsNoTracking()
                .Where(s => s.Active && s.ShowToSite && !s.Deleted)
                .Select(s => new PansionSiteMapDto { Id = s.Id, Name = s.Name })
                .ToList();
            return new BaseResultDto<List<PansionSiteMapDto>>(true, list);
        }

        public async Task<BaseResultDto<PansionVDto>> FindAsyncVDto(long id, bool includeDeleted = false)
        {
            var item = await _context.Pansions.Include(s => s.Picture).Include(s => s.Companion).ThenInclude(s => s.Owner).Include(s => s.Provider).ThenInclude(s => s.User).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.PansionPets.Where(x => !x.Deleted)).ThenInclude(s => s.Pet).Include(s => s.PansionComments).Include(s => s.PansionPictures).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && (includeDeleted || !s.Deleted));
            if (item != null)
            {
                return new BaseResultDto<PansionVDto>(true, mapper.Map<PansionVDto>(item));
            }
            return new BaseResultDto<PansionVDto>(false, mapper.Map<PansionVDto>(item));
        }

        public async Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite)
        {
            var affectedRows = await _context.Pansions
                .Where(x => x.Id == id && !x.Deleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ShowToSite, showToSite));

            if (affectedRows == 0)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            return new BaseResultDto(true);
        }

        public PansionSearchDto Search(PansionInputDto baseSearchDto, bool onlyDeleted = false)
        {
            var model = _context.Pansions.Include(s => s.Picture).Include(s => s.Companion).ThenInclude(s => s.Owner).Include(s => s.Companion).ThenInclude(s => s.Neighborhood).Include(s => s.Provider).ThenInclude(s => s.User).Include(s => s.City).ThenInclude(s => s.State).Include(s => s.PansionComments)
                .Include(s => s.PansionPictures).ThenInclude(s => s.Picture).AsQueryable();
            // حذف‌شده‌ها فقط وقتی ادمین صریحاً بخواهد (onlyDeleted) دیده می‌شوند؛ برای بقیه‌ی فراخوان‌ها هرگز.
            model = onlyDeleted ? model.Where(s => s.Deleted) : model.Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.ShowToSite.HasValue)
            {
                model = model.Where(s => s.ShowToSite == baseSearchDto.ShowToSite.Value);
            }
            if (baseSearchDto.IsDaycare.HasValue)
            {
                // NULL = مرکز هر دو خدمت را دارد؛ رکورد قدیمی NULL بدون تعرفه‌ی مهد فقط پانسیون شبانه حساب می‌شود.
                model = baseSearchDto.IsDaycare.Value
                    ? model.Where(s => s.IsDaycare == true || (s.IsDaycare == null && s.SchoolPrice > 0))
                    : model.Where(s => s.IsDaycare != true);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.ProviderId.HasValue)
            {
                model = model.Where(s => s.ProviderId == baseSearchDto.ProviderId.Value);
            }
            // پنل/اپ ارائه‌دهنده: همه‌ی پانسیون‌های «این کاربر» (ارائه‌دهنده یا نمایندگی قدیمی)؛ سمت کنترلر از کاربر پر می‌شود
            if (baseSearchDto.OwnerUserId.HasValue)
            {
                var owned = PansionOwnershipService.OwnedPansionIds(_context, baseSearchDto.OwnerUserId.Value, approvedOnly: false);
                model = model.Where(s => owned.Contains(s.Id));
            }
            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
            {
                var q = baseSearchDto.Q.Trim();
                model = model.Where(s => s.Name.Contains(q) || (s.AddressValue != null && s.AddressValue.Contains(q)) || (s.Discription != null && s.Discription.Contains(q)));
            }
            if (baseSearchDto.IsInfectious.HasValue)
            {
                model = model.Where(s => s.IsInfectious == baseSearchDto.IsInfectious.Value);
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
                            s.IsDaycare == true
                                ? s.SchoolPrice
                                : s.PansionPrice
                        );
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        model = model.OrderBy(s =>
                            s.IsDaycare == true
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
                    // پیام‌های اعتبارسنجی (نوع، ساعت کاری، قیمت، ...) باید به کلاینت برسد؛ قبلاً خالی برمی‌گشت
                    return new BaseResultDto<PansionDto>(false, modelCheker.Messages, dto);
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
                    var owner = await ValidateOwnerAsync(dto.ProviderId, dto.CompanionId, requireRole: true);
                    if (owner.Error != null)
                        return new BaseResultDto<PansionDto>(false, owner.Error, dto);
                    item.ProviderId = owner.ProviderId;
                    item.CompanionId = owner.CompanionId;
                    await _context.Pansions.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await TryCreateNoticeAsync(item, NoticeTypeLabels.PansionSubmitted, owner.OwnerUserId);
                    if (_registrationSms != null)
                        await _registrationSms.SendAsync(Application.Services.Setting.MessageSenderSrv.VenueRegistrationKind.Pansion, item.Name, owner.OwnerUserId);
                    return new BaseResultDto<PansionDto>(true, mapper.Map<PansionDto>(item));

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating pansion request for companion {CompanionId} failed.", dto?.CompanionId);
                return new BaseResultDto<PansionDto>(false, Resource.Notification.PansionCreateRequestFailed, dto);
            }
        }
        private sealed record PansionOwner(long? ProviderId, long? CompanionId, long OwnerUserId, string Error);

        /// <summary>
        /// مالک پانسیون (۲۰۲۶-۱۱-۰۳): ارائه‌دهنده (با نقش تأییدشده‌ی «پانسیون») یا برای سابقه نمایندگیِ فعال/تأییدشده.
        /// نمایندگی‌ای که مال همان ارائه‌دهنده نیست کنار گذاشته می‌شود تا مالکیت و تسویه دوگانه نشود.
        /// requireRole=false فقط وقتی ارائه‌دهنده‌ی پانسیونِ موجود عوض نشده است (ویرایش ادمین بعد از حذف نقش).
        /// </summary>
        private async Task<PansionOwner> ValidateOwnerAsync(long? providerId, long? companionId, bool requireRole)
        {
            if (providerId is > 0)
            {
                var provider = await _context.Providers.AsNoTracking()
                    .Where(p => p.Id == providerId.Value && !p.Deleted)
                    .Select(p => new { p.Id, p.UserId })
                    .FirstOrDefaultAsync();
                if (provider == null)
                    return new PansionOwner(null, null, 0, Resource.Notification.ProviderNotFound);
                if (requireRole && !await _context.ProviderRoleAssignments.AsNoTracking().AnyAsync(a =>
                        a.ProviderId == provider.Id && a.Approved && !a.Deleted
                        && a.ProviderRole.Label == ProviderRoleLabels.Pansion && a.ProviderRole.Active && !a.ProviderRole.Deleted))
                    return new PansionOwner(null, null, 0, Resource.Notification.ProviderRoleRequired);
                long? keptCompanionId = companionId is > 0 && await _context.Companions.AsNoTracking()
                    .AnyAsync(c => c.Id == companionId.Value && c.ProviderId == provider.Id && !c.Deleted)
                    ? companionId : null;
                return new PansionOwner(provider.Id, keptCompanionId, provider.UserId, null);
            }
            if (companionId is > 0)
            {
                var companion = await _context.Companions.AsNoTracking()
                    .Where(c => c.Id == companionId.Value && !c.Deleted && c.Active && c.Approved)
                    .Select(c => new { c.Id, c.ProviderId, c.OwnerId })
                    .FirstOrDefaultAsync();
                if (companion == null)
                    return new PansionOwner(null, null, 0, Resource.Notification.NothingFound);
                return new PansionOwner(companion.ProviderId, companion.Id, companion.OwnerId, null);
            }
            return new PansionOwner(null, null, 0, Resource.Notification.PansionProviderRequired);
        }

        // کمیسیون‌ها در فرم ویرایش نیستند و فقط با مسیر کمیسیون عوض می‌شوند.
        protected override void OnBeforeUpdateSave(Pansion item)
        {
            var entry = _context.Entry(item);
            entry.Property(nameof(Pansion.CommissionPercent)).IsModified = false;
            entry.Property(nameof(Pansion.DailyCommissionPercent)).IsModified = false;
            entry.Property(nameof(Pansion.HourlyCommissionPercent)).IsModified = false;
        }

        /// <summary>ویرایش ادمین: مالک را هم اعتبارسنجی می‌کند (عوض‌شدن ارائه‌دهنده نقش «پانسیون» می‌خواهد).</summary>
        public override BaseResultDto UpdateDto(PansionDto dto)
        {
            if (dto == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var current = _context.Pansions.AsNoTracking()
                .Where(s => s.Id == dto.Id)
                .Select(s => new { s.ProviderId })
                .FirstOrDefault();
            if (current == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var owner = ValidateOwnerAsync(dto.ProviderId, dto.CompanionId, requireRole: dto.ProviderId != current.ProviderId)
                .GetAwaiter().GetResult();
            if (owner.Error != null)
                return new BaseResultDto(false, owner.Error);
            dto.ProviderId = owner.ProviderId;
            dto.CompanionId = owner.CompanionId;
            return base.UpdateDto(dto);
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
            // IsDaycare: true = فقط مهد، false = فقط پانسیون، null = هر دو (هر دو تعرفه لازم است). پانسیون عفونی مهد ندارد.
            if (dto.IsDaycare == null && dto.IsInfectious)
                errors.Add(Tuple.Create(Resource.Notification.PansionInfectiousOvernightOnly, nameof(dto.IsDaycare)));
            if (dto.StateId <= 0 || !await _context.States.AnyAsync(s => s.Id == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.PansionSelectedStateInvalid, nameof(dto.StateId)));
            if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(s => s.Id == dto.CityId && s.StateId == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.PansionSelectedCityNotInState, nameof(dto.CityId)));
            if (string.IsNullOrWhiteSpace(dto.AddressValue))
                errors.Add(Tuple.Create(Resource.Notification.PansionPleaseEnterAddress, nameof(dto.AddressValue)));
            if (dto.Location == null)
                errors.Add(Tuple.Create(Resource.Notification.PansionPleaseSelectLocation, nameof(dto.Location)));
            else if (dto.Location.x < -180 || dto.Location.x > 180 || dto.Location.y < -90 || dto.Location.y > 90)
                errors.Add(Tuple.Create(Resource.Notification.PansionLocationCoordinatesNotValid, nameof(dto.Location)));
            if (string.IsNullOrWhiteSpace(dto.OpenHour))
                errors.Add(Tuple.Create(Resource.Notification.PansionPleaseEnterOpenHour, nameof(dto.OpenHour)));
            if (string.IsNullOrWhiteSpace(dto.CloseHour))
                errors.Add(Tuple.Create(Resource.Notification.PansionPleaseEnterCloseHour, nameof(dto.CloseHour)));
            if (!string.IsNullOrWhiteSpace(dto.OpenHour) && !string.IsNullOrWhiteSpace(dto.CloseHour) &&
                (!PansionReserveSrv.PansionReserveRules.TryParseHour(dto.OpenHour, out var openHour) ||
                 !PansionReserveSrv.PansionReserveRules.TryParseHour(dto.CloseHour, out var closeHour) ||
                 openHour >= closeHour))
                errors.Add(Tuple.Create(Resource.Notification.PansionWorkingHoursInvalid, nameof(dto.OpenHour)));
            if (dto.IsDaycare != false && dto.SchoolPrice <= 0)
                errors.Add(Tuple.Create(Resource.Notification.PansionSchoolPriceMustBeGreaterThanZero, nameof(dto.SchoolPrice)));
            if (dto.IsDaycare != true && dto.PansionPrice <= 0)
                errors.Add(Tuple.Create(Resource.Notification.PansionPriceMustBeGreaterThanZero, nameof(dto.PansionPrice)));
            if (dto.PictureId.HasValue && dto.PictureId.Value > 0 &&
                !await _context.Pictures.AnyAsync(s => s.Id == dto.PictureId.Value))
                errors.Add(Tuple.Create(Resource.Notification.PansionPictureInvalid, nameof(dto.PictureId)));
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            return new BaseResultDto(true);
        }
        public BaseResultDto UpdatePansionActiveDto(PansionActiveDto dto, long? ownerUserId = null)
        {
            var item = _context.Pansions.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && !PansionOwnershipService.OwnedPansionIds(_context, ownerUserId.Value).Any(id => id == item.Id))
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (dto.Active && !item.Approve)
                return new BaseResultDto(false, Resource.Notification.PansionActivationRequiresApproval);
            item.Active = dto.Active;
            _context.Pansions.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }

        public async Task<BaseResultDto> ResubmitAsyncDto(PansionDto dto, long ownerId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, Resource.Notification.PansionRequestIdInvalid);

            // فقط مالک (ارائه‌دهنده یا نمایندگی) فعال و تأییدشده
            var owned = PansionOwnershipService.OwnedPansionIds(_context, ownerId);
            if (!await owned.AnyAsync(id => id == dto.Id))
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            var checker = await ValidateRequestAsync(dto);
            if (!checker.IsSuccess)
                return checker;

            var item = await _context.Pansions
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            // مالک با ویرایش عوض نمی‌شود
            dto.CompanionId = item.CompanionId;
            dto.ProviderId = item.ProviderId;
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
                .Include(s => s.Provider)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
            var ownerUserId = item?.Provider?.UserId ?? item?.Companion?.OwnerId;
            if (ownerUserId == null)
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
                    ownerUserId.Value,
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
                        { "companionId", item.CompanionId?.ToString() ?? string.Empty },
                        { "providerId", item.ProviderId?.ToString() ?? string.Empty }
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

        // حذف نرم توسط مالک (ownerUserId = کاربر مالکِ ارائه‌دهنده/نمایندگی) یا ادمین (ownerUserId = null).
        // ردیف پاک نمی‌شود: رزروها، تسویه‌ها و گزارش مالی همان‌طور می‌مانند؛ فقط از فهرست‌ها، جستجو و رزرو جدید حذف می‌شود.
        // تا وقتی رزرو «باز» (پرداخت‌شده، لغو/تکمیل‌نشده) دارد حذف نمی‌شود تا مشتری بی‌خدمت نماند.
        public async Task<BaseResultDto> SoftDeleteAsync(long id, long? ownerUserId, long actorUserId)
        {
            try
            {
                var item = await _context.Pansions.AsNoTracking()
                    .Where(s => s.Id == id && !s.Deleted)
                    .Select(s => new { s.Id })
                    .FirstOrDefaultAsync();
                if (item == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                // حذف توسط مالک فقط برای «مالک» همان پانسیون (ارائه‌دهنده یا مالک نمایندگی)، نه اعضای تیم
                if (ownerUserId.HasValue &&
                    !await PansionOwnershipService.OwnedPansionIds(_context, ownerUserId.Value, approvedOnly: false).AnyAsync(x => x == id))
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);

                var complete = (long)PansionReserveStatusEnum.PansionReserveState_Complete;
                var registered = (long)PansionReserveStatusEnum.PansionReserveState_Registered;
                var hasOpenReserve = await _context.PansionReserves.AsNoTracking().AnyAsync(r =>
                    r.PansionId == id && !r.IsCancel && r.IsReserved && r.StatusId != complete && r.StatusId != registered);
                if (hasOpenReserve)
                    return new BaseResultDto(false, Resource.Notification.PansionDeleteHasOpenReserves);

                var now = DateTime.Now;
                var affected = await _context.Pansions
                    .Where(s => s.Id == id && !s.Deleted)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Deleted, true)
                        .SetProperty(s => s.DeleteDate, (DateTime?)now)
                        .SetProperty(s => s.DeletedByUserId, (long?)actorUserId)
                        // از همه‌ی نمایش‌ها هم بیرون برود: اگر بعداً بازگردانده شد، دوباره باید فعال/منتشر شود
                        .SetProperty(s => s.Active, false)
                        .SetProperty(s => s.ShowToSite, false)
                        .SetProperty(s => s.Suggested, false));
                return affected == 0
                    ? new BaseResultDto(false, Resource.Notification.NothingFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Soft-deleting pansion {PansionId} failed.", id);
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        // بازگردانی (فقط ادمین): پانسیون به حالت «غیرفعال و منتشرنشده» برمی‌گردد تا ادمین/مالک دوباره فعالش کنند
        public async Task<BaseResultDto> RestoreAsync(long id)
        {
            var affected = await _context.Pansions
                .Where(s => s.Id == id && s.Deleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Deleted, false)
                    .SetProperty(s => s.DeleteDate, (DateTime?)null)
                    .SetProperty(s => s.DeletedByUserId, (long?)null));
            return affected == 0
                ? new BaseResultDto(false, Resource.Notification.NothingFound)
                : new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<List<SearchPansionDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<Pansion>(request.SearchTerms, item => item.Name, item => item.Discription, item => item.AddressValue, item => item.City.Name, item => item.State.Name);
            var query = _context.Pansions.Where(p => p.Active && p.Approve && !p.Deleted);
            return await query.Where(predicate).OrderByDescending(p => p.RateAvg)
                .Take(SearchQueryHelper.CandidateCount(request.PansionCount))
                .ProjectTo<SearchPansionDto>(mapper.ConfigurationProvider).ToListAsync();
        }

    }
}
