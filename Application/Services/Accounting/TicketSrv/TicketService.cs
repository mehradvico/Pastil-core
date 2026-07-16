using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.Accounting.TicketItemSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Iface;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Application.Services.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.TicketSrv
{
    public class TicketService : ITicketService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly CurrentUserDto _currentUser;
        private readonly ICodeService _codeService;
        private readonly ITicketMessageService _ticketMessageService;
        private readonly IMessageSenderService _messageSenderService;

        public TicketService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUserHelper, ICodeService codeService, ITicketMessageService ticketMessageService, IMessageSenderService messageSenderService)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUserHelper.CurrentUser;
            _codeService = codeService;
            _ticketMessageService = ticketMessageService;
            _messageSenderService = messageSenderService;
        }

        public async Task<TicketSearchDto> SearchUserAsync(TicketInputDto dto, CancellationToken cancellationToken = default)
        {
            dto ??= new TicketInputDto();
            dto.UserId = _currentUser.UserId;
            dto.AdminId = null;
            dto.AllAdminId = true;
            return await SearchAsync(dto, false, false, cancellationToken);
        }

        public async Task<TicketSearchDto> SearchAdminAsync(TicketInputDto dto, CancellationToken cancellationToken = default)
        {
            dto ??= new TicketInputDto();
            return await SearchAsync(dto, true, false, cancellationToken);
        }

        public async Task<TicketSearchDto> SearchCurrentAdminAsync(TicketInputDto dto, CancellationToken cancellationToken = default)
        {
            dto ??= new TicketInputDto();
            dto.AdminId = _currentUser.UserId;
            dto.AllAdminId = false;
            return await SearchAsync(dto, true, true, cancellationToken);
        }

        public async Task<BaseResultDto<TicketVDto>> FindUserAsync(long id, CancellationToken cancellationToken = default)
        {
            var ticket = await TicketQuery().FirstOrDefaultAsync(s => s.Id == id && s.UserId == _currentUser.UserId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.NothingFound, null);
            }

            var result = await MapTicketAsync(ticket, false, cancellationToken);
            return new BaseResultDto<TicketVDto>(true, result);
        }

        public async Task<BaseResultDto<TicketVDto>> FindAdminAsync(long id, CancellationToken cancellationToken = default)
        {
            var ticket = await TicketQuery().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.NothingFound, null);
            }

            var result = await MapTicketAsync(ticket, true, cancellationToken);
            return new BaseResultDto<TicketVDto>(true, result);
        }
        public async Task<BaseResultDto<TicketVDto>> FindCurrentAdminAsync(long id, CancellationToken cancellationToken = default)
        {
            var ticket = await TicketQuery().FirstOrDefaultAsync(s => s.Id == id && s.AdminId == _currentUser.UserId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.ThisTicketBlongsToAnotherAdmin, null);
            }

            var result = await MapTicketAsync(ticket, true, cancellationToken);
            return new BaseResultDto<TicketVDto>(true, result);
        }
        public async Task<BaseResultDto<TicketVDto>> InsertUserAsyncDto(CreateTicketDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.NothingFound, null);
            }

            var validationResult = await ValidateCreateTicketAsync(dto.Name, dto.TicketCategoryId, dto.ProductId, cancellationToken);

            if (!validationResult.IsSuccess)
            {
                return new BaseResultDto<TicketVDto>(false, validationResult.Messages, null);
            }

            var waitingStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Waiting.ToString());
            var normalImportance = await _codeService.GetByLabelAsync(TicketImportanceEnum.TicketImportance_Normal.ToString());

            if (waitingStatus == null || normalImportance == null)
            {
                return new BaseResultDto<TicketVDto>(false, "تنظیمات تیکت کامل نیست.", null);
            }

            var createDate = DateTime.Now;
            var ticket = new Ticket
            {
                Name = await SanitizeTextHelper.ToSanitizeAsync(dto.Name.Trim()),
                UserId = _currentUser.UserId,
                AdminId = null,
                StatusId = waitingStatus.Id,
                ImportanceId = normalImportance.Id,
                TicketCategoryId = dto.TicketCategoryId,
                ProductId = dto.ProductId,
                CreateDate = createDate,
                UpdateDate = createDate,
                CloseDate = null,
                Deleted = false
            };

            var messageResult = await _ticketMessageService.PrepareInitialMessageAsync(ticket, _currentUser.UserId, dto.Body, dto.FileId, cancellationToken);

            if (!messageResult.IsSuccess)
            {
                return new BaseResultDto<TicketVDto>(false, messageResult.Messages, null);
            }

            ticket.TicketItems.Add(messageResult.Data);

            try
            {
                await _context.Tickets.AddAsync(ticket, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.SomethingWentWrong, null);
            }

            await SendMessageSafeAsync(MessageTypeEnum.UserRegisterTicket, _currentUser.Mobile, _currentUser.Email, _currentUser.FullName, ticket.Id.ToString());
            return await FindUserAsync(ticket.Id, cancellationToken);
        }

        public async Task<BaseResultDto<TicketVDto>> InsertAdminAsyncDto(CreateAdminTicketDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.NothingFound, null);
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.UserId, cancellationToken);

            if (user == null)
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.UserNotFound, null);
            }

            var validationResult = await ValidateCreateTicketAsync(dto.Name, dto.TicketCategoryId, dto.ProductId, cancellationToken);

            if (!validationResult.IsSuccess)
            {
                return new BaseResultDto<TicketVDto>(false, validationResult.Messages, null);
            }

            var answeredStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Answered.ToString());
            var normalImportance = await _codeService.GetByLabelAsync(TicketImportanceEnum.TicketImportance_Normal.ToString());

            if (answeredStatus == null || normalImportance == null)
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.TicketStatusIsNotComplete, null);
            }

            var importanceId = dto.ImportanceId ?? normalImportance.Id;

            if (!IsValidImportance(importanceId))
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.NothingFound, null);
            }

            var createDate = DateTime.Now;
            var ticket = new Ticket
            {
                Name = await SanitizeTextHelper.ToSanitizeAsync(dto.Name.Trim()),
                UserId = user.Id,
                AdminId = _currentUser.UserId,
                StatusId = answeredStatus.Id,
                ImportanceId = importanceId,
                TicketCategoryId = dto.TicketCategoryId,
                ProductId = dto.ProductId,
                CreateDate = createDate,
                UpdateDate = createDate,
                CloseDate = null,
                Deleted = false
            };

            var messageResult = await _ticketMessageService.PrepareInitialMessageAsync(ticket, _currentUser.UserId, dto.Body, dto.FileId, cancellationToken);

            if (!messageResult.IsSuccess)
            {
                return new BaseResultDto<TicketVDto>(false, messageResult.Messages, null);
            }

            ticket.TicketItems.Add(messageResult.Data);

            try
            {
                await _context.Tickets.AddAsync(ticket, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                return new BaseResultDto<TicketVDto>(false, Resource.Notification.SomethingWentWrong, null);
            }

            var userFullName = string.Format("{0} {1}", user.FirstName, user.LastName);
            await SendMessageSafeAsync(MessageTypeEnum.AdminRegisterTicket, user.Mobile, user.Email, userFullName, ticket.Id.ToString());
            return await FindAdminAsync(ticket.Id, cancellationToken);
        }

        public async Task<BaseResultDto> ChangeStatusAsync(ChangeTicketStatusDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null || !IsValidStatus(dto.StatusId))
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            var ticket = await _context.Tickets.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            var updateDate = DateTime.Now;
            ticket.StatusId = dto.StatusId;
            ticket.UpdateDate = updateDate;
            ticket.CloseDate = dto.StatusId == (long)TicketStatusEnum.TicketStatus_Close ? updateDate : null;

            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> ChangeImportanceAsync(ChangeTicketImportanceDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null || !IsValidImportance(dto.ImportanceId))
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            var ticket = await _context.Tickets.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            ticket.ImportanceId = dto.ImportanceId;
            ticket.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> AssignAdminAsync(AssignTicketAdminDto dto, CancellationToken cancellationToken = default)
        {
            if (dto == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            var ticket = await _context.Tickets.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            if (dto.AdminId.HasValue)
            {
                var adminExists = await _context.Users.AnyAsync(s => s.Id == dto.AdminId.Value, cancellationToken);

                if (!adminExists)
                {
                    return new BaseResultDto(false, Resource.Notification.UserNotFound);
                }
            }

            ticket.AdminId = dto.AdminId;
            ticket.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var ticket = await _context.Tickets.AsTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (ticket == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            ticket.Deleted = true;
            ticket.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true);
        }

        public async Task CloseTicketAsync(int hours = 24, CancellationToken cancellationToken = default)
        {
            if (hours < 1)
            {
                hours = 24;
            }

            var answeredStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Answered.ToString());
            var closeStatus = await _codeService.GetByLabelAsync(TicketStatusEnum.TicketStatus_Close.ToString());

            if (answeredStatus == null || closeStatus == null)
            {
                return;
            }

            var closeDate = DateTime.Now;
            var maximumUpdateDate = closeDate.AddHours(-hours);

            await _context.Tickets.Where(s => s.StatusId == answeredStatus.Id && s.UpdateDate < maximumUpdateDate).ExecuteUpdateAsync(setters => setters.SetProperty(s => s.StatusId, closeStatus.Id).SetProperty(s => s.CloseDate, closeDate), cancellationToken);
        }

        private async Task<TicketSearchDto> SearchAsync(TicketInputDto dto, bool isAdmin, bool currentAdminOnly, CancellationToken cancellationToken)
        {
            var pageIndex = dto.PageIndex < 1 ? 1 : dto.PageIndex;
            var pageSize = dto.PageSize < 1 ? 20 : dto.PageSize > 100 ? 100 : dto.PageSize;
            var query = TicketQuery();

            if (dto.UserId.HasValue)
            {
                query = query.Where(s => s.UserId == dto.UserId.Value);
            }

            if (currentAdminOnly)
            {
                query = query.Where(s => s.AdminId == _currentUser.UserId);
            }
            else if (!dto.AllAdminId && dto.AdminId.HasValue)
            {
                query = query.Where(s => s.AdminId == dto.AdminId.Value);
            }

            if (dto.Status.HasValue)
            {
                query = query.Where(s => s.StatusId == (long)dto.Status.Value);
            }

            if (dto.Importance.HasValue)
            {
                query = query.Where(s => s.ImportanceId == (long)dto.Importance.Value);
            }

            if (dto.TicketCategory.HasValue)
            {
                query = query.Where(s => s.TicketCategoryId == (long)dto.TicketCategory.Value);
            }

            if (dto.ProductId.HasValue)
            {
                query = query.Where(s => s.ProductId == dto.ProductId.Value);
            }

            if (dto.IsAssigned.HasValue)
            {
                query = dto.IsAssigned.Value ? query.Where(s => s.AdminId.HasValue) : query.Where(s => !s.AdminId.HasValue);
            }

            if (dto.DateFrom.HasValue)
            {
                query = query.Where(s => s.CreateDate >= dto.DateFrom.Value);
            }

            if (dto.DateTo.HasValue)
            {
                query = query.Where(s => s.CreateDate <= dto.DateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var q = dto.Q.Trim();
                query = query.Where(s => s.Name.Contains(q) || s.User.FirstName.Contains(q) || s.User.LastName.Contains(q) || s.Id.ToString() == q);
            }

            if (dto.HasUnreadMessages.HasValue)
            {
                if (isAdmin)
                {
                    query = dto.HasUnreadMessages.Value ? query.Where(s => s.TicketItems.Any(item => !item.IsSeen && item.UserId == s.UserId)) : query.Where(s => !s.TicketItems.Any(item => !item.IsSeen && item.UserId == s.UserId));
                }
                else
                {
                    query = dto.HasUnreadMessages.Value ? query.Where(s => s.TicketItems.Any(item => !item.IsSeen && item.UserId != s.UserId)) : query.Where(s => !s.TicketItems.Any(item => !item.IsSeen && item.UserId != s.UserId));
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var matchingTicketIds = query.Select(s => s.Id);

            var totalUnreadCount = isAdmin
                ? await _context.TicketItems.Where(s => matchingTicketIds.Contains(s.TicketId) && !s.IsSeen && s.UserId == s.Ticket.UserId).CountAsync(cancellationToken)
                : await _context.TicketItems.Where(s => matchingTicketIds.Contains(s.TicketId) && !s.IsSeen && s.UserId != _currentUser.UserId).CountAsync(cancellationToken);

            query = dto.SortBy == SortEnum.Old ? query.OrderBy(s => s.UpdateDate) : query.OrderByDescending(s => s.UpdateDate);

            var tickets = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            var ticketIds = tickets.Select(s => s.Id).ToList();
            var result = new TicketSearchDto(dto)
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalUnreadCount = totalUnreadCount
            };

            if (ticketIds.Count == 0)
            {
                return result;
            }

            var lastMessageIds = await _context.TicketItems.Where(s => ticketIds.Contains(s.TicketId)).GroupBy(s => s.TicketId).Select(group => group.Max(s => s.Id)).ToListAsync(cancellationToken);
            var lastMessages = await _context.TicketItems.AsNoTracking().Include(s => s.File).Where(s => lastMessageIds.Contains(s.Id)).ToListAsync(cancellationToken);

            var unreadCounts = isAdmin
                ? await _context.TicketItems.Where(s => ticketIds.Contains(s.TicketId) && !s.IsSeen && s.UserId == s.Ticket.UserId).GroupBy(s => s.TicketId).Select(group => new { TicketId = group.Key, Count = group.Count() }).ToDictionaryAsync(s => s.TicketId, s => s.Count, cancellationToken)
                : await _context.TicketItems.Where(s => ticketIds.Contains(s.TicketId) && !s.IsSeen && s.UserId != _currentUser.UserId).GroupBy(s => s.TicketId).Select(group => new { TicketId = group.Key, Count = group.Count() }).ToDictionaryAsync(s => s.TicketId, s => s.Count, cancellationToken);

            var lastMessageDictionary = lastMessages.ToDictionary(s => s.TicketId);

            foreach (var ticket in tickets)
            {
                var item = mapper.Map<TicketVDto>(ticket);
                item.CanReply = ticket.StatusId != (long)TicketStatusEnum.TicketStatus_Close;
                item.UnreadCount = unreadCounts.TryGetValue(ticket.Id, out var unreadCount) ? unreadCount : 0;

                if (lastMessageDictionary.TryGetValue(ticket.Id, out var lastMessage))
                {
                    item.LastMessage = MapMessagePreview(lastMessage, ticket.UserId);
                }

                result.List.Add(item);
            }

            return result;
        }

        private async Task<TicketVDto> MapTicketAsync(Ticket ticket, bool isAdmin, CancellationToken cancellationToken)
        {
            var result = mapper.Map<TicketVDto>(ticket);
            result.CanReply = ticket.StatusId != (long)TicketStatusEnum.TicketStatus_Close;

            var lastMessage = await _context.TicketItems.AsNoTracking().Include(s => s.File).Where(s => s.TicketId == ticket.Id).OrderByDescending(s => s.Id).FirstOrDefaultAsync(cancellationToken);

            result.UnreadCount = isAdmin
                ? await _context.TicketItems.CountAsync(s => s.TicketId == ticket.Id && !s.IsSeen && s.UserId == ticket.UserId, cancellationToken)
                : await _context.TicketItems.CountAsync(s => s.TicketId == ticket.Id && !s.IsSeen && s.UserId != ticket.UserId, cancellationToken);

            if (lastMessage != null)
            {
                result.LastMessage = MapMessagePreview(lastMessage, ticket.UserId);
            }

            return result;
        }

        private TicketItemMinVDto MapMessagePreview(TicketItem item, long ticketUserId)
        {
            var result = mapper.Map<TicketItemMinVDto>(item);
            result.IsMine = item.UserId == _currentUser.UserId;
            result.IsFromSupport = item.UserId != ticketUserId;
            return result;
        }

        private IQueryable<Ticket> TicketQuery()
        {
            return _context.Tickets
                .AsNoTracking()
                .Include(s => s.User)
                .ThenInclude(s => s.Role)
                .Include(s => s.Admin)
                .ThenInclude(s => s.Role)
                .Include(s => s.Status)
                .Include(s => s.Importance)
                .Include(s => s.TicketCategory)
                .Include(s => s.Product);
        }

        private async Task<BaseResultDto> ValidateCreateTicketAsync(string name, long ticketCategoryId, long? productId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheTitle);
            }

            if (name.Trim().Length > 200)
            {
                return new BaseResultDto(false, Resource.Notification.TitleCanNotBeMoreThan200Characters);
            }

            var categoryExists = await _context.Codes.AnyAsync(s => s.Id == ticketCategoryId && s.CodeGroupId == 36 && s.Active, cancellationToken);

            if (!categoryExists)
            {
                return new BaseResultDto(false, Resource.Notification.CategoryNotFound);
            }

            if (productId.HasValue)
            {
                var productExists = await _context.Products.AnyAsync(s => s.Id == productId.Value, cancellationToken);

                if (!productExists)
                {
                    return new BaseResultDto(false, Resource.Notification.ProductIsNotFound);
                }
            }

            return new BaseResultDto(true);
        }

        private static bool IsValidStatus(long statusId)
        {
            return Enum.GetValues<TicketStatusEnum>().Any(s => (long)s == statusId);
        }

        private static bool IsValidImportance(long importanceId)
        {
            return Enum.GetValues<TicketImportanceEnum>().Any(s => (long)s == importanceId);
        }

        private async Task SendMessageSafeAsync(MessageTypeEnum messageType, string mobile, string email, string fullName, string ticketId)
        {
            try
            {
                await _messageSenderService.SendMessageAsync(messageType: messageType, mobileReceptor: mobile, emailReceptor: email, token1: fullName, token2: ticketId, sendDate: DateTime.Now);
            }
            catch
            {
            }
        }
    }
}