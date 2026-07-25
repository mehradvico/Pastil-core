using Application.Common.Dto.Result;
using Application.Services.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// محصولات
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private IProductService productService;
        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var product = await productService.FindAsyncDto(id);
            return Ok(product);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [ProducesResponseType(typeof(ProductSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductInputDto dto)
        {
            dto.IsAdmin = true;
            var product = productService.Search(dto);
            return Ok(product);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// 

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]
        public async Task<IActionResult> Post(ProductDto productDto)
        {
            var item = await productService.InsertAsyncDto(productDto);
            return Ok(item);
        }
        /// <summary>
        /// کپی آیتم
        /// </summary>
        /// <param name="productDuplicateDto"></param>
        ///
        [HttpPost("duplicate")]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]

        public async Task<IActionResult> Duplicate([FromBody] ProductDuplicateDto productDuplicateDto)
        {
            var result = await productService.DuplicateAsyncDto(productDuplicateDto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]
        public async Task<IActionResult> Put(ProductDto productDto)
        {
            var item = await productService.UpdateDtoAsync(productDto);
            return Ok(item);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]
        public IActionResult Delete(long id)
        {
            var item = productService.DeleteDto(id);
            return Ok(item);
        }
    }
}
