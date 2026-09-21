using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interface;
using System;
using System.Linq;

using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationPurchaseSrv
{
    // اثر پرداخت روی خرید مشاوره؛ توسط PaymentService صدا زده می‌شود (الگوی PastilAiSubscriptionActivator).
    // idempotent: callback تکراری درگاه دوباره برداشت/تخفیف اعمال نمی‌کند.
    public class ConsultationPurchaseActivator : IConsultationPurchaseActivator
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IRebateService _rebateService;
        private readonly IServiceScopeFactory _scopeFactory;

        public ConsultationPurchaseActivator(IDataBaseContext context, IWalletService walletService, IRebateService rebateService, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _context = context;
            _walletService = walletService;
            _rebateService = rebateService;
        }

        public async Task<BaseResultDto> ActivateAfterPaymentAsync(long purchaseId, long paymentId)
        {
            // PaymentService (callback/تسویه‌ی داخلی) این متد را داخل تراکنش خودش صدا می‌زند و تراکنش تودرتو روی یک اتصال ممکن نیست؛
            // پس فقط اگر تراکنش جاری نیست خودمان می‌سازیم (همان الگوی WalletService.InsertUpdateReferenceAsync).
            await using var transaction = _context.CurrentTransaction == null
                ? await _context.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
                : null;

            var purchase = await _context.ConsultationPurchases.AsTracking()
                .Include(s => s.Rebate)
                .FirstOrDefaultAsync(s => s.Id == purchaseId);
            if (purchase == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            // قبلاً با همین پرداخت اعمال شده: تکرار callback
            if (purchase.Status != (int)ConsultationPurchaseStatusEnum.PendingPayment)
                return new BaseResultDto(purchase.PaymentId == paymentId && purchase.PaidDate.HasValue);

            var paymentIsValid = await _context.Payments.AsNoTracking().AnyAsync(s =>
                s.Id == paymentId &&
                s.UserId == purchase.UserId &&
                s.IsSuccess == true &&
                s.CallBackTypeLabel == PaymentCallbackTypeEnum.ConsultationPurchase.ToString() &&
                s.CallBackId == purchaseId.ToString());
            if (!paymentIsValid)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);

            if (purchase.FromWallet && purchase.WalletPrice > 0)
            {
                var walletResult = await _walletService.InsertUpdateConsultationPurchaseAsync(new WalletDto
                {
                    Amount = purchase.WalletPrice,
                    UserId = purchase.UserId,
                    ConsultationPurchaseId = purchase.Id,
                    Painding = false
                }, true);
                if (!walletResult.IsSuccess)
                    return new BaseResultDto(false, Resource.Notification.InsufficientFunds);
            }

            var now = DateTime.Now;
            purchase.PaymentId = paymentId;
            purchase.Status = (int)ConsultationPurchaseStatusEnum.Paid;
            purchase.PaidDate = now;
            purchase.StartDeadline = ConsultationPurchaseRules.ComputeStartDeadline(now);
            // کارمزد پاستیل ۰٪ (تصمیم محصول): کل مبلغ پرداخت‌شده سهم کلینیک
            purchase.CompanionShare = purchase.PaymentPrice;
            purchase.SiteShare = 0;

            if (purchase.Rebate != null)
                _rebateService.IncreaseUseCount(purchase.Rebate, purchase.UserId, purchase.RebatePrice);

            await _context.SaveChangesAsync();
            if (transaction != null)
                await transaction.CommitAsync();

            ScheduleNotification(purchase.Id);
            return new BaseResultDto(true);
        }

        // اعلان خرید (نماینده‌ها + کاربر) در scope جدا و با ۳ ثانیه تأخیر، چون PaymentService این متد را داخل تراکنش خودش صدا می‌زند
        // و باید commit شود تا اعلان «پرداخت‌شده» را ببیند. اگر پروسه ری‌استارت شود، job ConsultationNotifyPurchases جبران می‌کند.
        private void ScheduleNotification(long purchaseId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<IConsultationNotificationService>().NotifyPurchasedAsync(purchaseId);
                }
                catch
                {
                    // اعلان نباید روی پرداخت اثر بگذارد؛ job اطمینان دوباره تلاش می‌کند
                }
            });
        }

        public async Task<BaseResultDto> MarkPaymentFailedAsync(long purchaseId, long paymentId)
        {
            // فقط از «در انتظار پرداخت»؛ خریدی که پرداخت شده هرگز به خاطر یک callback ناموفق دیرهنگام لغو نمی‌شود
            var affected = await _context.ConsultationPurchases
                .Where(s => s.Id == purchaseId && s.Status == (int)ConsultationPurchaseStatusEnum.PendingPayment)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Status, (int)ConsultationPurchaseStatusEnum.Cancelled)
                    .SetProperty(s => s.PaymentId, (long?)paymentId)
                    .SetProperty(s => s.CancelDate, (DateTime?)DateTime.Now)
                    .SetProperty(s => s.CancelReason, "PaymentFailed"));
            return new BaseResultDto(affected > 0);
        }
    }
}
