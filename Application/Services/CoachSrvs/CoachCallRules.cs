using Entities.Entities.CoachField;
using System;

namespace Application.Services.CoachSrvs
{
    /// <summary>
    /// قواعد خالص تماس درون‌برنامه‌ای رزرو مربی. CallHub تماس‌ها را با یک کلید long ردیابی می‌کند: رزرو Companion = شناسه‌ی رزرو (مثبت)،
    /// جلسه‌ی آنلاین = منفیِ شناسه، رزرو مربی = CoachCallKeyBase + شناسه‌ی رزرو (بسیار بزرگ‌تر از هر شناسه‌ی رزرو Companion).
    /// </summary>
    public static class CoachCallRules
    {
        public const long CoachCallKeyBase = 2_000_000_000_000_000L;

        /// <summary>تماس تا این‌قدر قبل از شروع نوبت اجازه‌ی ورود دارد.</summary>
        public static readonly TimeSpan EarlyJoinWindow = TimeSpan.FromMinutes(30);

        public static long KeyFor(long coachReserveId) => CoachCallKeyBase + coachReserveId;

        public static bool IsCoachKey(long key) => key > CoachCallKeyBase;

        public static long ReserveIdOf(long key) => key - CoachCallKeyBase;

        /// <summary>
        /// تماس فقط برای رزرو «آنلاین» پرداخت‌شده، لغونشده و تأییدشده‌ی مربی (یا بدون نیاز به تأیید) و از چند دقیقه قبل از نوبت باز است.
        /// ownerDecision: ۰ نیاز نیست، ۲ تأیید.
        /// </summary>
        public static bool CanJoin(int mode, bool isReserved, bool isCancel, int ownerDecision, DateTime doDate, DateTime? callEndDate, DateTime now) =>
            mode == (int)CoachServiceModeEnum.Online
            && isReserved && !isCancel
            && (ownerDecision == 0 || ownerDecision == 2)
            && !callEndDate.HasValue
            && now >= doDate - EarlyJoinWindow;
    }
}
