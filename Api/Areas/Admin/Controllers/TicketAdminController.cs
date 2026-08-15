using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تخصیص مدیر رسیدگی به تیکت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketAdminController : ControllerBase
    {
        private ITicketService TicketService;
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

        /// <summary>
        /// اختصاص تیکت آزاد به ادمین جاری
        /// </summary>
        [HttpPut("{id}/Take")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Take(long id)
        {
            var result = await TicketService.TakeCurrentAdminAsync(id);
            return Ok(result);
        }
    }
}
