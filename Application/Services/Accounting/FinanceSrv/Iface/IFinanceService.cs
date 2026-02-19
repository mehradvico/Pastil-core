using Application.Common.Dto.Result;
using Application.Services.Accounting.FinanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.FinanceSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.FinanceSrv.Iface
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
