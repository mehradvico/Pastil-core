using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Order.MerchantSrv.Iface
{
    public interface IMerchantService : ICommonSrv<Merchant, MerchantDto>
    {
        BaseSearchDto<MerchantVDto> Search(BaseInputDto baseSearchDto);
        Task<BaseResultDto> StartAsync(PaymentStartDto dto);
        Task<BaseResultDto> CallbackAsync(Entities.Entities.Payment payment, bool test);
    }
}

