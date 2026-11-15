using System;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductImageSuggestionSrv.Dto
{
    // ---------- فروشنده ----------

    public class ProductImageSuggestionDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }

        // تصویر پیشنهادیِ همین فروشگاه
        public string PictureUrl { get; set; }

        // pending | approved | rejected
        public string Status { get; set; }

        // فقط وقتی rejected است
        public string RejectionReason { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }
    }

    public class ProductImageSuggestionListDto
    {
        public List<ProductImageSuggestionDto> Items { get; set; } = new();
        public int Total { get; set; }
    }

    // ---------- ادمین ----------

    public class ProductImageSuggestionAdminDto : ProductImageSuggestionDto
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string BrandName { get; set; }

        // تصویر اصلیِ فعلیِ محصول؛ null = محصول هنوز تصویر ندارد
        public string ProductPictureUrl { get; set; }
        public bool ProductHasPicture { get; set; }

        // فقط برای تأییدشده‌ای که جای تصویر قبلی نشسته
        public string ReplacedPictureUrl { get; set; }
    }

    public class ProductImageSuggestionAdminListDto
    {
        public List<ProductImageSuggestionAdminDto> Items { get; set; } = new();
        public int Total { get; set; }
    }

    public class ProductImageSuggestionAdminSearchDto
    {
        // pending (پیش‌فرض) | approved | rejected | all
        public string Status { get; set; }
        public string Q { get; set; }
        public long? StoreId { get; set; }
        public long? ProductId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ProductImageSuggestionApproveDto
    {
        public long Id { get; set; }

        // اگر محصول از قبل تصویر دارد، فقط با true جایگزین می‌شود (پنل قبلش مودال تأیید نشان می‌دهد)
        public bool ConfirmReplace { get; set; }
    }

    public class ProductImageSuggestionApproveResultDto
    {
        public long ProductId { get; set; }

        // true = تصویر قبلی محصول عوض شد؛ false = محصول تصویر نداشت و برایش ثبت شد
        public bool Replaced { get; set; }
    }

    public class ProductImageSuggestionRejectDto
    {
        public long Id { get; set; }
        public string Reason { get; set; }
    }
}
