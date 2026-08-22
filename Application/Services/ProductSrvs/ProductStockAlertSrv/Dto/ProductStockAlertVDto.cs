using Application.Common.Dto.Field;
using System;

namespace Application.Services.ProductSrvs.ProductStockAlertSrv.Dto
{
    public class ProductStockAlertVDto : Id_FieldDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? NotifiedDate { get; set; }
        public long? NotifiedStoreId { get; set; }
    }
}
