namespace Application.Services.Order.ProductOrderSrv.Dto
{
    // لغو کامل سفارش (فروشنده یا ادمین)
    public class ProductOrderCancelInputDto
    {
        public string OrderId { get; set; }

        /// <summary>دلیل لغو؛ در پیامک ادمین می‌آید (برای فروشنده الزامی، برای ادمین اختیاری).</summary>
        public string Reason { get; set; }
    }

    // کم یا حذف کردن یک کالا از سفارش. NewCount = 0 یعنی حذف؛ باید کمتر از تعداد فعلی باشد (افزایش مجاز نیست).
    public class ProductOrderItemAdjustInputDto
    {
        public long ProductOrderItemId { get; set; }
        public int NewCount { get; set; }
        public string Reason { get; set; }
    }
}
