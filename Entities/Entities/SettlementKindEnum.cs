namespace Entities.Entities
{
    /// <summary>
    /// استخر تسویه: تسویه برای ارائه‌دهنده است و نمایندگی‌ها و فروشگاه‌هایش جدا تسویه می‌شوند.
    /// ردیف‌های قدیمی (قبل از تسویه‌ی ارائه‌دهنده‌محور) هم با همین مقدار برچسب‌گذاری شده‌اند.
    /// </summary>
    public enum SettlementKindEnum
    {
        Companion = 1,
        Store = 2,
        Groomer = 3,
        Clinic = 4,
    }
}
