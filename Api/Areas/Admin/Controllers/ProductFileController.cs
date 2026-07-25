using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductFileSrv.Dto;
using Application.Services.ProductSrvs.ProductFileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فایل های محصول
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductFileController : ControllerBase
    {
        private readonly IProductFileService productFileService;
        public ProductFileController(IProductFileService productFileService)
        {
            this.productFileService = productFileService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductFileDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var productFile = await productFileService.FindAsyncDto(id);
            return Ok(productFile);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProductFileSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductFileInputDto dto)
        {
            var productFile = productFileService.SearchDto(dto);
            return Ok(productFile);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProductFileDto>), 200)]
        public async Task<IActionResult> Post(ProductFileDto productFileDto)
        {
            var result = await productFileService.InsertAsyncDto(productFileDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ProductFileDto>), 200)]
        public IActionResult Put(ProductFileDto productFileDto)
        {
            var result = productFileService.UpdateDto(productFileDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<ProductFileDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = productFileService.DeleteDto(id);
            return Ok(result);
        }
    }
}
