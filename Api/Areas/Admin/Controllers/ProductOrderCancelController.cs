using Application.Common.Dto.Result;
using Application.Services.Order.ProductOrderSrv;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// لغو کامل سفارش توسط ادمین (قبل از ارسال)؛ مبلغ پرداختی به کیف پول کاربر برمی‌گردد و سفر پیک (اگر هست) لغو می‌شود.
    /// طراحی: backend/Docs/PRODUCT_ORDER_CANCEL_ADJUST_FA.md
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderCancelController : ControllerBase
    {
        private readonly IProductOrderAdjustmentService _adjustmentService;

        public ProductOrderCancelController(IProductOrderAdjustmentService adjustmentService)
        {
            _adjustmentService = adjustmentService;
        }

        /// <summary>
        /// Reason برای ادمین اختیاری است (در پیامک/یادداشت سفارش می‌آید). پیامک به فروشگاه و کاربر، پوش به فروشگاه.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(ProductOrderCancelInputDto dto, CancellationToken cancellationToken)
            => Ok(await _adjustmentService.CancelOrderAsync(dto.OrderId, OrderAdjustActor.Admin, 0, dto.Reason, cancellationToken));
    }
}
