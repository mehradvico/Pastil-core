using Application.Common.Interface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Application.Services.CommonSrv.PushSubscriptionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api.Areas.EndUser.Controllers
{
    [Area("EndUser")]
    [Route("api/[area]/push")]
    [ApiController]
    public class PushController : ControllerBase
    {
        private readonly IPushSubscriptionService _pushSubscriptionService;
        private readonly ICurrentUserHelper _currentUser;

        public PushController(
            IPushSubscriptionService pushSubscriptionService,
            ICurrentUserHelper currentUser)
        {
            _pushSubscriptionService = pushSubscriptionService;
            _currentUser = currentUser;
        }

        [HttpGet("public-key")]
        [AllowAnonymous]
        public IActionResult PublicKey([FromServices] IOptions<VapidKeysOption> opt)
        {
            return Ok(new { publicKey = opt.Value.PublicKey });
        }

        [HttpPost("subscribe")]
        [AllowAnonymous]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscribeDto dto)
        {
            long? userId = null;
            if (User.Identity?.IsAuthenticated == true &&
                long.TryParse(User.FindFirst("UserId")?.Value, out var authenticatedUserId))
            {
                userId = authenticatedUserId;
            }

            var res = await _pushSubscriptionService.SubscribeAsync(userId, dto);
            return Ok(res);
        }

        [HttpPost("attach")]
        [Authorize]
        public async Task<IActionResult> Attach([FromBody] PushAttachDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var res = await _pushSubscriptionService.AttachAsync(userId, dto.DeviceKey);
            return Ok(res);
        }
    }
}
