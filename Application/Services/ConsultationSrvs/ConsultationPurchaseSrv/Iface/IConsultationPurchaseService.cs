using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface
{
    public interface IConsultationPurchaseService
    {
        // خرید پکیج + شروع پرداخت؛ نتیجه‌ی StartPayment (لینک درگاه یا تکمیل با کیف پول) را برمی‌گرداند
        Task<BaseResultDto> PurchaseAsync(long userId, ConsultationPurchaseCreateDto dto);

        Task<BaseResultDto<List<ConsultationPurchaseVDto>>> GetMineAsync(long userId);
        Task<BaseResultDto<ConsultationPurchaseVDto>> GetAsync(long userId, long id);

        // لغو توسط کاربر (فقط پرداخت‌شده و شروع‌نشده) + بازپرداخت کامل به کیف پول
        Task<BaseResultDto> CancelAsync(long userId, long id);

        // لغو دستی توسط ادمین: خرید پرداخت‌شده یا در جریان + بازپرداخت کامل به کیف پول (و بستن جلسه‌ی آنلاین در صورت وجود)
        Task<BaseResultDto> AdminCancelAsync(long id, string reason);

        // تکمیل دستی توسط ادمین: فقط مشاوره‌ی «در جریان» (جلسه بسته می‌شود؛ سهم کلینیک مثل تکمیل خودکار حساب می‌شود)
        Task<BaseResultDto> AdminCompleteAsync(long id);

        // ثبت نظر کاربر (فقط برای مشاوره‌ی «تکمیل‌شده»‌ی خودش، فقط یک‌بار)
        Task<BaseResultDto> ReviewAsync(long userId, long id, ConsultationPurchaseReviewDto dto);

        // لغو خودکار خریدهای پرداخت‌شده‌ای که نماینده تا مهلت شروع نکرده (job زمان‌بندی‌شده) + بازپرداخت؛ تعداد را برمی‌گرداند
        Task<int> ExpireOverdueAsync();
    }

    // فقط برای PaymentService (وابستگی حلقوی با IPaymentService را می‌شکند؛ الگوی IPastilAiSubscriptionActivator)
    public interface IConsultationPurchaseActivator
    {
        Task<BaseResultDto> ActivateAfterPaymentAsync(long purchaseId, long paymentId);
        Task<BaseResultDto> MarkPaymentFailedAsync(long purchaseId, long paymentId);
    }
}
