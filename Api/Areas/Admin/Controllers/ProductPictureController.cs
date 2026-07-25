using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductPictureSrv.Dto;
using Application.Services.ProductSrvs.ProductPictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تصاویر محصول
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductPictureController : ControllerBase
    {
        private readonly IProductPictureService productPictureService;
        public ProductPictureController(IProductPictureService productPictureService)
        {
            this.productPictureService = productPictureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductPictureDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var productPicture = await productPictureService.FindAsyncDto(id);
            return Ok(productPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProductPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductPictureInputDto dto)
        {
            var productPicture = productPictureService.SearchDto(dto);
            return Ok(productPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProductPictureDto>), 200)]
        public async Task<IActionResult> Post(ProductPictureDto productPictureDto)
        {
            var result = await productPictureService.InsertAsyncDto(productPictureDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ProductPictureDto>), 200)]
        public IActionResult Put(ProductPictureDto productPictureDto)
        {
            var result = productPictureService.UpdateDto(productPictureDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<ProductPictureDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = productPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
