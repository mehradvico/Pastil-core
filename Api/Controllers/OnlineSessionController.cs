using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.OnlineSessionSrv.Dto;
using Application.Services.CompanionSrvs.OnlineSessionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// جلسه‌ی آنلاین بدون رزرو (نماینده ← کاربر) — بدون Area چون مجاز بودن رابطه‌محور است؛
    /// شروع جلسه فقط برای نماینده/اپراتور و بقیه‌ی عملیات فقط برای دو طرف جلسه.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnlineSessionController : ControllerBase
    {
        private readonly IOnlineSessionService _onlineSessionService;

        public OnlineSessionController(IOnlineSessionService onlineSessionService)
        {
            _onlineSessionService = onlineSessionService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<OnlineSessionVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] OnlineSessionStartDto dto)
            => Ok(await _onlineSessionService.StartAsync(dto));

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<OnlineSessionVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
            => Ok(await _onlineSessionService.FindAsync(id));

        [HttpGet("SearchUsers")]
        [ProducesResponseType(typeof(BaseResultDto<List<OnlineSessionUserVDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
            => Ok(await _onlineSessionService.SearchUsersAsync(q));

        [HttpGet("Active")]
        [ProducesResponseType(typeof(BaseResultDto<List<OnlineSessionVDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Active()
            => Ok(await _onlineSessionService.ActiveAsync());
    }
}
