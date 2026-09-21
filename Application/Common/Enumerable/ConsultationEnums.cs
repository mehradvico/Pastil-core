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
        public static readonly int[] AllowedDurations = { 30, 60 };
        public static readonly int[] AllowedChannels =
        {
            (int)OnlineSessionChannelEnum.Chat,
            (int)OnlineSessionChannelEnum.InAppCall,
            (int)OnlineSessionChannelEnum.VideoCall,
            (int)OnlineSessionChannelEnum.Phone
        };
        // مهلت شروع نماینده بعد از پرداخت؛ بعد از آن لغو خودکار و بازپرداخت
        public static readonly System.TimeSpan StartDeadlineAfterPayment = System.TimeSpan.FromHours(24);

        public static bool IsValidCombination(int channelId, int durationMinutes) =>
            AllowedChannels.Contains(channelId) && AllowedDurations.Contains(durationMinutes);

        // همه‌ی ۸ ترکیب (کانال × مدت) به ترتیب ثابت؛ برای ماتریس تعریف پکیج
        public static IEnumerable<(int ChannelId, int DurationMinutes)> AllCombinations() =>
            AllowedChannels.SelectMany(channel => AllowedDurations.Select(duration => (channel, duration)));
    }
}
