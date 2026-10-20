using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
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
    public class ClinicScheduleController : ControllerBase
    {
        private readonly IClinicProfileService _service;
        public ClinicScheduleController(IClinicProfileService service) => _service = service;

        [HttpGet("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long clinicId) => Ok(await _service.GetTimesAsync(clinicId));

        /// <summary>کل ساعت‌های کاری را جایگزین می‌کند</summary>
        [HttpPut("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicTimeVDto>>), 200)]
        public async Task<IActionResult> SetTimes(ClinicProfileSetDto<ClinicTimeInputDto> dto) =>
            Ok(await _service.SetTimesAsync(dto.ClinicId, dto.Items, ownerUserId: null));

        [HttpGet("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long clinicId) => Ok(await _service.GetZonesAsync(clinicId));

        /// <summary>کل محدوده‌های پوشش را جایگزین می‌کند</summary>
        [HttpPut("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicZoneVDto>>), 200)]
        public async Task<IActionResult> SetZones(ClinicProfileSetDto<ClinicZoneInputDto> dto) =>
            Ok(await _service.SetZonesAsync(dto.ClinicId, dto.Items, ownerUserId: null));
    }
}
