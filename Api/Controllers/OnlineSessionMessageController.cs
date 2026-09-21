using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.OnlineSessionSrv.Dto;
using Application.Services.CompanionSrvs.OnlineSessionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// پیام‌های چت جلسه‌ی آنلاین — رابطه‌محور (فقط دو طرف جلسه).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnlineSessionMessageController : ControllerBase
    {
        private readonly IOnlineSessionService _onlineSessionService;

        public OnlineSessionMessageController(IOnlineSessionService onlineSessionService)
        {
            _onlineSessionService = onlineSessionService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<OnlineSessionMessageVDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] OnlineSessionMessageInputDto dto)
            => Ok(await _onlineSessionService.MessagesAsync(dto));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<OnlineSessionMessageVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] OnlineSessionMessageSendDto dto)
            => Ok(await _onlineSessionService.SendMessageAsync(dto));

        [HttpPut("Read")]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Read([FromBody] OnlineSessionMessageReadDto dto)
            => Ok(await _onlineSessionService.ReadAsync(dto));
    }
}
