using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto
{
    public class CompanionAssistancePackageSearchDto : BaseSearchDto<CompanionAssistancePackage, CompanionAssistancePackageVDto>, ICompanionAssistancePackageSearchFields
    {
        public CompanionAssistancePackageSearchDto(CompanionAssistancePackageInputDto dto, IQueryable<CompanionAssistancePackage> list, IMapper mapper) : base(dto, list, mapper)
        {

            this.CompanionAssistanceId = dto.CompanionAssistanceId;
            this.PetSize = dto.PetSize;
        }
        public long? CompanionAssistanceId { get; set; }
        public string PetSize { get; set; }

    }
}
