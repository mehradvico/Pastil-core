using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با تنوع محصولات 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ProductItem2Controller : ControllerBase
    {
        private IProductItemService ProductItemService;
        /// <summary>
        /// مرتبط با تنوع محصولات 
        /// </summary>
        public ProductItem2Controller(IProductItemService ProductItemService)
        {
            this.ProductItemService = ProductItemService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <param name="varietyItem1Id">تنوع اول</param>
        /// <param name="varietyItem2Id">تنوع دوم</param>
        /// <returns></returns>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductItemVarietyVDto>), 200)]
        public async Task<IActionResult> Get(long id, long? varietyItem1Id, long? varietyItem2Id, [FromQuery] long? storeId = null)
        {
            var ProductItem = await ProductItemService.GetVariety2Async(id, varietyItem1Id, varietyItem2Id, storeId);
            return Ok(ProductItem);
        }



    }
}
