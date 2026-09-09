using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت افزودن تخفیف رزرو دوره مدرسه
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolReserveSetRebateController : ControllerBase
    {
        private readonly ISchoolReserveService _schoolReserveService;
        public SchoolReserveSetRebateController(ISchoolReserveService schoolReserveService)
        {
            _schoolReserveService = schoolReserveService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolReserveRebateCodeDto dto)
        {
            var result = await _schoolReserveService.SetRebateCodeAsyncDto(dto);
            return Ok(result);
        }
    }
}
