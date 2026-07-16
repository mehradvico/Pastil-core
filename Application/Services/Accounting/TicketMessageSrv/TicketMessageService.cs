using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.Accounting.TicketItemSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Iface;
using Application.Services.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.TicketMessageSrv
{
    public class TicketMessageService : ITicketMessageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly CurrentUserDto _currentUser;
        private readonly ICodeService _codeService;

        public TicketMessageService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUserHelper, ICodeService codeService)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUserHelper.CurrentUser;
            _codeService = codeService;
        }

        public async Task<BaseResultDto<TicketMessageSearchDto>> GetUserMessagesAsync(long ticketId, TicketMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == ticketId && s.UserId == _currentUser.UserId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketMessageSearchDto>(false, Resource.Notification.NothingFound, null);
            }

            return await GetMessagesAsync(ticket, dto, cancellationToken);
        }

        public async Task<BaseResultDto<TicketMessageSearchDto>> GetAdminMessagesAsync(long ticketId, TicketMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == ticketId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketMessageSearchDto>(false, Resource.Notification.NothingFound, null);
            }

            if (ticket.AdminId.HasValue && ticket.AdminId.Value != _currentUser.UserId)
            {
                return new BaseResultDto<TicketMessageSearchDto>(false, Resource.Notification.ThisTicketBlongsToAnotherAdmin, null);
            }

            return await GetMessagesAsync(ticket, dto, cancellationToken);
        }

        public async Task<BaseResultDto<TicketItemVDto>> SendUserMessageAsync(long ticketId, SendTicketMessageDto dto, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.AsTracking().FirstOrDefaultAsync(s => s.Id == ticketId && s.UserId == _currentUser.UserId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.NothingFound, null);
            }

            var closeStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Close.ToString());

            if (closeStatus == null)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.TicketStatusIsNotComplete, null);
            }

            if (ticket.StatusId == closeStatus.Id)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.ThisTickerIsClosedAndYouCanNotSendMessageAnymore, null);
            }

            var validationResult = await ValidateMessageAsync(ticket.Id, dto, cancellationToken);

            if (!validationResult.IsSuccess)
            {
                return new BaseResultDto<TicketItemVDto>(false, validationResult.Messages, null);
            }

            var waitingStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Waiting.ToString());

            if (waitingStatus == null)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.TicketStatusIsNotComplete, null);
            }

            var createDate = DateTime.Now;
            var ticketItem = new TicketItem
            {
                Body = await SanitizeBodyAsync(dto.Body),
                UserId = _currentUser.UserId,
                TicketId = ticket.Id,
                FileId = dto.FileId,
                ReplyToTicketItemId = dto.ReplyToTicketItemId,
                CreateDate = createDate,
                IsSeen = false,
                SeenDate = null,
                Deleted = false
            };

            ticket.StatusId = waitingStatus.Id;
            ticket.UpdateDate = createDate;

            await _context.TicketItems.AddAsync(ticketItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = await FindMessageAsync(ticketItem.Id, ticket.UserId, cancellationToken);
            return new BaseResultDto<TicketItemVDto>(true, result);
        }

        public async Task<BaseResultDto<TicketItemVDto>> SendAdminMessageAsync(long ticketId, SendTicketMessageDto dto, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.AsTracking().FirstOrDefaultAsync(s => s.Id == ticketId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.NothingFound, null);
            }

            if (ticket.AdminId.HasValue && ticket.AdminId.Value != _currentUser.UserId)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.ThisTicketBlongsToAnotherAdmin, null);
            }

            var closeStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Close.ToString());

            if (closeStatus == null)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.TicketStatusIsNotComplete, null);
            }

            if (ticket.StatusId == closeStatus.Id)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.ThisTickerIsClosedAndYouCanNotSendMessageAnymore, null);
            }

            var validationResult = await ValidateMessageAsync(ticket.Id, dto, cancellationToken);

            if (!validationResult.IsSuccess)
            {
                return new BaseResultDto<TicketItemVDto>(false, validationResult.Messages, null);
            }

            var answeredStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Answered.ToString());

            if (answeredStatus == null)
            {
                return new BaseResultDto<TicketItemVDto>(false, Resource.Notification.TicketStatusIsNotComplete, null);
            }

            var createDate = DateTime.Now;
            var ticketItem = new TicketItem
            {
                Body = await SanitizeBodyAsync(dto.Body),
                UserId = _currentUser.UserId,
                TicketId = ticket.Id,
                FileId = dto.FileId,
                ReplyToTicketItemId = dto.ReplyToTicketItemId,
                CreateDate = createDate,
                IsSeen = false,
                SeenDate = null,
                Deleted = false
            };

            if (!ticket.AdminId.HasValue)
            {
                ticket.AdminId = _currentUser.UserId;
            }

            ticket.StatusId = answeredStatus.Id;
            ticket.UpdateDate = createDate;

            await _context.TicketItems.AddAsync(ticketItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = await FindMessageAsync(ticketItem.Id, ticket.UserId, cancellationToken);
            return new BaseResultDto<TicketItemVDto>(true, result);
        }

        public async Task<BaseResultDto<TicketSeenVDto>> MarkAsSeenForUserAsync(long ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == ticketId && s.UserId == _currentUser.UserId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketSeenVDto>(false, Resource.Notification.NothingFound, null);
            }

            var seenDate = DateTime.Now;
            var seenCount = await _context.TicketItems.Where(s => s.TicketId == ticket.Id && s.UserId != ticket.UserId && !s.IsSeen).ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsSeen, true).SetProperty(s => s.SeenDate, seenDate), cancellationToken);

            var result = new TicketSeenVDto
            {
                TicketId = ticket.Id,
                SeenCount = seenCount,
                SeenDate = seenDate
            };

            return new BaseResultDto<TicketSeenVDto>(true, result);
        }

        public async Task<BaseResultDto<TicketSeenVDto>> MarkAsSeenForAdminAsync(long ticketId, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == ticketId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketSeenVDto>(false, Resource.Notification.NothingFound, null);
            }

            if (ticket.AdminId.HasValue && ticket.AdminId.Value != _currentUser.UserId)
            {
                return new BaseResultDto<TicketSeenVDto>(false, Resource.Notification.ThisTicketBlongsToAnotherAdmin, null);
            }

            var seenDate = DateTime.Now;
            var seenCount = await _context.TicketItems.Where(s => s.TicketId == ticket.Id && s.UserId == ticket.UserId && !s.IsSeen).ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsSeen, true).SetProperty(s => s.SeenDate, seenDate), cancellationToken);

            var result = new TicketSeenVDto
            {
                TicketId = ticket.Id,
                SeenCount = seenCount,
                SeenDate = seenDate
            };

            return new BaseResultDto<TicketSeenVDto>(true, result);
        }

        public async Task<BaseResultDto<TicketItem>> PrepareInitialMessageAsync(Ticket ticket, long senderUserId, string body, long? fileId, CancellationToken cancellationToken = default)
        {
            if (ticket == null)
            {
                return new BaseResultDto<TicketItem>(false, Resource.Notification.NothingFound, null);
            }

            if (string.IsNullOrWhiteSpace(body) && !fileId.HasValue)
            {
                return new BaseResultDto<TicketItem>(false, Resource.Notification.ContentOrFileIsRequired, null);
            }

            if (!string.IsNullOrWhiteSpace(body) && body.Length > 2000)
            {
                return new BaseResultDto<TicketItem>(false, Resource.Notification.ContentCanNotBeOver2000Charecters, null);
            }

            if (fileId.HasValue)
            {
                var fileExists = await _context.Files.AnyAsync(s => s.Id == fileId.Value, cancellationToken);

                if (!fileExists)
                {
                    return new BaseResultDto<TicketItem>(false, Resource.Notification.FileCanNotBeFound, null);
                }
            }

            var ticketItem = new TicketItem
            {
                Body = await SanitizeBodyAsync(body),
                UserId = senderUserId,
                FileId = fileId,
                CreateDate = ticket.CreateDate,
                IsSeen = false,
                SeenDate = null,
                Deleted = false,
                Ticket = ticket
            };

            return new BaseResultDto<TicketItem>(true, ticketItem);
        }

        private async Task<BaseResultDto> ValidateMessageAsync(long ticketId, SendTicketMessageDto dto, CancellationToken cancellationToken)
        {
            if (dto == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            if (string.IsNullOrWhiteSpace(dto.Body) && !dto.FileId.HasValue)
            {
                return new BaseResultDto(false, Resource.Notification.ContentOrFileIsRequired);
            }

            if (!string.IsNullOrWhiteSpace(dto.Body) && dto.Body.Length > 2000)
            {
                return new BaseResultDto(false, Resource.Notification.ContentCanNotBeOver2000Charecters);
            }

            if (dto.FileId.HasValue)
            {
                var fileExists = await _context.Files.AnyAsync(s => s.Id == dto.FileId.Value, cancellationToken);

                if (!fileExists)
                {
                    return new BaseResultDto(false, Resource.Notification.FileCanNotBeFound);
                }
            }

            if (dto.ReplyToTicketItemId.HasValue)
            {
                var replyExists = await _context.TicketItems.AnyAsync(s => s.Id == dto.ReplyToTicketItemId.Value && s.TicketId == ticketId, cancellationToken);

                if (!replyExists)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }
            }

            return new BaseResultDto(true);
        }

        private async Task<BaseResultDto<TicketMessageSearchDto>> GetMessagesAsync(Ticket ticket, TicketMessageInputDto dto, CancellationToken cancellationToken)
        {
            dto ??= new TicketMessageInputDto();

            var pageSize = dto.PageSize;

            if (pageSize < 1)
            {
                pageSize = 30;
            }

            if (pageSize > 50)
            {
                pageSize = 50;
            }

            var query = _context.TicketItems
                .AsNoTracking()
                .Include(s => s.User)
                .ThenInclude(s => s.Role)
                .Include(s => s.File)
                .Include(s => s.ReplyToTicketItem)
                .ThenInclude(s => s.File)
                .Where(s => s.TicketId == ticket.Id);

            if (dto.BeforeId.HasValue)
            {
                query = query.Where(s => s.Id < dto.BeforeId.Value);
            }

            var items = await query.OrderByDescending(s => s.Id).Take(pageSize + 1).ToListAsync(cancellationToken);
            var hasMore = items.Count > pageSize;

            if (hasMore)
            {
                items.RemoveAt(items.Count - 1);
            }

            items.Reverse();

            var result = new TicketMessageSearchDto
            {
                TicketId = ticket.Id,
                PageSize = pageSize,
                HasMore = hasMore,
                NextBeforeId = hasMore && items.Count > 0 ? items.First().Id : null,
                List = items.Select(s => MapMessage(s, ticket.UserId)).ToList()
            };

            return new BaseResultDto<TicketMessageSearchDto>(true, result);
        }

        private async Task<TicketItemVDto> FindMessageAsync(long id, long ticketUserId, CancellationToken cancellationToken)
        {
            var item = await _context.TicketItems
                .AsNoTracking()
                .Include(s => s.User)
                .ThenInclude(s => s.Role)
                .Include(s => s.File)
                .Include(s => s.ReplyToTicketItem)
                .ThenInclude(s => s.File)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            return item == null ? null : MapMessage(item, ticketUserId);
        }

        private TicketItemVDto MapMessage(TicketItem item, long ticketUserId)
        {
            var result = mapper.Map<TicketItemVDto>(item);
            result.IsMine = item.UserId == _currentUser.UserId;
            result.IsFromSupport = item.UserId != ticketUserId;

            if (result.ReplyToTicketItem != null && item.ReplyToTicketItem != null)
            {
                result.ReplyToTicketItem.IsMine = item.ReplyToTicketItem.UserId == _currentUser.UserId;
                result.ReplyToTicketItem.IsFromSupport = item.ReplyToTicketItem.UserId != ticketUserId;
            }

            return result;
        }

        private async Task<string> SanitizeBodyAsync(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            return await SanitizeTextHelper.ToSanitizeAsync(body.Trim());
        }
    }
}