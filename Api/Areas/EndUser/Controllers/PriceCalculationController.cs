using Application.Common.Dto.Result;
using Application.Services.TripSrv.PriceCalculationSrv.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت  سفر ها
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PriceCalculationController : ControllerBase
    {
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILogger<PriceCalculationController> _logger;
        public PriceCalculationController(IPriceCalculationService priceCalculationService, ILogger<PriceCalculationController> logger)
        {
            _priceCalculationService = priceCalculationService;
            _logger = logger;
        }

        /// <summary>
        ///  محاسبه قیمت سفر
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EnableRateLimiting("TripPrice")]
        [ProducesResponseType(typeof(BaseResultDto<double>), 200)]
        public async Task<IActionResult> Post(TripDto dto)
        {
            try
            {
                var price = await _priceCalculationService.CalculateTripPrice(dto);
                return Ok(new BaseResultDto<double>(true, price));
            }
            catch (Exception exception)
            {
                // اینجا نباید Exception خام (شامل Stack Trace) به کلاینت درز کنه؛ قبلاً همین اتفاق می‌افتاد و
                // چون وب‌اپ فقط چک truthy می‌کرد، متن خطا موفق تلقی می‌شد و در نهایت به شکل «۰ تومان» نمایش داده می‌شد.
                _logger.LogError(exception, "PriceCalculationController.Post: trip price calculation failed.");
                return Ok(new BaseResultDto<double>(false, Resource.Notification.TripPriceCalculationFailed, 0));
            }
        }


    }
}
