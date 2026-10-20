using Entities.Entities.ClinicField;
using System;

namespace Application.Services.ClinicSrvs
{
    /// <summary>
    /// قواعد خالص تماس درون‌برنامه‌ای رزرو کلینیک. CallHub تماس‌ها را با یک کلید long ردیابی می‌کند: رزرو Companion = شناسه‌ی رزرو (مثبت)،
    /// جلسه‌ی آنلاین = منفیِ شناسه، رزرو کلینیک = ClinicCallKeyBase + شناسه‌ی رزرو (بسیار بزرگ‌تر از هر شناسه‌ی رزرو Companion).
    /// </summary>
    public static class ClinicCallRules
    {
        public const long ClinicCallKeyBase = 1_000_000_000_000_000L;

        /// <summary>تماس تا این‌قدر قبل از شروع نوبت اجازه‌ی ورود دارد.</summary>
        public static readonly TimeSpan EarlyJoinWindow = TimeSpan.FromMinutes(30);

        public static long KeyFor(long clinicReserveId) => ClinicCallKeyBase + clinicReserveId;

        public static bool IsClinicKey(long key) => key > ClinicCallKeyBase && key < 2_000_000_000_000_000L;

        public static long ReserveIdOf(long key) => key - ClinicCallKeyBase;

        /// <summary>
        /// تماس فقط برای رزرو «آنلاین» پرداخت‌شده، لغونشده و تأییدشده‌ی کلینیک (یا بدون نیاز به تأیید) و از چند دقیقه قبل از نوبت باز است.
        /// ownerDecision: ۰ نیاز نیست، ۲ تأیید.
        /// </summary>
        public static bool CanJoin(int mode, bool isReserved, bool isCancel, int ownerDecision, DateTime doDate, DateTime? callEndDate, DateTime now) =>
            mode == (int)ClinicServiceModeEnum.Online
            && isReserved && !isCancel
            && (ownerDecision == 0 || ownerDecision == 2)
            && !callEndDate.HasValue
            && now >= doDate - EarlyJoinWindow;
    }
}
