using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// تغییر کد پیگیری سفارش ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderTrackingCodeController : ControllerBase
    {
        private readonly IProductOrderService _productOrderService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// تغییر کد پیگیری سفارش ها
        /// </summary>
        ///
        public ProductOrderTrackingCodeController(IProductOrderService productOrderService, ICurrentUserHelper currentUser)
        {
            _productOrderService = productOrderService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// تغییر کد پیگیری سفارش ها
        /// </summary>

        [HttpPut("")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductOrderDto productOrderDto)
        {
            var existing = await _productOrderService.FindAsyncVDto(productOrderDto.Id);
            if (!(existing is BaseResultDto<ProductOrderVDto> typed) || !typed.IsSuccess ||
                typed.Data?.ProductOrderStores == null || !typed.Data.ProductOrderStores.Any(s => s.StoreId == _currentUser.CurrentUser.StoreId))
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = await _productOrderService.ChangeTrackingCode(productOrderDto);
            return Ok(dto);
        }

    }
}
