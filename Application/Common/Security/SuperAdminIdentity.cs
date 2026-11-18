using Application.Common.Enumerable;
using Entities.Entities.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Application.Common.Security
{
    /// <summary>
    /// «ادمین اصلی»: کاربران صریحاً پیکربندی‌شده که بدون هیچ مجوز RolePermission‌ای به همه‌ی endpointهای ناحیه‌ی Admin و به
    /// داده‌های محرمانه (همه‌ی تیکت‌ها، خاطرات پت‌ها، چت‌های خصوصی PastilMatch / PastilAI / رزروها) دسترسی دارد.
    /// بقیه‌ی ادمین‌ها نقش «مدیر» (RoleId = 2) هستند و مثل هر نقش دیگر تابع RolePermission‌اند.
    ///
    /// هویت ادمین اصلی در دیتابیس ذخیره نمی‌شود (نه ستون، نه نقش جدا): با پیکربندی سرور سنجیده می‌شود،
    /// پس هیچ باگ mass-assignment یا ویرایش پنل نمی‌تواند کسی را به ادمین اصلی تبدیل کند. شرط‌ها (همه با هم):
    ///   ۱) موبایل کاربر در SuperAdmin:Mobile یا SuperAdmin:AdditionalMobiles،  ۲) نقشش «مدیر» (RoleId = 2) است،  ۳) حذف/قفل نشده.
    /// اگر پیکربندی خالی باشد ادمین اصلی وجود ندارد (fail-closed).
    /// </summary>
    public static class SuperAdminIdentity
    {
        /// <summary>کلید HttpContext.Items؛ فقط OnTokenValidatedService بعد از تطبیق با دیتابیس آن را می‌گذارد (از کلاینت قابل جعل نیست).</summary>
        public const string VerifiedItemKey = "Pastil.SuperAdminVerified";

        public const string MobileConfigKey = "SuperAdmin:Mobile";
        public const string AdditionalMobilesConfigKey = "SuperAdmin:AdditionalMobiles";
        public const string RequireTwoFactorConfigKey = "SuperAdmin:RequireTwoFactor";

        /// <summary>۰۹۱۲… / +98912… / ۹۸۹۱۲… و ارقام فارسی → ۰۹۱۲…</summary>
        public static string NormalizeMobile(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return string.Empty;

            var digits = new string(mobile.Select(ToEnglishDigit).Where(char.IsDigit).ToArray());
            if (digits.StartsWith("0098")) digits = digits.Substring(4);
            else if (digits.StartsWith("98") && digits.Length == 12) digits = digits.Substring(2);
            if (digits.Length == 10 && digits.StartsWith("9")) digits = "0" + digits;
            return digits;
        }

        private static char ToEnglishDigit(char c) =>
            c >= '۰' && c <= '۹' ? (char)('0' + (c - '۰')) :
            c >= '٠' && c <= '٩' ? (char)('0' + (c - '٠')) : c;

        public static string ConfiguredMobile(IConfiguration configuration) =>
            NormalizeMobile(configuration?[MobileConfigKey]);

        public static string[] ConfiguredMobiles(IConfiguration configuration) =>
            new[] { ConfiguredMobile(configuration) }
                .Concat(configuration?[AdditionalMobilesConfigKey]?.Split(',') ?? System.Array.Empty<string>())
                .Select(NormalizeMobile)
                .Where(mobile => mobile.Length == 11 && mobile.StartsWith("09") && mobile.All(char.IsDigit))
                .Distinct()
                .ToArray();

        public static bool IsSuperAdminMobile(string mobile, IConfiguration configuration)
        {
            var normalized = NormalizeMobile(mobile);
            return normalized.Length > 0 && ConfiguredMobiles(configuration).Contains(normalized);
        }

        /// <summary>آیا این رکورد کاربر همان ادمین اصلی است؟ (ورود، توکن و هر بررسی دسترسی از همین یک تابع می‌گذرد)</summary>
        public static bool Matches(User user, IConfiguration configuration) =>
            user != null &&
            !user.Deleted &&
            !user.Locked &&
            user.RoleId == (long)RoleEnum.Admin &&
            IsSuperAdminMobile(user.Mobile, configuration);

        /// <summary>ورود ادمین اصلی همیشه با OTP + رمز (۲FA)؛ پیش‌فرض روشن، فقط با پیکربندی صریح خاموش می‌شود.</summary>
        public static bool RequireTwoFactor(IConfiguration configuration) =>
            !bool.TryParse(configuration?[RequireTwoFactorConfigKey], out var required) || required;

        /// <summary>آیا درخواست جاری از طرف ادمین اصلیِ تأییدشده (توسط OnTokenValidatedService) است؟</summary>
        public static bool IsVerified(HttpContext context) =>
            context?.Items != null &&
            context.Items.TryGetValue(VerifiedItemKey, out var verified) &&
            verified is true;

        public static bool IsVerified(IHttpContextAccessor accessor) => IsVerified(accessor?.HttpContext);
    }
}
