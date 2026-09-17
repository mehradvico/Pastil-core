using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    // مدل‌های داخلی پردازش — فقط بین مراحل AnalyzeAsync رد‌وبدل می‌شوند، بخشی از قرارداد عمومی API نیستند.

    public class AiProductMatchWorkingRow
    {
        public string RowId { get; set; }
        public string Name { get; set; }
        public string ExternalCode { get; set; }
        public double? Price { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
        public long? SourcePictureId { get; set; }

        // فقط برای ردیف‌های استخراج‌شده از عکس قفسه پر می‌شوند؛ سیگنال ساختاریافته‌ی مرحله‌ی تطبیق،
        // به‌جای اتکای صرف به شباهت متنی روی detectedName.
        public string Brand { get; set; }
        public string AnimalType { get; set; }
        public double? PackageSizeValue { get; set; }
        public string PackageSizeUnit { get; set; }
    }

    public class AiProductMatchCandidateProduct
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public string CodeValue { get; set; }
        public List<AiProductMatchPackageDto> Packages { get; set; } = new();
    }
}
