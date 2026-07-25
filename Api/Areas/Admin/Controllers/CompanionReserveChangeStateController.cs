using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رزروهای نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveChangeStateController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;
        public CompanionReserveChangeStateController(ICompanionReserveService companionReserveService)
        {
            this._companionReserveService = companionReserveService;
        }

        /// <summary>
        ///  تغییر وضعیت آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionReserveChangeStateDto dto)
        {
            var companion = await _companionReserveService.UpdateReserveStateDto(dto);
            return Ok(companion);
        }
    }
}
