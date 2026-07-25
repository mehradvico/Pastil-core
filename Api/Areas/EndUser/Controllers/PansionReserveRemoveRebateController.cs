using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت حذف تخفیف رزرو نماینده
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveRemoveRebateController : ControllerBase
    {
        private readonly IPansionReserveService _PansionReserveService;
        public PansionReserveRemoveRebateController(IPansionReserveService PansionReserveService)
        {
            _PansionReserveService = PansionReserveService;
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(long id)
        {
            var Pansion = await _PansionReserveService.ClearRebateCodeAsync(id);
            return Ok(Pansion);
        }
    }
}
