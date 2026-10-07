namespace Application.Common.Enumerable
{
    // وضعیت یک پکیج داخل رزرو نماینده
    public enum CompanionReservePackageItemStatusEnum
    {
        Pending = 1,    // در انتظار تأیید نماینده
        Approved = 2,   // تأیید شده؛ نماینده در زمان رزرو ارائه می‌دهد
        Cancelled = 3   // لغو شده؛ سهم پرداختی به کیف پول کاربر برگشته
    }
}
