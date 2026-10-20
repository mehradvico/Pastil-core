using Application.Common.Dto.Result;
using Application.Services.TripSrv.PetResanServiceSrv.Dto;
using Application.Services.TripSrv.PetResanServiceSrv.Iface;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// سرویس‌های هفتگی پت‌رسان
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetResanServiceController : ControllerBase
    {
        private readonly IPetResanServiceService _petResanServiceService;
        private readonly ITripService _tripService;

        public PetResanServiceController(IPetResanServiceService petResanServiceService, ITripService tripService)
        {
            _petResanServiceService = petResanServiceService;
            _tripService = tripService;
        }

        /// <summary>
        /// فهرست سرویس‌های هفتگی (Available: true فعال / false متوقف / خالی همه؛ Q: نام، موبایل یا آدرس)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseSearchDto<PetResanServiceAdminVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] PetResanServiceAdminSearchDto dto)
        {
            return Ok(await _petResanServiceService.AdminSearchAsync(dto));
        }

        /// <summary>
        /// لغو گروهی سرویس‌های هفتگی: هر سرویس غیرفعال می‌شود و نوبت‌های شروع‌نشده‌اش با برگشت پول لغو می‌شوند؛
        /// دیگر هیچ نوبتی برایشان ساخته نمی‌شود.
        /// </summary>
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(BaseResultDto<PetResanServiceBulkCancelResultDto>), 200)]
        public async Task<IActionResult> Cancel(PetResanServiceBulkCancelDto dto)
        {
            return Ok(await _tripService.AdminCancelPetResanServicesAsync(dto));
        }
    }
}
