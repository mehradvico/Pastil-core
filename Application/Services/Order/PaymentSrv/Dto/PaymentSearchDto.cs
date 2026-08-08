using Application.Common.Dto.Result;
using Application.Services.Order.ProductOrderSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.Order.PaymentSrv.Dto
{
    public class PaymentSearchDto : BaseSearchDto<Payment, PaymentVDto>, IPaymentSearchFields
    {
        public PaymentSearchDto(PaymentInputDto dto, IQueryable<Payment> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.ProductOrderId = dto.ProductOrderId;
        }
        public string ProductOrderId { get; set; }
    }
}
