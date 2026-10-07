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
    /// کم یا حذف کردن کالای فروشگاه خودش از سفارش (قبل از ارسال)؛ هزینه‌ی کم‌شده به کیف پول کاربر برمی‌گردد
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderItemAdjustController : ControllerBase
    {
        private readonly IProductOrderAdjustmentService _adjustmentService;
        private readonly ICurrentUserHelper _currentUser;

        public ProductOrderItemAdjustController(IProductOrderAdjustmentService adjustmentService, ICurrentUserHelper currentUser)
        {
            _adjustmentService = adjustmentService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// NewCount = تعداد جدید (۰ = حذف) و باید کمتر از تعداد فعلی باشد. پیامک به کاربر (عذرخواهی)، پوش به ادمین
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductOrderItemAdjustInputDto dto, CancellationToken cancellationToken)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _adjustmentService.AdjustItemAsync(dto.ProductOrderItemId, dto.NewCount, OrderAdjustActor.Store, storeId, dto.Reason, cancellationToken));
        }
    }
}
