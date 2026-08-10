using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.Order.ProductOrderOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Order.ProductOrderSrv.Iface
{
    public interface IProductOrderService : ICommonSrv<ProductOrder, ProductOrderDto>
    {
        ProductOrderSearchDto Search(ProductOrderInputDto baseSearchDto);
        Task<BaseResultDto> ProductPaymentCallback(string productOrderId, bool fromWallet = false);
        Task<BaseResultDto> FindAsyncVDto(string id, long? userId = null);
        Task<BaseResultDto> ChangeStatusAsync(ProductOrderDto dto);
        Task<BaseResultDto> ChangeStateAsync(ProductOrderDto dto);
        Task<BaseResultDto> ChangeTrackingCode(ProductOrderDto order);
        Task<BaseResultDto> ChangeDescriptions(ProductOrderDto order);
        Task UpdateWalletAsync(string productOrderId, bool complete);
        BaseResultDto<List<ProductOrderVDto>> GetReserved(long userId, long addressId);
        Task<BaseResultDto> SetCancelRequestAsync(ProductOrderDto productOrder);
        Task<BaseResultDto> AnswerCancelRequestAsync(ProductOrderDto productOrder);
        Task<BaseResultDto> UpdatePermittedAsyncDto(string id);


    }
}
