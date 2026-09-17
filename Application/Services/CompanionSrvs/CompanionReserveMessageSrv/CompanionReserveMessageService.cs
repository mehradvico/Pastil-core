using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageSrv
{
    public class CompanionReserveMessageService : CommonSrv<CompanionReserveMessage, CompanionReserveMessageDto>, ICompanionReserveMessageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;

        // عیناً همون منطق «روش ارتباط مجاز» سمت وب‌اپ (app/utils/companionCall.ts) - چون کلاینت
        // قابل دستکاری است، این چک باید سمت بک‌اند هم تکرار بشه، نه فقط UI.
        private static readonly Regex ChatNamePattern = new(@"چت|گفتگو|گفت‌وگو|پیام|chat", RegexOptions.IgnoreCase);
        private static readonly Regex ExternalChannelPattern = new(@"واتس|تلگرام|whatsapp|telegram|اینستا|instagram|ایتا|بله|روبیکا|sms|پیامک|شماره", RegexOptions.IgnoreCase);

        public CompanionReserveMessageService(
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

        public async Task<BaseResultDto<CompanionReserveMessageVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<CompanionReserveMessageVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.CompanionReserve, userId))
                {
                    return new BaseResultDto<CompanionReserveMessageVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<CompanionReserveMessageVDto>(true, mapper.Map<CompanionReserveMessageVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveMessageVDto>(false, ex.Message, null);
            }
        }

        public CompanionReserveMessageSearchDto Search(CompanionReserveMessageInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetMessageQuery().Where(s => !s.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s =>
                    s.CompanionReserve.BookerId == userId ||
                    s.CompanionReserve.CompanionAssistance.Companion.OwnerId == userId ||
                    (s.CompanionReserve.CompanionAssistanceUserId.HasValue && s.CompanionReserve.CompanionAssistanceUser.UserId == userId));
            }

            if (dto.CompanionReserveId.HasValue)
            {
                model = model.Where(s => s.CompanionReserveId == dto.CompanionReserveId.Value);
            }

            if (dto.SenderUserId.HasValue)
            {
                model = model.Where(s => s.SenderUserId == dto.SenderUserId.Value);
            }

            if (dto.CompanionReserveMessageTypeId.HasValue)
            {
                model = model.Where(s => s.CompanionReserveMessageTypeId == dto.CompanionReserveMessageTypeId.Value);
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

            return new CompanionReserveMessageSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionReserveMessageDto>> InsertAsyncDto(CompanionReserveMessageDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<CompanionReserveMessageDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (!Enum.IsDefined(typeof(CompanionReserveMessageTypeEnum), (int)dto.CompanionReserveMessageTypeId))
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.InvalidCompanionReserveMessageType, dto);
                }

                if (dto.CompanionReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_System)
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var reserve = await GetReserveQuery().FirstOrDefaultAsync(s => s.Id == dto.CompanionReserveId);

                if (reserve == null)
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (reserve.IsCancel)
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (!IsChatOnlineReserve(reserve))
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.CompanionReserveChatNotAvailable, dto);
                }

                if (!isAdmin && (!dto.SenderUserId.HasValue || dto.SenderUserId.Value != userId || !IsParticipant(reserve, userId)))
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                if (dto.CompanionReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Text && string.IsNullOrWhiteSpace(dto.Content))
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.CompanionReserveMessageContentRequired, dto);
                }

                if (dto.ReplyToMessageId.HasValue)
                {
                    var replyMessageExists = await _context.CompanionReserveMessages.AnyAsync(s => s.Id == dto.ReplyToMessageId.Value && s.CompanionReserveId == dto.CompanionReserveId && !s.Deleted);

                    if (!replyMessageExists)
                    {
                        return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.InvalidCompanionReserveReplyMessage, dto);
                    }
                }

                var item = mapper.Map<CompanionReserveMessage>(dto);

                item.DeliveredDate = null;
                item.ReadDate = null;
                item.Deleted = false;
                item.CreateDate = DateTime.Now;

                await _context.CompanionReserveMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                var senderIsBooker = reserve.BookerId == dto.SenderUserId.Value;
                var representativeUserId = reserve.CompanionAssistanceUser?.UserId ?? reserve.CompanionAssistance.Companion.OwnerId;
                var receiverUserId = senderIsBooker ? representativeUserId : reserve.BookerId;
                var senderName = senderIsBooker ? reserve.Booker?.FirstName : reserve.CompanionAssistance.Companion.Name;

                await _pushNotificationService.SendPushAsync(
                    PushTypeEnum.PushCompanionReserveNewMessage,
                    receiverUserId,
                    senderName,
                    GetMessagePreview(dto),
                    dto.CompanionReserveId.ToString());

                return new BaseResultDto<CompanionReserveMessageDto>(true, mapper.Map<CompanionReserveMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveMessageDto>(false, ex.Message, dto);
            }
        }

        public async Task<BaseResultDto> UpdateDeliveredDto(CompanionReserveMessageDeliveredDto dto)
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

                if ((!isAdmin && !IsParticipant(item.CompanionReserve, userId)) || item.SenderUserId == null || item.SenderUserId == userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (!item.DeliveredDate.HasValue)
                {
                    item.DeliveredDate = DateTime.Now;
                    _context.CompanionReserveMessages.Update(item);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdateReadDto(CompanionReserveMessageReadDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var reserve = await GetReserveQuery().FirstOrDefaultAsync(s => s.Id == dto.CompanionReserveId);

                if (reserve == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && !IsParticipant(reserve, userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                var lastMessageExists = await _context.CompanionReserveMessages.AnyAsync(s => s.Id == dto.LastMessageId && s.CompanionReserveId == dto.CompanionReserveId && !s.Deleted);

                if (!lastMessageExists)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                var messages = await _context.CompanionReserveMessages
                    .Where(s => s.CompanionReserveId == dto.CompanionReserveId && s.Id <= dto.LastMessageId && s.SenderUserId != null && s.SenderUserId != userId && !s.Deleted && !s.ReadDate.HasValue)
                    .ToListAsync();

                var now = DateTime.Now;

                foreach (var message in messages)
                {
                    message.DeliveredDate ??= now;
                    message.ReadDate = now;
                }

                if (messages.Any())
                {
                    _context.CompanionReserveMessages.UpdateRange(messages);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public async Task<BaseResultDto<CompanionReserveMessageDto>> InsertSystemMessageAsync(long companionReserveId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.CompanionReserveMessageContentRequired, null);
                }

                var reserveExists = await _context.CompanionReserves.AnyAsync(s => s.Id == companionReserveId);

                if (!reserveExists)
                {
                    return new BaseResultDto<CompanionReserveMessageDto>(false, Resource.Notification.NothingFound, null);
                }

                var item = new CompanionReserveMessage
                {
                    CompanionReserveId = companionReserveId,
                    SenderUserId = null,
                    CompanionReserveMessageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_System,
                    ReplyToMessageId = null,
                    Content = content,
                    DeliveredDate = null,
                    ReadDate = null,
                    Deleted = false,
                    CreateDate = DateTime.Now
                };

                await _context.CompanionReserveMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionReserveMessageDto>(true, mapper.Map<CompanionReserveMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveMessageDto>(false, ex.Message, null);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.CompanionReserveMessages.FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.SenderUserId == null || item.SenderUserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                _context.CompanionReserveMessages.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(CompanionReserveMessageDto dto)
        {
            return DeleteDto(dto.Id);
        }

        // چک مجاز بودن دقیقاً همون منطق سه‌حالته‌ی Api/Hubs/CallHub.cs:JoinCall - رزروکننده، صاحب
        // Companion، یا کارمند تخصیص‌یافته، هر سه طرف مجاز به دیدن/شرکت در گفتگو هستن.
        private static bool IsParticipant(CompanionReserve reserve, long userId)
        {
            if (reserve.BookerId == userId)
                return true;

            if (reserve.CompanionAssistance?.Companion?.OwnerId == userId)
                return true;

            if (reserve.CompanionAssistanceUserId.HasValue && reserve.CompanionAssistanceUser?.UserId == userId)
                return true;

            return false;
        }

        private static bool IsChatOnlineReserve(CompanionReserve reserve)
        {
            var name = reserve.CompanionAssistancePackageOnlineSelection?.CompanionAssistancePackageOnline?.Name ?? string.Empty;
            return ChatNamePattern.IsMatch(name) && !ExternalChannelPattern.IsMatch(name);
        }

        private static string GetMessagePreview(CompanionReserveMessageDto dto)
        {
            if (dto.CompanionReserveMessageTypeId == (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Image)
                return Resource.Notification.CompanionReserveMessagePreviewImage;

            var content = dto.Content?.Trim().Replace("\r", " ").Replace("\n", " ") ?? Resource.Notification.CompanionReserveMessagePreviewNewMessage;
            return content.Length <= 80 ? content : $"{content[..80]}…";
        }

        private IQueryable<CompanionReserve> GetReserveQuery()
        {
            return _context.CompanionReserves
                .Include(r => r.Booker)
                .Include(r => r.CompanionAssistanceUser)
                .Include(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .Include(r => r.CompanionAssistancePackageOnlineSelection).ThenInclude(s => s.CompanionAssistancePackageOnline)
                .AsQueryable();
        }

        private IQueryable<CompanionReserveMessage> GetMessageQuery()
        {
            return _context.CompanionReserveMessages
                .Include(s => s.CompanionReserveMessageType)
                .Include(s => s.ReplyToMessage)
                .Include(s => s.Attachments.Where(a => !a.Deleted))
                .Include(s => s.Reactions.Where(r => !r.Deleted))
                .Include(s => s.CompanionReserve).ThenInclude(r => r.Booker)
                .Include(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistanceUser)
                .Include(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .AsQueryable();
        }
    }
}
