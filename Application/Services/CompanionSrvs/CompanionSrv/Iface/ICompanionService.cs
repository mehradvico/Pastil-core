using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionSrv.Iface
{
    public interface ICompanionService : ICommonSrv<Companion, CompanionDto>
    {
        CompanionSearchDto Search(CompanionInputDto baseSearchDto);
        Task<BaseResultDto<CompanionVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateGoldAccountDto(CompanionGoldAccountDto dto);
        BaseResultDto UpdateSilverAccountDto(CompanionSilverAccountDto dto);
        BaseResultDto ActivationDto(CompanionActivationDto dto);
        Task<BaseResultDto> UpdateAsyncDto(CompanionDto dto);
        Task<List<SearchCompanionDto>> SearchMinAsync(SearchRequestDto request);
        void UpdateCompanionCommentCount(long companionId);
        Task UpdateCompanionCommentRateAsync(long Id);

    }
}
