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
    }
}
