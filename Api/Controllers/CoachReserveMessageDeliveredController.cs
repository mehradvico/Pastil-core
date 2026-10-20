using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// ثبت تحویل پیام چت خدمات آنلاین
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveMessageDeliveredController : ControllerBase
    {
        private readonly ICoachReserveMessageService _companionReserveMessageService;

        public CoachReserveMessageDeliveredController(ICoachReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] CoachReserveMessageDeliveredDto dto)
        {
            var result = await _companionReserveMessageService.UpdateDeliveredDto(dto);
            return Ok(result);
        }
    }
}
