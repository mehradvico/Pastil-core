namespace Application.Common.Enumerable.Code
{
    public enum SchoolReserveStatusEnum
    {
        Registered = 1,
        Paid = 2,
        Complete = 3,
        Cancelled = 4,
        // ارائه‌دهنده شروع دوره را ثبت کرده؛ از این به بعد مشتری خودش نمی‌تواند لغو کند
        Started = 5
    }
}
