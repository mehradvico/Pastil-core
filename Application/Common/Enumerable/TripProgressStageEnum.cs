namespace Application.Common.Enumerable
{
    /// <summary>
    /// مرحله‌ی ریز پیشرفت یک سفرِ پذیرفته‌شده (Trip.TripStatusId = Accepted).
    /// این مقدار مستقل از TripStatusEnum است و آن را جایگزین نمی‌کند — فقط پیشرفت داخل
    /// وضعیت «پذیرفته‌شده» را دقیق‌تر نشان می‌دهد (برای پت‌رسان).
    /// </summary>
    public enum TripProgressStageEnum
    {
        None = 0,
        EnRouteOrigin = 1,
        ArrivedOrigin = 2,
        PetPickedUp = 3,
        ArrivedDestination = 4
    }
}
