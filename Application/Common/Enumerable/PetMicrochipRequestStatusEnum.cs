namespace Application.Common.Enumerable
{
    // وضعیت درخواست جستجوی میکروچیپ
    public enum PetMicrochipRequestStatusEnum
    {
        Searched = 1,            // فقط جستجو شده (درخواست پیگیری نداده)
        FollowUpRequested = 2,   // «درخواست پیگیری توسط پاستیل» داده شده؛ در صف ادمین
        InProgress = 3,          // ادمین در حال پیگیری است
        Resolved = 4,            // پیگیری انجام و نتیجه‌گیری شد
        Closed = 5               // بسته شد بدون اقدام (نامعتبر/بی‌پاسخ)
    }
}
