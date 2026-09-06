using Application.Common.Dto.Field;
using System;

namespace Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto
{
    public class CompanionInstantCallRequestDto : Id_FieldDto
    {
        public long CompanionAssistancePackageOnlineSelectionId { get; set; }
        public long BookerId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
