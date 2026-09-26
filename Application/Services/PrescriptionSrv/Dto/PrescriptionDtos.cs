using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.PrescriptionSrv.Dto
{
    // هدف نسخه: دقیقاً یکی از سه مقدار (شناسه‌ی جلسه‌ی آنلاین برای تماس‌های مشاوره است و به خرید مربوطش تبدیل می‌شود)
    public class PrescriptionTargetDto
    {
        public long? CompanionReserveId { get; set; }
        public long? ConsultationPurchaseId { get; set; }
        public long? OnlineSessionId { get; set; }
    }

    public class PrescriptionUpsertDto : PrescriptionTargetDto
    {
        [MaxLength(4000)]
        public string Text { get; set; }
        // شناسه‌ی تصاویر آپلودشده (POST /api/upload/picture)؛ ترتیب همین لیست است
        public List<long> PictureIds { get; set; } = new();
    }

    public class PrescriptionVDto
    {
        public long Id { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? ConsultationPurchaseId { get; set; }
        public string Text { get; set; }
        public List<PictureVDto> Pictures { get; set; } = new();
        public string AuthorName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
