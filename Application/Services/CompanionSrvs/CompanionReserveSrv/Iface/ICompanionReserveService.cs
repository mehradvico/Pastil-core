using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionReserveSrv.Iface
{
    public interface ICompanionReserveService : ICommonSrv<CompanionReserve, CompanionReserveDto>
    {
        CompanionReserveSearchDto Search(CompanionReserveInputDto baseSearchDto);
        Task<BaseResultDto> CompanionReservePaymentCallback(long? reserveId, bool fromWallet = false);
        Task<BaseResultDto<CompanionReserveVDto>> FindAsyncVDto(long id, long? bookerId = null);
        Task<BaseResultDto<CompanionReserveAdminVDto>> FindAsyncAdminVDto(long id);
        Task<BaseResultDto> UpdateCancelDto(CompanionReserveCancelDto dto);
        Task<BaseResultDto> UpdateAsyncDto(CompanionReserveUpdateDto dto);
        Task<BaseResultDto> CompanionReserveOperatorUpdateAsyncDto(CompanionReserveOperatorDto dto);
        Task<BaseResultDto> CompanionReserveCompanionUpdateAsyncDto(CompanionReserveOperatorDto dto);

        Task<BaseResultDto> CompanionReserveUserResponseAsyncDto(CompanionReserveUserResponseDto dto);
        Task<BaseResultDto> UpdateReserveStateDto(CompanionReserveChangeStateDto dto);
        Task<BaseResultDto> SetRebateCodeAsyncDto(CompanionReserveSetRebateCodeDto dto);
        Task<BaseResultDto> SetWalletAsyncDto(CompanionReserveSetWalletDto dto);
        Task<BaseResultDto> ClearRebateCodeAsync(long id);
        Task<BaseResultDto<int>> ReserveCountAsync(long id);
        Task<BaseResultDto> UpdatePermittedAsyncDto(long id);
        Task<BaseResultDto<List<CompanionReserveAssigneeVDto>>> GetCompanionReserveAssigneesAsync(long reserveId, bool adminAccess = false);
        Task<BaseResultDto<CompanionReserveAdminVDto>> AssignCompanionReserveAsync(CompanionReserveAssignDto dto, bool adminAccess = false);
        Task<BaseResultDto<CompanionReserveVDto>> FindAsyncOperatorVDto(long id);

    }
}
