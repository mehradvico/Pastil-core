using Application.Common.Dto.Result;
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
    public class ProductOrderTrackingCodeController : ControllerBase
    {
        private readonly IProductOrderService _productOrderService;
        public ProductOrderTrackingCodeController(IProductOrderService productOrderService)
        {
            this._productOrderService = productOrderService;
        }

        /// <summary>
        /// تغییر کد پیگیری آیتم
        /// </summary>

        [HttpPut("")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductOrderDto productOrderDto)
        {
            var dto = await _productOrderService.ChangeTrackingCode(productOrderDto);
            return Ok(dto);
        }

    }
}
