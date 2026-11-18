namespace Application.Services.Accounting.UserTokenSrv
{
    public enum RefreshReuseOutcome
    {
        /// <summary>نشست پیش‌تر با خروج یا ورود از دستگاه دیگر بسته شده؛ جانشینی ندارد. فقط رد می‌شود، نشست‌های دیگر کاربر دست نمی‌خورد.</summary>
        SessionEnded,
        /// <summary>رقابت بی‌ضرر (چند تب/درخواست هم‌زمان): همان لحظه rotate شده؛ توکن تازه‌ی هم‌زنجیره صادر می‌شود.</summary>
        Reissue,
        /// <summary>توکن rotate‌شده از دستگاه دیگر یا کلاینت بدون شناسهٔ دستگاه دوباره آمده؛ همه‌ی نشست‌ها باطل می‌شود.</summary>
        TheftRevokeAll
    }

    // طبقه‌بندی «استفاده از رفرش‌توکنی که دیگر زنده نیست» (بدون دیتابیس، تست‌پذیر).
    // نکته‌ی مهم: ردیف Deleted دو علت دارد. ۱) rotate شده با refresh (جانشین دارد: RotatedFromTokenId)، ۲) باطل‌شده با خروج یا ورودِ
    // دستگاه دیگر (ResetTokenAsync/SignOut؛ جانشین ندارد). توکن قدیمیِ پنل از همان دستگاه نیز ممکن است از تب خوابیده برسد.
    // دستگاه اولی که بعد از ورود دستگاه دوم refresh می‌زند، نشست تازه‌ی دستگاه دوم را هم می‌کُشد.
    public static class RefreshReuseRules
    {
        public static RefreshReuseOutcome Classify(bool wasRotated, bool rotatedWithinGrace, bool deviceMatches, bool isPanelSession = false)
        {
            if (!wasRotated)
                return RefreshReuseOutcome.SessionEnded;
            if (rotatedWithinGrace && deviceMatches)
                return RefreshReuseOutcome.Reissue;
            if (isPanelSession && deviceMatches)
                return RefreshReuseOutcome.SessionEnded;
            return RefreshReuseOutcome.TheftRevokeAll;
        }
    }
}
