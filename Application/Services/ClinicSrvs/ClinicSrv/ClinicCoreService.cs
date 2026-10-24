using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicSrv
{
    public class ClinicCoreService : IClinicCoreService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ClinicCoreService> _logger;
        private readonly IClinicNotifier _notifier;

        public ClinicCoreService(IDataBaseContext context, IMapper mapper, ILogger<ClinicCoreService> logger, IClinicNotifier notifier = null)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notifier = notifier;
        }

        public ClinicSearchDto Search(ClinicInputDto input, bool onlyDeleted = false)
        {
            var model = _context.Clinics
                .Include(g => g.Picture).Include(g => g.Icon).Include(g => g.City)
                .AsQueryable();
            // حذف‌شده‌ها فقط وقتی ادمین صریحاً بخواهد (onlyDeleted) دیده می‌شوند.
            model = onlyDeleted ? model.Where(g => g.Deleted) : model.Where(g => !g.Deleted);

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(g => g.Name.Contains(q) || g.Phone == q || g.AddressValue.Contains(q));
            }
            if (input.Available.HasValue)
                model = model.Where(g => g.Active == input.Available.Value);
            if (input.Approve.HasValue)
                model = model.Where(g => g.Approved == input.Approve.Value);
            if (input.ShowToSite.HasValue)
                model = model.Where(g => g.ShowToSite == input.ShowToSite.Value);
            if (input.Suggested.HasValue)
                model = model.Where(g => g.Suggested == input.Suggested.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(g => g.ProviderId == input.ProviderId.Value);
            if (input.OwnerId.HasValue)
                model = model.Where(g => g.OwnerId == input.OwnerId.Value);
            if (input.StateId.HasValue)
                model = model.Where(g => g.StateId == input.StateId.Value);
            if (input.CityId.HasValue)
                model = model.Where(g => g.CityId == input.CityId.Value);
            if (input.NeighborhoodIds != null && input.NeighborhoodIds.Any())
                model = model.Where(g => g.NeighborhoodId != null && input.NeighborhoodIds.Contains(g.NeighborhoodId.Value));
            if (input.ServiceAtHome.HasValue)
                model = model.Where(g => g.ServiceAtHome == input.ServiceAtHome.Value);
            if (input.ServiceInPerson.HasValue)
                model = model.Where(g => g.ServiceInPerson == input.ServiceInPerson.Value);
            if (input.ServiceOnline.HasValue)
                model = model.Where(g => g.ServiceOnline == input.ServiceOnline.Value);
            if (input.TypeId.HasValue)
                model = model.Where(g => g.ClinicTypes.Any(x => !x.Deleted && x.TypeId == input.TypeId.Value));
            if (input.PetId.HasValue)
                model = model.Where(g => g.ClinicPets.Any(p => !p.Deleted && p.PetId == input.PetId.Value));
            if (input.AssistanceId.HasValue)
                model = model.Where(g => g.ClinicServices.Any(s => !s.Deleted && s.Active && s.Approved && s.AssistanceId == input.AssistanceId.Value));

            var now = DateTime.Now;
            model = input.SortBy switch
            {
                SortEnum.Old => model.OrderBy(g => g.Id),
                SortEnum.MoreVisit => model.OrderByDescending(g => g.RateAvg).ThenByDescending(g => g.Id),
                SortEnum.LessVisit => model.OrderBy(g => g.RateAvg).ThenByDescending(g => g.Id),
                // اولویت ویژه: طلایی، بعد نقره‌ای، بعد بقیه
                SortEnum.MorePriority => model
                    .OrderBy(g => g.GoldAccountDate != null && g.GoldAccountDate > now ? 0 : g.SilverAccountDate != null && g.SilverAccountDate > now ? 1 : 2)
                    .ThenByDescending(g => g.Id),
                _ => model.OrderByDescending(g => g.Id),
            };
            return new ClinicSearchDto(input, model, _mapper);
        }

        public async Task<BaseResultDto<ClinicVDto>> FindAsyncVDto(long id, bool includeDeleted = false)
        {
            var item = await _context.Clinics
                .Include(g => g.Picture).Include(g => g.Icon).Include(g => g.BackgroundPicture)
                .Include(g => g.City).Include(g => g.State).Include(g => g.Owner).Include(g => g.Provider)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == id && (includeDeleted || !g.Deleted));
            if (item == null)
                return new BaseResultDto<ClinicVDto>(false, Resource.Notification.NothingFound, null);

            var view = _mapper.Map<ClinicVDto>(item);
            view.PetIds = await _context.ClinicPets.AsNoTracking()
                .Where(p => p.ClinicId == id && !p.Deleted).Select(p => p.PetId).ToListAsync();
            view.TypeIds = await _context.ClinicTypes.AsNoTracking()
                .Where(x => x.ClinicId == id && !x.Deleted).Select(x => x.TypeId).ToListAsync();
            return new BaseResultDto<ClinicVDto>(true, view);
        }

        public async Task<BaseResultDto<ClinicVDto>> CreateAsync(ClinicDto dto, bool byAdmin, long actorUserId)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.IsSuccess)
                    return new BaseResultDto<ClinicVDto>(false, validation.Messages, null);

                // مالک بدون ProviderId ارسال‌شده، کلینیک را زیر ارائه‌دهنده‌ی خودش می‌سازد.
                var provider = await ResolveProviderAsync(dto.ProviderId, byAdmin ? (long?)null : actorUserId);
                if (provider == null)
                    return new BaseResultDto<ClinicVDto>(false, Resource.Notification.ClinicProviderRequired, null);
                // مالک فقط کاربر خودِ ارائه‌دهنده است؛ مالک بودن یک کاربر دیگر معنی ندارد (اشتراک و تسویه روی ارائه‌دهنده است).
                if (!byAdmin && provider.UserId != actorUserId)
                    return new BaseResultDto<ClinicVDto>(false, Resource.Notification.AccessDenied, null);

                var item = _mapper.Map<Clinic>(dto);
                item.Id = 0;
                item.ProviderId = provider.Id;
                item.OwnerId = provider.UserId;
                item.CreateDate = DateTime.Now;
                item.Deleted = false;
                item.Suggested = false;
                item.LicenseApproved = false;
                item.CommissionPercent = 0;
                item.CommentCount = 0;
                item.RateAvg = 0;
                item.RateCount = 0;
                item.ShowToSite = false;
                // ادمین مستقیم تأییدشده می‌سازد؛ درخواست خودِ ارائه‌دهنده منتظر تأیید ادمین می‌ماند.
                item.Approved = byAdmin;
                item.Active = byAdmin;
                item.ActivationValue = null;
                item.StaffCount = 0;
                item.SearchKey = BuildSearchKey(item);

                await _context.Clinics.AddAsync(item);
                await _context.SaveChangesAsync();
                await ReplaceTypesAsync(item.Id, dto.TypeIds);
                return await FindAsyncVDto(item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating clinic failed.");
                return new BaseResultDto<ClinicVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public ClinicSearchDto SearchPublic(ClinicInputDto input)
        {
            // فقط کلینیک تأییدشده و فعال؛ فیلترهای مدیریتی از ورودی عمومی نادیده گرفته می‌شوند.
            input.Approve = true;
            input.Available = true;
            input.OwnerId = null;
            input.ShowToSite = null;
            var result = Search(input);
            var onlineIds = OnlineBookableIds(result.List.Select(item => item.Id).ToList());
            foreach (var item in result.List)
            {
                Sanitize(item);
                item.ServiceOnline = item.ServiceOnline && onlineIds.Contains(item.Id);
            }
            return result;
        }

        public async Task<BaseResultDto<ClinicVDto>> FindPublicAsync(long id)
        {
            var found = await FindAsyncVDto(id);
            if (!found.IsSuccess || !found.Data.Active || !found.Data.Approved)
                return new BaseResultDto<ClinicVDto>(false, Resource.Notification.NothingFound, null);
            var view = found.Data;
            Sanitize(view);
            view.ServiceOnline = view.ServiceOnline && OnlineBookableIds(new List<long> { view.Id }).Contains(view.Id);
            view.Owner = null;
            view.Provider = null;
            view.ActivationValue = null;
            view.LegacyCompanionId = null;
            view.DeleteDate = null;
            return new BaseResultDto<ClinicVDto>(true, view);
        }

        /// <summary>
        /// «آنلاین» فقط وقتی به مشتری نشان داده می‌شود که حداقل یک بسته‌ی فعال واقعاً در حالت آنلاین (Mode=۳۷) رزروشدنی باشد، یا بسته‌ی مشاوره‌ی آنلاین فوری داشته باشد:
        /// بسته‌ی بدون ردیف قیمت همه‌ی حالت‌ها را دارد، بسته‌ی دارای ردیف فقط حالت‌های لیست‌شده را. پرچم ذخیره‌شده‌ی ServiceOnline دست‌نخورده می‌ماند.
        /// </summary>
        private HashSet<long> OnlineBookableIds(IReadOnlyCollection<long> ids)
        {
            if (ids.Count == 0)
                return new HashSet<long>();
            var online = (int)Entities.Entities.ClinicField.ClinicServiceModeEnum.Online;
            var result = _context.ClinicServicePackages.AsNoTracking()
                .Where(p => !p.Deleted && p.Active
                            && !p.ClinicService.Deleted && p.ClinicService.Active && p.ClinicService.Approved
                            && ids.Contains(p.ClinicService.ClinicId)
                            && (!p.Types.Any(t => !t.Deleted) || p.Types.Any(t => !t.Deleted && t.Mode == online)))
                .Select(p => p.ClinicService.ClinicId)
                .Distinct()
                .ToList()
                .ToHashSet();
            // یا مرکز بسته‌ی مشاوره‌ی آنلاین فوری (چت/تماس) فعال با قیمت مثبت دارد (روی خودِ مرکز کلید خورده)
            var consultation = _context.ConsultationPackages.AsNoTracking()
                .Where(p => p.ClinicId != null && ids.Contains(p.ClinicId.Value) && !p.Deleted && p.Active && p.Price > 0)
                .Select(p => p.ClinicId.Value)
                .Distinct()
                .ToList();
            result.UnionWith(consultation);
            return result;
        }

        private static void Sanitize(ClinicMinVDto item)
        {
            item.OwnerId = 0;
            item.ProviderId = null;
        }

        public async Task<BaseResultDto> UpdateAsync(ClinicDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.Id <= 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                var validation = await ValidateAsync(dto);
                if (!validation.IsSuccess)
                    return validation;

                var item = await _context.Clinics.AsTracking().FirstOrDefaultAsync(g => g.Id == dto.Id && !g.Deleted);
                if (item == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);

                // ارائه‌دهنده/مالک با ویرایش عادی عوض نمی‌شود؛ شمارنده‌ها، تأیید و کمیسیون هم دست‌نخورده می‌مانند.
                item.Name = dto.Name.Trim();
                item.PictureId = dto.PictureId;
                item.BackgroundPictureId = dto.BackgroundPictureId;
                item.IconId = dto.IconId;
                item.Phone = dto.Phone?.Trim();
                item.AddressValue = dto.AddressValue?.Trim();
                item.StateId = dto.StateId;
                item.CityId = dto.CityId;
                item.NeighborhoodId = dto.NeighborhoodId;
                item.Location = _mapper.Map<NetTopologySuite.Geometries.Point>(dto.Location);
                item.Summary = dto.Summary;
                item.Description = dto.Description;
                item.Regulations = dto.Regulations;
                item.EstablishedYear = dto.EstablishedYear;
                item.LicenseNumber = dto.LicenseNumber?.Trim();
                item.InstagramUrl = dto.InstagramUrl?.Trim();
                item.WebsiteUrl = dto.WebsiteUrl?.Trim();
                item.ServiceAtHome = dto.ServiceAtHome;
                item.ServiceInPerson = dto.ServiceInPerson;
                item.ServiceOnline = dto.ServiceOnline;
                item.HasParking = dto.HasParking;
                item.HasWaitingRoom = dto.HasWaitingRoom;
                item.HasCamera = dto.HasCamera;
                item.MaxConcurrentPets = dto.MaxConcurrentPets;
                item.SeoH1 = dto.SeoH1;
                item.SeoMinDescription = dto.SeoMinDescription;
                item.SeoDescription = dto.SeoDescription;
                item.SeoTitle = dto.SeoTitle;
                item.SeoPictureAlt = dto.SeoPictureAlt;
                item.SeoUrlText = dto.SeoUrlText;
                item.SeoNoIndex = dto.SeoNoIndex;
                item.SeoNoFollow = dto.SeoNoFollow;
                item.SeoCanonical = dto.SeoCanonical;
                item.SearchKey = BuildSearchKey(item);

                if (ownerUserId.HasValue)
                {
                    // ویرایش توسط مالک دوباره بازبینی ادمین می‌خواهد (مثل پانسیون)؛ تا آن موقع از سایت و رزرو جدید بیرون می‌رود.
                    item.Approved = false;
                    item.Active = false;
                    item.ShowToSite = false;
                    item.ActivationValue = null;
                }

                await _context.SaveChangesAsync();
                if (dto.TypeIds != null && dto.TypeIds.Count > 0)
                    await ReplaceTypesAsync(item.Id, dto.TypeIds);
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating clinic {ClinicId} failed.", dto?.Id);
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> UpdateActiveAsync(ClinicActiveDto dto, long? ownerUserId)
        {
            var item = await _context.Clinics.AsTracking().FirstOrDefaultAsync(g => g.Id == dto.Id && !g.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            // کلینیکِ تأییدنشده را نمی‌شود فعال کرد (فقط خاموش‌کردن آزاد است).
            if (dto.Active && !item.Approved)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            item.Active = dto.Active;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateApproveAsync(ClinicApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            // یک UPDATE اتمی: تأیید/رد، فعال‌شدن و بیرون‌رفتن از سایت با هم؛ خواندن قدیمیِ یک نمونه‌ی ردیابی‌شده نتیجه را خراب نمی‌کند.
            var approve = dto.Approve;
            var reason = approve ? null : dto.ApprovalValue.Trim();
            var affected = await _context.Clinics
                .Where(g => g.Id == dto.Id && !g.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.Approved, approve)
                    .SetProperty(g => g.Active, approve)
                    .SetProperty(g => g.ActivationValue, reason)
                    .SetProperty(g => g.ShowToSite, g => approve && g.ShowToSite));
            if (affected == 0)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (_notifier != null)
                await _notifier.ClinicDecidedAsync(dto.Id, approve, reason);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateAdminSettingsAsync(ClinicAdminSettingsDto dto)
        {
            if (dto.CommissionPercent < 0 || dto.CommissionPercent > 100)
                return new BaseResultDto(false, Resource.Notification.ClinicValueOutOfRange);

            var affected = await _context.Clinics
                .Where(g => g.Id == dto.Id && !g.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.Suggested, dto.Suggested)
                    .SetProperty(g => g.LicenseApproved, dto.LicenseApproved)
                    .SetProperty(g => g.CommissionPercent, dto.CommissionPercent));
            return affected == 0
                ? new BaseResultDto(false, Resource.Notification.NothingFound)
                : new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite)
        {
            // روی سایت فقط کلینیکِ تأییدشده و فعال دیده می‌شود.
            var affected = await _context.Clinics
                .Where(g => g.Id == id && !g.Deleted && (!showToSite || (g.Approved && g.Active)))
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.ShowToSite, showToSite));
            return affected == 0
                ? new BaseResultDto(false, Resource.Notification.AccessDenied)
                : new BaseResultDto(true);
        }

        // حذف نرم توسط مالک (ownerUserId) یا ادمین (null): ردیف پاک نمی‌شود؛ رزرو، تسویه و گزارش مالی می‌مانند.
        public async Task<BaseResultDto> SoftDeleteAsync(long id, long? ownerUserId, long actorUserId)
        {
            try
            {
                var item = await _context.Clinics.AsNoTracking()
                    .Where(g => g.Id == id && !g.Deleted)
                    .Select(g => new { g.Id, g.OwnerId })
                    .FirstOrDefaultAsync();
                if (item == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);

                var now = DateTime.Now;
                var affected = await _context.Clinics
                    .Where(g => g.Id == id && !g.Deleted)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(g => g.Deleted, true)
                        .SetProperty(g => g.DeleteDate, (DateTime?)now)
                        .SetProperty(g => g.DeletedByUserId, (long?)actorUserId)
                        .SetProperty(g => g.Active, false)
                        .SetProperty(g => g.ShowToSite, false)
                        .SetProperty(g => g.Suggested, false));
                return affected == 0
                    ? new BaseResultDto(false, Resource.Notification.NothingFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Soft-deleting clinic {ClinicId} failed.", id);
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> RestoreAsync(long id)
        {
            var affected = await _context.Clinics
                .Where(g => g.Id == id && g.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.Deleted, false)
                    .SetProperty(g => g.DeleteDate, (DateTime?)null)
                    .SetProperty(g => g.DeletedByUserId, (long?)null));
            return affected == 0
                ? new BaseResultDto(false, Resource.Notification.NothingFound)
                : new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetPetsAsync(long clinicId, IReadOnlyCollection<long> petIds, long? ownerUserId)
        {
            var wanted = (petIds ?? Array.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            var clinic = await _context.Clinics.AsNoTracking()
                .Where(g => g.Id == clinicId && !g.Deleted).Select(g => new { g.OwnerId }).FirstOrDefaultAsync();
            if (clinic == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && clinic.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (wanted.Count > 0 && await _context.Pets.CountAsync(p => wanted.Contains(p.Id)) != wanted.Count)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var existing = await _context.ClinicPets.AsTracking().Where(p => p.ClinicId == clinicId).ToListAsync();
            foreach (var row in existing)
                row.Deleted = !wanted.Contains(row.PetId);
            foreach (var petId in wanted.Where(id => existing.All(e => e.PetId != id)))
                await _context.ClinicPets.AddAsync(new ClinicPet { ClinicId = clinicId, PetId = petId });
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task UpdateCommentCountAsync(long clinicId)
        {
            var accepted = (long)CommentEnum.Comment_Accept;
            var stats = await _context.ClinicComments.AsNoTracking()
                .Where(c => c.ClinicId == clinicId && c.StatusId == accepted)
                .GroupBy(c => 1)
                .Select(g => new { Count = g.Count(), Rated = g.Count(c => c.Rate != null), RateSum = g.Sum(c => c.Rate ?? 0) })
                .FirstOrDefaultAsync();
            var commentCount = stats?.Count ?? 0;
            var rateCount = stats?.Rated ?? 0;
            var rateAvg = rateCount == 0 ? 0d : Math.Round((double)stats.RateSum / rateCount, 2);
            await _context.Clinics.Where(g => g.Id == clinicId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.CommentCount, commentCount)
                    .SetProperty(g => g.RateCount, rateCount)
                    .SetProperty(g => g.RateAvg, rateAvg));
        }

        public async Task<List<SearchClinicDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<Clinic>(request.SearchTerms,
                item => item.Name, item => item.Summary, item => item.AddressValue, item => item.City.Name);
            return await _context.Clinics
                .Where(g => g.Active && g.Approved && !g.Deleted)
                .Where(predicate)
                .OrderByDescending(g => g.RateAvg)
                .Take(SearchQueryHelper.CandidateCount(request.PansionCount))
                .ProjectTo<SearchClinicDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public BaseResultDto GetSiteMap()
        {
            var list = _context.Clinics.AsNoTracking()
                .Where(g => g.Active && g.Approved && g.ShowToSite && !g.Deleted)
                .Select(g => new ClinicSiteMapDto { Id = g.Id, Name = g.Name })
                .ToList();
            return new BaseResultDto<List<ClinicSiteMapDto>>(true, list);
        }

        private async Task<Entities.Entities.ProviderField.Provider> ResolveProviderAsync(long? providerId, long? fallbackUserId)
        {
            var query = _context.Providers.AsNoTracking().Where(p => p.Active && p.Approved && !p.Deleted);
            if (providerId.HasValue && providerId.Value > 0)
                return await query.FirstOrDefaultAsync(p => p.Id == providerId.Value);
            if (fallbackUserId.HasValue)
                return await query.FirstOrDefaultAsync(p => p.UserId == fallbackUserId.Value);
            return null;
        }

        private async Task<BaseResultDto> ValidateAsync(ClinicDto dto)
        {
            if (dto == null)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var errors = new List<Tuple<string, string>>();
            dto.Name = dto.Name?.Trim();
            dto.AddressValue = dto.AddressValue?.Trim();
            dto.Phone = dto.Phone?.Trim();

            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 200)
                errors.Add(Tuple.Create(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            if (string.IsNullOrWhiteSpace(dto.Phone) || dto.Phone.Length > 40)
                errors.Add(Tuple.Create(Resource.Notification.ClinicPleaseEnterPhone, nameof(dto.Phone)));
            if (string.IsNullOrWhiteSpace(dto.AddressValue))
                errors.Add(Tuple.Create(Resource.Notification.ClinicPleaseEnterAddress, nameof(dto.AddressValue)));
            if (dto.StateId <= 0 || !await _context.States.AnyAsync(s => s.Id == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.ClinicSelectedStateInvalid, nameof(dto.StateId)));
            else if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(c => c.Id == dto.CityId && c.StateId == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.ClinicSelectedCityNotInState, nameof(dto.CityId)));
            if (dto.NeighborhoodId.HasValue &&
                !await _context.Neighborhoods.AnyAsync(n => n.Id == dto.NeighborhoodId.Value && n.CityId == dto.CityId))
                errors.Add(Tuple.Create(Resource.Notification.ClinicSelectedCityNotInState, nameof(dto.NeighborhoodId)));
            if (dto.Location == null)
                errors.Add(Tuple.Create(Resource.Notification.ClinicPleaseSelectLocation, nameof(dto.Location)));
            else if (dto.Location.x < -180 || dto.Location.x > 180 || dto.Location.y < -90 || dto.Location.y > 90)
                errors.Add(Tuple.Create(Resource.Notification.ClinicLocationCoordinatesNotValid, nameof(dto.Location)));

            foreach (var (pictureId, field) in new[]
                     {
                         (dto.PictureId, nameof(dto.PictureId)),
                         (dto.BackgroundPictureId, nameof(dto.BackgroundPictureId)),
                         (dto.IconId, nameof(dto.IconId)),
                     })
            {
                if (pictureId.HasValue && pictureId.Value > 0 && !await _context.Pictures.AnyAsync(p => p.Id == pictureId.Value))
                    errors.Add(Tuple.Create(Resource.Notification.ClinicPictureInvalid, field));
            }

            var typeIds = dto.TypeIds ?? new List<long>();
            if (typeIds.Any(x => !AllowedTypeIds.Contains(x)))
                errors.Add(Tuple.Create(Resource.Notification.ClinicValueOutOfRange, nameof(dto.TypeIds)));

            if (dto.MaxConcurrentPets < 0 || dto.MaxConcurrentPets > 1000 ||
                (dto.EstablishedYear.HasValue && (dto.EstablishedYear < 1300 || dto.EstablishedYear > 2200)) ||
                (dto.InstagramUrl?.Length ?? 0) > 300 || (dto.WebsiteUrl?.Length ?? 0) > 300 || (dto.LicenseNumber?.Length ?? 0) > 100)
                errors.Add(Tuple.Create(Resource.Notification.ClinicValueOutOfRange, nameof(dto.MaxConcurrentPets)));

            return errors.Any() ? new BaseResultDto(false, errors) : new BaseResultDto(true);
        }

        private static readonly long[] AllowedTypeIds =
        {
            (long)Application.Common.Enumerable.Code.CompanionTypeEnum.CompanionType_Clinic,
            (long)Application.Common.Enumerable.Code.CompanionTypeEnum.CompanionType_Laboratory,
            (long)Application.Common.Enumerable.Code.CompanionTypeEnum.CompanionType_Nurse,
        };

        /// <summary>انواع کلینیک را جایگزین می‌کند؛ خالی = کلینیک (۴۰). ردیف قدیمی حذف نرم می‌شود.</summary>
        private async Task ReplaceTypesAsync(long clinicId, IEnumerable<long> typeIds)
        {
            var wanted = (typeIds ?? Array.Empty<long>()).Distinct().Where(x => AllowedTypeIds.Contains(x)).ToList();
            if (wanted.Count == 0)
                wanted.Add(AllowedTypeIds[0]);
            var existing = await _context.ClinicTypes.AsTracking().Where(x => x.ClinicId == clinicId).ToListAsync();
            foreach (var row in existing)
                row.Deleted = !wanted.Contains(row.TypeId);
            foreach (var typeId in wanted.Where(w => existing.All(e => e.TypeId != w)))
                await _context.ClinicTypes.AddAsync(new ClinicType { ClinicId = clinicId, TypeId = typeId });
            await _context.SaveChangesAsync();
        }

        private static string BuildSearchKey(Clinic item) =>
            string.Join(" ", new[] { item.Name, item.Summary, item.AddressValue, item.Phone }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
