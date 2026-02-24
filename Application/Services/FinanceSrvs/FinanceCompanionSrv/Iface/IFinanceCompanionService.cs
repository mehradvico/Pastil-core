using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface
{
    public interface IFinanceCompanionService
    {
        BaseResultDto<FinanceCompanionVDto> Search(FinanceCompanionInputDto dto);

    }
}
