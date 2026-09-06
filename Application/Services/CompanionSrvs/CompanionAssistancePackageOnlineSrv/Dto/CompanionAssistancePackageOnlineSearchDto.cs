using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Dto
{
    public class CompanionAssistancePackageOnlineSearchDto : BaseSearchDto<CompanionAssistancePackageOnline, CompanionAssistancePackageOnlineVDto>, ICompanionAssistancePackageOnlineSearchFields
    {
        public CompanionAssistancePackageOnlineSearchDto(CompanionAssistancePackageOnlineInputDto dto, IQueryable<CompanionAssistancePackageOnline> list, IMapper mapper) : base(dto, list, mapper)
        {
        }
    }
}
