using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentSrv.Iface
{
    public interface IPaymentService : ICommonSrv<Payment, PaymentDto>
    {
        Task<Payment> FindAsync(long id);
        BaseSearchDto<PaymentVDto> Search(PaymentInputDto baseSearchDto);
        Task<BaseResultDto> StartPayment(PaymentStartDto dto);
        Task<BaseResultDto> InsertReservePaymentAsyncDto(PaymentStartDto dto);
        Task<BaseResultDto> InsertTripPaymentAsyncDto(PaymentStartDto dto);
        Task<BaseResultDto> InsertWalletPaymentAsyncDto(PaymentStartDto dto);
        Task<BaseResultDto> InsertCargoPaymentAsyncDto(PaymentStartDto dto);
        Task<BaseResultDto> InsertCompanionInsurancePackageSalePaymentAsyncDto(PaymentStartDto dto);
        Task<BaseResultDto> InsertPansionReservePaymentAsyncDto(PaymentStartDto dto);
        Task<BaseResultDto<PaymentDto>> CallbackPayment(long paymentId, bool test);
    }
}

