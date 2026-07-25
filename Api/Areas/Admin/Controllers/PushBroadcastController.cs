using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پیام پوش
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PushBroadcastController : ControllerBase
    {
        private readonly IPushBroadcastService _broadcastService;

        public PushBroadcastController(IPushBroadcastService broadcastService)
        {
            _broadcastService = broadcastService;
        }
        /// <summary>
        /// ارسال آیتم
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(PushBroadcastDto dto)
        {
            var res = await _broadcastService.BroadcastAsync(dto);
            return Ok(res);
        }
    }
}
