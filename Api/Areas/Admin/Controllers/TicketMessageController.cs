using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketItemSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پیام‌های تیکت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketMessageController : ControllerBase
    {
        private ITicketMessageService TicketMessageService;
        public TicketMessageController(ITicketMessageService TicketMessageService)
        {
            this.TicketMessageService = TicketMessageService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        ///
        [HttpGet("{ticketId}")]
        [ProducesResponseType(typeof(BaseResultDto<TicketMessageSearchDto>), 200)]
        public async Task<IActionResult> Get(long ticketId, [FromQuery] TicketMessageInputDto dto)
        {
            var searchDto = await TicketMessageService.GetAdminMessagesAsync(ticketId, dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// ارسال پیام جدید توسط ادمین
        /// </summary>
        ///
        [HttpPost("{ticketId}")]
        [ProducesResponseType(typeof(BaseResultDto<TicketItemVDto>), 200)]
        public async Task<IActionResult> Post(long ticketId, SendTicketMessageDto dto)
        {
            var result = await TicketMessageService.SendAdminMessageAsync(ticketId, dto);
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
            var result = await TicketMessageService.MarkAsSeenForAdminAsync(ticketId);
            return Ok(result);
        }
    }
}