using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushNotificationSrv
{
    // پیش‌فرض برای هر Host ای که (مثل Payment) هاب SignalR واقعی را میزبانی نمی‌کند — در
    // Api/Program.cs با SignalRPushSender واقعی Override می‌شود (نگاه کنید به INoticeRealtimePublisher
    // برای همین الگوی دقیق).
    public class NullSignalRPushSender : ISignalRPushSender
    {
        public Task SendAsync(long userId, string title, string body, string url, string icon, string tag, string notificationId = null, string type = null)
            => Task.CompletedTask;
    }
}
