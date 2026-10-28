using Application.Common.Interface;
using Application.Services.CommonSrv.CallIceSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// iceServers (STUN + TURN با credential) برای ساخت RTCPeerConnection همه‌ی تماس‌ها (رزرو، جلسه‌ی آنلاین/مشاوره، کلینیک، مربی).
    /// کلاینت قبل از هر تماس می‌گیرد؛ پاسخ کش نشود چون credential کوتاه‌عمر است.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CallIceServersController : ControllerBase
    {
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly CallIceOptions _options;

        public CallIceServersController(ICurrentUserHelper currentUserHelper, IOptions<CallIceOptions> options)
        {
            _currentUserHelper = currentUserHelper;
            _options = options.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            Response.Headers.CacheControl = "no-store";
            var userId = _currentUserHelper.CurrentUser?.UserId ?? 0;
            return Ok(new { iceServers = CallIceRules.Build(_options, userId, DateTime.UtcNow) });
        }
    }
}
