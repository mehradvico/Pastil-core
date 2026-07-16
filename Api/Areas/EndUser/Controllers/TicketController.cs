using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketSrv.Dto;
using Application.Services.Accounting.TicketSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت تیکت‌ ها
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private ITicketService TicketService;

        /// <summary>
        /// مدیریت تیکت‌ ها
        /// </summary>
        public TicketController(ITicketService TicketService)
        {
            this.TicketService = TicketService;
        }

        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        ///
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TicketVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var ticket = await TicketService.FindUserAsync(id);
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
            var searchDto = await TicketService.SearchUserAsync(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        ///
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TicketVDto>), 200)]
        public async Task<IActionResult> Post(CreateTicketDto dto)
        {
            var result = await TicketService.InsertUserAsyncDto(dto);
            return Ok(result);
        }
    }
}