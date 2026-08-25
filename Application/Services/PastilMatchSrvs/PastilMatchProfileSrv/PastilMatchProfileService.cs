using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv
{
    public class PastilMatchProfileService : CommonSrv<PastilMatchProfile, PastilMatchProfileDto>, IPastilMatchProfileService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly INoticeService _noticeService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<PastilMatchProfileService> _logger;

        private static readonly Regex UsernamePattern = new(
            "^[a-z][a-z0-9_]{4,31}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static string NormalizeUsername(string username)
        {
            return string.IsNullOrWhiteSpace(username)
                ? null
                : username.Trim().ToLowerInvariant();
        }

        private static bool IsValidUsername(string username)
        {
            return username != null && UsernamePattern.IsMatch(username);
        }

        public PastilMatchProfileService(
            IDataBaseContext _context,
            IMapper mapper,
            ICurrentUserHelper currentUser,
            INoticeService noticeService,
            IPushNotificationService pushNotificationService,
            ILogger<PastilMatchProfileService> logger
        ) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
            this._noticeService = noticeService;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        public async Task<BaseResultDto<PastilMatchProfileVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PastilMatchProfiles
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.User)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.Picture)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.Pet)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.PetBreed)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.PetBreed2)
                .Include(s => s.EnergyLevel)
                .Include(s => s.SocialLevel)
                .Include(s => s.City)
                .Include(s => s.Neighborhood)
                .Include(s => s.PastilMatchProfileGoals.Where(g => !g.Deleted))
                    .ThenInclude(s => s.PastilMatchGoal)
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    !s.Deleted
                );

            if (item == null)
            {
                return new BaseResultDto<PastilMatchProfileVDto>(
                    false,
                    Resource.Notification.NothingFound,
                    null
                );
            }

            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin =
                _currentUser.CurrentUser.RoleEnum ==
                RoleEnum.Admin.ToString();

            if (!isAdmin &&
                item.UserPet.UserId != userId &&
                !item.IsActive)
            {
                return new BaseResultDto<PastilMatchProfileVDto>(
                    false,
                    Resource.Notification.NothingFound,
                    null
                );
            }

            var result = mapper.Map<PastilMatchProfileVDto>(item);

            if (!isAdmin && item.UserPet.UserId != userId)
            {
                result.AdminDescription = null;
            }

            return new BaseResultDto<PastilMatchProfileVDto>(
                true,
                result
            );
        }

        public PastilMatchProfileSearchDto Search(PastilMatchProfileInputDto baseSearchDto)
        {
            var model = _context.PastilMatchProfiles
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.User)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.Picture)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.Pet)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.PetBreed)
                .Include(s => s.UserPet)
                    .ThenInclude(s => s.PetBreed2)
                .Include(s => s.EnergyLevel)
                .Include(s => s.SocialLevel)
                .Include(s => s.City)
                .Include(s => s.Neighborhood)
                .Include(s => s.PastilMatchProfileGoals.Where(g => !g.Deleted))
                    .ThenInclude(s => s.PastilMatchGoal)
                .Where(s => !s.Deleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
            {
                var q = baseSearchDto.Q.Trim();
                var normalizedQ = q.ToLowerInvariant();

                model = model.Where(s =>
                    s.Description.Contains(q) ||
                    s.UserPet.Name.Contains(q) ||
                    s.Username.Contains(normalizedQ)
                );
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.Username))
            {
                var username = NormalizeUsername(baseSearchDto.Username);
                model = model.Where(s => s.Username == username);
            }

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s =>
                    s.IsActive == baseSearchDto.Available.Value
                );
            }

            if (baseSearchDto.UserPetId.HasValue)
            {
                model = model.Where(s =>
                    s.UserPetId == baseSearchDto.UserPetId.Value
                );
            }

            if (baseSearchDto.EnergyLevelId.HasValue)
            {
                model = model.Where(s =>
                    s.EnergyLevelId ==
                    baseSearchDto.EnergyLevelId.Value
                );
            }

            if (baseSearchDto.SocialLevelId.HasValue)
            {
                model = model.Where(s =>
                    s.SocialLevelId ==
                    baseSearchDto.SocialLevelId.Value
                );
            }

            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s =>
                    s.CityId == baseSearchDto.CityId.Value
                );
            }

            if (baseSearchDto.NeighborhoodId.HasValue)
            {
                model = model.Where(s =>
                    s.NeighborhoodId ==
                    baseSearchDto.NeighborhoodId.Value
                );
            }

            if (baseSearchDto.IsVerified.HasValue)
            {
                model = model.Where(s =>
                    s.IsVerified ==
                    baseSearchDto.IsVerified.Value
                );
            }

            if (baseSearchDto.LiveLocation != null &&
                baseSearchDto.MaxDistanceInKilometers.HasValue)
            {
                var location =
                    mapper.Map<Point>(baseSearchDto.LiveLocation);

                location.SRID = 4326;

                var maxDistanceInMeters =
                    baseSearchDto.MaxDistanceInKilometers.Value * 1000;

                model = model.Where(s =>
                    s.LiveLocation != null &&
                    s.LiveLocation.Distance(location) <=
                    maxDistanceInMeters
                );
            }

            switch (baseSearchDto.SortBy)
            {
                case SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                case SortEnum.MoreVisit:
                    {
                        model = model.OrderByDescending(s => s.LikeCount);
                        break;
                    }
                case SortEnum.LessVisit:
                    {
                        model = model.OrderBy(s => s.LikeCount);
                        break;
                    }
                default:
                    {
                        model = model.OrderByDescending(
                            s => s.LastActiveDate
                        );
                        break;
                    }
            }

            var result = new PastilMatchProfileSearchDto(
                baseSearchDto,
                model,
                mapper
            );

            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin =
                _currentUser.CurrentUser.RoleEnum ==
                RoleEnum.Admin.ToString();

            if (!isAdmin && result.List != null)
            {
                foreach (var profile in result.List)
                {
                    if (profile.UserPet?.UserId != userId)
                    {
                        profile.AdminDescription = null;
                    }
                }
            }

            return result;
        }

        public override async Task<BaseResultDto<PastilMatchProfileDto>> InsertAsyncDto(PastilMatchProfileDto dto)
        {
            try
            {
                var modelChecker =
                    ModelHelper<PastilMatchProfileDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var normalizedUsername = NormalizeUsername(dto.Username);
                if (normalizedUsername != null && !IsValidUsername(normalizedUsername))
                {
                    return new BaseResultDto<PastilMatchProfileDto>(
                        false,
                        Resource.Notification.PastilMatchUsernameInvalidFormat,
                        dto
                    );
                }

                dto.Username = normalizedUsername;

                var userId = _currentUser.CurrentUser.UserId;

                var userPet = await _context.UserPets
                    .FirstOrDefaultAsync(s =>
                        s.Id == dto.UserPetId &&
                        s.UserId == userId &&
                        s.Active &&
                        !s.Deleted
                    );

                if (userPet == null)
                {
                    return new BaseResultDto<PastilMatchProfileDto>(
                        false,
                        Resource.Notification.AccessDenied,
                        dto
                    );
                }

                var profileExists =
                    await _context.PastilMatchProfiles.AnyAsync(s =>
                        s.UserPetId == dto.UserPetId &&
                        !s.Deleted
                    );

                if (profileExists)
                {
                    return new BaseResultDto<PastilMatchProfileDto>(
                        false,
                        Resource.Notification.PastilMatchProfileAlreadyExists,
                        dto
                    );
                }

                if (normalizedUsername != null && await _context.PastilMatchProfiles.AnyAsync(s =>
                        !s.Deleted &&
                        s.Username == normalizedUsername))
                {
                    return new BaseResultDto<PastilMatchProfileDto>(
                        false,
                        Resource.Notification.PastilMatchUsernameAlreadyTaken,
                        dto
                    );
                }

                var lastDeletedProfile =
                    await _context.PastilMatchProfiles
                        .Where(s =>
                            s.UserPetId == dto.UserPetId &&
                            s.Deleted
                        )
                        .OrderByDescending(s => s.DeleteDate)
                        .FirstOrDefaultAsync();

                if (lastDeletedProfile?.DeleteDate != null &&
                    lastDeletedProfile.DeleteDate.Value.AddHours(24) >
                    DateTime.Now)
                {
                    return new BaseResultDto<PastilMatchProfileDto>(
                        false,
                        Resource.Notification.PastilMatchProfileCreateCooldown,
                        dto
                    );
                }

                var item = mapper.Map<PastilMatchProfile>(dto);

                item.LikeCount = 0;
                item.IsActive = true;
                item.IsVerified = false;
                item.AdminDescription = null;
                item.VerificationDate = null;
                item.Deleted = false;
                item.DeleteDate = null;
                item.CreateDate = DateTime.Now;
                item.LastActiveDate = DateTime.Now;

                if (item.LiveLocation != null)
                {
                    item.LiveLocation.SRID = 4326;
                }

                await _context.PastilMatchProfiles.AddAsync(item);
                await _context.SaveChangesAsync();
                await _noticeService.CreateAsync(new NoticeCreateDto
                {
                    Label = NoticeTypeLabels.PastilMatchProfileSubmitted,
                    ActorUserId = userId,
                    ReferenceType = "PastilMatchProfile",
                    ReferenceId = item.Id,
                    DeduplicationKey = $"{NoticeTypeLabels.PastilMatchProfileSubmitted}:{item.Id}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "userPetId", item.UserPetId.ToString() }
                    }
                });

                return new BaseResultDto<PastilMatchProfileDto>(
                    true,
                    mapper.Map<PastilMatchProfileDto>(item)
                );
            }
            catch (DbUpdateException ex) when (
                ex.InnerException?.Message.Contains(
                    "IX_PastilMatchProfiles_Username",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                return new BaseResultDto<PastilMatchProfileDto>(
                    false,
                    Resource.Notification.PastilMatchUsernameAlreadyTaken,
                    dto
                );
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchProfileDto>(
                    isSuccess: false,
                    val: ex.Message,
                    data: dto
                );
            }
        }

        public override BaseResultDto UpdateDto(PastilMatchProfileDto dto)
        {
            try
            {
                var modelChecker =
                    ModelHelper<PastilMatchProfileDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin =
                    _currentUser.CurrentUser.RoleEnum ==
                    RoleEnum.Admin.ToString();

                var item = _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .FirstOrDefault(s =>
                        s.Id == dto.Id &&
                        !s.Deleted
                    );

                if (item == null)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.NothingFound
                    );
                }

                if (!isAdmin && item.UserPet.UserId != userId)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.AccessDenied
                    );
                }

                var normalizedUsername = dto.Username == null
                    ? item.Username
                    : NormalizeUsername(dto.Username);

                if (normalizedUsername != null && !IsValidUsername(normalizedUsername))
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.PastilMatchUsernameInvalidFormat
                    );
                }

                if (normalizedUsername != null && _context.PastilMatchProfiles.Any(s =>
                        s.Id != item.Id &&
                        !s.Deleted &&
                        s.Username == normalizedUsername))
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.PastilMatchUsernameAlreadyTaken
                    );
                }

                dto.Username = normalizedUsername;

                var userPetId = item.UserPetId;
                var likeCount = item.LikeCount;
                var isActive = item.IsActive;
                var isVerified = item.IsVerified;
                var adminDescription = item.AdminDescription;
                var verificationDate = item.VerificationDate;
                var deleted = item.Deleted;
                var deleteDate = item.DeleteDate;
                var createDate = item.CreateDate;

                mapper.Map(dto, item);

                item.UserPetId = userPetId;
                item.LikeCount = likeCount;
                item.IsActive = isActive;
                item.IsVerified = isVerified;
                item.AdminDescription = adminDescription;
                item.VerificationDate = verificationDate;
                item.Deleted = deleted;
                item.DeleteDate = deleteDate;
                item.CreateDate = createDate;
                item.LastActiveDate = DateTime.Now;

                if (item.LiveLocation != null)
                {
                    item.LiveLocation.SRID = 4326;
                }

                _context.PastilMatchProfiles.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException?.Message.Contains(
                    "IX_PastilMatchProfiles_Username",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                return new BaseResultDto(
                    false,
                    Resource.Notification.PastilMatchUsernameAlreadyTaken
                );
            }
            catch (Exception ex)
            {
                return new BaseResultDto(
                    isSuccess: false,
                    val: ex.Message
                );
            }
        }

        public BaseResultDto UpdateActiveDto(
            PastilMatchProfileActiveDto dto
        )
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin =
                    _currentUser.CurrentUser.RoleEnum ==
                    RoleEnum.Admin.ToString();

                var item = _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .FirstOrDefault(s =>
                        s.Id == dto.Id &&
                        !s.Deleted
                    );

                if (item == null)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.NothingFound
                    );
                }

                if (!isAdmin && item.UserPet.UserId != userId)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.AccessDenied
                    );
                }

                item.IsActive = dto.IsActive;
                item.LastActiveDate = DateTime.Now;

                _context.PastilMatchProfiles.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(
                    isSuccess: false,
                    val: ex.Message
                );
            }
        }

        public async Task<BaseResultDto> RequestVerificationDto(
            PastilMatchProfileVerificationRequestDto dto
        )
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var profileId = dto.PastilMatchProfileId.GetValueOrDefault();

                if (profileId <= 0)
                {
                    profileId = dto.Id;
                }

                if (profileId <= 0)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.NothingFound
                    );
                }

                var item = await _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .FirstOrDefaultAsync(s =>
                        s.Id == profileId &&
                        !s.Deleted
                    );

                if (item == null)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.NothingFound
                    );
                }

                if (item.UserPet.UserId != userId)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.AccessDenied
                    );
                }

                if (item.IsVerified == true)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.PastilMatchProfileAlreadyVerified
                    );
                }

                if (item.IsVerified == null)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.PastilMatchVerificationRequestAlreadySent
                    );
                }

                item.IsVerified = null;
                item.AdminDescription = null;
                item.VerificationDate = null;

                _context.PastilMatchProfiles.Update(item);
                await _context.SaveChangesAsync();

                try
                {
                    await _noticeService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.PastilMatchVerificationRequested,
                        ActorUserId = userId,
                        ReferenceType = "PastilMatchProfile",
                        ReferenceId = item.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.PastilMatchVerificationRequested}:{item.Id}:{DateTime.UtcNow.Ticks}",
                        Metadata = new Dictionary<string, string>
                        {
                            { "userPetId", item.UserPetId.ToString() }
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Pastil Match verification request {ProfileId} was saved, but its admin notice failed.",
                        item.Id
                    );
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(
                    isSuccess: false,
                    val: ex.Message
                );
            }
        }

        public async Task<BaseResultDto> UpdateVerificationDto(
            PastilMatchProfileVerificationDto dto
        )
        {
            try
            {
                var item = await _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.Id == dto.Id &&
                        !s.Deleted
                    );

                if (item == null)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.NothingFound
                    );
                }

                if (!dto.IsVerified &&
                    string.IsNullOrWhiteSpace(dto.AdminDescription))
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.PastilMatchAdminDescriptionRequired
                    );
                }

                var adminDescription = dto.IsVerified
                    ? null
                    : dto.AdminDescription.Trim();
                var verificationDate = DateTime.Now;

                var updatedRows = await _context.PastilMatchProfiles
                    .Where(s =>
                        s.Id == dto.Id &&
                        !s.Deleted
                    )
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            profile => profile.IsVerified,
                            (bool?)dto.IsVerified
                        )
                        .SetProperty(
                            profile => profile.AdminDescription,
                            adminDescription
                        )
                        .SetProperty(
                            profile => profile.VerificationDate,
                            verificationDate
                        ));

                if (updatedRows != 1)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.OperationFailed
                    );
                }

                try
                {
                    await _pushNotificationService.SendPushAsync(
                        dto.IsVerified
                            ? PushTypeEnum.PushPastilMatchVerificationApproved
                            : PushTypeEnum.PushPastilMatchVerificationRejected,
                        item.UserPet.UserId,
                        item.UserPet.Name,
                        adminDescription);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Pastil Match profile {ProfileId} verification was saved, but its push notification failed.",
                        item.Id
                    );
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(
                    isSuccess: false,
                    val: ex.Message
                );
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin =
                    _currentUser.CurrentUser.RoleEnum ==
                    RoleEnum.Admin.ToString();

                var item = _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .FirstOrDefault(s =>
                        s.Id == id &&
                        !s.Deleted
                    );

                if (item == null)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.NothingFound
                    );
                }

                if (!isAdmin && item.UserPet.UserId != userId)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.AccessDenied
                    );
                }

                item.Deleted = true;
                item.IsActive = false;
                item.DeleteDate = DateTime.Now;

                _context.PastilMatchProfiles.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(
                    isSuccess: false,
                    val: ex.Message
                );
            }
        }

        public override BaseResultDto DeleteDto(
            PastilMatchProfileDto dto
        )
        {
            return DeleteDto(dto.Id);
        }
    }
}
