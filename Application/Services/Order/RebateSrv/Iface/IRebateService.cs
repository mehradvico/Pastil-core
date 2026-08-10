using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.RebateSrv.Dto;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.PansionField;

namespace Application.Services.Order.RebateSrv.Iface
{
    public interface IRebateService : ICommonSrv<Rebate, RebateDto>
    {
        BaseSearchDto<RebateDto> Search(BaseInputDto baseSearchDto);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Cart cart, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(CompanionReserve companionReserve, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Cargo cargo, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(CompanionInsurancePackageSale insurance, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(Trip trip, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(PansionReserve pansion, string Code);
        BaseResultDto<RebateVDto> GetRebateByCodeAsync(double basePrice, long userId, string typeLabel, string code, long? targetId = null);
        void IncreaseUseCount(ProductOrder order);
        void IncreaseUseCount(CompanionReserve reserve);
        void IncreaseUseCount(Cargo cargo);
        void IncreaseUseCount(Trip trip);
        void IncreaseUseCount(CompanionInsurancePackageSale insurance);
        void IncreaseUseCount(PansionReserve pansion);
        void IncreaseUseCount(Rebate rebate, long userId, double fundedValue = 0);

    }
}
