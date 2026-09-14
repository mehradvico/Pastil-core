using System.Collections.Generic;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    public class AiProductMatchCandidateDto
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public double Confidence { get; set; }
        public List<AiProductMatchPackageDto> ProductItems { get; set; } = new();
    }
}
