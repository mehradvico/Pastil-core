using System;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.MissingProductSrv.Dto
{
    // ---------- فروشنده ----------

    public class MissingProductBatchItemDto
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public string PackageSize { get; set; }
        public string Source { get; set; }
    }

    public class MissingProductBatchInputDto
    {
        public List<MissingProductBatchItemDto> Items { get; set; } = new();
    }

    public class MissingProductBatchResultDto
    {
        public int Created { get; set; }
        public int AlreadyKnown { get; set; }
    }

    public class MissingProductUpdateDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string PackageSize { get; set; }
        public string Description { get; set; }
        public long? Price { get; set; }
        public int? Quantity { get; set; }
    }

    public class MissingProductDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string PackageSize { get; set; }
        public string Description { get; set; }
        public long? Price { get; set; }
        public int? Quantity { get; set; }
        public string PictureUrl { get; set; }

        // draft | submitted | approved | rejected
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public string Source { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        // بعد از تأیید: محصولی که در کاتالوگ ساخته شده
        public long? ProductId { get; set; }
    }

    public class MissingProductListDto
    {
        public List<MissingProductDto> Items { get; set; } = new();
        public int Total { get; set; }
    }

    // ---------- ادمین ----------

    public class MissingProductSimilarProductDto
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public string PictureUrl { get; set; }
    }

    public class MissingProductAdminDto : MissingProductDto
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public long? ProductItemId { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }

        // فقط در جزئیات یک درخواست (GET {id}) پر می‌شود؛ برای تشخیص سریع تکراری‌بودن نسبت به کاتالوگ
        public List<MissingProductSimilarProductDto> SimilarProducts { get; set; }
    }

    public class MissingProductAdminListDto
    {
        public List<MissingProductAdminDto> Items { get; set; } = new();
        public int Total { get; set; }
    }

    public class MissingProductAdminSearchDto
    {
        // draft | submitted | approved | rejected؛ خالی = همه
        public string Status { get; set; }
        public long? StoreId { get; set; }
        public string Q { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MissingProductApproveDto
    {
        public long Id { get; set; }

        // نام رسمی محصول در کاتالوگ؛ خالی = همان نام فروشنده
        public string Name { get; set; }

        // فقط حروف انگلیسی/عدد (برای Slug)، مثل Label سایر محصولات
        public string ProductLabel { get; set; }
        public long CategoryId { get; set; }
        public long? BrandId { get; set; }
        public string SecondName { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
    }

    public class MissingProductApproveResultDto
    {
        public long ProductId { get; set; }

        // ساخته‌شدن ProductItem (قیمت/موجودی) برای فروشگاه درخواست‌دهنده
        public bool StoreItemCreated { get; set; }
        public long? ProductItemId { get; set; }
    }

    public class MissingProductRejectDto
    {
        public long Id { get; set; }
        public string Reason { get; set; }
    }
}
