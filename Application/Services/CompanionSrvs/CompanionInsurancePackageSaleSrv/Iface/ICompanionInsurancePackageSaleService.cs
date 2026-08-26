using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.TripSrv.TripSrv.Dto;
using Entities.Entities;
using Entities.Entities.CompanionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface
{
    public interface ICompanionInsurancePackageSaleService : ICommonSrv<CompanionInsurancePackageSale, CompanionInsurancePackageSaleDto>
    {
        CompanionInsurancePackageSaleSearchDto Search(CompanionInsurancePackageSaleInputDto baseSearchDto);
        Task<BaseResultDto<CompanionInsurancePackageSaleVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<CompanionInsurancePackageSaleManualPayDto>> CompanionInsurancePackageSaleManualPayAsync(CompanionInsurancePackageSaleManualPayDto dto, long? companionId = null);
        Task<BaseResultDto> CompanionInsurancePackageSalePaymentCallback(long? insuranceId, bool fromWallet = false);
        Task<BaseResultDto> SetRebateCodeAsyncDto(CompanionInsurancePackageSaleSetRebateCodeDto dto);
        Task<BaseResultDto> ClearRebateCodeAsync(long id);
        Task<BaseResultDto> SetWalletAsyncDto(CompanionInsurancePackageSaleSetWalletDto dto);


    }
}
