using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// روزها و بازه‌های تحویل قابل انتخاب (ارسال با میاره)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ShippingSlotController : ControllerBase
    {
        private readonly IShippingSlotService _slotService;

        public ShippingSlotController(IShippingSlotService slotService)
        {
            _slotService = slotService;
        }

        /// <summary>
        /// روزهای آینده و بازه‌های هر روز برای یک فروشگاه (storeId)، متناسب با حداکثر زمان آماده‌سازی همان فروشگاه، به‌همراه ظرفیت باقی‌مانده. Date و SlotId همین پاسخ باید هنگام انتخاب ارسال (ShippingSelection) فرستاده شود.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ShippingSlotDayVDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] long storeId, CancellationToken cancellationToken)
            => Ok(await _slotService.GetAvailableAsync(storeId, cancellationToken));
    }
}
