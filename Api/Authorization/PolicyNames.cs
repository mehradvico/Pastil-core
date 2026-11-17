namespace Api.Authorization
{
    public static class PolicyNames
    {
        /// <summary>
        /// نقش «مدیر» (claim RoleId = 2): ادمین اصلی و هر مدیر دیگر. فقط برای مسیرهای بدون controller/action (هاب SignalR، آپلود File) و
        /// به‌عنوان لایه‌ی اضافه روی کنترلرهایی که policy «AdminArea» (RolePermission) هم دارند. دسترسی بیشتر از این به‌تنهایی نمی‌دهد.
        /// </summary>
        public const string AdminOnly = "AdminOnly";

        /// <summary>
        /// فقط «ادمین اصلی» (SuperAdminIdentity، نشانه‌ی تأییدشده‌ی OnTokenValidatedService): نقش‌ها و مجوزها (ارتقای دسترسی)، مانیتورینگ،
        /// کیف پول/مرچنت و داده‌های محرمانه (خاطرات پت‌ها، چت‌های خصوصی). مجوز RolePermission و نقش «مدیر» این policy را باز نمی‌کند.
        /// </summary>
        public const string SuperAdminOnly = "SuperAdminOnly";

        /// <summary>
        /// بقیه‌ی ناحیه‌ی Admin: Admin، یا نقشی که OnTokenValidatedService برای همین controller/action مجوز RolePermission را تأیید کرده است
        /// (جدول مجوز قبلاً از policy AdminOnly اثر نمی‌گرفت و نقش‌های تفویض‌شده عملاً کار نمی‌کردند).
        /// </summary>
        public const string AdminArea = "AdminArea";

        public const string AreaMemberPrefix = "AreaMember:";

        /// <summary>عضویت در ناحیه‌ی Seller/Companion/Driver (فروشگاه/مرکز/راننده داشتن).</summary>
        public static string AreaMember(string area) => AreaMemberPrefix + area;
    }
}
