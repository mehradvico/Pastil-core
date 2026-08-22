using Application.Common.Dto.Field;

namespace Application.Services.ProductSrvs.ProductStockAlertSrv.Dto
{
    public class ProductStockAlertDto : Id_FieldDto
    {
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public bool IsActive { get; set; }
        public long? NotifiedStoreId { get; set; }
    }
}
