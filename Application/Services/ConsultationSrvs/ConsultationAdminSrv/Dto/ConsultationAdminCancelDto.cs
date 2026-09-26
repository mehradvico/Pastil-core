using System.ComponentModel.DataAnnotations;

namespace Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto
{
    public class ConsultationAdminCancelDto
    {
        [MaxLength(500)]
        public string Reason { get; set; }
    }
}
