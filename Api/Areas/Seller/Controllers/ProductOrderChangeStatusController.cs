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
    /// تغییر وضعیت سفارش ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderChangeStatusController : ControllerBase
    {
        private readonly IProductOrderService _productOrderService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// تغییر وضعیت سفارش ها
        /// </summary>
        ///
        public ProductOrderChangeStatusController(IProductOrderService productOrderService, ICurrentUserHelper currentUser)
        {
            this._productOrderService = productOrderService;
            this._currentUser = currentUser;
        }

        /// <summary>
        /// تغییر وضعیت سفارش ها
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductOrderDto productOrderDto)
        {
            var existing = await _productOrderService.FindAsyncVDto(productOrderDto.Id);
            if (!(existing is BaseResultDto<ProductOrderVDto> typed) || !typed.IsSuccess ||
                typed.Data?.ProductOrderStores == null || !typed.Data.ProductOrderStores.Any(s => s.StoreId == _currentUser.CurrentUser.StoreId))
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = await _productOrderService.ChangeStatusAsync(productOrderDto);
            return Ok(dto);
        }

    }
}
