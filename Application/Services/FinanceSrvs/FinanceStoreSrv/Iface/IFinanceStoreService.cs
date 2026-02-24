using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv.Iface
{
    public interface IFinanceStoreService
    {
        BaseResultDto<FinanceStoreVDto> Search(FinanceStoreInputDto dto);
    }
}
