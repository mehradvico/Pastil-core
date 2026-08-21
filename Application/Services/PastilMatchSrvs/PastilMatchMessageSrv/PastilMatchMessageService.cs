using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv
{
    public class PastilMatchMessageService : CommonSrv<PastilMatchMessage, PastilMatchMessageDto>, IPastilMatchMessageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;

        public PastilMatchMessageService(
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

        public async Task<BaseResultDto<PastilMatchMessageVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchMessageVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsMatchParticipant(item.PastilMatch, userId))
                {
                    return new BaseResultDto<PastilMatchMessageVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<PastilMatchMessageVDto>(true, mapper.Map<PastilMatchMessageVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchMessageVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchMessageSearchDto Search(PastilMatchMessageInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetMessageQuery().Where(s => !s.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s => s.PastilMatch.FirstProfile.UserPet.UserId == userId || s.PastilMatch.SecondProfile.UserPet.UserId == userId);
            }

            if (dto.PastilMatchId.HasValue)
            {
                model = model.Where(s => s.PastilMatchId == dto.PastilMatchId.Value);
            }

            if (dto.SenderProfileId.HasValue)
            {
                model = model.Where(s => s.SenderProfileId == dto.SenderProfileId.Value);
            }

            if (dto.PastilMatchMessageTypeId.HasValue)
            {
                model = model.Where(s => s.PastilMatchMessageTypeId == dto.PastilMatchMessageTypeId.Value);
            }

            if (dto.ReplyToMessageId.HasValue)
            {
                model = model.Where(s => s.ReplyToMessageId == dto.ReplyToMessageId.Value);
            }

            if (dto.ParkId.HasValue)
            {
                model = model.Where(s => s.ParkId == dto.ParkId.Value);
            }

            if (dto.IsPinned.HasValue)
            {
                model = model.Where(s => s.IsPinned == dto.IsPinned.Value);
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

            return new PastilMatchMessageSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<PastilMatchMessageDto>> InsertAsyncDto(PastilMatchMessageDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<PastilMatchMessageDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (!dto.SenderProfileId.HasValue)
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.PastilMatchMessageSenderRequired, dto);
                }

                if (!Enum.IsDefined(typeof(PastilMatchMessageTypeEnum), (int)dto.PastilMatchMessageTypeId))
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.InvalidPastilMatchMessageType, dto);
                }

                if (dto.PastilMatchMessageTypeId == (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_System)
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var pastilMatch = await GetMatchQuery().FirstOrDefaultAsync(s => s.Id == dto.PastilMatchId);

                if (pastilMatch == null)
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (pastilMatch.StatusId != (long)PastilMatchStatusEnum.PastilMatchStatus_Active)
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.PastilMatchNotActive, dto);
                }

                string parkName = null;
                if (dto.ParkId.HasValue)
                {
                    if (pastilMatch.PastilMatchGoalId != (long)PastilMatchGoalEnum.PastilMatchGoal_ParkMeetup ||
                        dto.PastilMatchMessageTypeId != (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_Text)
                    {
                        return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.InvalidPastilMatchGoal, dto);
                    }

                    parkName = await _context.Parks
                        .Where(s => s.Id == dto.ParkId.Value)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync();

                    if (parkName == null)
                    {
                        return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.NothingFound, dto);
                    }
                }

                var senderProfile = dto.SenderProfileId.Value == pastilMatch.FirstProfileId ? pastilMatch.FirstProfile : dto.SenderProfileId.Value == pastilMatch.SecondProfileId ? pastilMatch.SecondProfile : null;

                if (senderProfile == null)
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                if (!isAdmin && senderProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.AccessDenied, dto);
                }

                if (dto.PastilMatchMessageTypeId == (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_Text &&
                    !dto.ParkId.HasValue && string.IsNullOrWhiteSpace(dto.Content))
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.PastilMatchMessageContentRequired, dto);
                }

                if (dto.ReplyToMessageId.HasValue)
                {
                    var replyMessageExists = await _context.PastilMatchMessages.AnyAsync(s => s.Id == dto.ReplyToMessageId.Value && s.PastilMatchId == dto.PastilMatchId && !s.Deleted);

                    if (!replyMessageExists)
                    {
                        return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.InvalidPastilMatchReplyMessage, dto);
                    }
                }

                var item = mapper.Map<PastilMatchMessage>(dto);

                item.IsEdited = false;
                item.EditDate = null;
                item.IsPinned = false;
                item.PinDate = null;
                item.DeliveredDate = null;
                item.ReadDate = null;
                item.Deleted = false;
                item.CreateDate = DateTime.Now;

                await _context.PastilMatchMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                var receiverProfile = senderProfile.Id == pastilMatch.FirstProfileId
                    ? pastilMatch.SecondProfile
                    : pastilMatch.FirstProfile;
                await _pushNotificationService.SendPushAsync(
                    PushTypeEnum.PushPastilMatchNewMessage,
                    receiverProfile.UserPet.UserId,
                    senderProfile.UserPet.User.FirstName,
                    GetMessagePreview(dto, parkName),
                    dto.PastilMatchId.ToString(),
                    receiverProfile.Id.ToString());

                return new BaseResultDto<PastilMatchMessageDto>(true, mapper.Map<PastilMatchMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchMessageDto>(false, ex.Message, dto);
            }
        }

        public async Task<BaseResultDto> UpdateEditDto(PastilMatchMessageEditDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchMessageContentRequired);
                }

                var item = await _context.PastilMatchMessages.Include(s => s.SenderProfile).ThenInclude(s => s.UserPet).Include(s => s.PastilMatch).FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (item.SenderProfileId == null || (!isAdmin && item.SenderProfile.UserPet.UserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (item.PastilMatch.StatusId != (long)PastilMatchStatusEnum.PastilMatchStatus_Active)
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchNotActive);
                }

                if (item.PastilMatchMessageTypeId == (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_System || item.PastilMatchMessageTypeId == (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_Voice)
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchMessageEditNotAvailable);
                }

                item.Content = dto.Content.Trim();
                item.IsEdited = true;
                item.EditDate = DateTime.Now;

                _context.PastilMatchMessages.Update(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdatePinDto(PastilMatchMessagePinDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await _context.PastilMatchMessages
                    .Include(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                    .Include(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                    .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && !IsMatchParticipant(item.PastilMatch, userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (item.PastilMatch.StatusId != (long)PastilMatchStatusEnum.PastilMatchStatus_Active)
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchNotActive);
                }

                item.IsPinned = dto.IsPinned;
                item.PinDate = dto.IsPinned ? DateTime.Now : null;

                _context.PastilMatchMessages.Update(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdateDeliveredDto(PastilMatchMessageDeliveredDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await _context.PastilMatchMessages
                    .Include(s => s.SenderProfile).ThenInclude(s => s.UserPet)
                    .Include(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                    .Include(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                    .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if ((!isAdmin && !IsMatchParticipant(item.PastilMatch, userId)) || item.SenderProfileId == null || item.SenderProfile.UserPet.UserId == userId)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (!item.DeliveredDate.HasValue)
                {
                    item.DeliveredDate = DateTime.Now;
                    _context.PastilMatchMessages.Update(item);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdateReadDto(PastilMatchMessageReadDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var pastilMatch = await GetMatchQuery().FirstOrDefaultAsync(s => s.Id == dto.PastilMatchId);

                if (pastilMatch == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && !IsMatchParticipant(pastilMatch, userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                var lastMessageExists = await _context.PastilMatchMessages.AnyAsync(s => s.Id == dto.LastMessageId && s.PastilMatchId == dto.PastilMatchId && !s.Deleted);

                if (!lastMessageExists)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                var messages = await _context.PastilMatchMessages
                    .Include(s => s.SenderProfile).ThenInclude(s => s.UserPet)
                    .Where(s => s.PastilMatchId == dto.PastilMatchId && s.Id <= dto.LastMessageId && s.SenderProfileId != null && s.SenderProfile.UserPet.UserId != userId && !s.Deleted && !s.ReadDate.HasValue)
                    .ToListAsync();

                var now = DateTime.Now;

                foreach (var message in messages)
                {
                    message.DeliveredDate ??= now;
                    message.ReadDate = now;
                }

                if (messages.Any())
                {
                    _context.PastilMatchMessages.UpdateRange(messages);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public async Task<BaseResultDto<PastilMatchMessageDto>> InsertSystemMessageAsync(long pastilMatchId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.PastilMatchMessageContentRequired, null);
                }

                var matchExists = await _context.PastilMatches.AnyAsync(s => s.Id == pastilMatchId);

                if (!matchExists)
                {
                    return new BaseResultDto<PastilMatchMessageDto>(false, Resource.Notification.NothingFound, null);
                }

                var item = new PastilMatchMessage
                {
                    PastilMatchId = pastilMatchId,
                    SenderProfileId = null,
                    PastilMatchMessageTypeId = (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_System,
                    ReplyToMessageId = null,
                    Content = content,
                    IsEdited = false,
                    EditDate = null,
                    IsPinned = false,
                    PinDate = null,
                    DeliveredDate = null,
                    ReadDate = null,
                    Deleted = false,
                    CreateDate = DateTime.Now
                };

                await _context.PastilMatchMessages.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<PastilMatchMessageDto>(true, mapper.Map<PastilMatchMessageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchMessageDto>(false, ex.Message, null);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.PastilMatchMessages.Include(s => s.SenderProfile).ThenInclude(s => s.UserPet).FirstOrDefault(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.SenderProfileId == null || item.SenderProfile.UserPet.UserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;
                item.IsPinned = false;
                item.PinDate = null;

                _context.PastilMatchMessages.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchMessageDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private bool IsMatchParticipant(PastilMatch pastilMatch, long userId)
        {
            return pastilMatch.FirstProfile.UserPet.UserId == userId || pastilMatch.SecondProfile.UserPet.UserId == userId;
        }

        private IQueryable<PastilMatch> GetMatchQuery()
        {
            return _context.PastilMatches
                .Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .AsQueryable();
        }

        private static string GetMessagePreview(PastilMatchMessageDto dto, string parkName = null)
        {
            if (dto.ParkId.HasValue)
                return string.IsNullOrWhiteSpace(parkName) ? "پیشنهاد پارک" : $"پارک {parkName}";

            if (dto.PastilMatchMessageTypeId == (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_Image)
                return "تصویر";

            if (dto.PastilMatchMessageTypeId == (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_Voice)
                return "پیام صوتی";

            var content = dto.Content?.Trim().Replace("\r", " ").Replace("\n", " ") ?? "پیام جدید";
            return content.Length <= 80 ? content : $"{content[..80]}…";
        }

        private IQueryable<PastilMatchMessage> GetMessageQuery()
        {
            return _context.PastilMatchMessages
                .Include(s => s.PastilMatchMessageType)
                .Include(s => s.ReplyToMessage).ThenInclude(s => s.Park).ThenInclude(s => s.Picture)
                .Include(s => s.ReplyToMessage).ThenInclude(s => s.Park).ThenInclude(s => s.ParkPictures.Where(p => !p.Deleted)).ThenInclude(s => s.Picture)
                .Include(s => s.Park).ThenInclude(s => s.Picture)
                .Include(s => s.Park).ThenInclude(s => s.ParkPictures.Where(p => !p.Deleted)).ThenInclude(s => s.Picture)
                .Include(s => s.Park).ThenInclude(s => s.Neighborhood).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.Attachments.Where(a => !a.Deleted))
                .Include(s => s.Reactions.Where(r => !r.Deleted))
                .Include(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                .AsQueryable();
        }
    }
}
