using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv
{
    public class ClinicReserveMessageReactionService : CommonSrv<ClinicReserveMessageReaction, ClinicReserveMessageReactionDto>, IClinicReserveMessageReactionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicReserveMessageReactionService(
            IDataBaseContext context,
            IMapper mapper,
            ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<ClinicReserveMessageReactionVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetReactionQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && !s.ClinicReserveMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<ClinicReserveMessageReactionVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.ClinicReserveMessage.ClinicReserve, userId))
                {
                    return new BaseResultDto<ClinicReserveMessageReactionVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<ClinicReserveMessageReactionVDto>(true, mapper.Map<ClinicReserveMessageReactionVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClinicReserveMessageReactionVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public ClinicReserveMessageReactionSearchDto Search(ClinicReserveMessageReactionInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetReactionQuery().Where(s => !s.Deleted && !s.ClinicReserveMessage.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s =>
                    s.ClinicReserveMessage.ClinicReserve.BookerId == userId ||
                    s.ClinicReserveMessage.ClinicReserve.Clinic.OwnerId == userId ||
                    s.ClinicReserveMessage.ClinicReserve.AssigneeUserId == userId);
            }

            if (dto.ClinicReserveMessageId.HasValue)
            {
                model = model.Where(s => s.ClinicReserveMessageId == dto.ClinicReserveMessageId.Value);
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

            return new ClinicReserveMessageReactionSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<ClinicReserveMessageReactionDto>> InsertAsyncDto(ClinicReserveMessageReactionDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<ClinicReserveMessageReactionDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Reaction))
                {
                    return new BaseResultDto<ClinicReserveMessageReactionDto>(false, Resource.Notification.ClinicReserveMessageReactionRequired, dto);
                }

                var reactionValue = dto.Reaction.Trim();

                if (reactionValue.Length > 32)
                {
                    return new BaseResultDto<ClinicReserveMessageReactionDto>(false, Resource.Notification.ClinicReserveMessageReactionTooLong, dto);
                }

                var message = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.ClinicReserveMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<ClinicReserveMessageReactionDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (!isAdmin && !IsParticipant(message.ClinicReserve, userId))
                {
                    return new BaseResultDto<ClinicReserveMessageReactionDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var item = await _context.ClinicReserveMessageReactions.FirstOrDefaultAsync(s => s.ClinicReserveMessageId == dto.ClinicReserveMessageId && s.ReactorUserId == userId);

                if (item != null && !item.Deleted && item.Reaction == reactionValue)
                {
                    return new BaseResultDto<ClinicReserveMessageReactionDto>(true, mapper.Map<ClinicReserveMessageReactionDto>(item));
                }

                if (item != null)
                {
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    _context.ClinicReserveMessageReactions.Update(item);
                }
                else
                {
                    item = mapper.Map<ClinicReserveMessageReaction>(dto);
                    item.ReactorUserId = userId;
                    item.Reaction = reactionValue;
                    item.Deleted = false;

                    await _context.ClinicReserveMessageReactions.AddAsync(item);
                }

                await _context.SaveChangesAsync();

                // پوش اختصاصی برای ری‌اکشن نداریم (برخلاف پاستیل‌مچ) - سمت کلاینت با همون Polling
                // ۴۵۰۰ms سینک می‌شه؛ فقط پیام جدید Push می‌گیره، نه ری‌اکشن.
                return new BaseResultDto<ClinicReserveMessageReactionDto>(true, mapper.Map<ClinicReserveMessageReactionDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClinicReserveMessageReactionDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.ClinicReserveMessageReactions.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && item.ReactorUserId != userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                _context.ClinicReserveMessageReactions.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(ClinicReserveMessageReactionDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private static bool IsParticipant(ClinicReserve reserve, long userId)
        {
            return reserve.BookerId == userId ||
                   reserve.Clinic.OwnerId == userId ||
                   reserve.AssigneeUserId == userId;
        }

        private IQueryable<ClinicReserveMessage> GetMessageQuery()
        {
            return _context.ClinicReserveMessages
                .Include(s => s.ClinicReserve).ThenInclude(r => r.Clinic)
                .AsQueryable();
        }

        private IQueryable<ClinicReserveMessageReaction> GetReactionQuery()
        {
            return _context.ClinicReserveMessageReactions
                .Include(s => s.ClinicReserveMessage).ThenInclude(s => s.ClinicReserve).ThenInclude(r => r.Clinic)
                .AsQueryable();
        }
    }
}
