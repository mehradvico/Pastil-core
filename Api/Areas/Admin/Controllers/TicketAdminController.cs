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
    public class TicketAdminController : ControllerBase
    {
        private ITicketService TicketService;

        /// <summary>
        /// مدیریت تیکت‌ ها
        /// </summary>
        public TicketAdminController(ITicketService TicketService)
        {
            this.TicketService = TicketService;
        }

        /// <summary>
        /// اطلاهات آیتم
        /// </summary>
        ///
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TicketVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var ticket = await TicketService.FindCurrentAdminAsync(id);
            return Ok(ticket);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        ///
        [HttpGet]
        [ProducesResponseType(typeof(TicketSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] TicketInputDto dto)
        {
            var searchDto = await TicketService.SearchCurrentAdminAsync(dto);
            return Ok(searchDto);
        }
    }
}