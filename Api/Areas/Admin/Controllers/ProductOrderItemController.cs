using Application.Common.Dto.Result;
using Application.Services.Order.ProductOrderItemOrderSrv.Dto;
using Application.Services.Order.ProductOrderItemSrv.Dto;
using Application.Services.Order.ProductOrderItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آیتم های سفارش
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderItemController : ControllerBase
    {
        private IProductOrderItemService productOrderItemService;
        public ProductOrderItemController(IProductOrderItemService productOrderItem)
        {
            productOrderItemService = productOrderItem;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductOrderItemDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var productOrder = await productOrderItemService.FindAsyncDto(id);
            return Ok(productOrder);
        }
        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ProductOrderItemDto dto)
        {
            var productOrder = productOrderItemService.UpdateDto(dto);
            return Ok(productOrder);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(ProductOrderItemSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductOrderItemInputDto dto)
        {
            var productOrderItems = productOrderItemService.Search(dto);
            return Ok(productOrderItems);
        }

    }
}
