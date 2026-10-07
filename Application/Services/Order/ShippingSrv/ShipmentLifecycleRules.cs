using Entities.Entities.ShippingField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Order.ShippingSrv
{
    // قواعد خالص چرخه‌ی عمر مرسوله‌ی میاره. طراحی: backend/Docs/MIARE_SHIPPING_LIFECYCLE_FA.md
    public static class ShipmentLifecycleRules
    {
        /// <summary>بعد از «تحویل شد» تا این مدت مشتری می‌تواند «تحویل نگرفتم» بزند (میاره بعد از ۳ ساعت گزارش مشکل نمی‌پذیرد).</summary>
        public static readonly TimeSpan DisputeWindow = TimeSpan.FromHours(3);

        public static bool IsTerminal(ShipmentStatusEnum status) =>
            status is ShipmentStatusEnum.Delivered or ShipmentStatusEnum.Cancelled or ShipmentStatusEnum.Failed;

        // ترتیب مراحل عادی؛ بقیه (لغو/ناموفق) خارج از این مسیرند
        public static int Rank(ShipmentStatusEnum status) => status switch
        {
            ShipmentStatusEnum.Pending => 0,
            ShipmentStatusEnum.AwaitingSellerConfirm => 1,
            ShipmentStatusEnum.Preparing => 2,
            ShipmentStatusEnum.Requested => 3,
            ShipmentStatusEnum.Accepted => 4,
            ShipmentStatusEnum.PickedUp => 5,
            ShipmentStatusEnum.Delivered => 6,
            _ => -1
        };

        // وب‌هوک میاره تکرار می‌شود و ممکن است جابه‌جا برسد: فقط رو به جلو، و بعد از وضعیت نهایی هیچ چیز عوض نمی‌شود.
        public static bool CanAdvance(ShipmentStatusEnum current, ShipmentStatusEnum next)
        {
            if (IsTerminal(current))
                return false;
            if (next is ShipmentStatusEnum.Cancelled or ShipmentStatusEnum.Failed)
                return true;
            return Rank(next) > Rank(current);
        }

        // کد تحویل فقط وقتی به مشتری نشان داده می‌شود که کالا تحویل پیک شده باشد.
        public static bool IsCodeVisible(ShipmentStatusEnum? status) =>
            status is ShipmentStatusEnum.PickedUp or ShipmentStatusEnum.Delivered;

        public static bool InDisputeWindow(DateTime? deliveredAtUtc, DateTime nowUtc) =>
            deliveredAtUtc.HasValue && nowUtc - deliveredAtUtc.Value <= DisputeWindow;

        // بعد از لغو سفر (تأخیر): فقط یک بار و فقط اگر هنوز وقت کافی تا پایان بازه مانده.
        public static bool MayRetryCourier(int retryCount, DateTime? slotEndUtc, DateTime nowUtc, int minDeliveryMinutes, int confirmMinutes)
        {
            if (retryCount >= 1 || !slotEndUtc.HasValue)
                return false;
            var latestUseful = slotEndUtc.Value.AddMinutes(-Math.Max(0, minDeliveryMinutes));
            return latestUseful > nowUtc.AddMinutes(Math.Max(15, Math.Min(confirmMinutes, 30)));
        }

        // «آیا تحویل گرفتید؟» وقتی پرسیده می‌شود که پایان بازه‌ی همه‌ی مرسوله‌های فعال (غیر لغو/ناموفق) سفارش گذشته باشد
        // (چند فروشگاه، بازه‌ی دیرتر را ملاک می‌گیرد). endsUtc: پایان بازه‌ی هر مرسوله‌ی فعال؛ null یعنی بازه‌اش مشخص نیست.
        public static bool ShouldAskReceived(IReadOnlyCollection<DateTime?> activeSlotEndsUtc, DateTime nowUtc) =>
            activeSlotEndsUtc.Count > 0 && activeSlotEndsUtc.All(end => end.HasValue && end.Value <= nowUtc);

        // «۹ تا ۱۳»: ساعت‌های رُند بدون دقیقه (۹، ۱۳)، غیر رُند با دقیقه (۹:۳۰)
        public static string FormatHour(TimeSpan time) =>
            time.Minutes == 0 ? time.Hours.ToString() : $"{time.Hours}:{time.Minutes:D2}";

        // توکن‌های کاوه‌نگار فاصله نمی‌پذیرند؛ فاصله با نیم‌فاصله (ZWNJ) عوض می‌شود.
        public static string ForSmsToken(string value, int maxLength = 100)
        {
            var clean = string.Join(' ', (value ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).Replace(' ', '‌');
            return clean.Length > maxLength ? clean.Substring(0, maxLength) : clean;
        }
    }
}
