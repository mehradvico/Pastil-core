using Application.Services.Dto;
using Microsoft.AspNetCore.Http;

namespace Application.Common.CurrentUser
{
    /// <summary>
    /// نگه‌داری هویت جانشین (مالک ارائه‌دهنده) برای همان یک درخواست. فقط ProviderPanelAccessFilter (Api) بعد از اعتبارسنجی کامل
    /// عضویت و دسترسیِ endpoint مقدار می‌گذارد؛ CurrentUserHelper اگر این مقدار باشد آن را برمی‌گرداند. هر درخواست جدا ساخته می‌شود
    /// (HttpContext.Items)، پس به درخواست دیگر یا ناحیه‌ی دیگر نشت نمی‌کند.
    /// </summary>
    public static class ProviderActingContext
    {
        public const string HeaderName = "X-Provider-Id";
        private const string ItemKey = "Pastil.ProviderActing";

        public static void Set(HttpContext context, CurrentUserDto actingUser) => context.Items[ItemKey] = actingUser;

        public static CurrentUserDto Get(HttpContext context) =>
            context != null && context.Items.TryGetValue(ItemKey, out var value) ? value as CurrentUserDto : null;

        /// <summary>کاربر واقعی توکن (کارمند)، حتی وقتی هویت درخواست جانشین شده است.</summary>
        public static long RealUserId(CurrentUserDto current) => current?.ActorUserId ?? current?.UserId ?? 0;
    }
}
