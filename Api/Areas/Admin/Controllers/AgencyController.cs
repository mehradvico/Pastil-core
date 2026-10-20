using Application.Services.AgencySrvs.AgencySrv.Dto;
using Application.Services.AgencySrvs.AgencySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// لیست یکپارچه‌ی نمایندگی‌ها (فروشگاه، کلینیک، آرایشگاه و ...)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyService _service;
        public AgencyController(IAgencyService service)
        {
            _service = service;
        }

        /// <summary>
        /// جستجو در همه‌ی انواع نمایندگی؛ Kind خالی = همه، 1 = نمایندگی (کلینیک/آرایشگاه/مربی)، 2 = فروشگاه
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AgencySearchVDto), 200)]
        public async Task<IActionResult> Get([FromQuery] AgencyInputDto dto)
        {
            return Ok(await _service.SearchAsync(dto));
        }
    }
}
