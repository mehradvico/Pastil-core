using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveMessageSrv
{
    public class CoachReserveMessageService : CommonSrv<CoachReserveMessage, CoachReserveMessageDto>, ICoachReserveMessageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;

        public CoachReserveMessageService(
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

        public async Task<BaseResultDto<CoachReserveMessageVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<CoachReserveMessageVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.CoachReserve, userId))
                {
                    return new BaseResultDto<CoachReserveMessageVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<CoachReserveMessageVDto>(true, mapper.Map<CoachReserveMessageVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CoachReserveMessageVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public CoachReserveMessageSearchDto Search(CoachReserveMessageInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetMessageQuery().Where(s => !s.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s =>
                    s.CoachReserve.BookerId == userId ||
                    s.CoachReserve.Coach.OwnerId == userId ||
                    s.CoachReserve.AssigneeUserId == userId);
            }

            if (dto.CoachReserveId.HasValue)
            {
                model = model.Where(s => s.CoachReserveId == dto.CoachReserveId.Value);
            }

            if (dto.SenderUserId.HasValue)
            {
                model = model.Where(s => s.SenderUserId == dto.SenderUserId.Value);
            }

            if (dto.CoachReserveMessageTypeId.HasValue)
            {
                model = model.Where(s => s.CoachReserveMessageTypeId == dto.CoachReserveMessageTypeId.Value);
            }

            if (dto.ReplyToMessageId.HasValue)
            {
                model = model.Where(s => s.ReplyToMessageId == dto.ReplyToMessageId.Value);
            }

            if (dto.IsRead.HasValue)
            {
                model = dto.IsRead.Value ? model.Where(s => s.ReadDate.HasValue) : model.Where(s => !s.ReadDate.HasValue);
            }

            if (dto.BeforeMessageId.HasValue)
            {
                model = model.Where(s => s.Id < dto.BeforeMessageId.Value);
            }

            if (dto.AfterMessageId.HasValue)
            {
                model = model.Where(s => s.Id > dto.AfterMessageId.Value);
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

            return new CoachReserveMessageSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<CoachReserveMessageDto>> InsertAsyncDto(CoachReserveMessageDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<CoachReserveMessageDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (!Enum.IsDefined(typeof(CompanionReserveMessageTypeEnum), (int)dto.CoachReserveMessageTypeId))
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.InvalidCoachReserveMessageType, dto);
                }

                if (dto.CoachReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_System)
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var reserve = await GetReserveQuery().FirstOrDefaultAsync(s => s.Id == dto.CoachReserveId);

                if (reserve == null)
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (reserve.IsCancel)
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (!IsChatOnlineReserve(reserve))
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.CompanionReserveChatNotAvailable, dto);
                }

                if (!isAdmin && (!dto.SenderUserId.HasValue || dto.SenderUserId.Value != userId || !IsParticipant(reserve, userId)))
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                if (dto.CoachReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Text && string.IsNullOrWhiteSpace(dto.Content))
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.CoachReserveMessageContentRequired, dto);
                }

                if (dto.ReplyToMessageId.HasValue)
                {
                    var replyMessageExists = await _context.CoachReserveMessages.AnyAsync(s => s.Id == dto.ReplyToMessageId.Value && s.CoachReserveId == dto.CoachReserveId && !s.Deleted);

                    if (!replyMessageExists)
                    {
                        return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.InvalidCompanionReserveReplyMessage, dto);
                    }
                }

                var item = mapper.Map<CoachReserveMessage>(dto);

                item.DeliveredDate = null;
                item.ReadDate = null;
                item.Deleted = false;
                item.CreateDate = DateTime.Now;

                await _context.CoachReserveMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                var senderIsBooker = reserve.BookerId == dto.SenderUserId.Value;
                var representativeUserId = reserve.AssigneeUserId ?? reserve.Coach.OwnerId;
                var receiverUserId = senderIsBooker ? representativeUserId : reserve.BookerId;
                var senderName = senderIsBooker ? reserve.Booker?.FirstName : reserve.Coach.Name;

                await _pushNotificationService.SendPushAsync(
                    PushTypeEnum.PushCoachReserveNewMessage,
                    receiverUserId,
                    senderName,
                    GetMessagePreview(dto),
                    dto.CoachReserveId.ToString());

                return new BaseResultDto<CoachReserveMessageDto>(true, mapper.Map<CoachReserveMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CoachReserveMessageDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public async Task<BaseResultDto> UpdateDeliveredDto(CoachReserveMessageDeliveredDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if ((!isAdmin && !IsParticipant(item.CoachReserve, userId)) || item.SenderUserId == null || item.SenderUserId == userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (!item.DeliveredDate.HasValue)
                {
                    item.DeliveredDate = DateTime.Now;
                    _context.CoachReserveMessages.Update(item);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> UpdateReadDto(CoachReserveMessageReadDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var reserve = await GetReserveQuery().FirstOrDefaultAsync(s => s.Id == dto.CoachReserveId);

                if (reserve == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && !IsParticipant(reserve, userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                var lastMessageExists = await _context.CoachReserveMessages.AnyAsync(s => s.Id == dto.LastMessageId && s.CoachReserveId == dto.CoachReserveId && !s.Deleted);

                if (!lastMessageExists)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                var messages = await _context.CoachReserveMessages
                    .Where(s => s.CoachReserveId == dto.CoachReserveId && s.Id <= dto.LastMessageId && s.SenderUserId != null && s.SenderUserId != userId && !s.Deleted && !s.ReadDate.HasValue)
                    .ToListAsync();

                var now = DateTime.Now;

                foreach (var message in messages)
                {
                    message.DeliveredDate ??= now;
                    message.ReadDate = now;
                }

                if (messages.Any())
                {
                    _context.CoachReserveMessages.UpdateRange(messages);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto<CoachReserveMessageDto>> InsertSystemMessageAsync(long companionReserveId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.CoachReserveMessageContentRequired, null);
                }

                var reserveExists = await _context.CoachReserves.AnyAsync(s => s.Id == companionReserveId);

                if (!reserveExists)
                {
                    return new BaseResultDto<CoachReserveMessageDto>(false, Resource.Notification.NothingFound, null);
                }

                var item = new CoachReserveMessage
                {
                    CoachReserveId = companionReserveId,
                    SenderUserId = null,
                    CoachReserveMessageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_System,
                    ReplyToMessageId = null,
                    Content = content,
                    DeliveredDate = null,
                    ReadDate = null,
                    Deleted = false,
                    CreateDate = DateTime.Now
                };

                await _context.CoachReserveMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CoachReserveMessageDto>(true, mapper.Map<CoachReserveMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CoachReserveMessageDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.CoachReserveMessages.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.SenderUserId == null || item.SenderUserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                _context.CoachReserveMessages.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(CoachReserveMessageDto dto)
        {
            return DeleteDto(dto.Id);
        }

        // چک مجاز بودن دقیقاً همون منطق سه‌حالته‌ی Api/Hubs/CallHub.cs:JoinCall - رزروکننده، صاحب
        // Companion، یا کارمند تخصیص‌یافته، هر سه طرف مجاز به دیدن/شرکت در گفتگو هستن.
        private static bool IsParticipant(CoachReserve reserve, long userId)
        {
            if (reserve.BookerId == userId)
                return true;

            if (reserve.Coach?.OwnerId == userId)
                return true;

            return reserve.AssigneeUserId.HasValue && reserve.AssigneeUserId == userId;
        }

        private static bool IsChatOnlineReserve(CoachReserve reserve)
        {
            // گفتگو فقط برای رزرو پرداخت‌شده‌ی «آنلاین» (نحوه‌ی ارائه ۳۷) باز است.
            return reserve.IsReserved && !reserve.IsCancel && reserve.Mode == (int)CoachServiceModeEnum.Online;
        }

        private static string GetMessagePreview(CoachReserveMessageDto dto)
        {
            if (dto.CoachReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Image)
                return Resource.Notification.CoachReserveMessagePreviewImage;

            var content = dto.Content?.Trim().Replace("\r", " ").Replace("\n", " ") ?? Resource.Notification.CoachReserveMessagePreviewNewMessage;
            return content.Length <= 80 ? content : $"{content[..80]}…";
        }

        private IQueryable<CoachReserve> GetReserveQuery()
        {
            return _context.CoachReserves
                .Include(r => r.Booker)
                .Include(r => r.Coach)
                .AsQueryable();
        }

        private IQueryable<CoachReserveMessage> GetMessageQuery()
        {
            return _context.CoachReserveMessages
                .Include(s => s.CoachReserveMessageType)
                .Include(s => s.ReplyToMessage)
                .Include(s => s.Attachments.Where(a => !a.Deleted))
                .Include(s => s.Reactions.Where(r => !r.Deleted))
                .Include(s => s.CoachReserve).ThenInclude(r => r.Booker)
                .Include(s => s.CoachReserve).ThenInclude(r => r.Coach)
                .AsQueryable();
        }
    }
}
