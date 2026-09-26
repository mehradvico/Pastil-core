using Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// لیست یکپارچه‌ی رزروهای خدمت و خریدهای مشاوره آنلاین نمایندگان
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveUnifiedController : ControllerBase
    {
        private readonly IUnifiedReserveService _service;

        public CompanionReserveUnifiedController(IUnifiedReserveService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UnifiedReserveListDto), 200)]
        public async Task<IActionResult> Get([FromQuery] UnifiedReserveInputDto dto)
        {
            return Ok(await _service.SearchAsync(dto));
        }
    }
}
