using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    // چرخهٔ وضعیت: Draft → Submitted → Approved | Rejected (Rejected با ویرایش برمی‌گردد به Draft)
    public enum MissingProductStatus
    {
        Draft = 0,
        Submitted = 1,
        Approved = 2,
        Rejected = 3
    }

    // محصولی که فروشنده در کاتالوگ پاستیل پیدا نکرده و برای افزوده‌شدن به کاتالوگ پیشنهاد می‌دهد.
    public class MissingProduct : Id_Field
    {
        public long StoreId { get; set; }
        public string Name { get; set; }

        // ی/ک/ارقام یکسان‌شده و بدون فاصله؛ همراه StoreId کلید یکتاست تا اسکن‌های تکراری رکورد تکراری نسازند.
        public string NormalizedName { get; set; }
        public string Brand { get; set; }
        public string PackageSize { get; set; }
        public string Description { get; set; }
        public long? Price { get; set; }
        public int? Quantity { get; set; }
        public long? PictureId { get; set; }
        public MissingProductStatus Status { get; set; }
        public string RejectionReason { get; set; }
        public string Source { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public long? ReviewedByUserId { get; set; }

        // بعد از تأیید: محصول ساخته‌شده در کاتالوگ و ProductItem همان فروشگاه (اگر قیمت داشت)
        public long? ProductId { get; set; }
        public long? ProductItemId { get; set; }

        public Store Store { get; set; }

        // کاور (همان تصویر SortOrder=0 داخل Pictures) — برای اینکه کوئری‌ها/ایندکس‌های موجود دست‌نخورده بمانند.
        public Picture Picture { get; set; }
        public ICollection<MissingProductPicture> Pictures { get; set; } = new List<MissingProductPicture>();
        public Product Product { get; set; }
    }
}
