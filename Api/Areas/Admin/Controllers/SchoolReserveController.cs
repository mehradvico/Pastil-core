using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ثبت‌نام‌های دوره‌های مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolReserveController : ControllerBase
    {
        private readonly ISchoolReserveService _schoolReserveService;
        public SchoolReserveController(ISchoolReserveService schoolReserveService)
        {
            this._schoolReserveService = schoolReserveService;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolReserveInputDto dto)
        {
            var search = _schoolReserveService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var reserve = await _schoolReserveService.FindAsyncVDto(id);
            return Ok(reserve);
        }
    }
}
