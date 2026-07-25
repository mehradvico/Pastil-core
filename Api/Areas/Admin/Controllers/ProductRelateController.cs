using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductRelateSrv.Dto;
using Application.Services.ProductSrvs.ProductRelateSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// محصولات مرتبط
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductRelateController : ControllerBase
    {
        private IProductRelateService ProductRelateService;
        public ProductRelateController(IProductRelateService ProductRelateService)
        {
            this.ProductRelateService = ProductRelateService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [ProducesResponseType(typeof(BaseResultDto<ProductRelateVDto>), 200)]
        [HttpGet]
        public IActionResult Get([FromQuery] ProductRelateDto dto)
        {
            var ProductRelate = ProductRelateService.GetForProduct(dto);
            return Ok(ProductRelate);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Post(ProductRelateVDto productRelate)
        {
            var result = ProductRelateService.InsertOrUpdate(productRelate);
            return Ok(result);
        }
    }
}
