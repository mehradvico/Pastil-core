using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv
{
    public class PastilMatchMessageAttachmentService : CommonSrv<PastilMatchMessageAttachment, PastilMatchMessageAttachmentDto>, IPastilMatchMessageAttachmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public PastilMatchMessageAttachmentService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<PastilMatchMessageAttachmentVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = await GetAttachmentQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsMatchParticipant(item.PastilMatchMessage.PastilMatch, userId))
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<PastilMatchMessageAttachmentVDto>(true, mapper.Map<PastilMatchMessageAttachmentVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchMessageAttachmentVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchMessageAttachmentSearchDto Search(PastilMatchMessageAttachmentInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

            var model = GetAttachmentQuery().Where(s => !s.Deleted && !s.PastilMatchMessage.Deleted);

            if (!isAdmin)
            {
                model = model.Where(s => s.PastilMatchMessage.PastilMatch.FirstProfile.UserPet.UserId == userId || s.PastilMatchMessage.PastilMatch.SecondProfile.UserPet.UserId == userId);
            }

            if (dto.PastilMatchMessageId.HasValue)
            {
                model = model.Where(s => s.PastilMatchMessageId == dto.PastilMatchMessageId.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.ContentType))
            {
                model = model.Where(s => s.ContentType.Contains(dto.ContentType));
            }

            switch (dto.SortBy)
            {
                case SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    {
                        model = model.OrderBy(s => s.Order).ThenBy(s => s.Id);
                        break;
                    }
            }

            return new PastilMatchMessageAttachmentSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<PastilMatchMessageAttachmentDto>> InsertAsyncDto(PastilMatchMessageAttachmentDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<PastilMatchMessageAttachmentDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;

                if (string.IsNullOrWhiteSpace(dto.Url))
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchAttachmentUrlRequired, dto);
                }

                if (string.IsNullOrWhiteSpace(dto.ContentType))
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchAttachmentContentTypeRequired, dto);
                }

                if (dto.FileSize <= 0)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchAttachmentFileSizeInvalid, dto);
                }

                if (dto.Order < 0)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchAttachmentOrderInvalid, dto);
                }

                var message = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.PastilMatchMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (message.SenderProfileId == null || message.SenderProfile.UserPet.UserId != userId)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.AccessDenied, dto);
                }

                if (message.PastilMatch.StatusId != (long)PastilMatchStatusEnum.PastilMatchStatus_Active)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchNotActive, dto);
                }

                var imageTypeId = (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_Image;
                var voiceTypeId = (long)PastilMatchMessageTypeEnum.PastilMatchMessageType_Voice;

                if (message.PastilMatchMessageTypeId != imageTypeId && message.PastilMatchMessageTypeId != voiceTypeId)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchMessageAttachmentNotAvailable, dto);
                }

                var normalizedContentType = dto.ContentType.Trim().ToLower();

                if (message.PastilMatchMessageTypeId == imageTypeId && !normalizedContentType.StartsWith("image/"))
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.InvalidPastilMatchImageContentType, dto);
                }

                if (message.PastilMatchMessageTypeId == voiceTypeId && !normalizedContentType.StartsWith("audio/"))
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.InvalidPastilMatchVoiceContentType, dto);
                }

                if (message.PastilMatchMessageTypeId == voiceTypeId && (!dto.Duration.HasValue || dto.Duration.Value <= 0))
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchVoiceDurationRequired, dto);
                }

                var duplicateUrlExists = await _context.PastilMatchMessageAttachments.AnyAsync(s => s.PastilMatchMessageId == dto.PastilMatchMessageId && s.Url == dto.Url && !s.Deleted);

                if (duplicateUrlExists)
                {
                    return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchAttachmentAlreadyExists, dto);
                }

                if (message.PastilMatchMessageTypeId == voiceTypeId)
                {
                    var voiceAttachmentExists = await _context.PastilMatchMessageAttachments.AnyAsync(s => s.PastilMatchMessageId == dto.PastilMatchMessageId && !s.Deleted);

                    if (voiceAttachmentExists)
                    {
                        return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, Resource.Notification.PastilMatchVoiceAttachmentAlreadyExists, dto);
                    }
                }

                var item = mapper.Map<PastilMatchMessageAttachment>(dto);

                item.Url = dto.Url.Trim();
                item.ContentType = normalizedContentType;
                item.Deleted = false;

                await _context.PastilMatchMessageAttachments.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<PastilMatchMessageAttachmentDto>(true, mapper.Map<PastilMatchMessageAttachmentDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchMessageAttachmentDto>(false, ex.Message, dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = _context.PastilMatchMessageAttachments
                    .Include(s => s.PastilMatchMessage).ThenInclude(s => s.SenderProfile).ThenInclude(s => s.UserPet)
                    .Include(s => s.PastilMatchMessage).ThenInclude(s => s.Attachments)
                    .FirstOrDefault(s => s.Id == id && !s.Deleted && !s.PastilMatchMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.PastilMatchMessage.SenderProfileId == null || item.PastilMatchMessage.SenderProfile.UserPet.UserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                var activeAttachmentCount = item.PastilMatchMessage.Attachments.Count(s => s.Id != item.Id && !s.Deleted);

                if (activeAttachmentCount == 0)
                {
                    item.PastilMatchMessage.Deleted = true;
                    item.PastilMatchMessage.IsPinned = false;
                    item.PastilMatchMessage.PinDate = null;
                    _context.PastilMatchMessages.Update(item.PastilMatchMessage);
                }

                _context.PastilMatchMessageAttachments.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(PastilMatchMessageAttachmentDto dto)
        {
            return DeleteDto(dto.Id);
        }

        private bool IsMatchParticipant(PastilMatch pastilMatch, long userId)
        {
            return pastilMatch.FirstProfile.UserPet.UserId == userId || pastilMatch.SecondProfile.UserPet.UserId == userId;
        }

        private IQueryable<PastilMatchMessage> GetMessageQuery()
        {
            return _context.PastilMatchMessages
                .Include(s => s.SenderProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                .AsQueryable();
        }

        private IQueryable<PastilMatchMessageAttachment> GetAttachmentQuery()
        {
            return _context.PastilMatchMessageAttachments
                .Include(s => s.PastilMatchMessage).ThenInclude(s => s.SenderProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatchMessage).ThenInclude(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatchMessage).ThenInclude(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                .AsQueryable();
        }
    }
}
