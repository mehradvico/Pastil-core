using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv
{
    public class ClinicReserveMessageService : CommonSrv<ClinicReserveMessage, ClinicReserveMessageDto>, IClinicReserveMessageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;

        public ClinicReserveMessageService(
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

        public async Task<BaseResultDto<ClinicReserveMessageVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<ClinicReserveMessageVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.ClinicReserve, userId))
                {
                    return new BaseResultDto<ClinicReserveMessageVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<ClinicReserveMessageVDto>(true, mapper.Map<ClinicReserveMessageVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClinicReserveMessageVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public ClinicReserveMessageSearchDto Search(ClinicReserveMessageInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetMessageQuery().Where(s => !s.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s =>
                    s.ClinicReserve.BookerId == userId ||
                    s.ClinicReserve.Clinic.OwnerId == userId ||
                    s.ClinicReserve.AssigneeUserId == userId);
            }

            if (dto.ClinicReserveId.HasValue)
            {
                model = model.Where(s => s.ClinicReserveId == dto.ClinicReserveId.Value);
            }

            if (dto.SenderUserId.HasValue)
            {
                model = model.Where(s => s.SenderUserId == dto.SenderUserId.Value);
            }

            if (dto.ClinicReserveMessageTypeId.HasValue)
            {
                model = model.Where(s => s.ClinicReserveMessageTypeId == dto.ClinicReserveMessageTypeId.Value);
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

            return new ClinicReserveMessageSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<ClinicReserveMessageDto>> InsertAsyncDto(ClinicReserveMessageDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<ClinicReserveMessageDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (!Enum.IsDefined(typeof(CompanionReserveMessageTypeEnum), (int)dto.ClinicReserveMessageTypeId))
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.InvalidClinicReserveMessageType, dto);
                }

                if (dto.ClinicReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_System)
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var reserve = await GetReserveQuery().FirstOrDefaultAsync(s => s.Id == dto.ClinicReserveId);

                if (reserve == null)
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (reserve.IsCancel)
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (!IsChatOnlineReserve(reserve))
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.CompanionReserveChatNotAvailable, dto);
                }

                if (!isAdmin && (!dto.SenderUserId.HasValue || dto.SenderUserId.Value != userId || !IsParticipant(reserve, userId)))
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                if (dto.ClinicReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Text && string.IsNullOrWhiteSpace(dto.Content))
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.ClinicReserveMessageContentRequired, dto);
                }

                if (dto.ReplyToMessageId.HasValue)
                {
                    var replyMessageExists = await _context.ClinicReserveMessages.AnyAsync(s => s.Id == dto.ReplyToMessageId.Value && s.ClinicReserveId == dto.ClinicReserveId && !s.Deleted);

                    if (!replyMessageExists)
                    {
                        return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.InvalidCompanionReserveReplyMessage, dto);
                    }
                }

                var item = mapper.Map<ClinicReserveMessage>(dto);

                item.DeliveredDate = null;
                item.ReadDate = null;
                item.Deleted = false;
                item.CreateDate = DateTime.Now;

                await _context.ClinicReserveMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                var senderIsBooker = reserve.BookerId == dto.SenderUserId.Value;
                var representativeUserId = reserve.AssigneeUserId ?? reserve.Clinic.OwnerId;
                var receiverUserId = senderIsBooker ? representativeUserId : reserve.BookerId;
                var senderName = senderIsBooker ? reserve.Booker?.FirstName : reserve.Clinic.Name;

                await _pushNotificationService.SendPushAsync(
                    PushTypeEnum.PushClinicReserveNewMessage,
                    receiverUserId,
                    senderName,
                    GetMessagePreview(dto),
                    dto.ClinicReserveId.ToString());

                return new BaseResultDto<ClinicReserveMessageDto>(true, mapper.Map<ClinicReserveMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClinicReserveMessageDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public async Task<BaseResultDto> UpdateDeliveredDto(ClinicReserveMessageDeliveredDto dto)
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

                if ((!isAdmin && !IsParticipant(item.ClinicReserve, userId)) || item.SenderUserId == null || item.SenderUserId == userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (!item.DeliveredDate.HasValue)
                {
                    item.DeliveredDate = DateTime.Now;
                    _context.ClinicReserveMessages.Update(item);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> UpdateReadDto(ClinicReserveMessageReadDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var reserve = await GetReserveQuery().FirstOrDefaultAsync(s => s.Id == dto.ClinicReserveId);

                if (reserve == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && !IsParticipant(reserve, userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                var lastMessageExists = await _context.ClinicReserveMessages.AnyAsync(s => s.Id == dto.LastMessageId && s.ClinicReserveId == dto.ClinicReserveId && !s.Deleted);

                if (!lastMessageExists)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                var messages = await _context.ClinicReserveMessages
                    .Where(s => s.ClinicReserveId == dto.ClinicReserveId && s.Id <= dto.LastMessageId && s.SenderUserId != null && s.SenderUserId != userId && !s.Deleted && !s.ReadDate.HasValue)
                    .ToListAsync();

                var now = DateTime.Now;

                foreach (var message in messages)
                {
                    message.DeliveredDate ??= now;
                    message.ReadDate = now;
                }

                if (messages.Any())
                {
                    _context.ClinicReserveMessages.UpdateRange(messages);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto<ClinicReserveMessageDto>> InsertSystemMessageAsync(long companionReserveId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.ClinicReserveMessageContentRequired, null);
                }

                var reserveExists = await _context.ClinicReserves.AnyAsync(s => s.Id == companionReserveId);

                if (!reserveExists)
                {
                    return new BaseResultDto<ClinicReserveMessageDto>(false, Resource.Notification.NothingFound, null);
                }

                var item = new ClinicReserveMessage
                {
                    ClinicReserveId = companionReserveId,
                    SenderUserId = null,
                    ClinicReserveMessageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_System,
                    ReplyToMessageId = null,
                    Content = content,
                    DeliveredDate = null,
                    ReadDate = null,
                    Deleted = false,
                    CreateDate = DateTime.Now
                };

                await _context.ClinicReserveMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<ClinicReserveMessageDto>(true, mapper.Map<ClinicReserveMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClinicReserveMessageDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.ClinicReserveMessages.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.SenderUserId == null || item.SenderUserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                _context.ClinicReserveMessages.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(ClinicReserveMessageDto dto)
        {
            return DeleteDto(dto.Id);
        }

        // چک مجاز بودن دقیقاً همون منطق سه‌حالته‌ی Api/Hubs/CallHub.cs:JoinCall - رزروکننده، صاحب
        // Companion، یا کارمند تخصیص‌یافته، هر سه طرف مجاز به دیدن/شرکت در گفتگو هستن.
        private static bool IsParticipant(ClinicReserve reserve, long userId)
        {
            if (reserve.BookerId == userId)
                return true;

            if (reserve.Clinic?.OwnerId == userId)
                return true;

            return reserve.AssigneeUserId.HasValue && reserve.AssigneeUserId == userId;
        }

        private static bool IsChatOnlineReserve(ClinicReserve reserve)
        {
            // گفتگو فقط برای رزرو پرداخت‌شده‌ی «آنلاین» (نحوه‌ی ارائه ۳۷) باز است.
            return reserve.IsReserved && !reserve.IsCancel && reserve.Mode == (int)ClinicServiceModeEnum.Online;
        }

        private static string GetMessagePreview(ClinicReserveMessageDto dto)
        {
            if (dto.ClinicReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Image)
                return Resource.Notification.ClinicReserveMessagePreviewImage;

            var content = dto.Content?.Trim().Replace("\r", " ").Replace("\n", " ") ?? Resource.Notification.ClinicReserveMessagePreviewNewMessage;
            return content.Length <= 80 ? content : $"{content[..80]}…";
        }

        private IQueryable<ClinicReserve> GetReserveQuery()
        {
            return _context.ClinicReserves
                .Include(r => r.Booker)
                .Include(r => r.Clinic)
                .AsQueryable();
        }

        private IQueryable<ClinicReserveMessage> GetMessageQuery()
        {
            return _context.ClinicReserveMessages
                .Include(s => s.ClinicReserveMessageType)
                .Include(s => s.ReplyToMessage)
                .Include(s => s.Attachments.Where(a => !a.Deleted))
                .Include(s => s.Reactions.Where(r => !r.Deleted))
                .Include(s => s.ClinicReserve).ThenInclude(r => r.Booker)
                .Include(s => s.ClinicReserve).ThenInclude(r => r.Clinic)
                .AsQueryable();
        }
    }
}
