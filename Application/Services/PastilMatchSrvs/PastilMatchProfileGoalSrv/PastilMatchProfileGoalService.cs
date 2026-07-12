using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionAssistanceTypeSrv.Iface;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv
{
    public class PastilMatchProfileGoalService : CommonSrv<PastilMatchProfileGoal, PastilMatchProfileGoalDto>, IPastilMatchProfileGoalService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchProfileGoalService(IDataBaseContext _context, IMapper mapper, ICurrentUserHelper currentUser) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
        }

        public async Task<BaseResultDto<PastilMatchProfileGoalVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PastilMatchProfileGoals
                .Include(s => s.PastilMatchGoal)
                .FirstOrDefaultAsync(s =>
                    s.Id == id &&
                    !s.Deleted
                );

            if (item == null)
            {
                return new BaseResultDto<PastilMatchProfileGoalVDto>(
                    false,
                    Resource.Notification.NothingFound,
                    null
                );
            }

            return new BaseResultDto<PastilMatchProfileGoalVDto>(
                true,
                mapper.Map<PastilMatchProfileGoalVDto>(item)
            );
        }

        public PastilMatchProfileGoalSearchDto Search(PastilMatchProfileGoalInputDto baseSearchDto)
        {
            var model = _context.PastilMatchProfileGoals
                .Include(s => s.PastilMatchGoal)
                .Where(s => !s.Deleted)
                .AsQueryable();

            if (baseSearchDto.PastilMatchProfileId.HasValue)
            {
                model = model.Where(s =>
                    s.PastilMatchProfileId ==
                    baseSearchDto.PastilMatchProfileId.Value
                );
            }

            if (baseSearchDto.PastilMatchGoalId.HasValue)
            {
                model = model.Where(s =>
                    s.PastilMatchGoalId ==
                    baseSearchDto.PastilMatchGoalId.Value
                );
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
            {
                var q = baseSearchDto.Q.Trim();

                model = model.Where(s =>
                    s.PastilMatchGoal.Name.Contains(q) ||
                    s.PastilMatchGoal.Label.Contains(q)
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
                default:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
            }

            return new PastilMatchProfileGoalSearchDto(
                baseSearchDto,
                model,
                mapper
            );
        }

        public override async Task<BaseResultDto<PastilMatchProfileGoalDto>> InsertAsyncDto(PastilMatchProfileGoalDto dto)
        {
            try
            {
                var modelChecker =
                    ModelHelper<PastilMatchProfileGoalDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                if (!Enum.IsDefined(
                        typeof(PastilMatchGoalEnum),
                        (int)dto.PastilMatchGoalId
                    ))
                {
                    return new BaseResultDto<PastilMatchProfileGoalDto>(
                        false,
                        Resource.Notification.InvalidPastilMatchGoal,
                        dto
                    );
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin =
                    _currentUser.CurrentUser.RoleEnum ==
                    RoleEnum.Admin.ToString();

                var profile = await _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .FirstOrDefaultAsync(s =>
                        s.Id == dto.PastilMatchProfileId &&
                        !s.Deleted
                    );

                if (profile == null)
                {
                    return new BaseResultDto<PastilMatchProfileGoalDto>(
                        false,
                        Resource.Notification.NothingFound,
                        dto
                    );
                }

                if (!isAdmin && profile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchProfileGoalDto>(
                        false,
                        Resource.Notification.AccessDenied,
                        dto
                    );
                }

                var currentItem =
                    await _context.PastilMatchProfileGoals.FirstOrDefaultAsync(s =>
                        s.PastilMatchProfileId == dto.PastilMatchProfileId &&
                        s.PastilMatchGoalId == dto.PastilMatchGoalId
                    );

                if (currentItem != null && !currentItem.Deleted)
                {
                    return new BaseResultDto<PastilMatchProfileGoalDto>(
                        false,
                        Resource.Notification.PastilMatchProfileGoalAlreadyExists,
                        dto
                    );
                }

                if (currentItem != null)
                {
                    var id = currentItem.Id;

                    mapper.Map(dto, currentItem);

                    currentItem.Id = id;
                    currentItem.Deleted = false;

                    _context.PastilMatchProfileGoals.Update(currentItem);
                    await _context.SaveChangesAsync();

                    return new BaseResultDto<PastilMatchProfileGoalDto>(
                        true,
                        mapper.Map<PastilMatchProfileGoalDto>(currentItem)
                    );
                }

                var item = mapper.Map<PastilMatchProfileGoal>(dto);
                item.Deleted = false;

                await _context.PastilMatchProfileGoals.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<PastilMatchProfileGoalDto>(
                    true,
                    mapper.Map<PastilMatchProfileGoalDto>(item)
                );
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchProfileGoalDto>(
                    isSuccess: false,
                    val: ex.Message,
                    data: dto
                );
            }
        }

        public override BaseResultDto UpdateDto(PastilMatchProfileGoalDto dto)
        {
            try
            {
                var modelChecker =
                    ModelHelper<PastilMatchProfileGoalDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                if (!Enum.IsDefined(
                        typeof(PastilMatchGoalEnum),
                        (int)dto.PastilMatchGoalId
                    ))
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.InvalidPastilMatchGoal
                    );
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin =
                    _currentUser.CurrentUser.RoleEnum ==
                    RoleEnum.Admin.ToString();

                var item = _context.PastilMatchProfileGoals
                    .Include(s => s.PastilMatchProfile)
                        .ThenInclude(s => s.UserPet)
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

                if (!isAdmin &&
                    item.PastilMatchProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.AccessDenied
                    );
                }

                var duplicateExists =
                    _context.PastilMatchProfileGoals.Any(s =>
                        s.Id != item.Id &&
                        s.PastilMatchProfileId == item.PastilMatchProfileId &&
                        s.PastilMatchGoalId == dto.PastilMatchGoalId &&
                        !s.Deleted
                    );

                if (duplicateExists)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.PastilMatchProfileGoalAlreadyExists
                    );
                }

                var profileId = item.PastilMatchProfileId;
                var deleted = item.Deleted;

                mapper.Map(dto, item);

                item.PastilMatchProfileId = profileId;
                item.Deleted = deleted;

                _context.PastilMatchProfileGoals.Update(item);
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

                var item = _context.PastilMatchProfileGoals
                    .Include(s => s.PastilMatchProfile)
                        .ThenInclude(s => s.UserPet)
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

                if (!isAdmin &&
                    item.PastilMatchProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto(
                        false,
                        Resource.Notification.AccessDenied
                    );
                }

                item.Deleted = true;

                _context.PastilMatchProfileGoals.Update(item);
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

        public override BaseResultDto DeleteDto(PastilMatchProfileGoalDto dto)
        {
            return DeleteDto(dto.Id);
        }
    }
}
