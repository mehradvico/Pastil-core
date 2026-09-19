using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv
{
    public class CompanionReserveMessageReactionService : CommonSrv<CompanionReserveMessageReaction, CompanionReserveMessageReactionDto>, ICompanionReserveMessageReactionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public CompanionReserveMessageReactionService(
            IDataBaseContext context,
            IMapper mapper,
            ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<CompanionReserveMessageReactionVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetReactionQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && !s.CompanionReserveMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<CompanionReserveMessageReactionVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.CompanionReserveMessage.CompanionReserve, userId))
                {
                    return new BaseResultDto<CompanionReserveMessageReactionVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<CompanionReserveMessageReactionVDto>(true, mapper.Map<CompanionReserveMessageReactionVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveMessageReactionVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public CompanionReserveMessageReactionSearchDto Search(CompanionReserveMessageReactionInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetReactionQuery().Where(s => !s.Deleted && !s.CompanionReserveMessage.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s =>
                    s.CompanionReserveMessage.CompanionReserve.BookerId == userId ||
                    s.CompanionReserveMessage.CompanionReserve.CompanionAssistance.Companion.OwnerId == userId ||
                    (s.CompanionReserveMessage.CompanionReserve.CompanionAssistanceUserId.HasValue && s.CompanionReserveMessage.CompanionReserve.CompanionAssistanceUser.UserId == userId));
            }

            if (dto.CompanionReserveMessageId.HasValue)
            {
                model = model.Where(s => s.CompanionReserveMessageId == dto.CompanionReserveMessageId.Value);
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

            return new CompanionReserveMessageReactionSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionReserveMessageReactionDto>> InsertAsyncDto(CompanionReserveMessageReactionDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<CompanionReserveMessageReactionDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Reaction))
                {
                    return new BaseResultDto<CompanionReserveMessageReactionDto>(false, Resource.Notification.CompanionReserveMessageReactionRequired, dto);
                }

                var reactionValue = dto.Reaction.Trim();

                if (reactionValue.Length > 32)
                {
                    return new BaseResultDto<CompanionReserveMessageReactionDto>(false, Resource.Notification.CompanionReserveMessageReactionTooLong, dto);
                }

                var message = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.CompanionReserveMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<CompanionReserveMessageReactionDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (!isAdmin && !IsParticipant(message.CompanionReserve, userId))
                {
                    return new BaseResultDto<CompanionReserveMessageReactionDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var item = await _context.CompanionReserveMessageReactions.FirstOrDefaultAsync(s => s.CompanionReserveMessageId == dto.CompanionReserveMessageId && s.ReactorUserId == userId);

                if (item != null && !item.Deleted && item.Reaction == reactionValue)
                {
                    return new BaseResultDto<CompanionReserveMessageReactionDto>(true, mapper.Map<CompanionReserveMessageReactionDto>(item));
                }

                if (item != null)
                {
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    _context.CompanionReserveMessageReactions.Update(item);
                }
                else
                {
                    item = mapper.Map<CompanionReserveMessageReaction>(dto);
                    item.ReactorUserId = userId;
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    await _context.CompanionReserveMessageReactions.AddAsync(item);
                }

                await _context.SaveChangesAsync();

                // پوش اختصاصی برای ری‌اکشن نداریم (برخلاف پاستیل‌مچ) - سمت کلاینت با همون Polling
                // ۴۵۰۰ms سینک می‌شه؛ فقط پیام جدید Push می‌گیره، نه ری‌اکشن.
                return new BaseResultDto<CompanionReserveMessageReactionDto>(true, mapper.Map<CompanionReserveMessageReactionDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveMessageReactionDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.CompanionReserveMessageReactions.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && item.ReactorUserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                _context.CompanionReserveMessageReactions.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(CompanionReserveMessageReactionDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private static bool IsParticipant(CompanionReserve reserve, long userId)
        {
            return reserve.BookerId == userId ||
                   reserve.CompanionAssistance.Companion.OwnerId == userId ||
                   (reserve.CompanionAssistanceUserId.HasValue && reserve.CompanionAssistanceUser.UserId == userId);
        }

        private IQueryable<CompanionReserveMessage> GetMessageQuery()
        {
            return _context.CompanionReserveMessages
                .Include(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .Include(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistanceUser)
                .AsQueryable();
        }

        private IQueryable<CompanionReserveMessageReaction> GetReactionQuery()
        {
            return _context.CompanionReserveMessageReactions
                .Include(s => s.CompanionReserveMessage).ThenInclude(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .Include(s => s.CompanionReserveMessage).ThenInclude(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistanceUser)
                .AsQueryable();
        }
    }
}
