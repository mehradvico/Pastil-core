using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.TripOngoingSrv
{
    // اعلان ماندگار «سفر در جریان است» برای مسافر پت‌رسان: از سوار شدن پت تا پایان/لغو سفر.
    // طراحی: backend/Docs/TRIP_ONGOING_NOTIFICATION_FA.md
    public interface ITripOngoingNotificationService
    {
        // هر دقیقه (Hangfire): به‌روزرسانی اعلان سفرهای فعال + بستن اعلان سفرهای تمام‌شده
        Task SyncAsync(CancellationToken cancellationToken = default);
    }
}
