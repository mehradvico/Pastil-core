namespace Application.Services.Accounting.UserTokenSrv
{
    public enum RefreshReuseOutcome
    {
        /// <summary>نشست پیش‌تر با خروج یا ورود از دستگاه دیگر بسته شده؛ جانشینی ندارد. فقط رد می‌شود، نشست‌های دیگر کاربر دست نمی‌خورد.</summary>
        SessionEnded,
        /// <summary>رقابت بی‌ضرر (چند تب/درخواست هم‌زمان): همان لحظه rotate شده؛ توکن تازه‌ی هم‌زنجیره صادر می‌شود.</summary>
        Reissue,
        /// <summary>رفرش‌توکنِ rotate‌شده بیرون از مهلت دوباره آمده = احتمال سرقت؛ همه‌ی نشست‌های کاربر باطل می‌شود.</summary>
        TheftRevokeAll
    }

    // طبقه‌بندی «استفاده از رفرش‌توکنی که دیگر زنده نیست» (بدون دیتابیس، تست‌پذیر).
    // نکته‌ی مهم: ردیف Deleted دو علت دارد. ۱) rotate شده با refresh (جانشین دارد: RotatedFromTokenId)، ۲) باطل‌شده با خروج یا ورودِ
    // دستگاه دیگر (ResetTokenAsync/SignOut؛ جانشین ندارد). فقط مورد ۱ «استفاده‌ی مجدد» است؛ مورد ۲ را نباید سرقت حساب کرد، وگرنه
    // دستگاه اولی که بعد از ورود دستگاه دوم refresh می‌زند، نشست تازه‌ی دستگاه دوم را هم می‌کُشد.
    public static class RefreshReuseRules
    {
        public static RefreshReuseOutcome Classify(bool wasRotated, bool rotatedWithinGrace, bool deviceMatches)
        {
            if (!wasRotated)
                return RefreshReuseOutcome.SessionEnded;
            if (rotatedWithinGrace && deviceMatches)
                return RefreshReuseOutcome.Reissue;
            return RefreshReuseOutcome.TheftRevokeAll;
        }
    }
}
