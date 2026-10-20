using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مرسوله‌های پیک (میاره) یک سفارش و تمدید مهلت فروشنده
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ICurrentUserHelper _currentUser;

        public OrderShipmentController(IShipmentService shipmentService, ICurrentUserHelper currentUser)
        {
            _shipmentService = shipmentService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// مرسوله‌های یک سفارش (وضعیت، مهلت‌ها، دلیل شکست)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<AdminShipmentVDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] string productOrderId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(productOrderId))
                return Ok(new BaseResultDto(false, Resource.Notification.InvalidData));
            return Ok(await _shipmentService.GetForOrderAsync(productOrderId.Trim(), cancellationToken));
        }

        /// <summary>
        /// تمدید مهلت فروشنده (۱۵ تا ۴۸۰ دقیقه)؛ مرسوله‌ی ناموفق‌شده به‌خاطر پایان مهلت هم احیا می‌شود
        /// </summary>
        [HttpPut("ExtendSellerDeadline")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> ExtendSellerDeadline(AdminExtendShipmentDeadlineDto dto, CancellationToken cancellationToken)
        {
            if (dto == null || dto.ShipmentId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.InvalidData));
            return Ok(await _shipmentService.ExtendSellerDeadlineAsync(dto.ShipmentId, dto.Minutes, _currentUser.CurrentUser.UserId, cancellationToken));
        }
    }
}
