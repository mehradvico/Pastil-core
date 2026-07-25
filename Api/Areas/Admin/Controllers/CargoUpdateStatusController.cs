using Application.Common.Dto.Result;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.Content.CargoSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  کارگو
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UpdateCargoStatusController : ControllerBase
    {
        private ICargoService _cargoService;
        public UpdateCargoStatusController(ICargoService cargoService)
        {
            _cargoService = cargoService;
        }
        /// <summary>
        /// تغییر وضعیت آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CargoUpdateStatusDto>), 200)]
        public async Task<IActionResult> Put(CargoUpdateStatusDto dto)
        {
            var result = await _cargoService.CargoUpdateStatusAsyncDto(dto);
            return Ok(result);
        }
    }
}
