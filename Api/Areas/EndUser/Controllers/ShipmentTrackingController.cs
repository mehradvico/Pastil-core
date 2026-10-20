using Application.Common.Dto.Result;
using Application.Common.Interface;
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
    /// ردیابی پیک سفارش مشتری (مراحل، پیک، موقعیت زنده)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ShipmentTrackingController : ControllerBase
    {
        private readonly IShipmentTrackingService _trackingService;
        private readonly ICurrentUserHelper _currentUserHelper;

        public ShipmentTrackingController(IShipmentTrackingService trackingService, ICurrentUserHelper currentUserHelper)
        {
            _trackingService = trackingService;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// مرسوله‌های پیک یک سفارش (به‌ازای هر فروشگاه یکی). <c>productOrderId</c> شناسه یا شماره‌ی سفارش است.
        /// سفارش دیگران یا سفارشِ بدون پیک = لیست خالی. تا وقتی <c>isFinal</c> نشده هر <c>pollSeconds</c> ثانیه دوباره بخوانید.
        /// نام پیک فقط نام کوچک، تلفن فقط وقتی پیک کالا را برداشته و می‌آید؛ بعد از تحویل/لغو هیچ‌کدام نمی‌آید.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ShipmentTrackingVDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] string productOrderId, CancellationToken cancellationToken)
        {
            return Ok(await _trackingService.GetForCustomerAsync(
                _currentUserHelper.CurrentUser.UserId,
                productOrderId,
                cancellationToken));
        }
    }
}
