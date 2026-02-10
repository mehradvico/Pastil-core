using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;

namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    public class SearchProductDto : Name_FieldDto
    {
        public string SecondName { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public long BasePrice { get; set; }
        public long Price { get; set; }
        public int DiscountPercent { get; set; }
        public string StoreName { get; set; }
        public long StoreId { get; set; }
        public PictureVDto Picture { get; set; }
        public StoreMinVDto Store { get; set; }
    }
}
