using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.PetResanServiceSrv.Dto;
using Application.Services.TripSrv.PetResanServiceSrv.Iface;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// سرویس پت‌رسان تکرارشونده‌ی هفتگی (رفت‌وبرگشت خودکار، تخصیص راننده با ادمین)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetResanServiceController : ControllerBase
    {
        private readonly IPetResanServiceService _service;
        private readonly ICurrentUserHelper _currentUser;
        private readonly ITripService _tripService;
        private readonly ILogger<PetResanServiceController> _logger;

        public PetResanServiceController(
            IPetResanServiceService service,
            ICurrentUserHelper currentUser,
            ITripService tripService,
            ILogger<PetResanServiceController> logger)
        {
            _service = service;
            _currentUser = currentUser;
            _tripService = tripService;
            _logger = logger;
        }

        /// <summary>
        /// پیش‌نمایش قیمت هر رفت‌وبرگشت، قبل از ثبت نهایی
        /// </summary>
        [HttpPost("preview-price")]
        [EnableRateLimiting("TripPrice")]
        [ProducesResponseType(typeof(BaseResultDto<double>), 200)]
        public async Task<IActionResult> PreviewPrice(PetResanServiceCreateDto dto)
        {
            var result = await _service.PreviewPriceAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PetResanServiceVDto>), 200)]
        public async Task<IActionResult> Post(
            PetResanServiceCreateDto dto,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
        {
            var result = await _service.InsertAsyncDto(dto, _currentUser.CurrentUser.UserId, idempotencyKey);
            if (result.IsSuccess && result.Data != null)
            {
                // سرویسی که بعد از ساعت job روزانه ثبت می‌شود، نوبت فردایش را همین حالا می‌گیرد (وگرنه یک هفته دیر می‌شد)
                try
                {
                    await _tripService.GeneratePetResanServiceTripsForNewServiceAsync(result.Data.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Generating first occurrences for new PetResan service {ServiceId} failed.", result.Data.Id);
                }
            }
            return Ok(result);
        }

        /// <summary>
        /// لیست سرویس‌های خودم
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<System.Collections.Generic.List<PetResanServiceVDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            var result = await _service.GetMyListAsync(_currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>
        /// جزئیات یک سرویس
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PetResanServiceVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _service.FindAsyncVDto(id, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>
        /// لغو سرویس
        /// </summary>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(long id)
        {
            var result = await _service.CancelAsync(id, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
