using Application.Common.Dto.Result;
using Application.Services.WeekDaySrv.WeekDaySrv.Dto;
using Application.Services.WeekDaySrv.WeekDaySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// روزهای هفته
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class WeekDayController : ControllerBase
    {
        private readonly IWeekDayService _weekDayService;
        public WeekDayController(IWeekDayService weekDayService)
        {
            this._weekDayService = weekDayService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(WeekDaySearchDto), 200)]
        public IActionResult Get([FromQuery] WeekDayInputDto dto)
        {
            var search = _weekDayService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه روز هفته</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<WeekDayDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _weekDayService.FindAsyncDto(id);
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<WeekDayDto>), 200)]
        public async Task<IActionResult> Post(WeekDayDto dto)
        {
            var result = await _weekDayService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(WeekDayDto dto)
        {
            var agency = _weekDayService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _weekDayService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
