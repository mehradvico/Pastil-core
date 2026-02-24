using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Iface
{
    public interface ISettlementService : ICommonSrv<Settlement, SettlementDto>
    {
        SettlementSearchDto Search(SettlementInputDto baseSearchDto);
        Task<BaseResultDto<SettlementVDto>> FindAsyncVDto(long id);
    }
}
