using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationPurchaseSrv
{
    // خرید پکیج مشاوره آنلاین (کاربر): ساخت خرید، شروع پرداخت (درگاه/کیف پول/تخفیف)، لغو و بازپرداخت.
    // الگو: PastilAiPlanService.PurchaseAsync. طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md
    public class ConsultationPurchaseService : IConsultationPurchaseService
    {
        private const string CancelReasonUser = "UserCancelled";
        private const string CancelReasonAdmin = "AdminCancelled";
        private const string CancelReasonPaymentFailed = "PaymentFailed";
        private const string CancelReasonStartDeadline = "StartDeadlineExpired";
        private const int ExpireBatchSize = 100;

        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IRebateService _rebateService;
        private readonly IPaymentService _paymentService;
        private readonly IConsultationNotificationService _notifications;

        public ConsultationPurchaseService(
            IDataBaseContext context,
            IWalletService walletService,
            IRebateService rebateService,
            IPaymentService paymentService,
            IConsultationNotificationService notifications)
        {
            _notifications = notifications;
            _context = context;
            _walletService = walletService;
            _rebateService = rebateService;
            _paymentService = paymentService;
        }

        public async Task<BaseResultDto> PurchaseAsync(long userId, ConsultationPurchaseCreateDto dto)
        {
            try
            {
                if (dto == null || dto.PackageId <= 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (await Application.Services.CompanionSrvs.CompanionReserveDebtSrv.CompanionReserveDebtRules.IsLockedAsync(_context, userId, DateTime.Now))
                    return new BaseResultDto(false, Resource.Notification.UnpaidDebtLocked);

                var package = await _context.ConsultationPackages.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.PackageId && !s.Deleted && s.Active && s.Price > 0);
                if (package == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                var clinicOpen = await _context.Companions.AsNoTracking()
                    .AnyAsync(s => s.Id == package.CompanionId && !s.Deleted && s.Active && s.Approved);
                if (!clinicOpen)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Id == userId && !s.Deleted && !s.Locked);
                if (user == null)
                    return new BaseResultDto(false, Resource.Notification.UserNotFound);

                // خرید تکراری اشتباهی: همان کلینیک و همان کانال، در حالی که یکی پرداخت‌شده/در جریان دارد
                var paid = (int)ConsultationPurchaseStatusEnum.Paid;
                var active = (int)ConsultationPurchaseStatusEnum.Active;
                var alreadyHasOne = await _context.ConsultationPurchases.AsNoTracking().AnyAsync(s =>
                    s.UserId == userId && s.CompanionId == package.CompanionId && s.ChannelId == package.ChannelId &&
                    (s.Status == paid || s.Status == active));
                if (alreadyHasOne)
                    return new BaseResultDto(false, Resource.Notification.ConsultationAlreadyInProgress);

                // تا وقتی زمانِ یک مشاوره‌ی خریداری‌شده تمام نشده (منتظر شروع و در مهلت، یا در جریان و پنجره‌اش باز)،
                // کاربر مشاوره‌ی دیگری — با هر کلینیک یا روشی — نمی‌تواند بخرد.
                var nowForBlock = DateTime.Now;
                var hasLiveConsultation = await _context.ConsultationPurchases.AsNoTracking().AnyAsync(s =>
                    s.UserId == userId &&
                    ((s.Status == paid && (s.StartDeadline == null || s.StartDeadline > nowForBlock)) ||
                     (s.Status == active && (s.ExpireDate == null || s.ExpireDate > nowForBlock))));
                if (hasLiveConsultation)
                    return new BaseResultDto(false, Resource.Notification.ConsultationAlreadyInProgress);

                // خرید در انتظار پرداختِ در جریان (درگاه باز است) دوباره ساخته نمی‌شود؛ بدون پرداخت‌های قدیمی همین پکیج کنار گذاشته می‌شوند
                var pending = (int)ConsultationPurchaseStatusEnum.PendingPayment;
                var recentWithPayment = DateTime.Now.AddMinutes(-15);
                var inProgress = await _context.ConsultationPurchases.AsNoTracking().AnyAsync(s =>
                    s.UserId == userId && s.ConsultationPackageId == package.Id && s.Status == pending &&
                    s.PaymentId != null && s.CreateDate > recentWithPayment);
                if (inProgress)
                    return new BaseResultDto(false, Resource.Notification.CompanionPaymentStartedFinancialDataLocked);
                await _context.ConsultationPurchases
                    .Where(s => s.UserId == userId && s.ConsultationPackageId == package.Id && s.Status == pending && s.PaymentId == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                        .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                        .SetProperty(s => s.CancelReason, "Superseded"));

                double rebateDiscount = 0;
                long? rebateId = null;
                if (!string.IsNullOrWhiteSpace(dto.RebateCode))
                {
                    var rebateResult = _rebateService.GetRebateByCodeAsync(
                        package.Price, userId, RebateTypeLabels.ConsultationPurchase, dto.RebateCode, package.Id);
                    if (!rebateResult.IsSuccess)
                        return new BaseResultDto(false, messages: rebateResult.Messages);

                    rebateId = rebateResult.Data.Id;
                    rebateDiscount = rebateResult.Data.FinalPrice;
                }

                // فقط موجودی نقد (اعتبار تبلیغاتی باشگاه برای مشاوره تعریف نشده و در برداشت نهایی اعمال نمی‌شود)
                var walletBalance = dto.FromWallet ? await _walletService.GetAmountValueAsync(userId) : 0;
                var amounts = ConsultationPurchaseRules.ComputeAmounts(package.Price, rebateDiscount, dto.FromWallet, walletBalance);
                if (amounts.Gateway > 0 && (!dto.MerchantId.HasValue || dto.MerchantId.Value <= 0))
                    return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

                var typeId = await _context.Codes.AsNoTracking()
                    .Where(s => s.Label == PaymentTypeEnum.PaymentType_ConsultationPurchase.ToString() && s.Active)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
                if (!typeId.HasValue)
                    return new BaseResultDto(false, Resource.Notification.PaymentTypeNotConfiguredInSystem);

                var purchase = new ConsultationPurchase
                {
                    UserId = userId,
                    ConsultationPackageId = package.Id,
                    CompanionId = package.CompanionId,
                    ChannelId = package.ChannelId,
                    DurationMinutes = package.DurationMinutes,
                    PackageName = package.Name,
                    Price = package.Price,
                    Status = pending,
                    FromWallet = dto.FromWallet,
                    WalletPrice = amounts.Wallet,
                    PaymentPrice = amounts.Payable,
                    RebateId = rebateId,
                    RebatePrice = amounts.RebatePrice,
                    Discount = amounts.RebatePrice,
                    CreateDate = DateTime.Now
                };
                if (!await TryInsertWithUniqueCodeAsync(purchase))
                    return new BaseResultDto(false, Resource.Notification.Unsuccess);

                var paymentDto = new PaymentStartDto
                {
                    MerchantId = dto.MerchantId,
                    Amount = amounts.Gateway,
                    GrossAmount = amounts.Payable + amounts.RebatePrice,
                    RebateAmount = amounts.RebatePrice,
                    WalletAmount = amounts.Wallet,
                    RebateId = rebateId,
                    UserId = userId,
                    User = new Application.Services.Dto.UserMinVDto { Id = user.Id, Mobile = user.Mobile, Email = user.Email },
                    TypeId = typeId.Value,
                    CallBackTypeLabel = PaymentCallbackTypeEnum.ConsultationPurchase.ToString(),
                    CallBackId = purchase.Id.ToString()
                };

                var result = await _paymentService.StartPayment(paymentDto);
                if (!result.IsSuccess)
                {
                    await _context.ConsultationPurchases
                        .Where(s => s.Id == purchase.Id && s.Status == pending)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                            .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                            .SetProperty(s => s.CancelReason, CancelReasonPaymentFailed));
                    return result;
                }

                var callbackId = purchase.Id.ToString();
                var paymentId = await _context.Payments.AsNoTracking()
                    .Where(s => s.UserId == userId && s.CallBackTypeLabel == PaymentCallbackTypeEnum.ConsultationPurchase.ToString() && s.CallBackId == callbackId)
                    .OrderByDescending(s => s.Id)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
                if (paymentId.HasValue)
                {
                    // فقط اگر هنوز خرید بدون پرداخت است؛ اگر پرداخت با کیف پول همان لحظه تکمیل شده (Paid) دست نمی‌خورد
                    await _context.ConsultationPurchases
                        .Where(s => s.Id == purchase.Id && s.PaymentId == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.PaymentId, paymentId));
                }
                return result;
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto<List<ConsultationPurchaseVDto>>> GetMineAsync(long userId)
        {
            try
            {
                var now = DateTime.Now;
                var rows = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.Id)
                    .Take(50)
                    .Select(s => new { Purchase = s, CompanionName = s.Companion.Name })
                    .ToListAsync();
                return new BaseResultDto<List<ConsultationPurchaseVDto>>(true, rows.Select(r => ToVDto(r.Purchase, r.CompanionName, now)).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPurchaseVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPurchaseVDto>> GetAsync(long userId, long id)
        {
            try
            {
                var row = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id && s.UserId == userId)
                    .Select(s => new { Purchase = s, CompanionName = s.Companion.Name })
                    .FirstOrDefaultAsync();
                if (row == null)
                    return new BaseResultDto<ConsultationPurchaseVDto>(false, Resource.Notification.NothingFound, null);
                return new BaseResultDto<ConsultationPurchaseVDto>(true, ToVDto(row.Purchase, row.CompanionName, DateTime.Now));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPurchaseVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto> CancelAsync(long userId, long id)
        {
            try
            {
                await using var transaction = _context.CurrentTransaction == null
                    ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : null;

                // گذار اتمی: فقط Paid → Cancelled؛ اگر همزمان نماینده «شروع» را زده باشد (Active) هیچ ردیفی تغییر نمی‌کند
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && s.UserId == userId && s.Status == (int)ConsultationPurchaseStatusEnum.Paid)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                        .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                        .SetProperty(s => s.CancelReason, CancelReasonUser));
                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (!await RefundAsync(id))
                    return new BaseResultDto(false, Resource.Notification.Unsuccess);

                if (transaction != null)
                    await transaction.CommitAsync();
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> AdminCancelAsync(long id, string reason)
        {
            try
            {
                await using var transaction = _context.CurrentTransaction == null
                    ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                    : null;

                var detail = string.IsNullOrWhiteSpace(reason) ? CancelReasonAdmin : reason.Trim();
                if (detail.Length > 500)
                    detail = detail.Substring(0, 500);
                var paid = (int)ConsultationPurchaseStatusEnum.Paid;
                var active = (int)ConsultationPurchaseStatusEnum.Active;

                var purchase = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id && (s.Status == paid || s.Status == active))
                    .Select(s => new { s.Id, s.OnlineSessionId })
                    .FirstOrDefaultAsync();
                if (purchase == null)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                // گذار اتمی؛ اگر همزمان وضعیت عوض شده باشد هیچ ردیفی تغییر نمی‌کند
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && (s.Status == paid || s.Status == active))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                        .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                        .SetProperty(s => s.CancelReason, detail));
                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (purchase.OnlineSessionId.HasValue)
                {
                    await _context.OnlineSessions
                        .Where(s => s.Id == purchase.OnlineSessionId.Value && s.EndDate == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.EndDate, (DateTime?)DateTime.Now));
                }

                if (!await RefundAsync(id))
                    return new BaseResultDto(false, Resource.Notification.Unsuccess);

                if (transaction != null)
                    await transaction.CommitAsync();
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> AdminCompleteAsync(long id)
        {
            try
            {
                var active = (int)ConsultationPurchaseStatusEnum.Active;
                var purchase = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id && s.Status == active)
                    .Select(s => new { s.Id, s.OnlineSessionId })
                    .FirstOrDefaultAsync();
                if (purchase == null)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                var now = DateTime.Now;
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && s.Status == active)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Completed));
                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                if (purchase.OnlineSessionId.HasValue)
                {
                    await _context.OnlineSessions
                        .Where(s => s.Id == purchase.OnlineSessionId.Value && s.EndDate == null)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.EndDate, (DateTime?)now));
                }
                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> ReviewAsync(long userId, long id, Dto.ConsultationPurchaseReviewDto dto)
        {
            try
            {
                if (dto == null || dto.Rate < 1 || dto.Rate > 5)
                    return new BaseResultDto(false, Resource.Notification.InvalidData);

                // گذار اتمی: فقط برای مشاوره‌ی «تکمیل‌شده»‌ای که هنوز نظری برایش ثبت نشده - هر کاربر فقط یک‌بار نظر می‌دهد
                var affected = await _context.ConsultationPurchases
                    .Where(s => s.Id == id && s.UserId == userId &&
                                s.Status == (int)ConsultationPurchaseStatusEnum.Completed &&
                                s.ReviewedAt == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Rate, dto.Rate)
                        .SetProperty(s => s.ReviewText, (dto.Text ?? string.Empty).Trim())
                        .SetProperty(s => s.ReviewedAt, (DateTime?)DateTime.Now));

                if (affected == 0)
                    return new BaseResultDto(false, Resource.Notification.CompanionReserveCommentOnlyForOwnCompletedReserve);

                return new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<int> ExpireOverdueAsync()
        {
            var now = DateTime.Now;
            var paid = (int)ConsultationPurchaseStatusEnum.Paid;
            var ids = await _context.ConsultationPurchases.AsNoTracking()
                .Where(s => s.Status == paid && s.StartDeadline != null && s.StartDeadline < now)
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .Take(ExpireBatchSize)
                .ToListAsync();

            var expired = 0;
            foreach (var id in ids)
            {
                try
                {
                    await using var transaction = _context.CurrentTransaction == null
                        ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                        : null;
                    var affected = await _context.ConsultationPurchases
                        .Where(s => s.Id == id && s.Status == paid)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Expired)
                            .SetProperty(s => s.CancelDate, (DateTime?)now)
                            .SetProperty(s => s.CancelReason, CancelReasonStartDeadline));
                    if (affected == 0)
                        continue;

                    if (!await RefundAsync(id))
                        continue; // rollback با dispose؛ در اجرای بعدی دوباره تلاش می‌شود

                    if (transaction != null)
                        await transaction.CommitAsync();
                    expired++;
                    try { await _notifications.NotifyExpiredRefundAsync(id); } catch { /* اعلان بازپرداخت نباید لغو را خراب کند */ }
                }
                catch
                {
                    // یک خرید خراب نباید بقیه را متوقف کند؛ اجرای بعدی job دوباره تلاش می‌کند
                }
            }
            return expired;
        }

        // بازپرداخت کامل مبلغ پرداخت‌شده به کیف پول؛ idempotent (نام ردیف دفتر + RefundDate). باید داخل تراکنش صدا زده شود.
        private async Task<bool> RefundAsync(long purchaseId)
        {
            var purchase = await _context.ConsultationPurchases.AsNoTracking().FirstOrDefaultAsync(s => s.Id == purchaseId);
            if (purchase == null)
                return false;
            if (purchase.RefundDate.HasValue)
                return true;

            var amount = ConsultationPurchaseRules.RefundAmount(purchase.PaymentPrice);
            if (amount > 0)
            {
                var ledgerName = ConsultationPurchaseRules.RefundLedgerName(purchaseId);
                var alreadyCredited = await _context.Wallets.AsNoTracking().AnyAsync(s => s.Name == ledgerName && s.UserId == purchase.UserId && !s.Deleted);
                if (!alreadyCredited)
                {
                    var credit = await _walletService.InsertAsyncDto(new WalletDto
                    {
                        Name = ledgerName,
                        Amount = amount,
                        IsIncrease = true,
                        UserId = purchase.UserId,
                        Painding = false
                    });
                    if (!credit.IsSuccess)
                        return false;
                }
            }

            await _context.ConsultationPurchases
                .Where(s => s.Id == purchaseId && s.RefundDate == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Refunded)
                    // خرید بازپرداخت‌شده درآمدی برای کلینیک/پاستیل ندارد؛ مبلغ پرداختی (PaymentPrice) برای سابقه می‌ماند
                    .SetProperty(s => s.CompanionShare, 0d)
                    .SetProperty(s => s.SiteShare, 0d)
                    .SetProperty(s => s.RefundDate, (DateTime?)DateTime.Now));
            return true;
        }

        private async Task<bool> TryInsertWithUniqueCodeAsync(ConsultationPurchase purchase)
        {
            for (var attempt = 0; attempt < 3; attempt++)
            {
                purchase.PurchaseCode = ConsultationPurchaseRules.NewPurchaseCode(DateTime.Now, RandomNumberGenerator.GetInt32(10000));
                try
                {
                    await _context.ConsultationPurchases.AddAsync(purchase);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException)
                {
                    // برخورد نادر با ایندکس یکتای PurchaseCode: موجودیت ناموفق را رها و کد جدید امتحان می‌شود
                    _context.Entry(purchase).State = EntityState.Detached;
                    purchase.Id = 0;
                }
            }
            return false;
        }

        private static ConsultationPurchaseVDto ToVDto(ConsultationPurchase s, string companionName, DateTime now) => new()
        {
            Id = s.Id,
            PurchaseCode = s.PurchaseCode,
            CompanionId = s.CompanionId,
            CompanionName = companionName,
            PackageName = s.PackageName,
            ChannelId = s.ChannelId,
            DurationMinutes = s.DurationMinutes,
            Price = s.Price,
            RebatePrice = s.RebatePrice,
            PaymentPrice = s.PaymentPrice,
            WalletPrice = s.WalletPrice,
            Status = s.Status,
            CreateDate = s.CreateDate,
            PaidDate = s.PaidDate,
            StartDeadline = s.StartDeadline,
            StartDate = s.StartDate,
            ExpireDate = s.ExpireDate,
            OnlineSessionId = s.OnlineSessionId,
            CancelDate = s.CancelDate,
            RefundDate = s.RefundDate,
            ServerNow = now,
            Rate = s.Rate,
            ReviewText = s.ReviewText,
            ReviewedAt = s.ReviewedAt
        };
    }
}
