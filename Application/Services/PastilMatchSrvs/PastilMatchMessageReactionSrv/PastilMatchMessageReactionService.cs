using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv
{
    public class PastilMatchMessageReactionService : CommonSrv<PastilMatchMessageReaction, PastilMatchMessageReactionDto>, IPastilMatchMessageReactionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;

        public PastilMatchMessageReactionService(
            IDataBaseContext context,
            IMapper mapper,
            ICurrentUserHelper currentUser,
            IPushNotificationService pushNotificationService) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
            _pushNotificationService = pushNotificationService;
        }

        public async Task<BaseResultDto<PastilMatchMessageReactionVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetReactionQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && !s.PastilMatchMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchMessageReactionVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsMatchParticipant(item.PastilMatchMessage.PastilMatch, userId))
                {
                    return new BaseResultDto<PastilMatchMessageReactionVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<PastilMatchMessageReactionVDto>(true, mapper.Map<PastilMatchMessageReactionVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchMessageReactionVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchMessageReactionSearchDto Search(PastilMatchMessageReactionInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetReactionQuery().Where(s => !s.Deleted && !s.PastilMatchMessage.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s => s.PastilMatchMessage.PastilMatch.FirstProfile.UserPet.UserId == userId || s.PastilMatchMessage.PastilMatch.SecondProfile.UserPet.UserId == userId);
            }

            if (dto.PastilMatchMessageId.HasValue)
            {
                model = model.Where(s => s.PastilMatchMessageId == dto.PastilMatchMessageId.Value);
            }

            if (dto.ReactorProfileId.HasValue)
            {
                model = model.Where(s => s.ReactorProfileId == dto.ReactorProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.Reaction))
            {
                model = model.Where(s => s.Reaction == dto.Reaction);
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

            return new PastilMatchMessageReactionSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<PastilMatchMessageReactionDto>> InsertAsyncDto(PastilMatchMessageReactionDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<PastilMatchMessageReactionDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Reaction))
                {
                    return new BaseResultDto<PastilMatchMessageReactionDto>(false, Resource.Notification.PastilMatchMessageReactionRequired, dto);
                }

                var reactionValue = dto.Reaction.Trim();

                if (reactionValue.Length > 32)
                {
                    return new BaseResultDto<PastilMatchMessageReactionDto>(false, Resource.Notification.PastilMatchMessageReactionTooLong, dto);
                }

                var message = await _context.PastilMatchMessages
                    .Include(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                    .Include(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == dto.PastilMatchMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<PastilMatchMessageReactionDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (message.PastilMatch.StatusId != (long)PastilMatchStatusEnum.PastilMatchStatus_Active)
                {
                    return new BaseResultDto<PastilMatchMessageReactionDto>(false, Resource.Notification.PastilMatchNotActive, dto);
                }

                long reactorProfileId;

                if (message.PastilMatch.FirstProfile.UserPet.UserId == userId)
                {
                    reactorProfileId = message.PastilMatch.FirstProfileId;
                }
                else if (message.PastilMatch.SecondProfile.UserPet.UserId == userId)
                {
                    reactorProfileId = message.PastilMatch.SecondProfileId;
                }
                else if (isAdmin)
                {
                    reactorProfileId = message.SenderProfileId.HasValue
                        ? (message.SenderProfileId == message.PastilMatch.FirstProfileId
                            ? message.PastilMatch.SecondProfileId
                            : message.PastilMatch.FirstProfileId)
                        : message.PastilMatch.FirstProfileId;
                }
                else
                {
                    return new BaseResultDto<PastilMatchMessageReactionDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var item = await _context.PastilMatchMessageReactions.FirstOrDefaultAsync(s => s.PastilMatchMessageId == dto.PastilMatchMessageId && s.ReactorProfileId == reactorProfileId);

                if (item != null && !item.Deleted && item.Reaction == reactionValue)
                {
                    return new BaseResultDto<PastilMatchMessageReactionDto>(true, mapper.Map<PastilMatchMessageReactionDto>(item));
                }

                if (item != null)
                {
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    _context.PastilMatchMessageReactions.Update(item);
                }
                else
                {
                    item = mapper.Map<PastilMatchMessageReaction>(dto);
                    item.ReactorProfileId = reactorProfileId;
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    await _context.PastilMatchMessageReactions.AddAsync(item);
                }

                await _context.SaveChangesAsync();

                if (message.SenderProfileId.HasValue)
                {
                    var reactorProfile = reactorProfileId == message.PastilMatch.FirstProfileId
                        ? message.PastilMatch.FirstProfile
                        : message.PastilMatch.SecondProfile;
                    var messageSenderProfile = message.SenderProfileId == message.PastilMatch.FirstProfileId
                        ? message.PastilMatch.FirstProfile
                        : message.PastilMatch.SecondProfile;

                    if (messageSenderProfile.UserPet.UserId != userId)
                    {
                        await _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushPastilMatchMessageReaction,
                            messageSenderProfile.UserPet.UserId,
                            reactorProfile.UserPet.User.FirstName,
                            reactionValue);
                    }
                }

                return new BaseResultDto<PastilMatchMessageReactionDto>(true, mapper.Map<PastilMatchMessageReactionDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchMessageReactionDto>(false, ex.Message, dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.PastilMatchMessageReactions.Include(s => s.ReactorProfile).ThenInclude(s => s.UserPet).FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && item.ReactorProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                _context.PastilMatchMessageReactions.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchMessageReactionDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private bool IsMatchParticipant(PastilMatch pastilMatch, long userId)
        {
            return pastilMatch.FirstProfile.UserPet.UserId == userId || pastilMatch.SecondProfile.UserPet.UserId == userId;
        }

        private IQueryable<PastilMatchMessageReaction> GetReactionQuery()
        {
            return _context.PastilMatchMessageReactions
                .Include(s => s.ReactorProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatchMessage).ThenInclude(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatchMessage).ThenInclude(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                .AsQueryable();
        }
    }
}
