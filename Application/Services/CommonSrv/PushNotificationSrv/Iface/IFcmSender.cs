using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushNotificationSrv.Iface
{
    public interface IFcmSender
    {
        bool IsConfigured { get; }

        Task<PushSendResult> SendAsync(
            string fcmToken,
            string title,
            string body,
            string url,
            string icon,
            string tag);
    }
}
