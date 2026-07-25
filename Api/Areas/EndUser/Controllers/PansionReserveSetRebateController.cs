using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت افزودن تخفیف رزرو نماینده
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveSetRebateController : ControllerBase
    {
        private readonly IPansionReserveService _PansionReserveService;
        public PansionReserveSetRebateController(IPansionReserveService PansionReserveService)
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
        public async Task<IActionResult> Put(PansionReserveRebateCodeDto dto)
        {
            var Pansion = await _PansionReserveService.SetRebateCodeAsyncDto(dto);
            return Ok(Pansion);
        }
    }
}
