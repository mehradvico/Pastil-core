using Application.Common.Enumerable.Code;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs
{
    /// <summary>
    /// یادآورهای Push زمان‌بندی‌شده‌ی رزرو آنلاین کلینیک (۱۰ دقیقه قبل و سر ساعت) برای کلینیک/پزشک.
    /// کلید رزرو در Token4 ذخیره می‌شود؛ با تغییر زمان رزرو جابه‌جا و با لغو رزرو حذف می‌شوند.
    /// </summary>
    public static class ClinicReserveReminders
    {
        private const int BeforeMinutes = 10;

        private static readonly long[] ReminderTypes =
        {
            (long)PushTypeEnum.PushClinicOnlineReserveReminderBefore,
            (long)PushTypeEnum.PushClinicOnlineReserveReminderAtTime,
        };

        /// <summary>یادآورهای هنوز نرفته را با زمان جدید شروع هماهنگ می‌کند (داخل تراکنش صدا زده می‌شود).</summary>
        public static async Task ShiftAsync(IDataBaseContext context, long reserveId, DateTime newStartAt)
        {
            var before = (long)PushTypeEnum.PushClinicOnlineReserveReminderBefore;
            var key = reserveId.ToString();
            var pending = await context.PushNotifications.Include(n => n.PushPattern).AsTracking()
                .Where(n => n.Token4 == key && !n.IsSend && n.Status == null && n.SendDate != null
                            && ReminderTypes.Contains(n.PushPattern.PushTypeId))
                .ToListAsync();
            foreach (var notification in pending)
                notification.SendDate = notification.PushPattern.PushTypeId == before ? newStartAt.AddMinutes(-BeforeMinutes) : newStartAt;
        }

        /// <summary>با لغو رزرو، یادآورهای هنوز نرفته حذف می‌شوند تا به پزشک برای نوبتِ لغوشده پوش نرود.</summary>
        public static async Task RemoveAsync(IDataBaseContext context, long reserveId)
        {
            var key = reserveId.ToString();
            await context.PushNotifications
                .Where(n => n.Token4 == key && !n.IsSend && n.Status == null && n.SendDate != null
                            && ReminderTypes.Contains(n.PushPattern.PushTypeId))
                .ExecuteDeleteAsync();
        }

        public static DateTime BeforeAt(DateTime startAt) => startAt.AddMinutes(-BeforeMinutes);
    }
}
