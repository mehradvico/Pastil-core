using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Interface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Areas.EndUser.Controllers
{
    [Area("EndUser")]
    [Route("api/[area]/push-test")]
    [ApiController]
    [Authorize]
    public class PushTestController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ICurrentUserHelper _currentUser;

        public PushTestController(
            IPushNotificationService pushNotificationService,
            ICurrentUserHelper currentUser)
        {
            _pushNotificationService = pushNotificationService;
            _currentUser = currentUser;
        }

        public class PushTestSendDto
        {
            public PushTypeEnum PushType { get; set; } = PushTypeEnum.PushSignInUser;
            public string Token1 { get; set; }
            public string Token2 { get; set; }
            public string Token3 { get; set; }
            public string Token4 { get; set; }
            public string Token5 { get; set; }
            public DateTime? SendDate { get; set; }
        }

        [HttpPost("send-to-me")]
        public async Task<IActionResult> SendToMe([FromBody] PushTestSendDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;

            await _pushNotificationService.SendPushAsync(
                pushType: dto.PushType,
                userId: userId,
                token1: dto.Token1,
                token2: dto.Token2,
                token3: dto.Token3,
                token4: dto.Token4,
                token5: dto.Token5,
                sendDate: dto.SendDate
            );

            return Ok(new BaseResultDto(true, "Queued"));
        }
    }
}
