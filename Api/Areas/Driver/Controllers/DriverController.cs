using Application.Common.Dto.Result;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.DriverSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// مدیریت رانندگان
    /// </summary>
    /// 
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _DriverService;
        public DriverController(IDriverService DriverService)
        {
            this._DriverService = DriverService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(DriverSearchDto), 200)]
        public IActionResult Get([FromQuery] DriverInputDto dto)
        {
            var search = _DriverService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه همکار</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DriverDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Driver = await _DriverService.FindAsyncVDto(id);
            return Ok(Driver);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(DriverDto dto)
        {
            dto.Approved = false;
            var Driver = await _DriverService.UpdateAsyncDto(dto);
            return Ok(Driver);
        }
    }
}
