using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv.Iface
{
    /// <summary>
    /// پوش سیستمی اعلان‌های پنل ادمین (Web Push روی دسکتاپ/اندروید و PWA نصب‌شده‌ی iOS).
    /// فقط دستگاه‌هایی که خودِ ادمین در هدر پنل «فعال‌سازی» کرده ردیف Platform="panel" دارند؛ اشتراک‌های وب‌اپ و اپ هرگز
    /// اعلان ادمین نمی‌گیرند و برعکس (پوش‌های کاربریِ PushNotificationService هم ردیف‌های panel را نادیده می‌گیرند).
    /// </summary>
    public interface IPanelNoticePushService
    {
        Task<BaseResultDto> SubscribeAsync(long userId, PushSubscribeDto dto);
        Task<BaseResultDto> UnsubscribeAsync(long userId, string endpoint);

        /// <summary>ارسال در پس‌زمینه؛ هرگز درخواست اصلی (مثلاً ثبت‌نام کاربر) را منتظر endpoint کند نگه نمی‌دارد.</summary>
        void QueueNotice(long noticeId, string title, string message, string navigationUrl);

        Task DeliverNoticeAsync(long noticeId, string title, string message, string navigationUrl);
    }
}
