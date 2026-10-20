using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ساعت کاری هفتگی و محدوده‌ی پوشش خدمات در محل
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachScheduleController : ControllerBase
    {
        private readonly ICoachProfileService _service;
        public CoachScheduleController(ICoachProfileService service) => _service = service;

        [HttpGet("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long coachId) => Ok(await _service.GetTimesAsync(coachId));

        /// <summary>کل ساعت‌های کاری را جایگزین می‌کند</summary>
        [HttpPut("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachTimeVDto>>), 200)]
        public async Task<IActionResult> SetTimes(CoachProfileSetDto<CoachTimeInputDto> dto) =>
            Ok(await _service.SetTimesAsync(dto.CoachId, dto.Items, ownerUserId: null));

        [HttpGet("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long coachId) => Ok(await _service.GetZonesAsync(coachId));

        /// <summary>کل محدوده‌های پوشش را جایگزین می‌کند</summary>
        [HttpPut("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachZoneVDto>>), 200)]
        public async Task<IActionResult> SetZones(CoachProfileSetDto<CoachZoneInputDto> dto) =>
            Ok(await _service.SetZonesAsync(dto.CoachId, dto.Items, ownerUserId: null));
    }
}
