using Application.Common.Dto.Result;
using Application.Services.Setting.NotifyMessageSrv.Dto;
using Application.Services.Setting.NotifyMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پیام های نوتیفیکیشن
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class NotifyMessageController : ControllerBase
    {
        private readonly INotifyMessageService _service;

        public NotifyMessageController(INotifyMessageService service)
        {
            _service = service;
        }

        ///<summary>
        ///ارسال آیتم
        /// </summary>
        ///<returns></returns>
        [HttpPost("send/all/{id}")]
        public async Task<IActionResult> SendAll(long id)
        {
            await _service.SendToAllAsync(id);
            return Ok("Sent!");
        }

        ///<summary>
        ///ارسال آیتم به کاربر
        /// </summary>
        ///<returns></returns>
        [HttpPost("send/user/{notifyMessageId}/{userId}")]
        public async Task<IActionResult> SendToUser(long notifyMessageId, long userId)
        {
            await _service.SendToUserAsync(notifyMessageId, userId);
            return Ok("Sent!");
        }
    }
}
