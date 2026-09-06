using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto;
using System;

namespace Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto
{
    public class CompanionInstantCallRequestVDto : Id_FieldDto
    {
        public long CompanionAssistancePackageOnlineSelectionId { get; set; }
        public long BookerId { get; set; }
        public DateTime CreateDate { get; set; }
        public CompanionAssistancePackageOnlineSelectionVDto CompanionAssistancePackageOnlineSelection { get; set; }
        public UserMinVDto Booker { get; set; }
    }
}
