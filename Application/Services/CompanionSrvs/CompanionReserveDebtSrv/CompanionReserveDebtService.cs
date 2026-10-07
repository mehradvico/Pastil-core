using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveDebtSrv
{
    public class ReserveDebtItemDto
    {
        public long ReserveId { get; set; }
        public string ReserveCode { get; set; }
        public string CompanionName { get; set; }
        public string AssistanceName { get; set; }
        public double Amount { get; set; }
        public DateTime UnpaidDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysLeft { get; set; }
        public bool Overdue { get; set; }
    }

    public class ReserveDebtSummaryDto
    {
        public List<ReserveDebtItemDto> Items { get; set; } = new();
        public double TotalAmount { get; set; }
        // true = کاربر رزرو/خرید جدید نمی‌تواند انجام دهد
        public bool Locked { get; set; }
        public double WalletBalance { get; set; }
    }

    public class ReserveDebtPayDto { public long ReserveId { get; set; } }
    public class ReserveDebtMarkPaidDto { public long ReserveId { get; set; } }

    public interface ICompanionReserveDebtService
    {
        Task<BaseResultDto<ReserveDebtSummaryDto>> GetMyDebtsAsync(long userId);
        Task<BaseResultDto> PayFromWalletAsync(long userId, long reserveId);
        Task<BaseResultDto> MarkPaidByClinicAsync(long ownerUserId, long reserveId);

        // ادمین: صفر کردن بدهی (یک رزرو یا همه‌ی بدهی‌های باز یک کاربر)؛ بدون تغییر در کیف پول و حسابداری
        Task<BaseResultDto> WriteOffAsync(long reserveId);
        Task<BaseResultDto> WriteOffAllForUserAsync(long userId);

        // کسر خودکار از کیف پول: بدهی‌های باز یک کاربر را از قدیمی‌ترین شروع می‌کند و تا جایی که کیف پول کافی است می‌پردازد.
        // تعداد بدهی‌های پرداخت‌شده را برمی‌گرداند. بعد از ثبت بدهی (بلافاصله) و در job دوره‌ای صدا زده می‌شود.
        Task<int> CollectForUserAsync(long userId);

        // job: کسر خودکار برای همه‌ی کاربرانِ دارای بدهی باز؛ تعداد بدهی‌های پرداخت‌شده را برمی‌گرداند
        Task<int> AutoCollectAsync();

        // job: یادآوری روزی ۳ بار به کاربرانی که بدهی باز دارند و کیف پولشان کافی نیست؛ تعداد کاربران اعلان‌شده را برمی‌گرداند
        Task<int> SendDebtRemindersAsync();
    }

    public class CompanionReserveDebtService : ICompanionReserveDebtService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IPushNotificationService _push;
        private const int UserBatchSize = 200;

        public CompanionReserveDebtService(IDataBaseContext context, IWalletService walletService, IPushNotificationService push)
        {
            _context = context;
            _walletService = walletService;
            _push = push;
        }

        public async Task<BaseResultDto<ReserveDebtSummaryDto>> GetMyDebtsAsync(long userId)
        {
            try
            {
                var now = DateTime.Now;
                var rows = await _context.CompanionReserves.AsNoTracking()
                    .Where(r => r.BookerId == userId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null && !r.IsCancel && r.OperatorUnpaidDate != null)
                    .OrderBy(r => r.OperatorUnpaidDate)
                    .Select(r => new
                    {
                        r.Id, r.ReserveCode, r.OperatorUnpaidAmount, Date = r.OperatorUnpaidDate.Value,
                        Companion = r.CompanionAssistance.Companion.Name, Assistance = r.CompanionAssistance.Assistance.Name
                    })
                    .ToListAsync();

                var items = rows.Select(r => new ReserveDebtItemDto
                {
                    ReserveId = r.Id, ReserveCode = r.ReserveCode, CompanionName = r.Companion, AssistanceName = r.Assistance,
                    Amount = r.OperatorUnpaidAmount, UnpaidDate = r.Date,
                    DueDate = CompanionReserveDebtRules.DueDate(r.Date),
                    DaysLeft = CompanionReserveDebtRules.DaysLeft(r.Date, now),
                    Overdue = now >= CompanionReserveDebtRules.DueDate(r.Date)
                }).ToList();

                var balance = await _walletService.GetAmountValueAsync(userId);
                return new BaseResultDto<ReserveDebtSummaryDto>(true, new ReserveDebtSummaryDto
                {
                    Items = items,
                    TotalAmount = items.Sum(x => x.Amount),
                    Locked = items.Any(x => x.Overdue),
                    WalletBalance = balance
                });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ReserveDebtSummaryDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        // پرداخت بدهی از کیف پول (اگر کم بود، کاربر اول کیف پول را از درگاه شارژ می‌کند)
        public async Task<BaseResultDto> PayFromWalletAsync(long userId, long reserveId)
        {
            try
            {
                await using var tx = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

                var reserve = await _context.CompanionReserves.AsNoTracking()
                    .Where(r => r.Id == reserveId && r.BookerId == userId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null && !r.IsCancel)
                    .Select(r => new { r.Id, r.OperatorUnpaidAmount, r.Permitted, Commission = r.CompanionAssistance.CommissionPercent })
                    .FirstOrDefaultAsync();
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);

                var balance = await _walletService.GetAmountValueAsync(userId);
                if (balance + 0.01 < reserve.OperatorUnpaidAmount)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtWalletInsufficient);

                // گذار اتمی: پرداخت هم‌زمان دوباره چیزی تغییر نمی‌دهد
                var marked = await _context.CompanionReserves
                    .Where(r => r.Id == reserveId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.OperatorDebtPaidDate, (DateTime?)DateTime.Now)
                        .SetProperty(r => r.OperatorDebtPaidByWallet, true));
                if (marked == 0)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);

                var debit = await _walletService.InsertAsyncDto(new WalletDto
                {
                    Name = $"پرداخت هزینه‌ی خدمت (رزرو {reserveId})",
                    Amount = reserve.OperatorUnpaidAmount,
                    IsIncrease = false,
                    UserId = userId,
                    CompanionReserveId = reserveId,
                    Painding = false
                });
                if (!debit.IsSuccess)
                {
                    await tx.RollbackAsync();
                    return new BaseResultDto(false, Resource.Notification.OperationFailed);
                }

                // مبلغی که از طریق پاستیل پرداخت شد به سهم کلینیک/سایت اضافه می‌شود (اگر رزرو هنوز در تسویه نیامده)
                if (!reserve.Permitted)
                {
                    var site = CompanionReserveDebtRules.SiteShareOf(reserve.OperatorUnpaidAmount, reserve.Commission);
                    var partner = reserve.OperatorUnpaidAmount - site;
                    await _context.CompanionReserves
                        .Where(r => r.Id == reserveId && !r.Permitted)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(r => r.SiteShare, r => r.SiteShare + site)
                            .SetProperty(r => r.CompanionShare, r => r.CompanionShare + partner));
                }

                await tx.CommitAsync();
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        // ---- کسر خودکار از کیف پول + یادآوری (طراحی: backend/Docs/COMPANION_DEBT_AUTO_COLLECT_FA.md)
        private static readonly System.Globalization.CultureInfo Persian = new("fa");

        private IQueryable<Entities.Entities.CompanionReserve> OpenDebts() =>
            _context.CompanionReserves.AsNoTracking().Where(r =>
                r.OperatorUnpaid && r.OperatorDebtPaidDate == null && !r.IsCancel &&
                r.OperatorUnpaidDate != null && r.OperatorUnpaidAmount > 0);

        public async Task<int> CollectForUserAsync(long userId)
        {
            if (userId <= 0)
                return 0;

            var debts = await OpenDebts()
                .Where(r => r.BookerId == userId)
                .OrderBy(r => r.OperatorUnpaidDate).ThenBy(r => r.Id)
                .Select(r => new { r.Id, Amount = r.OperatorUnpaidAmount, Companion = r.CompanionAssistance.Companion.Name })
                .ToListAsync();

            var collected = 0;
            foreach (var debt in debts)
            {
                // از قدیمی‌ترین شروع می‌کنیم؛ به محض اینکه کیف پول یک بدهی را پوشش نداد می‌ایستیم (ترتیب پرداخت حفظ می‌شود)
                var balance = await _walletService.GetAmountValueAsync(userId);
                if (!CompanionReserveDebtRules.WalletCovers(balance, debt.Amount))
                    break;

                var paid = await PayFromWalletAsync(userId, debt.Id);
                if (!paid.IsSuccess)
                    break;

                collected++;
                await SendOnceAsync(PushTypeEnum.PushCompanionDebtCollected, userId, CompanionReserveDebtRules.CollectedKey(debt.Id),
                    FormatAmount(debt.Amount), debt.Companion);
            }
            return collected;
        }

        public async Task<int> AutoCollectAsync()
        {
            var userIds = await OpenDebts().Select(r => r.BookerId).Distinct().OrderBy(id => id).Take(1000).ToListAsync();
            var collected = 0;
            foreach (var userId in userIds)
            {
                try { collected += await CollectForUserAsync(userId); }
                catch { /* خطا روی یک کاربر بقیه را متوقف نکند؛ اجرای بعدی دوباره تلاش می‌کند */ }
            }
            return collected;
        }

        public async Task<int> SendDebtRemindersAsync()
        {
            var now = DateTime.Now;
            var slot = CompanionReserveDebtRules.CurrentReminderSlot(now);
            if (!slot.HasValue)
                return 0;

            var key = CompanionReserveDebtRules.ReminderKey(now, slot.Value);
            var typeId = (long)PushTypeEnum.PushCompanionDebtReminder;
            var userIds = await OpenDebts().Select(r => r.BookerId).Distinct().OrderBy(id => id).Take(1000).ToListAsync();

            var notified = 0;
            foreach (var userId in userIds)
            {
                try
                {
                    var already = await _context.PushNotifications.AsNoTracking()
                        .AnyAsync(n => n.UserId == userId && n.Token2 == key && n.PushPattern.PushTypeId == typeId);
                    if (already)
                        continue;

                    // یک بار دیگر کسر خودکار را امتحان می‌کنیم (شاید کیف پول تازه شارژ شده)؛ اگر همه‌ی بدهی پرداخت شد، پوشی نمی‌رود
                    await CollectForUserAsync(userId);
                    var remaining = await OpenDebts().Where(r => r.BookerId == userId).SumAsync(r => (double?)r.OperatorUnpaidAmount) ?? 0;
                    if (remaining <= 0)
                        continue;

                    await _push.SendPushAsync(PushTypeEnum.PushCompanionDebtReminder, userId, token1: FormatAmount(remaining), token2: key);
                    notified++;
                }
                catch { /* اجرای بعدی (۱۵ دقیقه بعد، همان بازه) دوباره تلاش می‌کند */ }
            }
            return notified;
        }

        // هر (کاربر، نوع، کلید) فقط یک‌بار پوش می‌گیرد: token2 همیشه کلید یکتاست
        private async Task SendOnceAsync(PushTypeEnum type, long userId, string key, string token1, string token3 = null)
        {
            try
            {
                var typeId = (long)type;
                var already = await _context.PushNotifications.AsNoTracking()
                    .AnyAsync(n => n.UserId == userId && n.Token2 == key && n.PushPattern.PushTypeId == typeId);
                if (!already)
                    await _push.SendPushAsync(type, userId, token1: token1, token2: key, token3: token3);
            }
            catch { /* اعلان نباید روی پرداخت اثر بگذارد */ }
        }

        private static string FormatAmount(double amount) => Math.Round(amount).ToString("N0", Persian);

        // کلینیک تأیید می‌کند مبلغ را مستقیم از کاربر گرفته است (بدهی بسته می‌شود، بدون تغییر در حسابداری پاستیل)
        public async Task<BaseResultDto> MarkPaidByClinicAsync(long ownerUserId, long reserveId)
        {
            try
            {
                var marked = await _context.CompanionReserves
                    .Where(r => r.Id == reserveId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null
                        && r.CompanionAssistance.Companion.OwnerId == ownerUserId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.OperatorDebtPaidDate, (DateTime?)DateTime.Now)
                        .SetProperty(r => r.OperatorDebtPaidByWallet, false));
                return marked == 0
                    ? new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> WriteOffAsync(long reserveId)
        {
            try
            {
                var marked = await _context.CompanionReserves
                    .Where(r => r.Id == reserveId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.OperatorDebtPaidDate, (DateTime?)DateTime.Now)
                        .SetProperty(r => r.OperatorDebtPaidByWallet, false));
                return marked == 0
                    ? new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> WriteOffAllForUserAsync(long userId)
        {
            try
            {
                var marked = await _context.CompanionReserves
                    .Where(r => r.BookerId == userId && r.OperatorUnpaid && r.OperatorDebtPaidDate == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.OperatorDebtPaidDate, (DateTime?)DateTime.Now)
                        .SetProperty(r => r.OperatorDebtPaidByWallet, false));
                return marked == 0
                    ? new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }
    }
}
