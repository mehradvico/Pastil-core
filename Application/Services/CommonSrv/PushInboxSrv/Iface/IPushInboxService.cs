using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushInboxSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushInboxSrv.Iface
{
    /// <summary>
    /// Inbox اعلان‌های کاربر — منبع حقیقت مستقل از تحویل Push.
    /// اگر FCM/Web Push نرسد، کاربر با بازکردن اپ همین اعلان‌ها را می‌بیند.
    /// </summary>
    public interface IPushInboxService
    {
        Task<NotificationInboxSearchDto> SearchAsync(long userId, NotificationInboxInputDto dto);
        Task<int> GetUnreadCountAsync(long userId);
        Task<BaseResultDto> MarkReadAsync(long userId, long notificationId);
        Task<BaseResultDto> MarkAllReadAsync(long userId);
    }
}
