namespace Api.Authorization
{
    public static class PolicyNames
    {
        /// <summary>فقط نقش Admin (claim RoleId). برای بخش‌های سیستمی/ارتقای دسترسی: نقش‌ها، مجوزها، کاربران، مرچنت، کیف پول، مانیتورینگ و ...</summary>
        public const string AdminOnly = "AdminOnly";

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
