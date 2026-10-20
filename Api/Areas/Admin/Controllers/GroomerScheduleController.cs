using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
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
    public class GroomerScheduleController : ControllerBase
    {
        private readonly IGroomerProfileService _service;
        public GroomerScheduleController(IGroomerProfileService service) => _service = service;

        [HttpGet("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long groomerId) => Ok(await _service.GetTimesAsync(groomerId));

        /// <summary>کل ساعت‌های کاری را جایگزین می‌کند</summary>
        [HttpPut("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerTimeVDto>>), 200)]
        public async Task<IActionResult> SetTimes(GroomerProfileSetDto<GroomerTimeInputDto> dto) =>
            Ok(await _service.SetTimesAsync(dto.GroomerId, dto.Items, ownerUserId: null));

        [HttpGet("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long groomerId) => Ok(await _service.GetZonesAsync(groomerId));

        /// <summary>کل محدوده‌های پوشش را جایگزین می‌کند</summary>
        [HttpPut("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerZoneVDto>>), 200)]
        public async Task<IActionResult> SetZones(GroomerProfileSetDto<GroomerZoneInputDto> dto) =>
            Ok(await _service.SetZonesAsync(dto.GroomerId, dto.Items, ownerUserId: null));
    }
}
