using Entities.Entities;
using Entities.Entities.ProviderField;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Services.CompanionSrvs.AssistanceSrv
{
    /// <summary>
    /// قاعده‌ی واحد «خدمت ← نقش»: کاتالوگ Assistance مشترک است ولی هر خدمت مالکِ یک نقش (Assistance.ProviderRoleId) دارد
    /// و فقط برای مراکز همان نقش قابل تعریف/نمایش است. IsGrooming برای سازگاری با Groomer هم‌ارز نقش Groomer نگه داشته می‌شود.
    /// </summary>
    public static class AssistanceRoleRules
    {
        /// <summary>کلید شبه‌نقش برای نماینده‌های قدیمی (Companion: پانسیون/مدرسه/کلینیک قدیمی).</summary>
        public const string LegacyCompanion = "Companion";

        /// <summary>
        /// خدمت‌های مجاز برای مراکز نقش <paramref name="roleLabel"/>.
        /// Companion: بدون نقش + Clinic/Pansion/School (نه Groomer/Coach که پشته‌ی جدا دارند).
        /// </summary>
        public static Expression<Func<Assistance, bool>> ForRole(string roleLabel)
        {
            switch (roleLabel)
            {
                case ProviderRoleLabels.Groomer:
                    return a => a.IsGrooming || (a.ProviderRole != null && a.ProviderRole.Label == ProviderRoleLabels.Groomer);
                case LegacyCompanion:
                    return a => !a.IsGrooming && (a.ProviderRole == null ||
                        (a.ProviderRole.Label != ProviderRoleLabels.Groomer && a.ProviderRole.Label != ProviderRoleLabels.Coach));
                default:
                    // Clinic / Coach / Pansion / School و هر نقش دیگر: فقط خدمت‌های دقیقاً همین نقش.
                    return a => !a.IsGrooming && a.ProviderRole != null && a.ProviderRole.Label == roleLabel;
            }
        }

        public static IQueryable<Assistance> ForRole(this IQueryable<Assistance> query, string roleLabel) => query.Where(ForRole(roleLabel));

        /// <summary>نسخه‌ی درون‌حافظه‌ای (برای اعتبارسنجی یک خدمت بارگذاری‌شده).</summary>
        public static bool IsAllowedForRole(Assistance assistance, string roleLabel) =>
            assistance != null && ForRole(roleLabel).Compile()(assistance);
    }
}
