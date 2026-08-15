using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر وضعیت تیکت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketChangeStatusController : ControllerBase
    {
        private ITicketService TicketService;
        public TicketChangeStatusController(ITicketService TicketService)
        {
            this.TicketService = TicketService;
        }

        /// <summary>
        /// تغییر وضعیت آیتم
        /// </summary>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ChangeTicketStatusDto dto)
        {
            var result = await TicketService.ChangeStatusAsync(dto);
            return Ok(result);
        }
    }
}
