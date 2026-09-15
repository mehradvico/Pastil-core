using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class SignalRPushSender : ISignalRPushSender
    {
        private readonly IHubContext<PushHub> _hubContext;

        public SignalRPushSender(IHubContext<PushHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task SendAsync(
            long userId,
            string title,
            string body,
            string url,
            string icon,
            string tag,
            string notificationId = null,
            string type = null)
        {
            // اگر کلاینتی متصل نباشد، SendAsync روی یک گروه خالی بی‌خطا و بی‌اثر برمی‌گردد —
            // نیازی به چک‌کردن اتصال نیست.
            //
            // شکل payload عمداً با data پوش FCM یکسان نگه داشته شده تا اپ فلاتر برای هر دو
            // کانال یک مسیر پردازش داشته باشد و بتواند با notificationId دی‌دوپ کند.
            return _hubContext.Clients.Group(PushHub.GroupName(userId)).SendAsync("push", new
            {
                title,
                body,
                url,
                icon,
                tag,
                notificationId,
                type
            });
        }
    }
}
