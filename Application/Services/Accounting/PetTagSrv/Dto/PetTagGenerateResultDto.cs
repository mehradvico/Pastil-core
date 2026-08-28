using System.Collections.Generic;

namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagGeneratedItemDto
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
    }

    public class PetTagGenerateResultDto
    {
        public int GeneratedCount { get; set; }
        public List<PetTagGeneratedItemDto> Items { get; set; } = new();
    }
}
