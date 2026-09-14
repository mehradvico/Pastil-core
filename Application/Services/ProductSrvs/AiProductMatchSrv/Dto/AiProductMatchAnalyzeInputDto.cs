using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    // از multipart/form-data بایند می‌شود ([FromForm] در کنترلر)
    public class AiProductMatchAnalyzeInputDto
    {
        public string SourceType { get; set; }
        public List<IFormFile> Images { get; set; }
        public string RowsJson { get; set; }
        public string Currency { get; set; } = "IRT";
    }
}
