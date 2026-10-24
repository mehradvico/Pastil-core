using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachSrv
{
    public class CoachCoreService : ICoachCoreService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CoachCoreService> _logger;
        private readonly ICoachNotifier _notifier;

        public CoachCoreService(IDataBaseContext context, IMapper mapper, ILogger<CoachCoreService> logger, ICoachNotifier notifier = null)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notifier = notifier;
        }

        public CoachSearchDto Search(CoachInputDto input, bool onlyDeleted = false)
        {
            var model = _context.Coaches
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
            if (input.PetId.HasValue)
                model = model.Where(g => g.CoachPets.Any(p => !p.Deleted && p.PetId == input.PetId.Value));
            if (input.AssistanceId.HasValue)
                model = model.Where(g => g.CoachServices.Any(s => !s.Deleted && s.Active && s.Approved && s.AssistanceId == input.AssistanceId.Value));

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
            return new CoachSearchDto(input, model, _mapper);
        }

        public async Task<BaseResultDto<CoachVDto>> FindAsyncVDto(long id, bool includeDeleted = false)
        {
            var item = await _context.Coaches
                .Include(g => g.Picture).Include(g => g.Icon).Include(g => g.BackgroundPicture)
                .Include(g => g.City).Include(g => g.State).Include(g => g.Owner).Include(g => g.Provider)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == id && (includeDeleted || !g.Deleted));
            if (item == null)
                return new BaseResultDto<CoachVDto>(false, Resource.Notification.NothingFound, null);

            var view = _mapper.Map<CoachVDto>(item);
            view.PetIds = await _context.CoachPets.AsNoTracking()
                .Where(p => p.CoachId == id && !p.Deleted).Select(p => p.PetId).ToListAsync();
            return new BaseResultDto<CoachVDto>(true, view);
        }

        public async Task<BaseResultDto<CoachVDto>> CreateAsync(CoachDto dto, bool byAdmin, long actorUserId)
        {
            try
            {
                var validation = await ValidateAsync(dto);
                if (!validation.IsSuccess)
                    return new BaseResultDto<CoachVDto>(false, validation.Messages, null);

                // مالک بدون ProviderId ارسال‌شده، مربی را زیر ارائه‌دهنده‌ی خودش می‌سازد.
                var provider = await ResolveProviderAsync(dto.ProviderId, byAdmin ? (long?)null : actorUserId);
                if (provider == null)
                    return new BaseResultDto<CoachVDto>(false, Resource.Notification.CoachProviderRequired, null);
                // مالک فقط کاربر خودِ ارائه‌دهنده است؛ مالک بودن یک کاربر دیگر معنی ندارد (اشتراک و تسویه روی ارائه‌دهنده است).
                if (!byAdmin && provider.UserId != actorUserId)
                    return new BaseResultDto<CoachVDto>(false, Resource.Notification.AccessDenied, null);

                var item = _mapper.Map<Coach>(dto);
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

                await _context.Coaches.AddAsync(item);
                await _context.SaveChangesAsync();
                return await FindAsyncVDto(item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating coach failed.");
                return new BaseResultDto<CoachVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public CoachSearchDto SearchPublic(CoachInputDto input)
        {
            // فقط مربی تأییدشده و فعال؛ فیلترهای مدیریتی از ورودی عمومی نادیده گرفته می‌شوند.
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

        public async Task<BaseResultDto<CoachVDto>> FindPublicAsync(long id)
        {
            var found = await FindAsyncVDto(id);
            if (!found.IsSuccess || !found.Data.Active || !found.Data.Approved)
                return new BaseResultDto<CoachVDto>(false, Resource.Notification.NothingFound, null);
            var view = found.Data;
            Sanitize(view);
            view.ServiceOnline = view.ServiceOnline && OnlineBookableIds(new List<long> { view.Id }).Contains(view.Id);
            view.Owner = null;
            view.Provider = null;
            view.ActivationValue = null;
            view.LegacyCompanionId = null;
            view.DeleteDate = null;
            return new BaseResultDto<CoachVDto>(true, view);
        }

        /// <summary>
        /// «آنلاین» فقط وقتی به مشتری نشان داده می‌شود که حداقل یک بسته‌ی فعال واقعاً در حالت آنلاین (Mode=۳۷) رزروشدنی باشد، یا بسته‌ی مشاوره‌ی آنلاین فوری داشته باشد:
        /// بسته‌ی بدون ردیف قیمت همه‌ی حالت‌ها را دارد، بسته‌ی دارای ردیف فقط حالت‌های لیست‌شده را. پرچم ذخیره‌شده‌ی ServiceOnline دست‌نخورده می‌ماند.
        /// </summary>
        private HashSet<long> OnlineBookableIds(IReadOnlyCollection<long> ids)
        {
            if (ids.Count == 0)
                return new HashSet<long>();
            var online = (int)Entities.Entities.CoachField.CoachServiceModeEnum.Online;
            var result = _context.CoachServicePackages.AsNoTracking()
                .Where(p => !p.Deleted && p.Active
                            && !p.CoachService.Deleted && p.CoachService.Active && p.CoachService.Approved
                            && ids.Contains(p.CoachService.CoachId)
                            && (!p.Types.Any(t => !t.Deleted) || p.Types.Any(t => !t.Deleted && t.Mode == online)))
                .Select(p => p.CoachService.CoachId)
                .Distinct()
                .ToList()
                .ToHashSet();
            // یا مرکز بسته‌ی مشاوره‌ی آنلاین فوری (چت/تماس) فعال با قیمت مثبت دارد (روی خودِ مرکز کلید خورده)
            var consultation = _context.ConsultationPackages.AsNoTracking()
                .Where(p => p.CoachId != null && ids.Contains(p.CoachId.Value) && !p.Deleted && p.Active && p.Price > 0)
                .Select(p => p.CoachId.Value)
                .Distinct()
                .ToList();
            result.UnionWith(consultation);
            return result;
        }

        private static void Sanitize(CoachMinVDto item)
        {
            item.OwnerId = 0;
            item.ProviderId = null;
        }

        public async Task<BaseResultDto> UpdateAsync(CoachDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.Id <= 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                var validation = await ValidateAsync(dto);
                if (!validation.IsSuccess)
                    return validation;

                var item = await _context.Coaches.AsTracking().FirstOrDefaultAsync(g => g.Id == dto.Id && !g.Deleted);
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
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating coach {CoachId} failed.", dto?.Id);
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> UpdateActiveAsync(CoachActiveDto dto, long? ownerUserId)
        {
            var item = await _context.Coaches.AsTracking().FirstOrDefaultAsync(g => g.Id == dto.Id && !g.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            // مربیِ تأییدنشده را نمی‌شود فعال کرد (فقط خاموش‌کردن آزاد است).
            if (dto.Active && !item.Approved)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            item.Active = dto.Active;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateApproveAsync(CoachApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            // یک UPDATE اتمی: تأیید/رد، فعال‌شدن و بیرون‌رفتن از سایت با هم؛ خواندن قدیمیِ یک نمونه‌ی ردیابی‌شده نتیجه را خراب نمی‌کند.
            var approve = dto.Approve;
            var reason = approve ? null : dto.ApprovalValue.Trim();
            var affected = await _context.Coaches
                .Where(g => g.Id == dto.Id && !g.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.Approved, approve)
                    .SetProperty(g => g.Active, approve)
                    .SetProperty(g => g.ActivationValue, reason)
                    .SetProperty(g => g.ShowToSite, g => approve && g.ShowToSite));
            if (affected == 0)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (_notifier != null)
                await _notifier.CoachDecidedAsync(dto.Id, approve, reason);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> UpdateAdminSettingsAsync(CoachAdminSettingsDto dto)
        {
            if (dto.CommissionPercent < 0 || dto.CommissionPercent > 100)
                return new BaseResultDto(false, Resource.Notification.CoachValueOutOfRange);

            var affected = await _context.Coaches
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
            // روی سایت فقط مربیِ تأییدشده و فعال دیده می‌شود.
            var affected = await _context.Coaches
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
                var item = await _context.Coaches.AsNoTracking()
                    .Where(g => g.Id == id && !g.Deleted)
                    .Select(g => new { g.Id, g.OwnerId })
                    .FirstOrDefaultAsync();
                if (item == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                if (ownerUserId.HasValue && item.OwnerId != ownerUserId.Value)
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);

                var now = DateTime.Now;
                var affected = await _context.Coaches
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
                _logger.LogError(ex, "Soft-deleting coach {CoachId} failed.", id);
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> RestoreAsync(long id)
        {
            var affected = await _context.Coaches
                .Where(g => g.Id == id && g.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.Deleted, false)
                    .SetProperty(g => g.DeleteDate, (DateTime?)null)
                    .SetProperty(g => g.DeletedByUserId, (long?)null));
            return affected == 0
                ? new BaseResultDto(false, Resource.Notification.NothingFound)
                : new BaseResultDto(true, Resource.Notification.Success);
        }

        public async Task<BaseResultDto> SetPetsAsync(long coachId, IReadOnlyCollection<long> petIds, long? ownerUserId)
        {
            var wanted = (petIds ?? Array.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            var coach = await _context.Coaches.AsNoTracking()
                .Where(g => g.Id == coachId && !g.Deleted).Select(g => new { g.OwnerId }).FirstOrDefaultAsync();
            if (coach == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && coach.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (wanted.Count > 0 && await _context.Pets.CountAsync(p => wanted.Contains(p.Id)) != wanted.Count)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var existing = await _context.CoachPets.AsTracking().Where(p => p.CoachId == coachId).ToListAsync();
            foreach (var row in existing)
                row.Deleted = !wanted.Contains(row.PetId);
            foreach (var petId in wanted.Where(id => existing.All(e => e.PetId != id)))
                await _context.CoachPets.AddAsync(new CoachPet { CoachId = coachId, PetId = petId });
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task UpdateCommentCountAsync(long coachId)
        {
            var accepted = (long)CommentEnum.Comment_Accept;
            var stats = await _context.CoachComments.AsNoTracking()
                .Where(c => c.CoachId == coachId && c.StatusId == accepted)
                .GroupBy(c => 1)
                .Select(g => new { Count = g.Count(), Rated = g.Count(c => c.Rate != null), RateSum = g.Sum(c => c.Rate ?? 0) })
                .FirstOrDefaultAsync();
            var commentCount = stats?.Count ?? 0;
            var rateCount = stats?.Rated ?? 0;
            var rateAvg = rateCount == 0 ? 0d : Math.Round((double)stats.RateSum / rateCount, 2);
            await _context.Coaches.Where(g => g.Id == coachId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(g => g.CommentCount, commentCount)
                    .SetProperty(g => g.RateCount, rateCount)
                    .SetProperty(g => g.RateAvg, rateAvg));
        }

        public async Task<List<SearchCoachDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<Coach>(request.SearchTerms,
                item => item.Name, item => item.Summary, item => item.AddressValue, item => item.City.Name);
            return await _context.Coaches
                .Where(g => g.Active && g.Approved && !g.Deleted)
                .Where(predicate)
                .OrderByDescending(g => g.RateAvg)
                .Take(SearchQueryHelper.CandidateCount(request.PansionCount))
                .ProjectTo<SearchCoachDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public BaseResultDto GetSiteMap()
        {
            var list = _context.Coaches.AsNoTracking()
                .Where(g => g.Active && g.Approved && g.ShowToSite && !g.Deleted)
                .Select(g => new CoachSiteMapDto { Id = g.Id, Name = g.Name })
                .ToList();
            return new BaseResultDto<List<CoachSiteMapDto>>(true, list);
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

        private async Task<BaseResultDto> ValidateAsync(CoachDto dto)
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
                errors.Add(Tuple.Create(Resource.Notification.CoachPleaseEnterPhone, nameof(dto.Phone)));
            if (string.IsNullOrWhiteSpace(dto.AddressValue))
                errors.Add(Tuple.Create(Resource.Notification.CoachPleaseEnterAddress, nameof(dto.AddressValue)));
            if (dto.StateId <= 0 || !await _context.States.AnyAsync(s => s.Id == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.CoachSelectedStateInvalid, nameof(dto.StateId)));
            else if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(c => c.Id == dto.CityId && c.StateId == dto.StateId))
                errors.Add(Tuple.Create(Resource.Notification.CoachSelectedCityNotInState, nameof(dto.CityId)));
            if (dto.NeighborhoodId.HasValue &&
                !await _context.Neighborhoods.AnyAsync(n => n.Id == dto.NeighborhoodId.Value && n.CityId == dto.CityId))
                errors.Add(Tuple.Create(Resource.Notification.CoachSelectedCityNotInState, nameof(dto.NeighborhoodId)));
            if (dto.Location == null)
                errors.Add(Tuple.Create(Resource.Notification.CoachPleaseSelectLocation, nameof(dto.Location)));
            else if (dto.Location.x < -180 || dto.Location.x > 180 || dto.Location.y < -90 || dto.Location.y > 90)
                errors.Add(Tuple.Create(Resource.Notification.CoachLocationCoordinatesNotValid, nameof(dto.Location)));

            foreach (var (pictureId, field) in new[]
                     {
                         (dto.PictureId, nameof(dto.PictureId)),
                         (dto.BackgroundPictureId, nameof(dto.BackgroundPictureId)),
                         (dto.IconId, nameof(dto.IconId)),
                     })
            {
                if (pictureId.HasValue && pictureId.Value > 0 && !await _context.Pictures.AnyAsync(p => p.Id == pictureId.Value))
                    errors.Add(Tuple.Create(Resource.Notification.CoachPictureInvalid, field));
            }

            if (dto.MaxConcurrentPets < 0 || dto.MaxConcurrentPets > 1000 ||
                (dto.EstablishedYear.HasValue && (dto.EstablishedYear < 1300 || dto.EstablishedYear > 2200)) ||
                (dto.InstagramUrl?.Length ?? 0) > 300 || (dto.WebsiteUrl?.Length ?? 0) > 300 || (dto.LicenseNumber?.Length ?? 0) > 100)
                errors.Add(Tuple.Create(Resource.Notification.CoachValueOutOfRange, nameof(dto.MaxConcurrentPets)));

            return errors.Any() ? new BaseResultDto(false, errors) : new BaseResultDto(true);
        }

        private static string BuildSearchKey(Coach item) =>
            string.Join(" ", new[] { item.Name, item.Summary, item.AddressValue, item.Phone }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
