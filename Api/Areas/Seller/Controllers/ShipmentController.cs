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
        private readonly IShipmentTrackingService _trackingService;
        private readonly ICurrentUserHelper _currentUser;

        public ShipmentController(IShipmentService shipmentService, IShipmentTrackingService trackingService, ICurrentUserHelper currentUser)
        {
            _shipmentService = shipmentService;
            _trackingService = trackingService;
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
        /// ردیابی پیک یک سفارش این فروشگاه: مراحل، نام/تلفن/موقعیت پیک (فقط وقتی پیک به‌سمت فروشگاه می‌آید) و فاصله تا فروشگاه.
        /// سفارش فروشگاه دیگر = «یافت نشد». اپ تا وقتی <c>isFinal</c> نشده هر <c>pollSeconds</c> ثانیه دوباره بخواند.
        /// </summary>
        [HttpGet("Tracking")]
        [ProducesResponseType(typeof(BaseResultDto<ShipmentTrackingVDto>), 200)]
        public async Task<IActionResult> GetTracking([FromQuery] long productOrderStoreId, CancellationToken cancellationToken)
        {
            if (StoreId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _trackingService.GetForStoreAsync(StoreId, productOrderStoreId, cancellationToken));
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
