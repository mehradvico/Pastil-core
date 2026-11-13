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
        /// کالاهای مشابه: هم‌نوعِ دقیق (مثلاً خاک گربه ← خاک گربه) با حیوان سازگار؛ موجودها اول
        /// </summary>
        /// <param name="productId">محصول مبدأ</param>
        /// <param name="take">تعداد (پیش‌فرض ۱۲، حداکثر ۲۴)</param>
        [HttpGet("Similar")]
        [CustomOutputCache(CacheTypeEnum.ProductSearch)]
        [ProducesResponseType(typeof(ProductSimilarVDto), 200)]
        public async Task<IActionResult> Similar([FromQuery] long productId, [FromQuery] int take = 12)
        {
            return Ok(await ProductService.SimilarAsync(productId, take));
        }

        /// <summary>
        /// در کنارش خریداری شده: لوازم مکمل و مرتبط با همان حیوان (مثلاً خاک گربه ← ظرف خاک، بیلچه، مت)؛ فقط موجود
        /// </summary>
        /// <param name="productId">محصول مبدأ</param>
        /// <param name="take">تعداد (پیش‌فرض ۸، حداکثر ۲۴)</param>
        [HttpGet("BoughtTogether")]
        [CustomOutputCache(CacheTypeEnum.ProductRelated)]
        [ProducesResponseType(typeof(ProductSimilarVDto), 200)]
        public async Task<IActionResult> BoughtTogether([FromQuery] long productId, [FromQuery] int take = 8)
        {
            return Ok(await ProductService.BoughtTogetherAsync(productId, take));
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
