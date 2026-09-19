using System;

namespace Application.Services.Accounting.UserTokenSrv
{
    /// <summary>
    /// مهلت «رقابت بی‌ضرر» بعد از rotate شدن توکن. یک رفرش، توکن قبلی را همان لحظه Deleted می‌کند؛ درخواست‌های
    /// موازیِ همان کلاینت که با توکن قبلی در راه بودند (پنل ده‌ها درخواست هم‌زمان و polling دارد) بدون این
    /// مهلت ۴۰۱ می‌گیرند و هرکدام یک refresh دیگر راه می‌اندازند (آبشار rotate). هم برای refresh token
    /// (UserTokenService.RefreshTokenAsync) و هم برای access token (UserService.CheckUser) استفاده می‌شود.
    /// </summary>
    public static class TokenRotationPolicy
    {
        public static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(15);
    }
}
