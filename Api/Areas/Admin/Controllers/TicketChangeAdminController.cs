using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر مدیر رسیدگی به تیکت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketChangeAdminController : ControllerBase
    {
        private ITicketService TicketService;
        public TicketChangeAdminController(ITicketService TicketService)
        {
            this.TicketService = TicketService;
        }

        /// <summary>
        /// ویرایش ادمین آیتم
        /// </summary>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(AssignTicketAdminDto dto)
        {
            var result = await TicketService.AssignAdminAsync(dto);
            return Ok(result);
        }
    }
}
