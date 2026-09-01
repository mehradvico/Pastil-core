namespace Application.Common.Enumerable
{
    // چه کسی سفر را لغو کرده — عمداً Code-backed نیست (مثل TripProgressStageEnum)، چون یک مقدار ثابت دوتایی‌ست
    // و نیازی به مدیریت در پنل ندارد؛ فقط دلیل‌های لغو (Code-backed) در پنل مدیریت می‌شوند.
    public enum TripCancelInitiatorEnum
    {
        Driver = 1,
        User = 2
    }
}
