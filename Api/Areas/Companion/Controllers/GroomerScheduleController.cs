using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// ساعت کاری و محدوده‌ی پوشش آرایشگاه‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerScheduleController : ControllerBase
    {
        private readonly IGroomerProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerScheduleController(IGroomerProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long groomerId) => Ok(await _service.GetTimesAsync(groomerId));

        [HttpPut("Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerTimeVDto>>), 200)]
        public async Task<IActionResult> SetTimes(GroomerProfileSetDto<GroomerTimeInputDto> dto) =>
            Ok(await _service.SetTimesAsync(dto.GroomerId, dto.Items, UserId));

        [HttpGet("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long groomerId) => Ok(await _service.GetZonesAsync(groomerId));

        [HttpPut("Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerZoneVDto>>), 200)]
        public async Task<IActionResult> SetZones(GroomerProfileSetDto<GroomerZoneInputDto> dto) =>
            Ok(await _service.SetZonesAsync(dto.GroomerId, dto.Items, UserId));
    }
}
