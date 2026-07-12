using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv
{
    public class PastilMatchProfileService : CommonSrv<PastilMatchProfile, PastilMatchProfileDto>, IPastilMatchProfileService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchProfileService(
            IDataBaseContext _context,
            IMapper mapper,
            ICurrentUserHelper currentUser
        ) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
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

                model = model.Where(s =>
                    s.Description.Contains(q) ||
                    s.UserPet.Name.Contains(q)
                );
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

                return new BaseResultDto<PastilMatchProfileDto>(
                    true,
                    mapper.Map<PastilMatchProfileDto>(item)
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

        public BaseResultDto RequestVerificationDto(
            PastilMatchProfileVerificationRequestDto dto
        )
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;

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

        public BaseResultDto UpdateVerificationDto(
            PastilMatchProfileVerificationDto dto
        )
        {
            try
            {
                var isAdmin =
                    _currentUser.CurrentUser.RoleEnum ==
                    RoleEnum.Admin.ToString();

                if (!isAdmin)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.AccessDenied
                    );
                }

                var item = _context.PastilMatchProfiles
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

                if (item.IsVerified != null)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.PastilMatchVerificationRequestNotPending
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

                item.IsVerified = dto.IsVerified;
                item.AdminDescription = dto.IsVerified
                    ? null
                    : dto.AdminDescription;
                item.VerificationDate = DateTime.Now;

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