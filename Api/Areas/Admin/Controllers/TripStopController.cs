using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripStopSrv.Dto;
using Application.Services.TripSrv.TripStopSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// توقف های سفر
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripStopController : ControllerBase
    {
        private readonly ITripStopService _assistanceService;
        public TripStopController(ITripStopService assistanceService)
        {
            this._assistanceService = assistanceService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(TripStopSearchDto), 200)]
        public IActionResult Get([FromQuery] TripStopInputDto dto)
        {
            var search = _assistanceService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمات</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TripStopDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _assistanceService.FindAsyncDto(id);
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripStopDto>), 200)]
        public async Task<IActionResult> Post(TripStopDto dto)
        {
            var result = await _assistanceService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(TripStopDto dto)
        {
            var agency = _assistanceService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _assistanceService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
