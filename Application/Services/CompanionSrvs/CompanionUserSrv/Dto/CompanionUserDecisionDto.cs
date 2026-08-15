using System.ComponentModel.DataAnnotations;

namespace Application.Services.CompanionSrvs.CompanionUserSrv.Dto
{
    public class CompanionUserDecisionDto
    {
        [Required]
        public bool? UserAccept { get; set; }
    }
}
