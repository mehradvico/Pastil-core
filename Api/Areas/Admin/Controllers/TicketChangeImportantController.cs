using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر اهمیت تیکت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketChangeImportantController : ControllerBase
    {
        private ITicketService TicketService;
        public TicketChangeImportantController(ITicketService TicketService)
        {
            this.TicketService = TicketService;
        }

        /// <summary>
        /// تغییر اهمیت آیتم
        /// </summary>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ChangeTicketImportanceDto dto)
        {
            var result = await TicketService.ChangeImportanceAsync(dto);
            return Ok(result);
        }
    }
}
