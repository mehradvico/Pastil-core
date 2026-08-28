using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionSrv
{
    public class CompanionService : CommonSrv<Companion, CompanionDto>, ICompanionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly ICodeService _codeService;
        private readonly INoticeService _notificationService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<CompanionService> _logger;
        private readonly string connectionString;
        public CompanionService(IDataBaseContext _context, IMapper mapper, IConfiguration config, IUserService userService, INoticeService notificationService, ICodeService codeService, IPushNotificationService pushNotificationService, ILogger<CompanionService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._userService = userService;
            this._codeService = codeService;
            this._notificationService = notificationService;
            this._pushNotificationService = pushNotificationService;
            this._logger = logger;
            this.connectionString = config.GetValue<string>("connection");
        }

        public async Task<BaseResultDto<CompanionVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Companions.Include(s => s.Picture).Include(s => s.Icon).Include(s => s.CompanionPets).Include(s => s.BackgroundPicture).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.CompanionTypes).Include(s => s.CompanionZones).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.CompanionZones).ThenInclude(s => s.Neighborhood).Include(s => s.Pansions).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionVDto>(true, mapper.Map<CompanionVDto>(item));
            }
            return new BaseResultDto<CompanionVDto>(false, mapper.Map<CompanionVDto>(item));
        }

        public async Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite)
        {
            var affectedRows = await _context.Companions
                .Where(x => x.Id == id && !x.Deleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ShowToSite, showToSite));

            if (affectedRows == 0)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto<NearbyCompanionSearchDto>> GetNearbyAsync(long userId, NearbyCompanionInputDto inputDto)
        {
            if (inputDto.RadiusMeter <= 0 || inputDto.RadiusMeter > 50000 ||
                inputDto.PageIndex <= 0 || inputDto.PageSize <= 0 || inputDto.PageSize > 100)
                return new BaseResultDto<NearbyCompanionSearchDto>(false, Resource.Notification.InvalidData, null);

            var userLocation = await _context.UserCurrentLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (userLocation == null)
                return new BaseResultDto<NearbyCompanionSearchDto>(false, Resource.Notification.NothingFound, null);

            var companions = _context.Companions
                .AsNoTracking()
                .Where(x => !x.Deleted && x.Active && x.Approved && x.Location != null)
                .Where(x => x.Location.Distance(userLocation.Location) <= inputDto.RadiusMeter);

            if (!string.IsNullOrWhiteSpace(inputDto.Q))
            {
                var q = inputDto.Q.Trim();
                companions = companions.Where(x => x.Name.Contains(q) || x.SearchKey.Contains(q));
            }

            if (inputDto.TypeId.HasValue)
                companions = companions.Where(x => x.CompanionTypes.Any(t => !t.Deleted && t.TypeId == inputDto.TypeId.Value));

            if (inputDto.PetId.HasValue)
                companions = companions.Where(x => x.CompanionPets.Any(p => !p.Deleted && p.PetId == inputDto.PetId.Value));

            if (inputDto.AssistanceId.HasValue)
                companions = companions.Where(x => x.CompanionAssistances.Any(a =>
                    !a.Deleted && a.Active && a.Approved && a.AssistanceId == inputDto.AssistanceId.Value));

            var rankedQuery = companions.Select(x => new NearbyCompanionRank
            {
                CompanionId = x.Id,
                DistanceMeter = x.Location.Distance(userLocation.Location),
                HasServiceZone = x.CompanionZones.Any(z => !z.Deleted),
                IsInServiceArea = x.CompanionZones.Any(z =>
                    !z.Deleted &&
                    z.CityId == userLocation.CityId &&
                    (!z.NeighborhoodId.HasValue ||
                        (userLocation.NeighborhoodId.HasValue && z.NeighborhoodId == userLocation.NeighborhoodId)))
            });

            if (inputDto.OnlyInServiceArea)
                rankedQuery = rankedQuery.Where(x => x.IsInServiceArea);

            var totalCount = await rankedQuery.CountAsync();
            var skip = (inputDto.PageIndex - 1) * inputDto.PageSize;
            var ranks = await rankedQuery
                .OrderByDescending(x => x.IsInServiceArea)
                .ThenBy(x => x.HasServiceZone)
                .ThenBy(x => x.DistanceMeter)
                .Skip(skip)
                .Take(inputDto.PageSize)
                .ToListAsync();

            var companionIds = ranks.Select(x => x.CompanionId).ToList();
            var companionItems = await _context.Companions
                .AsNoTracking()
                .Include(x => x.Picture)
                .Include(x => x.City)
                .Include(x => x.Neighborhood)
                .Include(x => x.Pansions)
                .Where(x => companionIds.Contains(x.Id))
                .ToListAsync();
            var companionById = companionItems.ToDictionary(x => x.Id);

            var result = new NearbyCompanionSearchDto(inputDto)
            {
                TotalCount = totalCount,
                CenterLocation = mapper.Map<Application.Common.Dto.LocationPoint.PointDto>(userLocation.Location),
                CityId = userLocation.CityId,
                NeighborhoodId = userLocation.NeighborhoodId,
                List = ranks
                    .Where(x => companionById.ContainsKey(x.CompanionId))
                    .Select(x =>
                    {
                        var dto = mapper.Map<NearbyCompanionVDto>(companionById[x.CompanionId]);
                        dto.DistanceMeter = Math.Round(x.DistanceMeter);
                        dto.HasServiceZone = x.HasServiceZone;
                        dto.IsInServiceArea = x.HasServiceZone ? x.IsInServiceArea : null;
                        return dto;
                    })
                    .ToList()
            };

            return new BaseResultDto<NearbyCompanionSearchDto>(true, result);
        }

        public override async Task<BaseResultDto<CompanionDto>> FindAsyncDto(long id)
        {
            var item = await _context.Companions.Include(s => s.Picture).Include(s => s.Icon).Include(s => s.CompanionPets).Include(s => s.BackgroundPicture).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.CompanionTypes).Include(s => s.CompanionZones).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.CompanionZones).ThenInclude(s => s.Neighborhood).Include(s => s.Pansions).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionDto>(true, mapper.Map<CompanionDto>(item));
            }
            return new BaseResultDto<CompanionDto>(false, mapper.Map<CompanionDto>(item));
        }

        private class NearbyCompanionRank
        {
            public long CompanionId { get; set; }
            public double DistanceMeter { get; set; }
            public bool HasServiceZone { get; set; }
            public bool IsInServiceArea { get; set; }
        }
        public CompanionSearchDto Search(CompanionInputDto baseSearchDto)
        {
            var model = _context.Companions.Include(s => s.CompanionAssistances).Include(s => s.Owner).Include(s => s.Picture).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Neighborhood).Include(s => s.CompanionTypes).Include(s => s.CompanionPets).Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.CompanionTypes)
                .Include(s => s.CompanionZones).ThenInclude(s => s.City).ThenInclude(s => s.State).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.ShowToSite.HasValue)
            {
                model = model.Where(s => s.ShowToSite == baseSearchDto.ShowToSite.Value);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q) || s.Phone.Equals(baseSearchDto.Q));
            }
            if (baseSearchDto.AssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistances.Any(ca => ca.AssistanceId == baseSearchDto.AssistanceId.Value));
            }
            if (baseSearchDto.NeighborhoodIds != null && baseSearchDto.NeighborhoodIds.Any())
            {
                model = model.Where(s => s.Neighborhood != null && baseSearchDto.NeighborhoodIds.Contains(s.Neighborhood.Id));
            }

            if (baseSearchDto.IsPersonal.HasValue)
            {
                model = model.Where(s => s.IsPersonal == baseSearchDto.IsPersonal.Value);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.City.Id == baseSearchDto.CityId.Value);
            }
            if (baseSearchDto.StateId.HasValue)
            {
                model = model.Where(s => s.City.State.Id == baseSearchDto.StateId.Value);
            }
            if (baseSearchDto.Approved.HasValue)
            {
                model = model.Where(s => s.Approved == baseSearchDto.Approved.Value);
            }
            if (baseSearchDto.OwnerId.HasValue)
            {
                model = model.Where(s => s.Owner.Id == baseSearchDto.OwnerId.Value);
            }
            if (baseSearchDto.TypeId.HasValue)
            {
                model = model.Where(s => s.CompanionTypes.Any(p => p.TypeId == baseSearchDto.TypeId && !p.Deleted));
            }
            if (baseSearchDto.PetId.HasValue)
            {
                model = model.Where(s => s.CompanionPets.Any(p => p.PetId == baseSearchDto.PetId));
            }
            if (baseSearchDto.GoldAccount.HasValue && baseSearchDto.GoldAccount.Value &&
                baseSearchDto.SilverAccount.HasValue && baseSearchDto.SilverAccount.Value)
            {
                model = model.Where(s =>
                    (s.GoldAccountDate.HasValue && s.GoldAccountDate >= DateTime.Now) ||
                    (s.SilverAccountDate.HasValue && s.SilverAccountDate >= DateTime.Now)
                );
            }
            else if (baseSearchDto.GoldAccount.HasValue && baseSearchDto.GoldAccount.Value)
            {
                model = model.Where(s => s.GoldAccountDate.HasValue && s.GoldAccountDate >= DateTime.Now);
            }
            else if (baseSearchDto.SilverAccount.HasValue && baseSearchDto.SilverAccount.Value)
            {
                model = model.Where(s => s.SilverAccountDate.HasValue && s.SilverAccountDate >= DateTime.Now);
            }
            model = model.OrderByDescending(s => s.GoldAccountDate.HasValue).ThenByDescending(s => s.SilverAccountDate.HasValue);
            if (baseSearchDto.AssistanceType.HasValue)
            {
                var label = baseSearchDto.AssistanceType.ToString(); model = model.Where(s => s.CompanionAssistances.Any(ca => ca.Codes.Any(code => code.Label == label)));
            }
            if (baseSearchDto.HasInsurance.HasValue)
            {
                if (baseSearchDto.HasInsurance.Value)
                {
                    model = model.Where(s => s.CompanionInsurancePackages.Any(p => !p.Deleted && p.Active));
                }
                else
                {
                    model = model.Where(s => !s.CompanionInsurancePackages.Any(p => !p.Deleted && p.Active));
                }
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
                case Common.Enumerable.SortEnum.MorePriority:
                    {
                        var now = DateTime.Now;
                        model = model.OrderBy(ad =>
        ad.GoldAccountDate != null && ad.GoldAccountDate > now ? 0 :
        ad.SilverAccountDate != null && ad.SilverAccountDate > now ? 1 : 2)
    .ThenByDescending(ad => ad.SilverAccountCreateDate).ThenByDescending(ad => ad.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.MoreSell:
                    {
                        var now = DateTime.Now;
                        model = model.OrderByDescending(s => s.RateAvg).ThenBy(ad =>
        ad.GoldAccountDate != null && ad.GoldAccountDate > now ? 0 :
        ad.SilverAccountDate != null && ad.SilverAccountDate > now ? 1 : 2)
    .ThenByDescending(ad => ad.SilverAccountCreateDate).ThenByDescending(ad => ad.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessSell:
                    {
                        var now = DateTime.Now;
                        model = model.OrderBy(s => s.RateAvg).ThenBy(ad =>
        ad.GoldAccountDate != null && ad.GoldAccountDate > now ? 0 :
        ad.SilverAccountDate != null && ad.SilverAccountDate > now ? 1 : 2)
    .ThenByDescending(ad => ad.SilverAccountCreateDate).ThenByDescending(ad => ad.Id);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionSearchDto(baseSearchDto, model, mapper);
        }
        public override async Task<BaseResultDto<CompanionDto>> InsertAsyncDto(CompanionDto dto)
        {
            try
            {
                var modelCheker = await InsertCheckerAsync(dto);
                if (!modelCheker.IsSuccess)
                {
                    return new BaseResultDto<CompanionDto>(false, modelCheker.Messages, dto);
                }
                else
                {
                    var item = mapper.Map<Companion>(dto);
                    item.Active = false;
                    item.Approved = false;
                    item.ActivationValue = null;
                    item.ShowToSite = false;
                    item.GoldAccountDate = null;
                    item.SilverAccountDate = null;
                    item.SilverAccountCreateDate = null;
                    var ownerId = dto.OwnerId;
                    var model = await _context.Users.Include(s => s.Companions).FirstOrDefaultAsync(s => s.Id == ownerId && !s.Deleted);
                    if (model == null)
                    {
                        return new BaseResultDto<CompanionDto>(false, Resource.Notification.NothingFound, dto);
                    }
                    bool existed = await _context.Companions.AnyAsync(x => x.OwnerId == dto.OwnerId && !x.Deleted);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionDto>(false, Resource.Notification.AlreadyIsCompanion, dto);
                    }
                    item.ReferralCode = await ReferralCodeGenerator.CreateCompanionCodeAsync(_context);
                    await _context.Companions.AddAsync(item);
                    await _context.SaveChangesAsync();
                    try
                    {
                        await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionSubmitted, ActorUserId = item.OwnerId, ReferenceType = "Companion", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionSubmitted}:{item.Id}", Metadata = new Dictionary<string, string> { { "companionName", item.Name } } });
                    }
                    catch
                    {
                        // ثبت Notice نباید نتیجه ثبت موفق نماینده را ناموفق اعلام کند.
                    }
                    return new BaseResultDto<CompanionDto>(true, mapper.Map<CompanionDto>(item));

                }
            }
            catch (DbUpdateException)
            {
                return new BaseResultDto<CompanionDto>(false, Resource.Notification.CompanionDataConflictsWithExisting, dto);
            }
            catch (Exception)
            {
                return new BaseResultDto<CompanionDto>(false, Resource.Notification.CompanionInsertErrorRetry, dto);
            }
        }
        public async Task<BaseResultDto> InsertCheckerAsync(CompanionDto dto)
        {
            dto.Name = dto.Name?.Trim();
            dto.Phone = await dto.Phone?.Trim().ToEnglishDigitsAsync();
            var errors = new List<Tuple<string, string>>();

            if (string.IsNullOrEmpty(dto.Name))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            }
            if (string.IsNullOrEmpty(dto.Phone))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterThePhone, nameof(dto.Phone)));

            }
            else if (!Regex.IsMatch(dto.Phone, @"^\+?\d{10,13}$"))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.CompanionPhoneNotValid, nameof(dto.Phone)));
            }
            if (dto.OwnerId <= 0 || !await _context.Users.AnyAsync(s => s.Id == dto.OwnerId && !s.Deleted))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.CompanionOwnerSelectionNotValid, nameof(dto.OwnerId)));
            }
            if (dto.CityId <= 0 || !await _context.Cities.AnyAsync(s => s.Id == dto.CityId))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.CompanionCitySelectionNotValid, nameof(dto.CityId)));
            }
            if (dto.NeighborhoodId.HasValue &&
                !await _context.Neighborhoods.AnyAsync(s => s.Id == dto.NeighborhoodId.Value && s.CityId == dto.CityId))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.CompanionNeighborhoodNotInSelectedCity, nameof(dto.NeighborhoodId)));
            }
            if (string.IsNullOrWhiteSpace(dto.AddressValue))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.CompanionEnterAddress, nameof(dto.AddressValue)));
            }

            await ValidatePictureAsync(dto.PictureId, nameof(dto.PictureId), Resource.Notification.CompanionPictureTitleMain, errors);
            await ValidatePictureAsync(dto.BackgroundPictureId, nameof(dto.BackgroundPictureId), Resource.Notification.CompanionPictureTitleBackground, errors);
            await ValidatePictureAsync(dto.IconId, nameof(dto.IconId), Resource.Notification.CompanionPictureTitleIcon, errors);

            if (dto.Location != null &&
                (dto.Location.x < -180 || dto.Location.x > 180 || dto.Location.y < -90 || dto.Location.y > 90))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.CompanionSelectedLocationCoordinatesNotValid, nameof(dto.Location)));
            }
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            return new BaseResultDto(true);
        }

        private async Task ValidatePictureAsync(
            long? pictureId,
            string fieldName,
            string fieldTitle,
            List<Tuple<string, string>> errors)
        {
            if (pictureId.HasValue && pictureId.Value > 0 &&
                !await _context.Pictures.AnyAsync(s => s.Id == pictureId.Value))
            {
                errors.Add(new Tuple<string, string>(string.Format(Resource.Notification.CompanionSelectedIdNotValidFormat, fieldTitle), fieldName));
            }
        }

        public async Task<BaseResultDto> UpdateGoldAccountDto(CompanionGoldAccountDto dto)
        {
            var model = await _context.Companions
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);

            if (model == null)
            {
                return new BaseResultDto<CompanionGoldAccountDto>(false, mapper.Map<CompanionGoldAccountDto>(model));
            }

            bool isAlreadyGold = model.GoldAccountDate.HasValue;
            model.GoldAccountDate = dto.GoldAccountDate;

            if (!isAlreadyGold)
            {
                var previousGold = await _context.Companions.Where(s => s.GoldAccountDate.HasValue && s.Id != dto.Id && !s.Deleted).FirstOrDefaultAsync();
                if (previousGold != null)
                {
                    previousGold.GoldAccountDate = null;
                    _context.Companions.Update(previousGold);
                }
            }

            _context.Companions.Update(model);
            await _context.SaveChangesAsync();

            return new BaseResultDto<CompanionGoldAccountDto>(true, mapper.Map<CompanionGoldAccountDto>(model));
        }

        public BaseResultDto UpdateSilverAccountDto(CompanionSilverAccountDto dto)
        {
            var model = _context.Companions.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (model == null)
            {
                return new BaseResultDto<CompanionSilverAccountDto>(false, mapper.Map<CompanionSilverAccountDto>(model));
            }
            else
            {
                if (dto.SilverAccountDate != model.SilverAccountDate)
                    model.SilverAccountCreateDate = DateTime.Now;

                model.SilverAccountDate = dto.SilverAccountDate;

                if (model.SilverAccountDate == null)
                    model.SilverAccountCreateDate = null;
            }
            _context.Companions.Update(model);
            _context.SaveChanges();
            return new BaseResultDto<CompanionSilverAccountDto>(true, mapper.Map<CompanionSilverAccountDto>(model));
        }

        public async Task<BaseResultDto> UpdateAsyncDto(CompanionDto dto)
        {
            try
            {
                if (dto.Id <= 0)
                    return new BaseResultDto(false, Resource.Notification.CompanionIdNotValid);

                var item = await _context.Companions
                    .AsTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
                if (item == null)
                    return new BaseResultDto(false, Resource.Notification.CompanionNotFound);

                var modelCheker = await InsertCheckerAsync(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                var ownerHasAnotherCompanion = await _context.Companions.AnyAsync(s =>
                    s.Id != dto.Id && s.OwnerId == dto.OwnerId && !s.Deleted);
                if (ownerHasAnotherCompanion)
                    return new BaseResultDto(false, Resource.Notification.CompanionAlreadyRegisteredForOwner);

                mapper.Map(dto, item);
                await _context.SaveChangesAsync();

                try
                {
                    await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionUpdated, ActorUserId = dto.OwnerId, ReferenceType = "Companion", ReferenceId = dto.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionUpdated}:{dto.Id}:{DateTime.UtcNow.Ticks}", Metadata = new Dictionary<string, string> { { "companionName", dto.Name } } });
                }
                catch
                {
                    // ثبت Notice نباید ویرایش موفق را ناموفق اعلام کند.
                }

                return new BaseResultDto(true);
            }
            catch (DbUpdateException)
            {
                return new BaseResultDto(false, Resource.Notification.CompanionDataConflictsWithExisting);
            }
            catch (Exception)
            {
                return new BaseResultDto(false, Resource.Notification.CompanionUpdateErrorRetry);
            }
        }

        public async Task<BaseResultDto> ResubmitAsyncDto(CompanionDto dto, long ownerId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, Resource.Notification.CompanionRequestIdNotValid);

            var item = await _context.Companions
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && s.OwnerId == ownerId && !s.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            dto.OwnerId = ownerId;
            var checker = await InsertCheckerAsync(dto);
            if (!checker.IsSuccess)
                return checker;

            dto.Active = false;
            dto.Approved = false;
            dto.ActivationValue = null;
            dto.ShowToSite = false;
            var referralCode = item.ReferralCode;
            var goldAccountDate = item.GoldAccountDate;
            var silverAccountDate = item.SilverAccountDate;
            var silverAccountCreateDate = item.SilverAccountCreateDate;
            mapper.Map(dto, item);
            item.ReferralCode = referralCode;
            item.GoldAccountDate = goldAccountDate;
            item.SilverAccountDate = silverAccountDate;
            item.SilverAccountCreateDate = silverAccountCreateDate;
            await _context.SaveChangesAsync();

            try
            {
                await _notificationService.CreateAsync(new NoticeCreateDto
                {
                    Label = NoticeTypeLabels.CompanionUpdated,
                    ActorUserId = ownerId,
                    ReferenceType = "Companion",
                    ReferenceId = item.Id,
                    DeduplicationKey = $"{NoticeTypeLabels.CompanionUpdated}:{item.Id}:{DateTime.UtcNow.Ticks}",
                    Metadata = new Dictionary<string, string> { { "companionName", item.Name } }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating admin notice for resubmitted companion {CompanionId} failed.", item.Id);
            }

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> ActivationAsyncDto(CompanionActivationDto dto)
        {
            var item = await _context.Companions
                .AsTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            if (!dto.Approved && string.IsNullOrWhiteSpace(dto.ActivationValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            item.Approved = dto.Approved;
            item.Active = dto.Approved;
            item.ActivationValue = dto.Approved ? null : dto.ActivationValue.Trim();
            item.ShowToSite = dto.Approved && item.ShowToSite;
            await _context.SaveChangesAsync();

            try
            {
                await _pushNotificationService.SendPushAsync(
                    dto.Approved
                        ? PushTypeEnum.PushCompanionRequestApproved
                        : PushTypeEnum.PushCompanionRequestRejected,
                    item.OwnerId,
                    token1: item.Name,
                    token2: item.ActivationValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending companion decision push for {CompanionId} failed.", item.Id);
            }

            return new BaseResultDto(true);
        }

        public BaseResultDto ActivationDto(CompanionActivationDto dto)
        {
            var item = _context.Companions.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (!dto.Active && !dto.Approved)
            {
                item.Active = false;
                item.Approved = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Active && string.IsNullOrEmpty(dto.ActivationValue))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else if (dto.Active && !dto.Approved)
            {
                item.Active = true;
                item.Approved = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Approved && (string.IsNullOrWhiteSpace(dto.ActivationValue)))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else
            {
                item.Active = true;
                item.Approved = true;
                item.ActivationValue = dto.ActivationValue;
            }
            _context.Companions.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }

        public async Task<List<SearchCompanionDto>> SearchMinAsync(SearchRequestDto request)
        {
            var q = request.Q;

            var textPredicate = SearchQueryHelper.ContainsAny<Companion>(request.SearchTerms,
                item => item.Name,
                item => item.SearchKey,
                item => item.AddressValue,
                item => item.City.Name,
                item => item.Neighborhood.Name);
            System.Linq.Expressions.Expression<Func<Companion, bool>> assistancePredicate = item =>
                item.CompanionAssistances.Any(assistance =>
                    assistance.Active &&
                    assistance.Approved &&
                    !assistance.Deleted &&
                    !assistance.Assistance.Deleted &&
                    assistance.Assistance.Active &&
                    assistance.Assistance.Name.Contains(q));

            var query = _context.Companions
                .AsNoTracking()
                .Where(s =>
                    !s.Deleted &&
                    s.Active &&
                    s.Approved);

            var list = await query
                .Where(SearchQueryHelper.Or(textPredicate, assistancePredicate))
                .OrderByDescending(item => item.RateAvg)
                .Take(SearchQueryHelper.CandidateCount(request.CompanionCount))
                .Select(s => new SearchCompanionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    IconId = s.IconId,
                    RateAvg = s.RateAvg,
                    RateCount = s.RateCount,
                    Icon = mapper.Map<PictureVDto>(s.Icon)
                })
                .ToListAsync();

            return list;
        }


        public void UpdateCompanionCommentCount(long CompanionId)
        {
            var item = _context.Companions.Include(s => s.CompanionComments).ThenInclude(s => s.Status).AsTracking().FirstOrDefault(s => s.Id == CompanionId);
            item.CommentCount = item.CompanionComments.Count(c => c.Status.Label == CommentEnum.Comment_Accept.ToString());
            _context.Companions.Update(item);
            _context.SaveChanges();
        }
        public async Task UpdateCompanionCommentRateAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateCompanionCommentsRate", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
