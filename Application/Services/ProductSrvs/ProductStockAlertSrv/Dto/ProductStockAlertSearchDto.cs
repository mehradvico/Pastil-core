using Application.Common.Dto.Result;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.ProductSrvs.ProductStockAlertSrv.Dto
{
    public class ProductStockAlertSearchDto : BaseSearchDto<ProductStockAlert, ProductStockAlertVDto>
    {
        public ProductStockAlertSearchDto(ProductStockAlertInputDto dto, IQueryable<ProductStockAlert> list, IMapper mapper)
            : base(dto, list, mapper)
        {
            ProductId = dto.ProductId;
        }

        public long? ProductId { get; set; }
    }
}
