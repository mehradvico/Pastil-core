using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر وضعیت ثبت‌نام دوره‌ی مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolReserveChangeStatusController : ControllerBase
    {
        private readonly ISchoolReserveService _schoolReserveService;
        public SchoolReserveChangeStatusController(ISchoolReserveService schoolReserveService)
        {
            this._schoolReserveService = schoolReserveService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolReserveStatusDto dto)
        {
            var result = await _schoolReserveService.UpdateStatusDto(dto);
            return Ok(result);
        }
    }
}
