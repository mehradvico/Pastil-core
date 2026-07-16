using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketItemSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت پیام‌های تیکت
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketMessageController : ControllerBase
    {
        private ITicketMessageService TicketMessageService;

        /// <summary>
        /// مدیریت پیام‌های تیکت
        /// </summary>
        public TicketMessageController(ITicketMessageService TicketMessageService)
        {
            this.TicketMessageService = TicketMessageService;
        }

        /// <summary>
        /// دریافت پیام‌های تیکت کاربر
        /// </summary>
        ///
        [HttpGet("{ticketId}")]
        [ProducesResponseType(typeof(BaseResultDto<TicketMessageSearchDto>), 200)]
        public async Task<IActionResult> Get(long ticketId, [FromQuery] TicketMessageInputDto dto)
        {
            var searchDto = await TicketMessageService.GetUserMessagesAsync(ticketId, dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        ///
        [HttpPost("{ticketId}")]
        [ProducesResponseType(typeof(BaseResultDto<TicketItemVDto>), 200)]
        public async Task<IActionResult> Post(long ticketId, SendTicketMessageDto dto)
        {
            var result = await TicketMessageService.SendUserMessageAsync(ticketId, dto);
            return Ok(result);
        }

        /// <summary>
        /// ثبت پیام‌های تیکت به‌عنوان خوانده‌شده
        /// </summary>
        /// 
        [HttpPut("{ticketId}/Seen")]
        [ProducesResponseType(typeof(BaseResultDto<TicketSeenVDto>), 200)]
        public async Task<IActionResult> Put(long ticketId)
        {
            var result = await TicketMessageService.MarkAsSeenForUserAsync(ticketId);
            return Ok(result);
        }
    }
}