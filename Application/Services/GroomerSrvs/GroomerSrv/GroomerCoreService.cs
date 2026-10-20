using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerSrv
{
    public class GroomerCoreService : IGroomerCoreService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GroomerCoreService> _logger;
        private readonly IGroomerNotifier _notifier;

        public GroomerCoreService(IDataBaseContext context, IMapper mapper, ILogger<GroomerCoreService> logger, IGroomerNotifier notifier = null)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notifier = notifier;
        }

        public GroomerSearchDto Search(GroomerInputDto input, bool onlyDeleted = false)
        {
            var model = _context.Groomers
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
            if (input.ServiceInSalon.HasValue)
                model = model.Where(g => g.ServiceInSalon == input.ServiceInSalon.Value);
            if (input.PickupAvailable.HasValue)
                model = model.Where(g => g.PickupAvailable == input.PickupAvailable.Value);
            if (input.PetId.HasValue)
                model = model.Where(g => g.GroomerPets.Any(p => !p.Deleted && p.PetId == input.PetId.Value));
            if (input.AssistanceId.HasValue)
                model = model.Where(g => g.GroomerServices.Any(s => !s.Deleted && s.Active && s.Approved && s.AssistanceId == input.AssistanceId.Value));

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
            return new GroomerSearchDto(input, model, _mapper);
        }

        public async Task<BaseResultDto<GroomerVDto>> FindAsyncVDto(long id, bool includeDeleted = false)
        {
            var item = await _context.Groomers
                .Include(g => g.Picture).Include(g => g.Icon).Include(g => g.BackgroundPicture)
                .Include(g => g.City).Include(g => g.State).Include(g => g.Owner).Include(g => g.Provider)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == id && (includeDeleted || !g.Deleted));
            if (item == null)
                return new BaseResultDto<GroomerVDto>(false, Resource.Notification.NothingFound, null);

            var view = _mapper.Map<GroomerVDto>(item);
            view.PetIds = await _context.GroomerPets.AsNoTracking()
                .Where(p => p.GroomerId == id && !p.Deleted).Select(p => p.PetId).ToListAsync();
            return new BaseResultDto<GroomerVDto>(true, view);
        }

        public async Task<BaseResultDto<GroomerVDto>> CreateAsync(GroomerDto dto, bool byAdmin, long actorUserId)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.IsSuccess)
                    return new BaseResultDto<GroomerVDto>(false, validation.Messages, null);

                // مالک بدون ProviderId ارسال‌شده، آرایشگاه را زیر ارائه‌دهنده‌ی خودش می‌سازد.
                var provider = await ResolveProviderAsync(dto.ProviderId, byAdmin ? (long?)null : actorUserId);
                if (provider == null)
                    return new BaseResultDto<GroomerVDto>(false, Resource.Notification.GroomerProviderRequired, null);
                // مالک فقط کاربر خودِ ارائه‌دهنده است؛ مالک بودن یک کاربر دیگر معنی ندارد (اشتراک و تسویه روی ارائه‌دهنده است).
                if (!byAdmin && provider.UserId != actorUserId)
                    return new BaseResultDto<GroomerVDto>(false, Resource.Notification.AccessDenied, null);

                var item = _mapper.Map<Groomer>(dto);
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
                item.PortfolioCount = 0;
                item.PortfolioLikeCount = 0;
                item.ShowToSite = false;
                // ادمین مستقیم تأییدشده می‌سازد؛ درخواست خودِ ارائه‌دهنده منتظر تأیید ادمین می‌ماند.
                item.Approved = byAdmin;
                item.Active = byAdmin;
                item.ActivationValue = null;
                item.StaffCount = 0;
                item.SearchKey = BuildSearchKey(item);

                await _context.Groomers.AddAsync(item);
                await _context.SaveChangesAsync();
                return await FindAsyncVDto(item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating groomer failed.");
                return new BaseResultDto<GroomerVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public GroomerSearchDto SearchPublic(GroomerInputDto input)
        {
            // فقط آرایشگاه تأییدشده و فعال؛ فیلترهای مدیریتی از ورودی عمومی نادیده گرفته می‌شوند.
            input.Approve = true;
            input.Available = true;
            input.OwnerId = null;
            input.ShowToSite = null;
            var result = Search(input);
            foreach (var item in result.List)
                Sanitize(item);
            return result;
        }

        public async Task<BaseResultDto<GroomerVDto>> FindPublicAsync(long id)
        {
            var found = await FindAsyncVDto(id);
            if (!found.IsSuccess || !found.Data.Active || !found.Data.Approved)
                return new BaseResultDto<GroomerVDto>(false, Resource.Notification.NothingFound, null);
            var view = found.Data;
            Sanitize(view);
            view.Owner = null;
            view.Provider = null;
            view.ActivationValue = null;
            view.LegacyCompanionId = null;
            view.DeleteDate = null;
            return new BaseResultDto<GroomerVDto>(true, view);
        }

        private static void Sanitize(GroomerMinVDto item)
        {
            item.OwnerId = 0;
            item.ProviderId = null;
        }

        public async Task<BaseResultDto> UpdateAsync(GroomerDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.Id <= 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                var validation = await ValidateAsync(dto);
                if (!validation.IsSuccess)
                    return validation;

                var item = await _context.Groomers.AsTracking().FirstOrDefaultAsync(g => g.Id == dto.Id && !g.Deleted);
                if (item == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);

                // ارائه‌دهنده/مالک با ویرایش عادی عوض نمی‌شود؛ شمارنده‌ها، تأیید و کمیسیون هم دست‌نخورده می‌مانند.
                item.Name = dto.Name.Trim();
                item.IsPersonal = dto.IsPersonal;
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
                item.ServiceInSalon = dto.ServiceInSalon;
                item.PickupAvailable = dto.PickupAvailable;
                item.PickupFee = dto.PickupAvailable ? dto.PickupFee : 0;
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
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating groomer {GroomerId} failed.", dto?.Id);
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> UpdateActiveAsync(GroomerActiveDto dto, long? ownerUserId)
        {
            var item = await _context.Groomers.AsTracking().FirstOrDefaultAsync(g => g.Id == dto.Id && !g.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            // آرایشگاهِ تأییدنشده را نمی‌شود فعال کرد (فقط خاموش‌کردن آزاد است).
            if (dto.Active && !item.Approved)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            item.Active = dto.Active;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateApproveAsync(GroomerApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            // یک UPDATE اتمی: تأیید/رد، فعال‌شدن و بیرون‌رفتن از سایت با هم؛ خواندن قدیمیِ یک نمونه‌ی ردیابی‌شده نتیجه را خراب نمی‌کند.
            var approve = dto.Approve;
            var reason = approve ? null : dto.ApprovalValue.Trim();
            var affected = await _context.Groomers
                .Where(g => g.Id == dto.Id && !g.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.Approved, approve)
                    .SetProperty(g => g.Active, approve)
                    .SetProperty(g => g.ActivationValue, reason)
                    .SetProperty(g => g.ShowToSite, g => approve && g.ShowToSite));
            if (affected == 0)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (_notifier != null)
                await _notifier.GroomerDecidedAsync(dto.Id, approve, reason);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateAdminSettingsAsync(GroomerAdminSettingsDto dto)
        {
            if (dto.CommissionPercent < 0 || dto.CommissionPercent > 100)
                return new BaseResultDto(false, Resource.Notification.GroomerValueOutOfRange);

            var affected = await _context.Groomers
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
            // روی سایت فقط آرایشگاهِ تأییدشده و فعال دیده می‌شود.
            var affected = await _context.Groomers
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
                var item = await _context.Groomers.AsNoTracking()
                    .Where(g => g.Id == id && !g.Deleted)
                    .Select(g => new { g.Id, g.OwnerId })
                    .FirstOrDefaultAsync();
                if (item == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);

                var now = DateTime.Now;
                var affected = await _context.Groomers
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
                _logger.LogError(ex, "Soft-deleting groomer {GroomerId} failed.", id);
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> RestoreAsync(long id)
        {
            var affected = await _context.Groomers
                .Where(g => g.Id == id && g.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.Deleted, false)
                    .SetProperty(g => g.DeleteDate, (DateTime?)null)
                    .SetProperty(g => g.DeletedByUserId, (long?)null));
            return affected == 0
                ? new BaseResultDto(false, Resource.Notification.NothingFound)
                : new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetPetsAsync(long groomerId, IReadOnlyCollection<long> petIds, long? ownerUserId)
        {
            var wanted = (petIds ?? Array.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            var groomer = await _context.Groomers.AsNoTracking()
                .Where(g => g.Id == groomerId && !g.Deleted).Select(g => new { g.OwnerId }).FirstOrDefaultAsync();
            if (groomer == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && groomer.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (wanted.Count > 0 && await _context.Pets.CountAsync(p => wanted.Contains(p.Id)) != wanted.Count)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var existing = await _context.GroomerPets.AsTracking().Where(p => p.GroomerId == groomerId).ToListAsync();
            foreach (var row in existing)
                row.Deleted = !wanted.Contains(row.PetId);
            foreach (var petId in wanted.Where(id => existing.All(e => e.PetId != id)))
                await _context.GroomerPets.AddAsync(new GroomerPet { GroomerId = groomerId, PetId = petId });
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task UpdateCommentCountAsync(long groomerId)
        {
            var accepted = (long)CommentEnum.Comment_Accept;
            var stats = await _context.GroomerComments.AsNoTracking()
                .Where(c => c.GroomerId == groomerId && c.StatusId == accepted)
                .GroupBy(c => 1)
                .Select(g => new { Count = g.Count(), Rated = g.Count(c => c.Rate != null), RateSum = g.Sum(c => c.Rate ?? 0) })
                .FirstOrDefaultAsync();
            var commentCount = stats?.Count ?? 0;
            var rateCount = stats?.Rated ?? 0;
            var rateAvg = rateCount == 0 ? 0d : Math.Round((double)stats.RateSum / rateCount, 2);
            await _context.Groomers.Where(g => g.Id == groomerId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.CommentCount, commentCount)
                    .SetProperty(g => g.RateCount, rateCount)
                    .SetProperty(g => g.RateAvg, rateAvg));
        }

        public async Task<List<SearchGroomerDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<Groomer>(request.SearchTerms,
                item => item.Name, item => item.Summary, item => item.AddressValue, item => item.City.Name);
            return await _context.Groomers
                .Where(g => g.Active && g.Approved && !g.Deleted)
                .Where(predicate)
                .OrderByDescending(g => g.RateAvg)
                .Take(SearchQueryHelper.CandidateCount(request.PansionCount))
                .ProjectTo<SearchGroomerDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public BaseResultDto GetSiteMap()
        {
            var list = _context.Groomers.AsNoTracking()
                .Where(g => g.Active && g.Approved && g.ShowToSite && !g.Deleted)
                .Select(g => new GroomerSiteMapDto { Id = g.Id, Name = g.Name })
                .ToList();
            return new BaseResultDto<List<GroomerSiteMapDto>>(true, list);
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

        private async Task<BaseResultDto> ValidateAsync(GroomerDto dto)
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
                errors.Add(Tuple.Create(Resource.Notification.GroomerPleaseEnterPhone, nameof(dto.Phone)));
            if (string.IsNullOrWhiteSpace(dto.AddressValue))
                errors.Add(Tuple.Create(Resource.Notification.GroomerPleaseEnterAddress, nameof(dto.AddressValue)));
            if (dto.StateId <= 0 || !await _context.States.AnyAsync(s => s.Id == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.GroomerSelectedStateInvalid, nameof(dto.StateId)));
            else if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(c => c.Id == dto.CityId && c.StateId == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.GroomerSelectedCityNotInState, nameof(dto.CityId)));
            if (dto.NeighborhoodId.HasValue &&
                !await _context.Neighborhoods.AnyAsync(n => n.Id == dto.NeighborhoodId.Value && n.CityId == dto.CityId))
                errors.Add(Tuple.Create(Resource.Notification.GroomerSelectedCityNotInState, nameof(dto.NeighborhoodId)));
            if (dto.Location == null)
                errors.Add(Tuple.Create(Resource.Notification.GroomerPleaseSelectLocation, nameof(dto.Location)));
            else if (dto.Location.x < -180 || dto.Location.x > 180 || dto.Location.y < -90 || dto.Location.y > 90)
                errors.Add(Tuple.Create(Resource.Notification.GroomerLocationCoordinatesNotValid, nameof(dto.Location)));

            foreach (var (pictureId, field) in new[]
                     {
                         (dto.PictureId, nameof(dto.PictureId)),
                         (dto.BackgroundPictureId, nameof(dto.BackgroundPictureId)),
                         (dto.IconId, nameof(dto.IconId)),
                     })
            {
                if (pictureId.HasValue && pictureId.Value > 0 && !await _context.Pictures.AnyAsync(p => p.Id == pictureId.Value))
                    errors.Add(Tuple.Create(Resource.Notification.GroomerPictureInvalid, field));
            }

            if (dto.PickupFee < 0 || dto.MaxConcurrentPets < 0 || dto.MaxConcurrentPets > 1000 ||
                (dto.EstablishedYear.HasValue && (dto.EstablishedYear < 1300 || dto.EstablishedYear > 2200)) ||
                (dto.InstagramUrl?.Length ?? 0) > 300 || (dto.WebsiteUrl?.Length ?? 0) > 300 || (dto.LicenseNumber?.Length ?? 0) > 100)
                errors.Add(Tuple.Create(Resource.Notification.GroomerValueOutOfRange, nameof(dto.PickupFee)));

            return errors.Any() ? new BaseResultDto(false, errors) : new BaseResultDto(true);
        }

        private static string BuildSearchKey(Groomer item) =>
            string.Join(" ", new[] { item.Name, item.Summary, item.AddressValue, item.Phone }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
