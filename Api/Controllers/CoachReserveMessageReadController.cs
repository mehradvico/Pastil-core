using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// ثبت خوانده شدن پیام‌های چت خدمات آنلاین
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveMessageReadController : ControllerBase
    {
        private readonly ICoachReserveMessageService _companionReserveMessageService;

        public CoachReserveMessageReadController(ICoachReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] CoachReserveMessageReadDto dto)
        {
            var result = await _companionReserveMessageService.UpdateReadDto(dto);
            return Ok(result);
        }
    }
}
