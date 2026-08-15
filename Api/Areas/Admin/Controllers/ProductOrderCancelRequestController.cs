using Application.Common.Dto.Result;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// درخواست های لغو سفارش
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderCancelRequestController : ControllerBase
    {
        private readonly IProductOrderService _productOrderService;
        public ProductOrderCancelRequestController(IProductOrderService productOrderService)
        {
            this._productOrderService = productOrderService;
        }

        /// <summary>
        /// درخواست کنسلی آیتم
        /// </summary>

        [HttpPut("")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductOrderDto productOrderDto)
        {
            var dto = await _productOrderService.AnswerCancelRequestAsync(productOrderDto);
            return Ok(dto);
        }

    }
}
