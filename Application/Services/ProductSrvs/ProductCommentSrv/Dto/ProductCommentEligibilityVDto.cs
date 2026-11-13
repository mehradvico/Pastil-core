namespace Application.Services.ProductSrvs.ProductCommentSrv.Dto
{
    /// <summary>
    /// آیا کاربر می‌تواند برای این کالا دیدگاه ثبت کند؟ (فقط بعد از تحویل گرفتن سفارشی که شامل کالا بوده)
    /// </summary>
    public class ProductCommentEligibilityVDto
    {
        public long ProductId { get; set; }
        /// <summary>true = کاربر سفارشی حاوی این کالا را تحویل گرفته است.</summary>
        public bool Received { get; set; }
        /// <summary>true = کاربر قبلاً برای این کالا دیدگاه (ردنشده) ثبت کرده است.</summary>
        public bool HasCommented { get; set; }
        /// <summary>Received && !HasCommented — فرم ثبت دیدگاه فقط وقتی true است باز می‌شود.</summary>
        public bool CanComment { get; set; }
    }
}
