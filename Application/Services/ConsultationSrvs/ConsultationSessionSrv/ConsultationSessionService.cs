using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationSessionSrv
{
    // شروع/ورود نماینده و پنجره‌ی زمانی مشاوره. طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md §۵.۳ و §۵.۵
    public class ConsultationSessionService : IConsultationSessionService
    {
        private const int CompleteBatchSize = 100;

        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _pushNotificationService;

        public ConsultationSessionService(IDataBaseContext context, IPushNotificationService pushNotificationService)
        {
            _context = context;
            _pushNotificationService = pushNotificationService;
        }

        // کلینیک‌هایی که کاربر نماینده‌ی مجازشان است: مالک، یا کاربر تخصیص‌یافته روی خدمت «مشاوره آنلاین» همان کلینیک
        private IQueryable<long> AgentCompanionIds(long userId)
        {
            var owned = _context.Companions.AsNoTracking()
                .Where(s => s.OwnerId == userId && !s.Deleted)
                .Select(s => s.Id);
            var staff = _context.CompanionAssistanceUsers.AsNoTracking()
                .Where(s => s.UserId == userId && s.Active && !s.Deleted &&
                            !s.CompanionAssistance.Deleted && s.CompanionAssistance.Active &&
                            s.CompanionAssistance.AssistanceId == ConsultationRules.AssistanceId)
                .Select(s => s.CompanionAssistance.CompanionId);
            return owned.Union(staff);
        }

        private Task<bool> IsOwnerAsync(long userId, long companionId) =>
            _context.Companions.AsNoTracking().AnyAsync(s => s.Id == companionId && s.OwnerId == userId && !s.Deleted);

        public async Task<BaseResultDto<List<ConsultationAgentItemVDto>>> GetForAgentAsync(long agentUserId)
        {
            try
            {
                var now = DateTime.Now;
                var paid = (int)ConsultationPurchaseStatusEnum.Paid;
                var active = (int)ConsultationPurchaseStatusEnum.Active;
                var companionIds = AgentCompanionIds(agentUserId);

                var rows = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => companionIds.Contains(s.CompanionId) && (s.Status == paid || s.Status == active))
                    .Include(s => s.User).ThenInclude(u => u.Picture)
                    .Include(s => s.Companion)
                    .OrderBy(s => s.Status == active ? 0 : 1)
                    .ThenBy(s => s.PaidDate)
                    .Take(100)
                    .ToListAsync();

                var ownedIds = await _context.Companions.AsNoTracking()
                    .Where(s => s.OwnerId == agentUserId && !s.Deleted).Select(s => s.Id).ToListAsync();

                var list = rows.Select(s => new ConsultationAgentItemVDto
                {
                    Id = s.Id,
                    PurchaseCode = s.PurchaseCode,
                    CompanionId = s.CompanionId,
                    CompanionName = s.Companion?.Name,
                    PackageName = s.PackageName,
                    ChannelId = s.ChannelId,
                    DurationMinutes = s.DurationMinutes,
                    Status = s.Status,
                    PaidDate = s.PaidDate,
                    StartDeadline = s.StartDeadline,
                    StartDate = s.StartDate,
                    ExpireDate = s.ExpireDate,
                    OnlineSessionId = s.OnlineSessionId,
                    UserId = s.UserId,
                    UserFullName = $"{s.User?.FirstName} {s.User?.LastName}".Trim(),
                    UserMobile = s.User?.Mobile,
                    UserPicture = ToPictureVDto(s.User?.Picture),
                    AgentUserId = s.AgentUserId,
                    CanStart = ConsultationPurchaseRules.CanStart(s.Status, s.StartDeadline, now),
                    CanEnter = ConsultationPurchaseRules.CanAgentEnter(s.Status, s.ExpireDate, now, s.AgentUserId, agentUserId, ownedIds.Contains(s.CompanionId)),
                    ServerNow = now
                }).ToList();

                return new BaseResultDto<List<ConsultationAgentItemVDto>>(true, list);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationAgentItemVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationSessionInfoVDto>> StartAsync(long agentUserId, long purchaseId)
        {
            try
            {
                var purchase = await _context.ConsultationPurchases.AsNoTracking()
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == purchaseId);
                if (purchase == null)
                    return Fail(Resource.Notification.NothingFound);

                if (!await AgentCompanionIds(agentUserId).ContainsAsync(purchase.CompanionId))
                    return Fail(Resource.Notification.AccessDenied);

                var now = DateTime.Now;
                if (purchase.Status == (int)ConsultationPurchaseStatusEnum.Active)
                    return Fail(Resource.Notification.ConsultationAlreadyStarted);
                if (purchase.Status != (int)ConsultationPurchaseStatusEnum.Paid)
                    return Fail(Resource.Notification.InvalidData);
                if (!ConsultationPurchaseRules.CanStart(purchase.Status, purchase.StartDeadline, now))
                    return Fail(Resource.Notification.ConsultationStartDeadlinePassed);

                var expire = ConsultationPurchaseRules.ComputeExpireDate(now, purchase.DurationMinutes);

                // تراکنش یکپارچه: جلسه‌ی آنلاین + گذار اتمی خرید. اگر همزمان نماینده‌ی دیگری یا لغو کاربر برنده شد، هیچ جلسه‌ای باقی نمی‌ماند.
                await using var transaction = _context.CurrentTransaction == null
                    ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : null;

                var session = new OnlineSession
                {
                    InitiatorUserId = agentUserId,
                    TargetUserId = purchase.UserId,
                    ChannelId = purchase.ChannelId,
                    CreateDate = now,
                    ExpireDate = expire,
                    ConsultationPurchaseId = purchase.Id
                };
                await _context.OnlineSessions.AddAsync(session);
                await _context.SaveChangesAsync();

                var paid = (int)ConsultationPurchaseStatusEnum.Paid;
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == purchase.Id && s.Status == paid && (s.StartDeadline == null || s.StartDeadline >= now))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Active)
                        .SetProperty(s => s.AgentUserId, (long?)agentUserId)
                        .SetProperty(s => s.StartDate, (DateTime?)now)
                        .SetProperty(s => s.ExpireDate, (DateTime?)expire)
                        .SetProperty(s => s.OnlineSessionId, (long?)session.Id));
                if (affected == 0)
                    return Fail(Resource.Notification.ConsultationAlreadyStarted); // dispose ⇒ rollback جلسه‌ی ساخته‌شده

                if (transaction != null)
                    await transaction.CommitAsync();

                await NotifyUserAsync(purchase, agentUserId, session.Id);

                return new BaseResultDto<ConsultationSessionInfoVDto>(true, ToInfo(purchase, session.Id, now, expire, now));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationSessionInfoVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationSessionInfoVDto>> EnterAsync(long agentUserId, long purchaseId)
        {
            try
            {
                var purchase = await _context.ConsultationPurchases.AsNoTracking()
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == purchaseId);
                if (purchase == null || !purchase.OnlineSessionId.HasValue)
                    return Fail(Resource.Notification.NothingFound);

                var now = DateTime.Now;
                var isOwner = await IsOwnerAsync(agentUserId, purchase.CompanionId);
                if (purchase.AgentUserId != agentUserId && !isOwner)
                    return Fail(Resource.Notification.AccessDenied);
                if (!ConsultationPurchaseRules.IsWindowOpen(purchase.Status, purchase.ExpireDate, now))
                    return Fail(Resource.Notification.ConsultationWindowEnded);

                return new BaseResultDto<ConsultationSessionInfoVDto>(true,
                    ToInfo(purchase, purchase.OnlineSessionId.Value, purchase.StartDate ?? now, purchase.ExpireDate.Value, now));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationSessionInfoVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<ConsultationActiveWindowVDto>>> GetActiveWindowsAsync(long userId)
        {
            try
            {
                var now = DateTime.Now;
                var active = (int)ConsultationPurchaseStatusEnum.Active;
                var rows = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.UserId == userId && s.Status == active && s.OnlineSessionId != null && s.ExpireDate > now)
                    .Include(s => s.Companion)
                    .Include(s => s.AgentUser)
                    .OrderBy(s => s.ExpireDate)
                    .Take(10)
                    .ToListAsync();

                var list = rows.Select(s => new ConsultationActiveWindowVDto
                {
                    PurchaseId = s.Id,
                    OnlineSessionId = s.OnlineSessionId.Value,
                    PackageName = s.PackageName,
                    ChannelId = s.ChannelId,
                    DurationMinutes = s.DurationMinutes,
                    StartDate = s.StartDate ?? now,
                    ExpireDate = s.ExpireDate.Value,
                    ServerNow = now,
                    CompanionName = s.Companion?.Name,
                    AgentFullName = $"{s.AgentUser?.FirstName} {s.AgentUser?.LastName}".Trim()
                }).ToList();

                return new BaseResultDto<List<ConsultationActiveWindowVDto>>(true, list);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationActiveWindowVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<int> CompleteExpiredAsync()
        {
            var now = DateTime.Now;
            var active = (int)ConsultationPurchaseStatusEnum.Active;
            var due = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => s.Status == active && s.ExpireDate != null && s.ExpireDate <= now)
                .OrderBy(s => s.Id)
                .Select(s => new { s.Id, s.OnlineSessionId })
                .Take(CompleteBatchSize)
                .ToListAsync();

            var completed = 0;
            foreach (var item in due)
            {
                try
                {
                    var affected = await _context.ConsultationPurchases
                        .Where(s => s.Id == item.Id && s.Status == active)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Completed));
                    if (affected == 0)
                        continue;

                    // بستن جلسه‌ی آنلاین: دیگر پیامی ارسال/تماسی برقرار نمی‌شود (خواندن تاریخچه‌ی چت باز می‌ماند)
                    if (item.OnlineSessionId.HasValue)
                    {
                        await _context.OnlineSessions
                            .Where(s => s.Id == item.OnlineSessionId.Value && s.EndDate == null)
                            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.EndDate, (DateTime?)now));
                    }
                    completed++;
                }
                catch
                {
                    // خطا روی یک خرید بقیه را متوقف نکند؛ اجرای بعدی دوباره تلاش می‌کند
                }
            }
            return completed;
        }

        // خبر شروع به کاربر: چت ⇒ دعوت به گفتگو، تلفنی ⇒ اطلاع تماس؛ تماس صوتی/تصویری با اتصال نماینده به هاب زنگ می‌خورد
        private async Task NotifyUserAsync(ConsultationPurchase purchase, long agentUserId, long sessionId)
        {
            try
            {
                var agent = await _context.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Id == agentUserId);
                var agentName = $"{agent?.FirstName} {agent?.LastName}".Trim();
                if (purchase.ChannelId == (int)OnlineSessionChannelEnum.Chat)
                    await _pushNotificationService.SendPushAsync(PushTypeEnum.PushOnlineSessionChatInvite, purchase.UserId, agentName, sessionId.ToString());
                else if (purchase.ChannelId == (int)OnlineSessionChannelEnum.Phone)
                    await _pushNotificationService.SendPushAsync(PushTypeEnum.PushOnlineSessionPhoneCallStarted, purchase.UserId, agentName, sessionId.ToString());
            }
            catch
            {
                // نتیجه‌ی اعلان نباید شروع مشاوره را خراب کند
            }
        }

        private static BaseResultDto<ConsultationSessionInfoVDto> Fail(string message) =>
            new BaseResultDto<ConsultationSessionInfoVDto>(false, message, null);

        private static ConsultationSessionInfoVDto ToInfo(ConsultationPurchase purchase, long sessionId, DateTime start, DateTime expire, DateTime now) => new()
        {
            PurchaseId = purchase.Id,
            OnlineSessionId = sessionId,
            ChannelId = purchase.ChannelId,
            StartDate = start,
            ExpireDate = expire,
            ServerNow = now,
            UserId = purchase.UserId,
            UserFullName = $"{purchase.User?.FirstName} {purchase.User?.LastName}".Trim(),
            UserMobile = purchase.User?.Mobile
        };

        private static PictureVDto ToPictureVDto(Entities.Entities.Picture picture) => picture == null ? null : new PictureVDto
        {
            Id = picture.Id,
            BaseUrl = picture.Url,
            Url = picture.Url + "/" + picture.Name,
            OrginalName = picture.OrginalName,
            GuidName = picture.GuidName,
            Extension = picture.Extension
        };
    }
}
