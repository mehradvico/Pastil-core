using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto
{
    public class CompanionAssistancePackageOnlineSelectionSearchDto : BaseSearchDto<CompanionAssistancePackageOnlineSelection, CompanionAssistancePackageOnlineSelectionVDto>, ICompanionAssistancePackageOnlineSelectionSearchFields
    {
        public CompanionAssistancePackageOnlineSelectionSearchDto(CompanionAssistancePackageOnlineSelectionInputDto dto, IQueryable<CompanionAssistancePackageOnlineSelection> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.CompanionAssistancePackageId = dto.CompanionAssistancePackageId;
        }

        public long? CompanionAssistancePackageId { get; set; }
    }
}
