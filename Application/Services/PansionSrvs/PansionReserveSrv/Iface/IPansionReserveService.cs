using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv.Iface
{
    public interface IPansionReserveService : ICommonSrv<PansionReserve, PansionReserveDto>
    {
        PansionReserveSearchDto Search(PansionReserveInputDto baseSearchDto);
        Task<BaseResultDto> PansionReservePaymentCallback(long? reserveId, bool fromWallet = false);
        Task<BaseResultDto<PansionReserveVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdatePansionReserveCancelDto(PansionReserveCancelDto dto);
        Task<BaseResultDto> UpdatePansionReserveStatusDto(PansionReserveStatusDto dto);
        Task<BaseResultDto> SetRebateCodeAsyncDto(PansionReserveRebateCodeDto dto);
        Task<BaseResultDto> SetWalletAsyncDto(PansionReserveWalletDto dto);
        Task<BaseResultDto> ClearRebateCodeAsync(long id);
        Task<BaseResultDto<int>> ReserveCountAsync(long id);
        Task<BaseResultDto> UpdatePermittedAsyncDto(long id);

    }
}
