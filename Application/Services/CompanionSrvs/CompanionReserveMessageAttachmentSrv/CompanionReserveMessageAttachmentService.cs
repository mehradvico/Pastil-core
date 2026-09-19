using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv
{
    public class CompanionReserveMessageAttachmentService : CommonSrv<CompanionReserveMessageAttachment, CompanionReserveMessageAttachmentDto>, ICompanionReserveMessageAttachmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public CompanionReserveMessageAttachmentService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<CompanionReserveMessageAttachmentVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetAttachmentQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.CompanionReserveMessage.CompanionReserve, userId))
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<CompanionReserveMessageAttachmentVDto>(true, mapper.Map<CompanionReserveMessageAttachmentVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveMessageAttachmentVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public CompanionReserveMessageAttachmentSearchDto Search(CompanionReserveMessageAttachmentInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetAttachmentQuery().Where(s => !s.Deleted && !s.CompanionReserveMessage.Deleted);

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

            return new CompanionReserveMessageAttachmentSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionReserveMessageAttachmentDto>> InsertAsyncDto(CompanionReserveMessageAttachmentDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<CompanionReserveMessageAttachmentDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Url))
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentUrlRequired, dto);
                }

                if (string.IsNullOrWhiteSpace(dto.ContentType))
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentContentTypeRequired, dto);
                }

                if (dto.FileSize <= 0)
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentFileSizeInvalid, dto);
                }

                if (dto.Order < 0)
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentOrderInvalid, dto);
                }

                var message = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.CompanionReserveMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (message.SenderUserId == null || (!isAdmin && message.SenderUserId != userId))
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var imageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Image;

                if (message.CompanionReserveMessageTypeId != imageTypeId)
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveMessageAttachmentNotAvailable, dto);
                }

                var normalizedContentType = dto.ContentType.Trim().ToLower();

                if (!normalizedContentType.StartsWith("image/"))
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.InvalidCompanionReserveImageContentType, dto);
                }

                var duplicateUrlExists = await _context.CompanionReserveMessageAttachments.AnyAsync(s => s.CompanionReserveMessageId == dto.CompanionReserveMessageId && s.Url == dto.Url && !s.Deleted);

                if (duplicateUrlExists)
                {
                    return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentAlreadyExists, dto);
                }

                var item = mapper.Map<CompanionReserveMessageAttachment>(dto);

                item.Url = dto.Url.Trim();
                item.ContentType = normalizedContentType;
                item.Deleted = false;

                await _context.CompanionReserveMessageAttachments.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionReserveMessageAttachmentDto>(true, mapper.Map<CompanionReserveMessageAttachmentDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveMessageAttachmentDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.CompanionReserveMessageAttachments
                    .Include(s => s.CompanionReserveMessage).ThenInclude(s => s.Attachments)
                    .FirstOrDefault(s => s.Id == id && !s.Deleted && !s.CompanionReserveMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.CompanionReserveMessage.SenderUserId == null || item.CompanionReserveMessage.SenderUserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                var activeAttachmentCount = item.CompanionReserveMessage.Attachments.Count(s => s.Id != item.Id && !s.Deleted);

                if (activeAttachmentCount == 0)
                {
                    item.CompanionReserveMessage.Deleted = true;
                    _context.CompanionReserveMessages.Update(item.CompanionReserveMessage);
                }

                _context.CompanionReserveMessageAttachments.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(CompanionReserveMessageAttachmentDto dto)
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

        private IQueryable<CompanionReserveMessageAttachment> GetAttachmentQuery()
        {
            return _context.CompanionReserveMessageAttachments
                .Include(s => s.CompanionReserveMessage).ThenInclude(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .Include(s => s.CompanionReserveMessage).ThenInclude(s => s.CompanionReserve).ThenInclude(r => r.CompanionAssistanceUser)
                .AsQueryable();
        }
    }
}
