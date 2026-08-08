using Application.Common.Dto.Field;
using Application.Services.Order.BankSrv.Dto;

namespace Application.Services.Order.MerchantSrv.Dto
{
    public class MerchantVDto : Id_FieldDto
    {
        public long BankId { get; set; }
        public bool Active { get; set; }
        public BankVDto Bank { get; set; }

    }
}
