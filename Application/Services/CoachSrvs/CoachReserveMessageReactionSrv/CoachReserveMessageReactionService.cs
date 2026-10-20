using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveMessageReactionSrv
{
    public class CoachReserveMessageReactionService : CommonSrv<CoachReserveMessageReaction, CoachReserveMessageReactionDto>, ICoachReserveMessageReactionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public CoachReserveMessageReactionService(
            IDataBaseContext context,
            IMapper mapper,
            ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<CoachReserveMessageReactionVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetReactionQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && !s.CoachReserveMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<CoachReserveMessageReactionVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.CoachReserveMessage.CoachReserve, userId))
                {
                    return new BaseResultDto<CoachReserveMessageReactionVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<CoachReserveMessageReactionVDto>(true, mapper.Map<CoachReserveMessageReactionVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CoachReserveMessageReactionVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public CoachReserveMessageReactionSearchDto Search(CoachReserveMessageReactionInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetReactionQuery().Where(s => !s.Deleted && !s.CoachReserveMessage.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s =>
                    s.CoachReserveMessage.CoachReserve.BookerId == userId ||
                    s.CoachReserveMessage.CoachReserve.Coach.OwnerId == userId ||
                    s.CoachReserveMessage.CoachReserve.AssigneeUserId == userId);
            }

            if (dto.CoachReserveMessageId.HasValue)
            {
                model = model.Where(s => s.CoachReserveMessageId == dto.CoachReserveMessageId.Value);
            }

            if (dto.ReactorUserId.HasValue)
            {
                model = model.Where(s => s.ReactorUserId == dto.ReactorUserId.Value);
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

            return new CoachReserveMessageReactionSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<CoachReserveMessageReactionDto>> InsertAsyncDto(CoachReserveMessageReactionDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<CoachReserveMessageReactionDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Reaction))
                {
                    return new BaseResultDto<CoachReserveMessageReactionDto>(false, Resource.Notification.CoachReserveMessageReactionRequired, dto);
                }

                var reactionValue = dto.Reaction.Trim();

                if (reactionValue.Length > 32)
                {
                    return new BaseResultDto<CoachReserveMessageReactionDto>(false, Resource.Notification.CoachReserveMessageReactionTooLong, dto);
                }

                var message = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.CoachReserveMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<CoachReserveMessageReactionDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (!isAdmin && !IsParticipant(message.CoachReserve, userId))
                {
                    return new BaseResultDto<CoachReserveMessageReactionDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var item = await _context.CoachReserveMessageReactions.FirstOrDefaultAsync(s => s.CoachReserveMessageId == dto.CoachReserveMessageId && s.ReactorUserId == userId);

                if (item != null && !item.Deleted && item.Reaction == reactionValue)
                {
                    return new BaseResultDto<CoachReserveMessageReactionDto>(true, mapper.Map<CoachReserveMessageReactionDto>(item));
                }

                if (item != null)
                {
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    _context.CoachReserveMessageReactions.Update(item);
                }
                else
                {
                    item = mapper.Map<CoachReserveMessageReaction>(dto);
                    item.ReactorUserId = userId;
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    await _context.CoachReserveMessageReactions.AddAsync(item);
                }

                await _context.SaveChangesAsync();

                // پوش اختصاصی برای ری‌اکشن نداریم (برخلاف پاستیل‌مچ) - سمت کلاینت با همون Polling
                // ۴۵۰۰ms سینک می‌شه؛ فقط پیام جدید Push می‌گیره، نه ری‌اکشن.
                return new BaseResultDto<CoachReserveMessageReactionDto>(true, mapper.Map<CoachReserveMessageReactionDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CoachReserveMessageReactionDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.CoachReserveMessageReactions.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && item.ReactorUserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                _context.CoachReserveMessageReactions.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(CoachReserveMessageReactionDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private static bool IsParticipant(CoachReserve reserve, long userId)
        {
            return reserve.BookerId == userId ||
                   reserve.Coach.OwnerId == userId ||
                   reserve.AssigneeUserId == userId;
        }

        private IQueryable<CoachReserveMessage> GetMessageQuery()
        {
            return _context.CoachReserveMessages
                .Include(s => s.CoachReserve).ThenInclude(r => r.Coach)
                .AsQueryable();
        }

        private IQueryable<CoachReserveMessageReaction> GetReactionQuery()
        {
            return _context.CoachReserveMessageReactions
                .Include(s => s.CoachReserveMessage).ThenInclude(s => s.CoachReserve).ThenInclude(r => r.Coach)
                .AsQueryable();
        }
    }
}
