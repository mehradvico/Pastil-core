using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto
{
    public class CompanionInstantCallRequestSearchDto : BaseSearchDto<CompanionInstantCallRequest, CompanionInstantCallRequestVDto>, ICompanionInstantCallRequestSearchFields
    {
        public CompanionInstantCallRequestSearchDto(CompanionInstantCallRequestInputDto dto, IQueryable<CompanionInstantCallRequest> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.CompanionAssistancePackageOnlineSelectionId = dto.CompanionAssistancePackageOnlineSelectionId;
        }

        public long? CompanionAssistancePackageOnlineSelectionId { get; set; }
    }
}
