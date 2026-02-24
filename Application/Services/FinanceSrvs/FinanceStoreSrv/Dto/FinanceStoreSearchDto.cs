using Application.Common.Dto.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv.Dto
{
    public interface IFinanceStoreService
    {
        BaseResultDto<FinanceStoreVDto> Search(FinanceStoreInputDto dto);
    }
}
