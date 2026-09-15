using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushNotificationSrv.Iface
{
    public interface IFcmSender
    {
        bool IsConfigured { get; }

        // notificationId و type مطابق قرارداد payload اپ فلاتر در data قرار می‌گیرند:
        // notificationId برای idempotency/deduplication سمت اپ و type برای دسته‌بندی
        // و مسیردهی. اختیاری‌اند تا فراخوان‌های قدیمی بشکنند نشوند.
        Task<PushSendResult> SendAsync(
            string fcmToken,
            string title,
            string body,
            string url,
            string icon,
            string tag,
            string notificationId = null,
            string type = null);
    }
}
