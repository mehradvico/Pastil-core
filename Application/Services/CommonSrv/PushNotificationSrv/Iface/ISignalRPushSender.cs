using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushNotificationSrv.Iface
{
    // کانال زنده‌ی مکمل (نه جایگزین) برای WebPush/Fcm — مخصوص اپ فلاتر ویندوز که در آن Push واقعی
    // سیستم‌عامل (WNS) پیاده‌سازی نشده؛ وقتی اپ باز و به SignalR متصل است، پیام بلافاصله دریافت می‌شود.
    // چون بر پایه‌ی اتصال زنده است نه توکن ذخیره‌شده، «موفقیت» ارسال به این کانال هرگز شمارش/شکست
    // Push اصلی (WebPush/Fcm) را تحت تاثیر قرار نمی‌دهد — صرفاً یک تلاش «در صورت اتصال» است.
    public interface ISignalRPushSender
    {
        // notificationId و type دقیقاً معادل همان مقادیر در payload پوش FCM و فیلد id در
        // Inbox هستند؛ با آن‌ها اپ ویندوز می‌تواند پیام زنده را با آیتم Inbox تطبیق/دی‌دوپ
        // کند و آن را «خوانده‌شده» علامت بزند.
        Task SendAsync(
            long userId,
            string title,
            string body,
            string url,
            string icon,
            string tag,
            string notificationId = null,
            string type = null);
    }
}
