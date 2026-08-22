using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchBlockSrv
{
    public class PastilMatchBlockService : CommonSrv<PastilMatchBlock, PastilMatchBlockDto>, IPastilMatchBlockService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchBlockService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<PastilMatchBlockVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = await GetBlockQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchBlockVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && item.BlockerUserId != userId)
                {
                    return new BaseResultDto<PastilMatchBlockVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<PastilMatchBlockVDto>(true, mapper.Map<PastilMatchBlockVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchBlockVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchBlockSearchDto Search(PastilMatchBlockInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

            var model = GetBlockQuery().Where(s => !s.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s => s.BlockerUserId == userId);
            }

            if (dto.BlockerUserId.HasValue)
            {
                model = model.Where(s => s.BlockerUserId == dto.BlockerUserId.Value);
            }

            if (dto.BlockedUserId.HasValue)
            {
                model = model.Where(s => s.BlockedUserId == dto.BlockedUserId.Value);
            }

            if (dto.PastilMatchId.HasValue)
            {
                model = model.Where(s => s.PastilMatchId == dto.PastilMatchId.Value);
            }

            switch (dto.SortBy)
            {
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
            }

            return new PastilMatchBlockSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<PastilMatchBlockDto>> InsertAsyncDto(PastilMatchBlockDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<PastilMatchBlockDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;

                if (dto.BlockedUserId == userId)
                {
                    return new BaseResultDto<PastilMatchBlockDto>(false, Resource.Notification.PastilMatchCannotBlockYourself, dto);
                }

                var blockedUserExists = await _context.Users.AnyAsync(s => s.Id == dto.BlockedUserId);

                if (!blockedUserExists)
                {
                    return new BaseResultDto<PastilMatchBlockDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (dto.PastilMatchId.HasValue)
                {
                    var pastilMatch = await _context.PastilMatches.Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).FirstOrDefaultAsync(s => s.Id == dto.PastilMatchId.Value);

                    if (pastilMatch == null)
                    {
                        return new BaseResultDto<PastilMatchBlockDto>(false, Resource.Notification.NothingFound, dto);
                    }

                    var firstUserId = pastilMatch.FirstProfile.UserPet.UserId;
                    var secondUserId = pastilMatch.SecondProfile.UserPet.UserId;
                    var currentUserIsParticipant = firstUserId == userId || secondUserId == userId;
                    var blockedUserIsOtherParticipant = firstUserId == dto.BlockedUserId || secondUserId == dto.BlockedUserId;

                    if (!currentUserIsParticipant || !blockedUserIsOtherParticipant)
                    {
                        return new BaseResultDto<PastilMatchBlockDto>(false, Resource.Notification.AccessDenied, dto);
                    }
                }

                var item = await _context.PastilMatchBlocks.FirstOrDefaultAsync(s => s.BlockerUserId == userId && s.BlockedUserId == dto.BlockedUserId);

                if (item != null && !item.Deleted)
                {
                    await BlockActiveMatchesAsync(userId, dto.BlockedUserId);
                    return new BaseResultDto<PastilMatchBlockDto>(true, mapper.Map<PastilMatchBlockDto>(item));
                }

                if (item != null)
                {
                    item.Deleted = false;
                    item.PastilMatchId = dto.PastilMatchId;
                    item.CreateDate = DateTime.Now;

                    _context.PastilMatchBlocks.Update(item);
                }
                else
                {
                    item = mapper.Map<PastilMatchBlock>(dto);
                    item.BlockerUserId = userId;
                    item.Deleted = false;
                    item.CreateDate = DateTime.Now;

                    await _context.PastilMatchBlocks.AddAsync(item);
                }

                await BlockActiveMatchesAsync(userId, dto.BlockedUserId);
                await _context.SaveChangesAsync();

                return new BaseResultDto<PastilMatchBlockDto>(true, mapper.Map<PastilMatchBlockDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchBlockDto>(false, ex.Message, dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            using var transaction = _context.BeginTransaction();

            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = _context.PastilMatchBlocks.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && item.BlockerUserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;
                _context.PastilMatchBlocks.Update(item);

                var activeBlockExists = _context.PastilMatchBlocks.Any(block =>
                    !block.Deleted &&
                    block.Id != item.Id &&
                    ((block.BlockerUserId == item.BlockerUserId && block.BlockedUserId == item.BlockedUserId) ||
                     (block.BlockerUserId == item.BlockedUserId && block.BlockedUserId == item.BlockerUserId)));

                if (!activeBlockExists)
                {
                    var blockedStatusId = (long)PastilMatchStatusEnum.PastilMatchStatus_Blocked;
                    var activeStatusId = (long)PastilMatchStatusEnum.PastilMatchStatus_Active;
                    var blockedMatches = _context.PastilMatches
                        .Include(match => match.FirstProfile).ThenInclude(profile => profile.UserPet)
                        .Include(match => match.SecondProfile).ThenInclude(profile => profile.UserPet)
                        .Where(match => match.StatusId == blockedStatusId &&
                            ((match.FirstProfile.UserPet.UserId == item.BlockerUserId &&
                              match.SecondProfile.UserPet.UserId == item.BlockedUserId) ||
                             (match.FirstProfile.UserPet.UserId == item.BlockedUserId &&
                              match.SecondProfile.UserPet.UserId == item.BlockerUserId)))
                        .ToList();

                    foreach (var blockedMatch in blockedMatches)
                    {
                        blockedMatch.StatusId = activeStatusId;
                        blockedMatch.CloseDate = null;
                    }

                    if (blockedMatches.Any())
                        _context.PastilMatches.UpdateRange(blockedMatches);
                }

                _context.SaveChanges();
                transaction.Commit();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchBlockDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private async Task BlockActiveMatchesAsync(long blockerUserId, long blockedUserId)
        {
            var activeStatusId = (long)PastilMatchStatusEnum.PastilMatchStatus_Active;
            var blockedStatusId = (long)PastilMatchStatusEnum.PastilMatchStatus_Blocked;

            var matches = await _context.PastilMatches
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                .Where(s => s.StatusId == activeStatusId &&
                    ((s.FirstProfile.UserPet.UserId == blockerUserId && s.SecondProfile.UserPet.UserId == blockedUserId) ||
                     (s.FirstProfile.UserPet.UserId == blockedUserId && s.SecondProfile.UserPet.UserId == blockerUserId)))
                .ToListAsync();

            foreach (var item in matches)
            {
                item.StatusId = blockedStatusId;
                item.CloseDate = DateTime.Now;
            }

            if (matches.Any())
            {
                _context.PastilMatches.UpdateRange(matches);
            }
        }

        private IQueryable<PastilMatchBlock> GetBlockQuery()
        {
            return _context.PastilMatchBlocks.Include(s => s.BlockerUser).Include(s => s.BlockedUser).Include(s => s.PastilMatch).AsQueryable();
        }
    }
}
