using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ProductOrderSrv;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// لغو کامل سفارش توسط فروشنده (فقط سفارش تک‌فروشگاهی خودش و قبل از ارسال)؛ مبلغ به کیف پول کاربر برمی‌گردد
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderCancelController : ControllerBase
    {
        private readonly IProductOrderAdjustmentService _adjustmentService;
        private readonly ICurrentUserHelper _currentUser;

        public ProductOrderCancelController(IProductOrderAdjustmentService adjustmentService, ICurrentUserHelper currentUser)
        {
            _adjustmentService = adjustmentService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// لغو کامل سفارش. Reason الزامی است (به ادمین گزارش می‌شود). پیامک به ادمین و کاربر، پوش به ادمین
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(ProductOrderCancelInputDto dto, CancellationToken cancellationToken)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _adjustmentService.CancelOrderAsync(dto.OrderId, OrderAdjustActor.Store, storeId, dto.Reason, cancellationToken));
        }
    }
}
