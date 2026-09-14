using System.Collections.Generic;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    public class AiProductMatchResultItemDto
    {
        public string RowId { get; set; }
        public string DetectedName { get; set; }
        public string ExternalCode { get; set; }
        public double? Price { get; set; }
        public int? Quantity { get; set; }
        public long? ProductId { get; set; }
        public long? ProductItemId { get; set; }
        public string ProductName { get; set; }
        public double Confidence { get; set; }

        // اگر این ردیف از یک تصویر قفسه استخراج شده، همان Picture ثبت‌شده در سرویس File (برای بازبینی بعدی)
        public long? SourcePictureId { get; set; }

        public List<string> Issues { get; set; } = new();
        public List<AiProductMatchCandidateDto> Matches { get; set; } = new();
    }
}
