using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// ساعت کاری و محدوده‌ی پوشش مربی‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachScheduleController : ControllerBase
    {
        private readonly ICoachProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachScheduleController(ICoachProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long coachId) => Ok(await _service.GetTimesAsync(coachId));

        [HttpPut("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachTimeVDto>>), 200)]
        public async Task<IActionResult> SetTimes(CoachProfileSetDto<CoachTimeInputDto> dto) =>
            Ok(await _service.SetTimesAsync(dto.CoachId, dto.Items, UserId));

        [HttpGet("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long coachId) => Ok(await _service.GetZonesAsync(coachId));

        [HttpPut("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachZoneVDto>>), 200)]
        public async Task<IActionResult> SetZones(CoachProfileSetDto<CoachZoneInputDto> dto) =>
            Ok(await _service.SetZonesAsync(dto.CoachId, dto.Items, UserId));
    }
}
