using Application.Common.Dto.Result;
using Application.Services.Order.ProductOrderOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// سفارش ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderController : ControllerBase
    {
        private IProductOrderService productOrderService;
        public ProductOrderController(IProductOrderService productOrder)
        {
            productOrderService = productOrder;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductOrderVDto>), 200)]
        public async Task<IActionResult> Get(string id)
        {
            var productOrder = await productOrderService.FindAsyncVDto(id);
            return Ok(productOrder);
        }
        /// <summary>
        ///  جستجو
        /// </summary>

        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(BaseSearchDto<ProductOrderDto>), 200)]
        public IActionResult Get([FromQuery] ProductOrderInputDto dto)
        {
            var productOrders = productOrderService.Search(dto);
            return Ok(productOrders);
        }

    }
}
