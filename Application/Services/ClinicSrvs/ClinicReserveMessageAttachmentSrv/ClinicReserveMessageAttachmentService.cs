using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv
{
    public class ClinicReserveMessageAttachmentService : CommonSrv<ClinicReserveMessageAttachment, ClinicReserveMessageAttachmentDto>, IClinicReserveMessageAttachmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicReserveMessageAttachmentService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<BaseResultDto<ClinicReserveMessageAttachmentVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = await GetAttachmentQuery().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && !IsParticipant(item.ClinicReserveMessage.ClinicReserve, userId))
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<ClinicReserveMessageAttachmentVDto>(true, mapper.Map<ClinicReserveMessageAttachmentVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClinicReserveMessageAttachmentVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public ClinicReserveMessageAttachmentSearchDto Search(ClinicReserveMessageAttachmentInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

            var model = GetAttachmentQuery().Where(s => !s.Deleted && !s.ClinicReserveMessage.Deleted);

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

            return new ClinicReserveMessageAttachmentSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<ClinicReserveMessageAttachmentDto>> InsertAsyncDto(ClinicReserveMessageAttachmentDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<ClinicReserveMessageAttachmentDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                if (string.IsNullOrWhiteSpace(dto.Url))
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentUrlRequired, dto);
                }

                if (!Application.Common.Security.AttachmentUrlPolicy.IsAllowed(dto.Url) ||
                    !Application.Common.Security.AttachmentUrlPolicy.IsAllowedOptional(dto.ThumbnailUrl))
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.InvalidData, dto);
                }

                if (string.IsNullOrWhiteSpace(dto.ContentType))
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentContentTypeRequired, dto);
                }

                if (dto.FileSize <= 0)
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentFileSizeInvalid, dto);
                }

                if (dto.Order < 0)
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentOrderInvalid, dto);
                }

                var message = await GetMessageQuery().FirstOrDefaultAsync(s => s.Id == dto.ClinicReserveMessageId && !s.Deleted);

                if (message == null)
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.NothingFound, dto);
                }

                if (message.SenderUserId == null || (!isAdmin && message.SenderUserId != userId))
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.AccessDenied, dto);
                }

                var imageTypeId = (long)CompanionReserveMessageTypeEnum.CompanionReserveMessageType_Image;

                if (message.ClinicReserveMessageTypeId != imageTypeId)
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.ClinicReserveMessageAttachmentNotAvailable, dto);
                }

                var normalizedContentType = dto.ContentType.Trim().ToLower();

                if (!normalizedContentType.StartsWith("image/"))
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.InvalidCompanionReserveImageContentType, dto);
                }

                var duplicateUrlExists = await _context.ClinicReserveMessageAttachments.AnyAsync(s => s.ClinicReserveMessageId == dto.ClinicReserveMessageId && s.Url == dto.Url && !s.Deleted);

                if (duplicateUrlExists)
                {
                    return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Resource.Notification.CompanionReserveAttachmentAlreadyExists, dto);
                }

                var item = mapper.Map<ClinicReserveMessageAttachment>(dto);

                item.Url = dto.Url.Trim();
                item.ContentType = normalizedContentType;
                item.Deleted = false;

                await _context.ClinicReserveMessageAttachments.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<ClinicReserveMessageAttachmentDto>(true, mapper.Map<ClinicReserveMessageAttachmentDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClinicReserveMessageAttachmentDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleId == (long)RoleEnum.Admin;

                var item = _context.ClinicReserveMessageAttachments
                    .Include(s => s.ClinicReserveMessage).ThenInclude(s => s.Attachments)
                    .FirstOrDefault(s => s.Id == id && !s.Deleted && !s.ClinicReserveMessage.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                if (!isAdmin && (item.ClinicReserveMessage.SenderUserId == null || item.ClinicReserveMessage.SenderUserId != userId))
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                item.Deleted = true;

                var activeAttachmentCount = item.ClinicReserveMessage.Attachments.Count(s => s.Id != item.Id && !s.Deleted);

                if (activeAttachmentCount == 0)
                {
                    item.ClinicReserveMessage.Deleted = true;
                    _context.ClinicReserveMessages.Update(item.ClinicReserveMessage);
                }

                _context.ClinicReserveMessageAttachments.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public override BaseResultDto DeleteDto(ClinicReserveMessageAttachmentDto dto)
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

        private IQueryable<ClinicReserveMessageAttachment> GetAttachmentQuery()
        {
            return _context.ClinicReserveMessageAttachments
                .Include(s => s.ClinicReserveMessage).ThenInclude(s => s.ClinicReserve).ThenInclude(r => r.Clinic)
                .AsQueryable();
        }
    }
}
