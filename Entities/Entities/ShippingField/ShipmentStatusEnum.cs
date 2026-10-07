namespace Entities.Entities.ShippingField
{
    public enum ShipmentStatusEnum
    {
        Pending = 1,
        Requested = 2,
        Accepted = 3,
        PickedUp = 4,
        Delivered = 5,
        Cancelled = 6,
        Failed = 7,
        /// <summary>پرداخت شده؛ منتظر تأیید آماده‌سازی توسط فروشنده (سفر هنوز ساخته نشده).</summary>
        AwaitingSellerConfirm = 8,
        /// <summary>فروشنده سفارش را تأیید کرده و در حال آماده‌سازی است؛ هنوز «آماده تحویل به پیک» نزده (سفر ساخته نشده). برای مشتری نمایش داده نمی‌شود.</summary>
        Preparing = 9
    }
}
