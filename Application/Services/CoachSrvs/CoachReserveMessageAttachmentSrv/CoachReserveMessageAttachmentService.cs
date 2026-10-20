using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv
{
    public class CoachReserveMessageAttachmentService : CommonSrv<CoachReserveMessageAttachment, CoachReserveMessageAttachmentDto>, ICoachReserveMessageAttachmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public CoachReserveMessageAttachmentService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<CoachReserveMessageAttachmentVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetAttachmentQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.CoachReserveMessage.CoachReserve, userId))
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<CoachReserveMessageAttachmentVDto>(true, mapper.Map<CoachReserveMessageAttachmentVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CoachReserveMessageAttachmentVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public CoachReserveMessageAttachmentSearchDto Search(CoachReserveMessageAttachmentInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetAttachmentQuery().Where(s => !s.Deleted && !s.CoachReserveMessage.Deleted);

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

            return new CoachReserveMessageAttachmentSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<CoachReserveMessageAttachmentDto>> InsertAsyncDto(CoachReserveMessageAttachmentDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<CoachReserveMessageAttachmentDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Url))
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentUrlRequired, dto);
                }

                if (!Application.Common.Security.AttachmentUrlPolicy.IsAllowed(dto.Url) ||
                    !Application.Common.Security.AttachmentUrlPolicy.IsAllowedOptional(dto.ThumbnailUrl))
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.InvalidData, dto);
                }

                if (string.IsNullOrWhiteSpace(dto.ContentType))
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentContentTypeRequired, dto);
                }

                if (dto.FileSize <= 0)
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentFileSizeInvalid, dto);
                }

                if (dto.Order < 0)
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentOrderInvalid, dto);
                }

                var message = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.CoachReserveMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (message.SenderUserId == null || (!isAdmin && message.SenderUserId != userId))
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var imageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Image;

                if (message.CoachReserveMessageTypeId != imageTypeId)
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.CoachReserveMessageAttachmentNotAvailable, dto);
                }

                var normalizedContentType = dto.ContentType.Trim().ToLower();

                if (!normalizedContentType.StartsWith("image/"))
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.InvalidCompanionReserveImageContentType, dto);
                }

                var duplicateUrlExists = await _context.CoachReserveMessageAttachments.AnyAsync(s => s.CoachReserveMessageId == dto.CoachReserveMessageId && s.Url == dto.Url && !s.Deleted);

                if (duplicateUrlExists)
                {
                    return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentAlreadyExists, dto);
                }

                var item = mapper.Map<CoachReserveMessageAttachment>(dto);

                item.Url = dto.Url.Trim();
                item.ContentType = normalizedContentType;
                item.Deleted = false;

                await _context.CoachReserveMessageAttachments.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CoachReserveMessageAttachmentDto>(true, mapper.Map<CoachReserveMessageAttachmentDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CoachReserveMessageAttachmentDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.CoachReserveMessageAttachments
                    .Include(s => s.CoachReserveMessage).ThenInclude(s => s.Attachments)
                    .FirstOrDefault(s => s.Id == id && !s.Deleted && !s.CoachReserveMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.CoachReserveMessage.SenderUserId == null || item.CoachReserveMessage.SenderUserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                var activeAttachmentCount = item.CoachReserveMessage.Attachments.Count(s => s.Id != item.Id && !s.Deleted);

                if (activeAttachmentCount == 0)
                {
                    item.CoachReserveMessage.Deleted = true;
                    _context.CoachReserveMessages.Update(item.CoachReserveMessage);
                }

                _context.CoachReserveMessageAttachments.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(CoachReserveMessageAttachmentDto dto)
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

        private IQueryable<CoachReserveMessageAttachment> GetAttachmentQuery()
        {
            return _context.CoachReserveMessageAttachments
                .Include(s => s.CoachReserveMessage).ThenInclude(s => s.CoachReserve).ThenInclude(r => r.Coach)
                .AsQueryable();
        }
    }
}
