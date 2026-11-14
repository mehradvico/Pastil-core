using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.CartSrv.Iface
{
    public interface IAbandonedCartReminderService
    {
        /// <summary>job هر ۱۰ دقیقه: پوش اول (۱ ساعت) و دوم (۳ ساعت) سبد رهاشده، بیرون از ساعت سکوت. تعداد ارسال‌شده را برمی‌گرداند.</summary>
        Task<int> SendPushRemindersAsync(CancellationToken cancellationToken = default);

        /// <summary>job هر ۱۰ دقیقه: پیامک ۲۴ ساعت بعد (الگوی کاوه‌نگار UserAbandonedCart)، فقط ۱۰ تا ۱۹. تعداد ارسال‌شده را برمی‌گرداند.</summary>
        Task<int> SendSmsRemindersAsync(CancellationToken cancellationToken = default);
    }
}
