using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveAssignDto : Id_FieldDto
    {
        // یکی از این دو لازم است: اتصال خدمت (CompanionAssistanceUserId) یا عضویت کلینیک (CompanionUserId).
        // با CompanionUserId اگر همکار هنوز به این خدمت وصل نباشد، بک‌اند اتصال را خودش می‌سازد.
        public long CompanionAssistanceUserId { get; set; }
        public long? CompanionUserId { get; set; }
    }
}
