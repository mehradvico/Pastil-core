using System.ComponentModel.DataAnnotations;

namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagGenerateInputDto
    {
        [Range(1, 2000)]
        public int Count { get; set; }
    }
}
