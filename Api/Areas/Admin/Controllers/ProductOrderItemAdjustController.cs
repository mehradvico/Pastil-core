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
    /// کم یا حذف کردن یک کالا از سفارش توسط ادمین (قبل از ارسال)؛ هزینه‌ی کم‌شده به کیف پول کاربر برمی‌گردد
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderItemAdjustController : ControllerBase
    {
        private readonly IProductOrderAdjustmentService _adjustmentService;

        public ProductOrderItemAdjustController(IProductOrderAdjustmentService adjustmentService)
        {
            _adjustmentService = adjustmentService;
        }

        /// <summary>
        /// NewCount = تعداد جدید (۰ = حذف) و باید کمتر از تعداد فعلی باشد. پیامک به فروشگاه و کاربر، پوش به فروشگاه
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductOrderItemAdjustInputDto dto, CancellationToken cancellationToken)
            => Ok(await _adjustmentService.AdjustItemAsync(dto.ProductOrderItemId, dto.NewCount, OrderAdjustActor.Admin, 0, dto.Reason, cancellationToken));
    }
}
