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
    /// مدیریت سفارش ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderCancelRequestController : ControllerBase
    {
        private IProductOrderService productOrderService;
        private readonly ICurrentUserHelper _currentUser;

        /// <summary>
        /// مدیریت سفارش ها
        /// </summary>
        ///
        public ProductOrderCancelRequestController(IProductOrderService productOrder, ICurrentUserHelper currentUser)
        {
            productOrderService = productOrder;
            this._currentUser = currentUser;
        }
        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(ProductOrderDto productOrder)
        {
            // Note: SetCancelRequestAsync matches on the order's owning *customer* UserId, which is
            // never the seller's own UserId, so forcing productOrder.UserId to the current (seller)
            // user id here (as done in the EndUser-area sibling controller) would always fail to find
            // the order and would neuter this endpoint. Instead we verify the order actually belongs
            // to a store owned by the calling seller, consistent with the other Seller-area order
            // endpoints, which closes the same BOLA without breaking functionality.
            var existing = await productOrderService.FindAsyncVDto(productOrder.Id);
            if (!(existing is BaseResultDto<ProductOrderVDto> typed) || !typed.IsSuccess ||
                typed.Data?.ProductOrderStores == null || !typed.Data.ProductOrderStores.Any(s => s.StoreId == _currentUser.CurrentUser.StoreId))
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            productOrder.CancelRequest = DateTime.Now;
            productOrder.UserId = typed.Data.UserId;
            var dto = await productOrderService.SetCancelRequestAsync(productOrder);
            return Ok(dto);
        }
    }
}
