using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Pansion.Controllers
{
    /// <summary>
    /// مدیریت کنسلی رزرو نماینده
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveCancelController : ControllerBase
    {
        private readonly IPansionReserveService _PansionReserveService;
        public PansionReserveCancelController(IPansionReserveService PansionReserveService)
        {
            this._PansionReserveService = PansionReserveService;
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionReserveCancelDto dto)
        {
            var Pansion = await _PansionReserveService.UpdatePansionReserveCancelDto(dto);
            return Ok(Pansion);
        }
    }
}
