using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    /// <summary>خروجی «کالاهای مشابه» و «در کنارش خریداری شده» (همان شکل list/totalCount جستجو تا وب‌اپ کمترین تغییر را داشته باشد).</summary>
    public class ProductSimilarVDto
    {
        public List<ProductVDto> List { get; set; } = new List<ProductVDto>();
        public int TotalCount { get; set; }
    }
}
