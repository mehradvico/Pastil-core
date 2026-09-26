using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationNotificationSrv.Iface
{
    // اعلان‌های چرخه‌ی خرید مشاوره (طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md §۶).
    // همه idempotent هستند: هر (کاربر، نوع، خرید) فقط یک‌بار پوش می‌گیرد (بر اساس PushNotifications).
    public interface IConsultationNotificationService
    {
        // خرید موفق: به نماینده‌ها «رزرو مشاوره داری؛ بیا ارتباط را برقرار کن» و به کاربر «ثبت شد»
        Task NotifyPurchasedAsync(long purchaseId);

        // لغو خودکار (نماینده شروع نکرد) ⇒ به کاربر «مبلغ به کیف پول برگشت»
        Task NotifyExpiredRefundAsync(long purchaseId);

        // شبکه‌ی اطمینان (job): خریدهای پرداخت‌شده‌ای که اعلانشان (مثلاً به‌خاطر ری‌استارت) نرفته؛ تعداد اعلان‌شده را برمی‌گرداند
        Task<int> NotifyPendingPurchasesAsync();

        // ۵ دقیقه به پایان پنجره ⇒ به هر دو طرف (job)؛ تعداد خریدهای اعلان‌شده را برمی‌گرداند
        Task<int> NotifyEndingSoonAsync();

        // مالک مشاوره را به یک نماینده محول کرد ⇒ پوش به همان نماینده
        Task NotifyAssignedAsync(long purchaseId, long agentUserId);

        // یک نماینده مشاوره را برداشت/شروع کرد ⇒ به بقیه‌ی همکاران «دکتر X این مشاوره را برداشت»
        Task NotifyTakenByColleagueAsync(long purchaseId, long takerUserId);

        // شبکه‌ی اطمینان (job): مشاوره‌ی پرداخت‌شده‌ای که چند دقیقه گذشته و کسی شروعش نکرده ⇒ یادآوری به همه‌ی نمایندگان مجاز
        // (اگر تخصیص دارد: به نماینده‌ی تخصیص‌یافته و مالک). تعداد خریدهای اعلان‌شده را برمی‌گرداند
        Task<int> NotifyUnclaimedAsync();
    }
}
