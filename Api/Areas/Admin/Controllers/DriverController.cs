using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.DriverSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رانندگان
    /// </summary>
    /// 
    [Area("Admin")]
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
        /// <param name="id">شناسه نماینده</param>
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
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DriverDto>), 200)]
        public async Task<IActionResult> Post(DriverDto dto)
        {
            dto.StatusId = (long)DriverRequestStatusEnum.DriverRequestStatus_Accepted;
            var result = await _DriverService.InsertAsyncDto(dto);
            return Ok(result);
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
            var Driver = await _DriverService.UpdateAsyncDto(dto);
            return Ok(Driver);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _DriverService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
