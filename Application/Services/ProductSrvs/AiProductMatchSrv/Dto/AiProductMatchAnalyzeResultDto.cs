using System.Collections.Generic;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    public class AiProductMatchAnalyzeResultDto
    {
        public List<AiProductMatchResultItemDto> Items { get; set; } = new();
    }
}
