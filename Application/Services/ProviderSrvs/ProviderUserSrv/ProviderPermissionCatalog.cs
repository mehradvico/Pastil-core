using Entities.Entities.ProviderField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ProviderSrvs.ProviderUserSrv
{
    /// <summary>
    /// کاتالوگ ثابت دسترسی‌های پنل ارائه‌دهنده. دسترسیِ مادر «نوع نمایندگی» است (همان Labelهای <see cref="ProviderRoleLabels"/>:
    /// فروشگاه، کلینیک، آرایشگاه، مربی، پانسیون، مدرسه) و زیر هر نمایندگی بخش‌ها با دو سطح «مشاهده» و «مدیریت» می‌آیند.
    /// کلید هر دسترسی «{نمایندگی}.{بخش}.{سطح}» است (مثلاً Clinic.Finance.View). «مدیریت» خودش «مشاهده»ی همان بخش را هم می‌دهد.
    /// نقش‌ها فقط کلیدهای همین کاتالوگ را می‌پذیرند؛ نگاشت endpointها به این کلیدها در Api/Authorization/ProviderEndpointPolicy است.
    /// </summary>
    public static class ProviderPermissionCatalog
    {
        public const string View = "View";
        public const string Manage = "Manage";

        public static class Sections
        {
            public const string Profile = "Profile";
            public const string Service = "Service";
            public const string Reserve = "Reserve";
            public const string Consultation = "Consultation";
            public const string Review = "Review";
            public const string Staff = "Staff";
            public const string Finance = "Finance";
            public const string Portfolio = "Portfolio";
            public const string Course = "Course";
            public const string Product = "Product";
            public const string Order = "Order";
            public const string Discount = "Discount";
        }

        public sealed record SectionDef(string Key, bool HasManage);
        public sealed record ScopeDef(string Key, IReadOnlyList<SectionDef> Sections);

        private static SectionDef S(string key, bool hasManage = true) => new(key, hasManage);

        /// <summary>درخت کامل: نمایندگی ← بخش‌ها. حسابداری (Finance) فقط مشاهده دارد؛ جابه‌جایی پول همیشه کار خودِ مالک است.</summary>
        public static readonly IReadOnlyList<ScopeDef> Scopes = new List<ScopeDef>
        {
            new(ProviderRoleLabels.Store, new[] { S(Sections.Profile), S(Sections.Product), S(Sections.Order), S(Sections.Discount), S(Sections.Review), S(Sections.Finance, false) }),
            new(ProviderRoleLabels.Clinic, new[] { S(Sections.Profile), S(Sections.Service), S(Sections.Reserve), S(Sections.Consultation), S(Sections.Review), S(Sections.Staff), S(Sections.Finance, false) }),
            new(ProviderRoleLabels.Groomer, new[] { S(Sections.Profile), S(Sections.Service), S(Sections.Reserve), S(Sections.Portfolio), S(Sections.Review), S(Sections.Staff), S(Sections.Finance, false) }),
            new(ProviderRoleLabels.Coach, new[] { S(Sections.Profile), S(Sections.Service), S(Sections.Reserve), S(Sections.Consultation), S(Sections.Review), S(Sections.Staff), S(Sections.Finance, false) }),
            new(ProviderRoleLabels.Pansion, new[] { S(Sections.Profile), S(Sections.Reserve), S(Sections.Review), S(Sections.Finance, false) }),
            new(ProviderRoleLabels.School, new[] { S(Sections.Profile), S(Sections.Course), S(Sections.Reserve), S(Sections.Finance, false) }),
        };

        private static readonly HashSet<string> AllKeys = new(
            Scopes.SelectMany(scope => scope.Sections.SelectMany(section =>
                section.HasManage
                    ? new[] { Key(scope.Key, section.Key, View), Key(scope.Key, section.Key, Manage) }
                    : new[] { Key(scope.Key, section.Key, View) })),
            StringComparer.Ordinal);

        public static IReadOnlyCollection<string> Keys => AllKeys;

        public static string Key(string scope, string section, string level) => scope + "." + section + "." + level;

        public static bool IsKnown(string key) => key != null && AllKeys.Contains(key);

        /// <summary>نمایندگیِ مادر یک کلید (بخش اول آن)؛ برای کلید ناشناخته null.</summary>
        public static string ScopeOf(string key)
        {
            if (!IsKnown(key))
                return null;
            return key.Substring(0, key.IndexOf('.'));
        }

        /// <summary>
        /// آیا مجموعه‌ی دسترسی‌های نقش، کلید لازم endpoint را پوشش می‌دهد؟ تطبیق دقیق و حساس به حروف است؛ فقط «Manage» همان بخش «View» را هم می‌دهد.
        /// کلید ناشناخته هیچ‌وقت پوشش داده نمی‌شود (حتی اگر به‌اشتباه در دیتابیس باشد).
        /// </summary>
        public static bool Grants(IReadOnlyCollection<string> granted, string required)
        {
            if (granted == null || granted.Count == 0 || !IsKnown(required))
                return false;
            if (granted.Contains(required))
                return true;
            if (required.EndsWith("." + View, StringComparison.Ordinal))
            {
                var manage = required.Substring(0, required.Length - View.Length) + Manage;
                return IsKnown(manage) && granted.Contains(manage);
            }
            return false;
        }

        /// <summary>
        /// کلیدهای ورودی نقش را پاک‌سازی می‌کند: تکراری حذف، ترتیب ثابت. اگر کلیدی ناشناخته یا بیرون از نمایندگی‌های مجاز ارائه‌دهنده باشد null برمی‌گرداند
        /// (کل درخواست رد می‌شود؛ بی‌صدا دور ریخته نمی‌شود تا مالک فکر نکند دسترسی ثبت شده).
        /// </summary>
        public static List<string> Normalize(IEnumerable<string> keys, IReadOnlyCollection<string> allowedScopes)
        {
            var result = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var raw in keys ?? Array.Empty<string>())
            {
                var key = raw?.Trim();
                if (string.IsNullOrEmpty(key))
                    continue;
                var scope = ScopeOf(key);
                if (scope == null || allowedScopes == null || !allowedScopes.Contains(scope))
                    return null;
                result.Add(key);
            }
            return result.ToList();
        }
    }
}
