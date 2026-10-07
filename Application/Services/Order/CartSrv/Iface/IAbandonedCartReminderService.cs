using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.CartSrv.Iface
{
    public interface IAbandonedCartReminderService
    {
        /// <summary>job روزانه ساعت ۱۸ (تهران): پوش به کاربران دارای سبد رهاشده. تعداد ارسال‌شده را برمی‌گرداند.</summary>
        Task<int> SendPushRemindersAsync(CancellationToken cancellationToken = default);

        /// <summary>job روزانه ساعت ۲۴ (تهران): پیامک (الگوی کاوه‌نگار UserAbandonedCart) به همان کاربران. تعداد ارسال‌شده را برمی‌گرداند.</summary>
        Task<int> SendSmsRemindersAsync(CancellationToken cancellationToken = default);
    }
}
