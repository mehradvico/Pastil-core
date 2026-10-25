using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities.CommonField;
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
        /// <summary>ReserveDebtKindEnum: ۰ همراه (قدیمی)، ۱ مربی، ۲ کلینیک، ۳ آرایشگاه؛ موقع پرداخت همراه ReserveId برگردانده شود.</summary>
        public int Kind { get; set; }
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

    // Kind خالی/۰ = رزرو همراه (رفتار قبلی)
    public class ReserveDebtPayDto { public long ReserveId { get; set; } public int Kind { get; set; } }
    public class ReserveDebtMarkPaidDto { public long ReserveId { get; set; } public int Kind { get; set; } }

    public interface ICompanionReserveDebtService
    {
        Task<BaseResultDto<ReserveDebtSummaryDto>> GetMyDebtsAsync(long userId);
        Task<BaseResultDto> PayFromWalletAsync(long userId, long reserveId, ReserveDebtKindEnum kind = ReserveDebtKindEnum.Companion);
        Task<BaseResultDto> MarkPaidByClinicAsync(long ownerUserId, long reserveId, ReserveDebtKindEnum kind = ReserveDebtKindEnum.Companion);

        // ادمین: صفر کردن بدهی (یک رزرو یا همه‌ی بدهی‌های باز یک کاربر)؛ بدون تغییر در کیف پول و حسابداری
        Task<BaseResultDto> WriteOffAsync(long reserveId, ReserveDebtKindEnum kind = ReserveDebtKindEnum.Companion);
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

                var all = rows.Select(r => new DebtRow(ReserveDebtKindEnum.Companion, r.Id, r.ReserveCode, r.Companion, r.Assistance, r.OperatorUnpaidAmount, r.Date))
                    .Concat(await VenueDebtRowsAsync(userId))
                    .OrderBy(r => r.Date).ThenBy(r => r.Kind).ThenBy(r => r.Id);
                var items = all.Select(r => new ReserveDebtItemDto
                {
                    ReserveId = r.Id, Kind = (int)r.Kind, ReserveCode = r.ReserveCode, CompanionName = r.Provider, AssistanceName = r.Assistance,
                    Amount = r.Amount, UnpaidDate = r.Date,
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
        public async Task<BaseResultDto> PayFromWalletAsync(long userId, long reserveId, ReserveDebtKindEnum kind = ReserveDebtKindEnum.Companion)
        {
            switch (kind)
            {
                case ReserveDebtKindEnum.Companion:
                    break;
                case ReserveDebtKindEnum.Coach:
                    return await PayVenueFromWalletAsync(_context.CoachReserves, userId, reserveId,
                        Application.Services.ProductSrvs.WalletSrv.WalletDescriptionRules.CoachDebtName(reserveId), w => w.CoachReserveId = reserveId);
                case ReserveDebtKindEnum.Clinic:
                    return await PayVenueFromWalletAsync(_context.ClinicReserves, userId, reserveId,
                        Application.Services.ProductSrvs.WalletSrv.WalletDescriptionRules.ClinicDebtName(reserveId), w => w.ClinicReserveId = reserveId);
                case ReserveDebtKindEnum.Groomer:
                    return await PayVenueFromWalletAsync(_context.GroomerReserves, userId, reserveId,
                        Application.Services.ProductSrvs.WalletSrv.WalletDescriptionRules.GroomerDebtName(reserveId), w => w.GroomerReserveId = reserveId);
                default:
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);
            }
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
                    // کلید دفتر؛ در نمایش «پرداخت بدهی خدمت رزرو RSV-… از کیف پول» می‌شود (WalletDescriptionRules)
                    Name = Application.Services.ProductSrvs.WalletSrv.WalletDescriptionRules.CompanionDebtName(reserveId),
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

        // همان پرداخت بالا برای رزرو مربی/کلینیک/آرایشگاه: گذار اتمی، کسر از کیف پول، و (تا قبل از تسویه) افزودن به سهم ارائه‌دهنده/سایت
        private async Task<BaseResultDto> PayVenueFromWalletAsync<T>(DbSet<T> set, long userId, long reserveId, string ledgerName, Action<WalletDto> link)
            where T : class, IVenueCompletableReserve
        {
            try
            {
                await using var tx = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

                var reserve = await VenueReserveDebtQueries.Open(set.AsNoTracking())
                    .Where(r => r.Id == reserveId && r.BookerId == userId)
                    .Select(r => new { r.Id, r.DebtAmount, r.Permitted, r.CommissionPercent })
                    .FirstOrDefaultAsync();
                if (reserve == null)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);

                var balance = await _walletService.GetAmountValueAsync(userId);
                if (!CompanionReserveDebtRules.WalletCovers(balance, reserve.DebtAmount))
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtWalletInsufficient);

                var marked = await VenueReserveDebtQueries.Open(set)
                    .Where(r => r.Id == reserveId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.DebtPaidDate, (DateTime?)DateTime.Now)
                        .SetProperty(r => r.DebtPaidByWallet, true));
                if (marked == 0)
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);

                var wallet = new WalletDto { Name = ledgerName, Amount = reserve.DebtAmount, IsIncrease = false, UserId = userId, Painding = false };
                link(wallet);
                var debit = await _walletService.InsertAsyncDto(wallet);
                if (!debit.IsSuccess)
                {
                    await tx.RollbackAsync();
                    return new BaseResultDto(false, Resource.Notification.OperationFailed);
                }

                // کمیسیون فقط روی پولی که از پاستیل عبور کرده (با همان درصد اسنپ‌شاتِ رزرو)
                if (!reserve.Permitted)
                {
                    var site = CompanionReserveDebtRules.SiteShareOf(reserve.DebtAmount, reserve.CommissionPercent);
                    var provider = reserve.DebtAmount - site;
                    await set
                        .Where(r => r.Id == reserveId && !r.Permitted)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(r => r.SiteShare, r => r.SiteShare + site)
                            .SetProperty(r => r.ProviderShare, r => r.ProviderShare + provider));
                }

                await tx.CommitAsync();
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        private sealed record DebtRow(ReserveDebtKindEnum Kind, long Id, string ReserveCode, string Provider, string Assistance, double Amount, DateTime Date);

        // بدهی‌های باز رزرو مربی/کلینیک/آرایشگاه یک کاربر
        private async Task<List<DebtRow>> VenueDebtRowsAsync(long userId)
        {
            var rows = new List<DebtRow>();
            rows.AddRange(await VenueReserveDebtQueries.Open(_context.CoachReserves.AsNoTracking()).Where(r => r.BookerId == userId)
                .Select(r => new DebtRow(ReserveDebtKindEnum.Coach, r.Id, r.ReserveCode, r.Coach.Name, r.CoachService.Assistance.Name, r.DebtAmount, r.DoneDate.Value)).ToListAsync());
            rows.AddRange(await VenueReserveDebtQueries.Open(_context.ClinicReserves.AsNoTracking()).Where(r => r.BookerId == userId)
                .Select(r => new DebtRow(ReserveDebtKindEnum.Clinic, r.Id, r.ReserveCode, r.Clinic.Name, r.ClinicService.Assistance.Name, r.DebtAmount, r.DoneDate.Value)).ToListAsync());
            rows.AddRange(await VenueReserveDebtQueries.Open(_context.GroomerReserves.AsNoTracking()).Where(r => r.BookerId == userId)
                .Select(r => new DebtRow(ReserveDebtKindEnum.Groomer, r.Id, r.ReserveCode, r.Groomer.Name, r.GroomerService.Assistance.Name, r.DebtAmount, r.DoneDate.Value)).ToListAsync());
            return rows;
        }

        // کاربران دارای بدهی باز در هر چهار نوع رزرو (برای job کسر خودکار و یادآوری)
        private async Task<List<long>> DebtorIdsAsync()
        {
            var ids = await OpenDebts().Select(r => r.BookerId).Distinct().Take(1000).ToListAsync();
            ids.AddRange(await VenueReserveDebtQueries.Open(_context.CoachReserves.AsNoTracking()).Select(r => r.BookerId).Distinct().Take(1000).ToListAsync());
            ids.AddRange(await VenueReserveDebtQueries.Open(_context.ClinicReserves.AsNoTracking()).Select(r => r.BookerId).Distinct().Take(1000).ToListAsync());
            ids.AddRange(await VenueReserveDebtQueries.Open(_context.GroomerReserves.AsNoTracking()).Select(r => r.BookerId).Distinct().Take(1000).ToListAsync());
            return ids.Distinct().OrderBy(id => id).Take(1000).ToList();
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

            var companionDebts = await OpenDebts()
                .Where(r => r.BookerId == userId)
                .Select(r => new { r.Id, Amount = r.OperatorUnpaidAmount, Companion = r.CompanionAssistance.Companion.Name, Date = r.OperatorUnpaidDate.Value })
                .ToListAsync();
            var debts = companionDebts
                .Select(r => new DebtRow(ReserveDebtKindEnum.Companion, r.Id, null, r.Companion, null, r.Amount, r.Date))
                .Concat(await VenueDebtRowsAsync(userId))
                .OrderBy(r => r.Date).ThenBy(r => r.Kind).ThenBy(r => r.Id)
                .ToList();

            var collected = 0;
            foreach (var debt in debts)
            {
                // از قدیمی‌ترین شروع می‌کنیم؛ به محض اینکه کیف پول یک بدهی را پوشش نداد می‌ایستیم (ترتیب پرداخت حفظ می‌شود)
                var balance = await _walletService.GetAmountValueAsync(userId);
                if (!CompanionReserveDebtRules.WalletCovers(balance, debt.Amount))
                    break;

                var paid = await PayFromWalletAsync(userId, debt.Id, debt.Kind);
                if (!paid.IsSuccess)
                    break;

                collected++;
                await SendOnceAsync(PushTypeEnum.PushCompanionDebtCollected, userId, CompanionReserveDebtRules.CollectedKey(debt.Id, debt.Kind),
                    FormatAmount(debt.Amount), debt.Provider);
            }
            return collected;
        }

        public async Task<int> AutoCollectAsync()
        {
            var userIds = await DebtorIdsAsync();
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
            var userIds = await DebtorIdsAsync();

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
                    var remaining = (await OpenDebts().Where(r => r.BookerId == userId).SumAsync(r => (double?)r.OperatorUnpaidAmount) ?? 0)
                                    + (await VenueDebtRowsAsync(userId)).Sum(r => r.Amount);
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
        public async Task<BaseResultDto> MarkPaidByClinicAsync(long ownerUserId, long reserveId, ReserveDebtKindEnum kind = ReserveDebtKindEnum.Companion)
        {
            if (kind != ReserveDebtKindEnum.Companion)
            {
                // مالک یا پرسنلِ همان رزرو تأیید می‌کند مبلغ را مستقیم گرفته
                var allowed = kind switch
                {
                    ReserveDebtKindEnum.Coach => await _context.CoachReserves.AsNoTracking().AnyAsync(r => r.Id == reserveId && (r.Coach.OwnerId == ownerUserId || r.AssigneeUserId == ownerUserId)),
                    ReserveDebtKindEnum.Clinic => await _context.ClinicReserves.AsNoTracking().AnyAsync(r => r.Id == reserveId && (r.Clinic.OwnerId == ownerUserId || r.AssigneeUserId == ownerUserId)),
                    ReserveDebtKindEnum.Groomer => await _context.GroomerReserves.AsNoTracking().AnyAsync(r => r.Id == reserveId && (r.Groomer.OwnerId == ownerUserId || r.AssigneeUserId == ownerUserId)),
                    _ => false,
                };
                return allowed ? await CloseVenueDebtAsync(kind, reserveId) : new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound);
            }
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

        public async Task<BaseResultDto> WriteOffAsync(long reserveId, ReserveDebtKindEnum kind = ReserveDebtKindEnum.Companion)
        {
            if (kind != ReserveDebtKindEnum.Companion)
                return await CloseVenueDebtAsync(kind, reserveId);
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
                marked += await CloseVenueDebtsAsync(_context.CoachReserves, r => r.BookerId == userId)
                          + await CloseVenueDebtsAsync(_context.ClinicReserves, r => r.BookerId == userId)
                          + await CloseVenueDebtsAsync(_context.GroomerReserves, r => r.BookerId == userId);
                return marked == 0
                    ? new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        // بستن بدهی بدون تغییر در کیف پول و حسابداری (دریافت مستقیم ارائه‌دهنده / صفر کردن ادمین)
        private async Task<BaseResultDto> CloseVenueDebtAsync(ReserveDebtKindEnum kind, long reserveId)
        {
            try
            {
                var marked = kind switch
                {
                    ReserveDebtKindEnum.Coach => await CloseVenueDebtsAsync(_context.CoachReserves, r => r.Id == reserveId),
                    ReserveDebtKindEnum.Clinic => await CloseVenueDebtsAsync(_context.ClinicReserves, r => r.Id == reserveId),
                    ReserveDebtKindEnum.Groomer => await CloseVenueDebtsAsync(_context.GroomerReserves, r => r.Id == reserveId),
                    _ => 0,
                };
                return marked == 0
                    ? new BaseResultDto(false, Resource.Notification.UnpaidDebtNotFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        private static Task<int> CloseVenueDebtsAsync<T>(DbSet<T> set, System.Linq.Expressions.Expression<Func<T, bool>> filter)
            where T : class, IVenueCompletableReserve =>
            VenueReserveDebtQueries.Open(set).Where(filter)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.DebtPaidDate, (DateTime?)DateTime.Now)
                    .SetProperty(r => r.DebtPaidByWallet, false));
    }
}
