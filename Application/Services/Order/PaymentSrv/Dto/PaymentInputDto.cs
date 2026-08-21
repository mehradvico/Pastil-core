

using Application.Common.Dto.Input;
using Application.Services.Order.ProductOrderSrv.Iface;

namespace Application.Services.Order.PaymentSrv.Dto
{
    public class PaymentInputDto : BaseInputDto, IPaymentSearchFields
    {
        public string ProductOrderId { get; set; }
        public string PaymentCode { get; set; }

    }
}
