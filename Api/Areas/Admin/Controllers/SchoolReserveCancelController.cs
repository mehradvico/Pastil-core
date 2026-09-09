using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// لغو ثبت‌نام دوره‌ی مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolReserveCancelController : ControllerBase
    {
        private readonly ISchoolReserveService _schoolReserveService;
        public SchoolReserveCancelController(ISchoolReserveService schoolReserveService)
        {
            this._schoolReserveService = schoolReserveService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolReserveCancelDto dto)
        {
            var result = await _schoolReserveService.UpdateCancelDto(dto);
            return Ok(result);
        }
    }
}
