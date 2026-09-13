using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionTimeSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مشاهده ساعات کاری نمایندگان (مرکز) - برای کاربر نهایی هنگام رزرو
    /// </summary>
    ///
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionTimeController : ControllerBase
    {
        private readonly ICompanionTimeService _companionTimeService;
        public CompanionTimeController(ICompanionTimeService companionTimeService)
        {
            this._companionTimeService = companionTimeService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionTimeSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] CompanionTimeInputDto dto)
        {
            var search = await _companionTimeService.SearchWithAvailabilityAsync(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTimeVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionTimeService.FindAsyncVDto(id);
            return Ok(agency);
        }
    }
}
