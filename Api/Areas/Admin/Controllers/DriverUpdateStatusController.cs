using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.DriverSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر وضعیت راننده
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DriverUpdateStatusController : ControllerBase
    {
        private readonly IDriverService _DriverService;
        public DriverUpdateStatusController(IDriverService DriverService)
        {
            this._DriverService = DriverService;
        }

        /// <summary>
        ///  تغییر وضعیت آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(DriverUpdateStatusDto dto)
        {
            var Driver = await _DriverService.DriverUpdateStatusAsyncDto(dto);
            return Ok(Driver);
        }
    }
}
