using Application.Common.Enumerable;
using Application.Services.Dto;
using System.Collections.Generic;

namespace Application.Services.Order.MerchantSrv
{
    // کدام درگاه برای کدام کاربر: همه (و مهمان) زرین‌پال؛ «مدیر اصلی» (نقش با Id = 2، Label = main-admin) علاوه بر آن درگاه تستی پاستیل.
    // قبلاً مدیر اصلی «فقط» درگاه تستی می‌گرفت و چون وب‌اپ درگاه تستی را از لیست حذف می‌کرد، برای ادمین هیچ درگاهی نمی‌ماند.
    // با Id نقش مقایسه می‌شود نه Label: CurrentUserDto.RoleEnum همان Roles.Label است ("main-admin") و با "Admin" برابر نمی‌شد.
    public static class MerchantGatewayRules
    {
        private static readonly long[] PublicBanks = { (long)MerchantEnum.zarinpal };
        private static readonly long[] MainAdminBanks = { (long)MerchantEnum.zarinpal, (long)MerchantEnum.pastilTest };

        public static bool IsMainAdmin(CurrentUserDto user) =>
            user != null && user.RoleId == (long)RoleEnum.Admin;

        public static IReadOnlyCollection<long> AllowedBankIds(CurrentUserDto user) =>
            IsMainAdmin(user) ? MainAdminBanks : PublicBanks;
    }
}
