using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// لغو ثبت‌نام کاربر در یک دوره‌ی مدرسه
    /// </summary>
    [Area("EndUser")]
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
