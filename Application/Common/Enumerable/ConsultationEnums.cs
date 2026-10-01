using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Enumerable
{
    // وضعیت خرید مشاوره (ماشین حالت: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md §۴)
    public enum ConsultationPurchaseStatusEnum
    {
        PendingPayment = 1,
        Paid = 2,
        Active = 3,
        Completed = 4,
        Expired = 5,
        Cancelled = 6,
        Refunded = 7
    }

    // قواعد ثابت پکیج مشاوره
    public static class ConsultationRules
    {
        // خدمت کاتالوگ «مشاوره آنلاین» (Assistance شناسه‌ی ۱۵)
        public const long AssistanceId = 15;
        // مدت‌های ثابت قابل انتخاب برای پکیج (دقیقه). ذخیره‌شده‌ها با مدت قدیمی (۳۰/۶۰) همچنان معتبرند.
        public static readonly int[] AllowedDurations = { 15, 30, 45, 60, 90 };
        public static readonly int[] AllowedChannels =
        {
            (int)OnlineSessionChannelEnum.Chat,
            (int)OnlineSessionChannelEnum.InAppCall,
            (int)OnlineSessionChannelEnum.VideoCall,
            (int)OnlineSessionChannelEnum.Phone
        };
        // مهلت شروع نماینده بعد از پرداخت؛ بعد از آن لغو خودکار و بازپرداخت
        public static readonly System.TimeSpan StartDeadlineAfterPayment = System.TimeSpan.FromHours(24);

        // ---- پکیج «قابل رزرو» (کاربر ساعت انتخاب می‌کند). طراحی: backend/Docs/ONLINE_CONSULTATION_BOOKING_FA.md
        // شروع هر اسلات روی این شبکه‌ی دقیقه‌ای می‌نشیند (مثلاً ۱۶:۰۰، ۱۶:۱۵، …)
        public const int BookingSlotStepMinutes = 15;
        // نزدیک‌ترین زمان قابل رزرو از «الان» (تا نماینده فرصت آماده‌شدن داشته باشد)
        public static readonly System.TimeSpan BookingMinLead = System.TimeSpan.FromMinutes(30);
        // دورترین روز قابل رزرو (روز)
        public const int BookingMaxDaysAhead = 14;
        // نماینده از این مدت قبل از ساعت رزرو می‌تواند «شروع» را بزند
        public static readonly System.TimeSpan BookingEarlyStart = System.TimeSpan.FromMinutes(10);
        // مهلت دیرکرد نماینده بعد از ساعت رزرو؛ بعد از آن لغو خودکار و بازپرداخت
        public static readonly System.TimeSpan BookingStartGrace = System.TimeSpan.FromMinutes(15);
        // لغو آزاد توسط کاربر تا این مدت قبل از ساعت رزرو (با بازپرداخت کامل)؛ بعد از آن لغو کاربر ممکن نیست
        public static readonly System.TimeSpan BookingFreeCancelBefore = System.TimeSpan.FromHours(2);
        // رزروِ در انتظار پرداخت تا این مدت ظرفیت اسلات را نگه می‌دارد
        public static readonly System.TimeSpan BookingPendingHold = System.TimeSpan.FromMinutes(20);
        // یادآوری به هر دو طرف این مدت قبل از ساعت رزرو
        public static readonly System.TimeSpan BookingReminderLead = System.TimeSpan.FromMinutes(30);
        // سقف هم‌زمانی قابل تعریف برای یک بازه‌ی کاری
        public const int BookingMaxCapacity = 20;
    }
}
