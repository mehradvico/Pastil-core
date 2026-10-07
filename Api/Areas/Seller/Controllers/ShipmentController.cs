using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// تأیید آماده‌سازی سفارش‌های ارسال با میاره توسط فروشنده
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ICurrentUserHelper _currentUser;

        public ShipmentController(IShipmentService shipmentService, ICurrentUserHelper currentUser)
        {
            _shipmentService = shipmentService;
            _currentUser = currentUser;
        }

        private long StoreId => _currentUser.CurrentUser?.StoreId ?? 0;

        /// <summary>
        /// سفارش‌های این فروشگاه که منتظر اقدام فروشنده‌اند: Step=1 تأیید سفارش، Step=2 «آماده تحویل به پیک» (با مهلت‌ها)
        /// </summary>
        [HttpGet("Awaiting")]
        [ProducesResponseType(typeof(BaseResultDto<List<SellerShipmentVDto>>), 200)]
        public async Task<IActionResult> GetAwaiting(CancellationToken cancellationToken)
        {
            if (StoreId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _shipmentService.GetAwaitingForStoreAsync(StoreId, cancellationToken));
        }

        /// <summary>
        /// مرحله‌ی ۱: تأیید سفارش (در حال آماده‌سازی). سفر میاره هنوز ساخته نمی‌شود.
        /// </summary>
        [HttpPost("Confirm")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Confirm(ShipmentConfirmDto dto, CancellationToken cancellationToken)
        {
            if (StoreId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _shipmentService.ConfirmBySellerAsync(StoreId, dto.ProductOrderStoreId, cancellationToken));
        }

        /// <summary>
        /// مرحله‌ی ۲: «آماده تحویل به پیک». همین لحظه پیک میاره درخواست می‌شود تا کالا را از فروشگاه بگیرد.
        /// این وضعیت برای مشتری نمایش داده نمی‌شود.
        /// </summary>
        [HttpPost("Ready")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Ready(ShipmentConfirmDto dto, CancellationToken cancellationToken)
        {
            if (StoreId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _shipmentService.MarkReadyBySellerAsync(StoreId, dto.ProductOrderStoreId, cancellationToken));
        }
    }
}
