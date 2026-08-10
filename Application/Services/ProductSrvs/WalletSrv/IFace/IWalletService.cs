using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Entities.Entities;
using Entities.Entities.PastilClubField;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.WalletSrv.IFace
{
    public interface IWalletService
    {
        Task<double> GetAmountValueAsync(long userId);
        Task<double> GetSpendableAmountValueAsync(long userId, ClubRewardTargetTypeEnum scopeType, long? scopeId);
        Task<BaseResultDto<double>> GetAmountAsync(long userId);
        Task<BaseResultDto<WalletVDto>> FindAsyncVDto(long id);
        WalletSearchDto Search(WalletInputDto baseSearchDto);
        Task<BaseResultDto<WalletDto>> InsertAsyncDto(WalletDto dto);
        Task<BaseResultDto<WalletDto>> InsertUpdateProductOrderAsync(WalletDto dto, bool complete);
        Task<BaseResultDto<WalletDto>> InsertUpdateCargoAsync(WalletDto dto, bool complete);
        Task<BaseResultDto<WalletDto>> InsertUpdateReserveAsync(WalletDto dto, bool complete);
        Task<BaseResultDto<WalletDto>> InsertUpdatePansionReserveAsync(WalletDto dto, bool complete);
        Task<BaseResultDto<WalletDto>> InsertUpdateInsuranceAsync(WalletDto dto, bool complete);
        Task<BaseResultDto<WalletDto>> InsertUpdatePastilAiSubscriptionAsync(WalletDto dto, bool complete);
        Task<BaseResultDto<WalletDto>> InsertUpdateTripAsync(WalletDto dto, bool complete);
        Task<BaseResultDto> WalletPaymentCallback(Payment payment);
        Task<BaseResultDto> DeleteAsync(long id);
    }
}
