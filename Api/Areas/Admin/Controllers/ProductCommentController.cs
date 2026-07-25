using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductCommentSrv.Dto;
using Application.Services.ProductSrvs.ProductCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نظرات محصول
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductCommentController : ControllerBase
    {
        private readonly IProductCommentService ProductCommentService;
        public ProductCommentController(IProductCommentService ProductCommentService)
        {
            this.ProductCommentService = ProductCommentService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductCommentDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var ProductComment = await ProductCommentService.FindAsyncDto(id);
            return Ok(ProductComment);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProductCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductCommentInputDto dto)
        {
            var ProductComment = ProductCommentService.Search(dto);
            return Ok(ProductComment);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ProductCommentDto>), 200)]
        public async Task<IActionResult> Put(ProductCommentDto ProductCommentDto)
        {
            var result = await ProductCommentService.UpdateDtoAsync(ProductCommentDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<ProductCommentDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = ProductCommentService.DeleteDto(id);
            return Ok(result);
        }
    }
}
