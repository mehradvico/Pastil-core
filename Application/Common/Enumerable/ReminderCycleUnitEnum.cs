namespace Application.Common.Enumerable
{
    /// <summary>
    /// واحد تکرار یک «چرخه‌ی یادآوری» (ReminderCycle.Cycle × Unit). قبلاً Cycle همیشه به معنای «ماه» بود
    /// (یادآور واکسن)؛ این enum همان مقدار را صریح می‌کند و امکان چرخه‌ی روزانه/هفتگی را اضافه می‌کند
    /// بدون اینکه رفتار چرخه‌های ماهانه‌ی موجود تغییر کند.
    /// </summary>
    public enum ReminderCycleUnitEnum
    {
        Day = 1,
        Week = 2,
        Month = 3
    }
}
