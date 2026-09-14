namespace Application.Common.Enumerable
{
    // مکانیزم واقعیِ رسیدنِ پوش به دستگاه؛ مستقل از PushType/PushPattern (که «چه پیامی» را
    // تعیین می‌کنند، نه «از چه مسیری» ارسال شود).
    public enum PushProviderEnum
    {
        WebPush = 1, // مرورگر/PWA نصب‌شده (وب‌اپ) — Endpoint/P256dh/Auth
        Fcm = 2      // اپ Native فلاتر روی اندروید/iOS — FcmToken (از طریق Firebase Cloud Messaging)
    }
}
