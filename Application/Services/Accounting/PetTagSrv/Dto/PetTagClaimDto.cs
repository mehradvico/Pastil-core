using System.ComponentModel.DataAnnotations;

namespace Application.Services.Accounting.PetTagSrv.Dto
{
    public class PetTagClaimDto
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public long UserPetId { get; set; }
    }
}
