using Application.Common.Dto.Input;

namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagInputDto : BaseInputDto
    {
        public bool? Claimed { get; set; }
        public bool? Active { get; set; }
    }
}
