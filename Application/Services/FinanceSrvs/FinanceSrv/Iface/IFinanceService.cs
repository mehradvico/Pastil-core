using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceSrv.Iface
{
    public interface IFinanceService
    {
        FinanceSearchDto Search(FinanceInputDto dto);
        CompanionFinanceDetailVDto SearchCompanionDetail(long companionId);
        Task<BaseResultDto> UpdateStoreCommissionAsyncDto(FinanceStoreDto dto);
        Task<BaseResultDto> UpdateCompanionAssistanceCommissionAsyncDto(FinanceCompanionAssistanceDto dto);
        Task<BaseResultDto> UpdatePansionCommissionAsyncDto(FinancePansionDto dto);
    }
}
