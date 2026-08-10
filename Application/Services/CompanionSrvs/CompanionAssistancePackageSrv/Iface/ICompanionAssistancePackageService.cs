using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Entities.Entities;

namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface
{
    public interface ICompanionAssistancePackageService : ICommonSrv<CompanionAssistancePackage, CompanionAssistancePackageDto>
    {
        CompanionAssistancePackageSearchDto Search(CompanionAssistancePackageInputDto baseSearchDto);
        Task<BaseResultDto<CompanionAssistancePackageVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateAsyncDto(CompanionAssistancePackageDto dto);
        BaseResultDto ActivationDto(CompanionAssistancePackageActivationDto dto);
        Task<List<SearchCompanionAssistancePackageDto>> SearchMinAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
    }
}
