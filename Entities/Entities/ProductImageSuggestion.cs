using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    // چرخهٔ وضعیت: Pending → Approved | Rejected
    public enum ProductImageSuggestionStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    // تصویر پیشنهادیِ یک فروشگاه برای یک محصولِ موجود در کاتالوگ. فروشنده هنگام ثبت قیمت/موجودی عکس خودش را
    // می‌فرستد؛ ادمین که تأیید کند، همین تصویر «تصویر اصلی» (Product.PictureId) محصول می‌شود.
    public class ProductImageSuggestion : Id_Field
    {
        public long ProductId { get; set; }
        public long StoreId { get; set; }
        public long PictureId { get; set; }
        public ProductImageSuggestionStatus Status { get; set; }
        public string RejectionReason { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public long? ReviewedByUserId { get; set; }

        // فقط وقتی تأیید جای یک تصویر قدیمی نشسته: تصویر اصلیِ قبلی محصول (برای ردگیری؛ فایل پاک نمی‌شود)
        public long? ReplacedPictureId { get; set; }

        public Product Product { get; set; }
        public Store Store { get; set; }
        public Picture Picture { get; set; }
    }
}
