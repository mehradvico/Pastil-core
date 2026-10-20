using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// ساعت کاری و محدوده‌ی پوشش کلینیک‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicScheduleController : ControllerBase
    {
        private readonly IClinicProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicScheduleController(IClinicProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long clinicId) => Ok(await _service.GetTimesAsync(clinicId));

        [HttpPut("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicTimeVDto>>), 200)]
        public async Task<IActionResult> SetTimes(ClinicProfileSetDto<ClinicTimeInputDto> dto) =>
            Ok(await _service.SetTimesAsync(dto.ClinicId, dto.Items, UserId));

        [HttpGet("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long clinicId) => Ok(await _service.GetZonesAsync(clinicId));

        [HttpPut("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicZoneVDto>>), 200)]
        public async Task<IActionResult> SetZones(ClinicProfileSetDto<ClinicZoneInputDto> dto) =>
            Ok(await _service.SetZonesAsync(dto.ClinicId, dto.Items, UserId));
    }
}
