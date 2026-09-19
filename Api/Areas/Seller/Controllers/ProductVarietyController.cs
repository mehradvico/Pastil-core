using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// ساختار تنوع یک محصول برای فروشنده
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductVarietyController : ControllerBase
    {
        private readonly IProductItemService _productItemService;

        public ProductVarietyController(IProductItemService productItemService)
        {
            _productItemService = productItemService;
        }

        /// <summary>
        /// نوع(های) تنوع محصول (تعیین‌شده توسط ادمین) و مقدارهای قابل‌انتخاب هرکدام. Variety=null یعنی محصول بدون تنوع است.
        /// فروشنده تنوع یا مقدار جدید نمی‌سازد؛ فقط از همین مقدارها برای هر ترکیب قیمت و موجودی می‌دهد.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ProductVarietyStructureDto>), 200)]
        public async Task<IActionResult> Get(long productId)
            => Ok(await _productItemService.GetProductVarietyStructureAsync(productId));
    }
}
