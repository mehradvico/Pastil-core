using Application.Common.Dto.Input;

namespace Application.Services.ProductSrvs.ProductStockAlertSrv.Dto
{
    public class ProductStockAlertInputDto : BaseInputDto
    {
        public long UserId { get; set; }
        public long? ProductId { get; set; }
    }
}
