using Application.Common.Dto.Result;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر وضعیت سفارش
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderChangeStatusController : ControllerBase
    {
        private readonly IProductOrderService _productOrderService;
        public ProductOrderChangeStatusController(IProductOrderService productOrderService)
        {
            this._productOrderService = productOrderService;
        }

        /// <summary>
        /// تغییر وضعیت آیتم
        /// </summary>

        [HttpPut("")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductOrderDto productOrderDto)
        {
            var dto = await _productOrderService.ChangeStatusAsync(productOrderDto);
            return Ok(dto);
        }

    }
}
