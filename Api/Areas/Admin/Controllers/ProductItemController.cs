using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آیتم های محصول
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductItemController : ControllerBase
    {
        private readonly IProductItemService ProductItemService;
        public ProductItemController(IProductItemService ProductItemService)
        {
            this.ProductItemService = ProductItemService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductItemDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var ProductItem = await ProductItemService.FindAsyncDto(id);
            return Ok(ProductItem);
        }
        /// <summary>
        /// همه تنوع ها با شناسه محصول و فروشنده
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProductItemListUpdateDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ProductItemListRequestDto productItemListRequest)
        {

            var ProductItem = await ProductItemService.GetInsertOrUpdateListAsync(productItemListRequest);
            return Ok(ProductItem);
        }

        /// <summary>
        /// ویرایش و اضافه تنوع براساس فروشنده
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProductItemDto>), 200)]
        public async Task<IActionResult> Post(ProductItemListUpdateDto productItemListUpdate)
        {
            var result = await ProductItemService.InsertOrUpdateAsync(productItemListUpdate);
            return Ok(result);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<ProductItemDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = ProductItemService.DeleteDto(id);
            return Ok(result);
        }
    }
}
