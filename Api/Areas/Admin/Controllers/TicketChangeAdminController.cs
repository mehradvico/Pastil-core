using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت تیکت‌ ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketChangeAdminController : ControllerBase
    {
        private ITicketService TicketService;

        /// <summary>
        /// مدیریت تیکت‌ ها
        /// </summary>
        public TicketChangeAdminController(ITicketService TicketService)
        {
            this.TicketService = TicketService;
        }

        /// <summary>
        /// ویرایش آیتم
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