using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface;
using Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv
{
    public class PastilMatchRequestService : CommonSrv<PastilMatchRequest, PastilMatchRequestDto>, IPastilMatchRequestService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchRequestService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<PastilMatchRequestVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = await GetRequestQuery().FirstOrDefaultAsync(s => s.Id == id);

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchRequestVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && item.SenderProfile.UserPet.UserId != userId && item.ReceiverProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchRequestVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<PastilMatchRequestVDto>(true, mapper.Map<PastilMatchRequestVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchRequestVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchRequestSearchDto Search(PastilMatchRequestInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

            var model = GetRequestQuery();

            if (!isAdmin)
            {
                model = model.Where(s => s.SenderProfile.UserPet.UserId == userId || s.ReceiverProfile.UserPet.UserId == userId);
            }

            if (dto.SenderProfileId.HasValue)
            {
                model = model.Where(s => s.SenderProfileId == dto.SenderProfileId.Value);
            }

            if (dto.ReceiverProfileId.HasValue)
            {
                model = model.Where(s => s.ReceiverProfileId == dto.ReceiverProfileId.Value);
            }

            if (dto.PastilMatchGoalId.HasValue)
            {
                model = model.Where(s => s.PastilMatchGoalId == dto.PastilMatchGoalId.Value);
            }

            if (dto.StatusId.HasValue)
            {
                model = model.Where(s => s.StatusId == dto.StatusId.Value);
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

            return new PastilMatchRequestSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<PastilMatchRequestDto>> InsertAsyncDto(PastilMatchRequestDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<PastilMatchRequestDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;

                if (dto.SenderProfileId == dto.ReceiverProfileId)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.PastilMatchRequestCannotSendToItself, dto);
                }

                if (!Enum.IsDefined(typeof(PastilMatchGoalEnum), (int)dto.PastilMatchGoalId))
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.InvalidPastilMatchGoal, dto);
                }

                var senderProfile = await _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .Include(s => s.PastilMatchProfileGoals.Where(goal => !goal.Deleted))
                    .FirstOrDefaultAsync(s =>
                        s.Id == dto.SenderProfileId &&
                        s.IsActive &&
                        !s.Deleted &&
                        s.UserPet.Active &&
                        !s.UserPet.Deleted);

                if (senderProfile == null)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (senderProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var receiverProfile = await _context.PastilMatchProfiles
                    .Include(s => s.UserPet)
                    .Include(s => s.PastilMatchProfileGoals.Where(goal => !goal.Deleted))
                    .FirstOrDefaultAsync(s =>
                        s.Id == dto.ReceiverProfileId &&
                        s.IsActive &&
                        !s.Deleted &&
                        s.UserPet.Active &&
                        !s.UserPet.Deleted);

                if (receiverProfile == null)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (receiverProfile.UserPet.UserId == userId)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.PastilMatchRequestCannotSendToItself, dto);
                }

                var usersAreBlocked = await _context.PastilMatchBlocks.AnyAsync(block =>
                    !block.Deleted &&
                    ((block.BlockerUserId == userId &&
                      block.BlockedUserId == receiverProfile.UserPet.UserId) ||
                     (block.BlockerUserId == receiverProfile.UserPet.UserId &&
                      block.BlockedUserId == userId)));

                if (usersAreBlocked)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var senderHasGoal = await _context.PastilMatchProfileGoals.AnyAsync(s => s.PastilMatchProfileId == senderProfile.Id && s.PastilMatchGoalId == dto.PastilMatchGoalId && !s.Deleted);

                if (!senderHasGoal)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.PastilMatchGoalNotSelected, dto);
                }

                var receiverHasGoal = receiverProfile.PastilMatchProfileGoals.Any(goal =>
                    !goal.Deleted &&
                    goal.PastilMatchGoalId == dto.PastilMatchGoalId);

                if (!receiverHasGoal)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.PastilMatchGoalNotSelected, dto);
                }

                var pendingRequestExists = await _context.PastilMatchRequests.AnyAsync(s =>
                    s.StatusId == (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Pending &&
                    ((s.SenderProfileId == dto.SenderProfileId && s.ReceiverProfileId == dto.ReceiverProfileId) ||
                     (s.SenderProfileId == dto.ReceiverProfileId && s.ReceiverProfileId == dto.SenderProfileId)));

                if (pendingRequestExists)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.PastilMatchPendingRequestAlreadyExists, dto);
                }

                var activeMatchExists = await _context.PastilMatches.AnyAsync(s =>
                    s.StatusId == (long)PastilMatchStatusEnum.PastilMatchStatus_Active &&
                    ((s.FirstProfileId == dto.SenderProfileId && s.SecondProfileId == dto.ReceiverProfileId) ||
                     (s.FirstProfileId == dto.ReceiverProfileId && s.SecondProfileId == dto.SenderProfileId)));

                if (activeMatchExists)
                {
                    return new BaseResultDto<PastilMatchRequestDto>(false, Resource.Notification.PastilMatchAlreadyExists, dto);
                }

                var item = mapper.Map<PastilMatchRequest>(dto);

                item.StatusId = (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Pending;
                item.CompatibilityPercent = CalculateCompatibilityPercent(senderProfile, receiverProfile);
                item.ResponseDate = null;
                item.CancelDate = null;
                item.CreateDate = DateTime.Now;

                await _context.PastilMatchRequests.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<PastilMatchRequestDto>(true, mapper.Map<PastilMatchRequestDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchRequestDto>(false, ex.Message, dto);
            }
        }

        public async Task<BaseResultDto> UpdateResponseDto(PastilMatchRequestResponseDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var acceptedStatusId = (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Accepted;
                var rejectedStatusId = (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Rejected;

                if (dto.StatusId != acceptedStatusId && dto.StatusId != rejectedStatusId)
                {
                    return new BaseResultDto(false, Resource.Notification.InvalidPastilMatchRequestStatus);
                }

                var item = await _context.PastilMatchRequests.Include(s => s.ReceiverProfile).ThenInclude(s => s.UserPet).FirstOrDefaultAsync(s => s.Id == dto.Id);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (item.ReceiverProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (item.StatusId != (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Pending)
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchRequestNotPending);
                }

                item.StatusId = dto.StatusId;
                item.ResponseDate = DateTime.Now;

                if (dto.StatusId == acceptedStatusId)
                {
                    var matchExists = await _context.PastilMatches.AnyAsync(s =>
                        (s.FirstProfileId == item.SenderProfileId && s.SecondProfileId == item.ReceiverProfileId) ||
                        (s.FirstProfileId == item.ReceiverProfileId && s.SecondProfileId == item.SenderProfileId));

                    if (matchExists)
                    {
                        return new BaseResultDto(false, Resource.Notification.PastilMatchAlreadyExists);
                    }

                    var pastilMatch = new PastilMatch
                    {
                        PastilMatchRequestId = item.Id,
                        FirstProfileId = item.SenderProfileId,
                        SecondProfileId = item.ReceiverProfileId,
                        PastilMatchGoalId = item.PastilMatchGoalId,
                        StatusId = (long)PastilMatchStatusEnum.PastilMatchStatus_Active,
                        CompatibilityPercent = item.CompatibilityPercent,
                        CloseDate = null,
                        CreateDate = DateTime.Now
                    };

                    await _context.PastilMatches.AddAsync(pastilMatch);
                }

                _context.PastilMatchRequests.Update(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;

                var item = _context.PastilMatchRequests.Include(s => s.SenderProfile).ThenInclude(s => s.UserPet).FirstOrDefault(s => s.Id == id);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (item.SenderProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (item.StatusId != (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Pending)
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchRequestNotPending);
                }

                item.StatusId = (long)PastilMatchRequestStatusEnum.PastilMatchRequestStatus_Cancelled;
                item.CancelDate = DateTime.Now;

                _context.PastilMatchRequests.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchRequestDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private int CalculateCompatibilityPercent(PastilMatchProfile senderProfile, PastilMatchProfile receiverProfile)
        {
            var score = PastilMatchCompatibilityCalculator.Calculate(
                senderProfile.UserPet.Birthday,
                receiverProfile.UserPet.Birthday,
                senderProfile.UserPet.PetId,
                receiverProfile.UserPet.PetId,
                GetBreedIds(senderProfile),
                GetBreedIds(receiverProfile),
                senderProfile.PastilMatchProfileGoals
                    .Where(goal => !goal.Deleted)
                    .Select(goal => goal.PastilMatchGoalId),
                receiverProfile.PastilMatchProfileGoals
                    .Where(goal => !goal.Deleted)
                    .Select(goal => goal.PastilMatchGoalId),
                senderProfile.EnergyLevelId,
                receiverProfile.EnergyLevelId,
                senderProfile.SocialLevelId,
                receiverProfile.SocialLevelId,
                senderProfile.LiveLocation?.X,
                senderProfile.LiveLocation?.Y,
                receiverProfile.LiveLocation?.X,
                receiverProfile.LiveLocation?.Y
            );

            return score.TotalPercent;
        }

        private static IEnumerable<long> GetBreedIds(PastilMatchProfile profile)
        {
            if (profile.UserPet.PetBreedId.HasValue)
            {
                yield return profile.UserPet.PetBreedId.Value;
            }

            if (profile.UserPet.PetBreed2Id.HasValue)
            {
                yield return profile.UserPet.PetBreed2Id.Value;
            }
        }

        private IQueryable<PastilMatchRequest> GetRequestQuery()
        {
            return _context.PastilMatchRequests
                .Include(s => s.SenderProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.SenderProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Picture)
                .Include(s => s.SenderProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Pet)
                .Include(s => s.SenderProfile).ThenInclude(s => s.EnergyLevel)
                .Include(s => s.SenderProfile).ThenInclude(s => s.SocialLevel)
                .Include(s => s.SenderProfile).ThenInclude(s => s.City)
                .Include(s => s.SenderProfile).ThenInclude(s => s.Neighborhood)
                .Include(s => s.SenderProfile).ThenInclude(s => s.PastilMatchProfileGoals).ThenInclude(s => s.PastilMatchGoal)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Picture)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Pet)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.EnergyLevel)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.SocialLevel)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.City)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.Neighborhood)
                .Include(s => s.ReceiverProfile).ThenInclude(s => s.PastilMatchProfileGoals).ThenInclude(s => s.PastilMatchGoal)
                .Include(s => s.PastilMatchGoal)
                .Include(s => s.Status)
                .AsQueryable();
        }
    }
}
