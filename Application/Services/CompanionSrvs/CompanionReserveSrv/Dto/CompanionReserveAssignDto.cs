using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveAssignDto : Id_FieldDto
    {
        [Range(1, long.MaxValue)]
        public long CompanionAssistanceUserId { get; set; }
    }
}
