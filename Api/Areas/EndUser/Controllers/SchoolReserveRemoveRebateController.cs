using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت حذف تخفیف رزرو دوره مدرسه
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolReserveRemoveRebateController : ControllerBase
    {
        private readonly ISchoolReserveService _schoolReserveService;
        public SchoolReserveRemoveRebateController(ISchoolReserveService schoolReserveService)
        {
            _schoolReserveService = schoolReserveService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(long id)
        {
            var result = await _schoolReserveService.ClearRebateCodeAsync(id);
            return Ok(result);
        }
    }
}
