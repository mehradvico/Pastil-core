using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با محصولات 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ProductController : ControllerBase
    {
        private const int PublicProductMaxPageSize = 100;
        private IProductService ProductService;
        /// <summary>
        /// مرتبط با محصولات 
        /// </summary>
        public ProductController(IProductService ProductService)
        {
            this.ProductService = ProductService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns></returns>
        /// 
        [HttpGet("{id}")]
        [CustomOutputCache(CacheTypeEnum.ProductOne)]
        [ProducesResponseType(typeof(BaseResultDto<ProductVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Product = await ProductService.FindAsyncVDto(id, visit: true);
            return Ok(Product);
        }


        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [CustomOutputCache(CacheTypeEnum.ProductSearch)]
        [ProducesResponseType(typeof(ProductSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductInputDto dto)
        {
            dto.Available = true;
            // endpoint عمومی و بدون لاگین: سقف کلی ۱۰۰۰ اجازه می‌داد کل کاتالوگ (~۳٫۳ مگابایت) با یک درخواست کشیده شود
            dto.PageSize = Math.Min(dto.PageSize, PublicProductMaxPageSize);
            var Product = ProductService.Search(dto);
            return Ok(Product);
        }

    }
}
