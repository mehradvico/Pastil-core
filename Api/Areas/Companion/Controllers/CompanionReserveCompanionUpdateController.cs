using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت انجام رزرو نماینده
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveOperatorUpdateController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;
        public CompanionReserveOperatorUpdateController(ICompanionReserveService companionReserveService)
        {
            this._companionReserveService = companionReserveService;
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionReserveOperatorDto dto)
        {
            var companion = await _companionReserveService.CompanionReserveCompanionUpdateAsyncDto(dto);
            return Ok(companion);
        }
    }
}
