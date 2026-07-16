using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت تیکت‌ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private ITicketService TicketService;

        /// <summary>
        /// مدیریت تیکت‌ها
        /// </summary>
        public TicketController(ITicketService TicketService)
        {
            this.TicketService = TicketService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        ///
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TicketVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var ticket = await TicketService.FindAdminAsync(id);
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
            var searchDto = await TicketService.SearchAdminAsync(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// ایتم جدید
        /// </summary>
        ///
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TicketVDto>), 200)]
        public async Task<IActionResult> Post(CreateAdminTicketDto dto)
        {
            var result = await TicketService.InsertAdminAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await TicketService.DeleteAsync(id);
            return Ok(result);
        }
    }
}