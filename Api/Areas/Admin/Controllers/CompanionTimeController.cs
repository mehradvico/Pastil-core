using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionTimeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ساعات کاری نمایندگان (مرکز)
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
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
        public IActionResult Get([FromQuery] CompanionTimeInputDto dto)
        {
            var search = _companionTimeService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه نمایندگی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTimeUpdateListDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionTimeService.GetListAsync(id);
            return Ok(agency);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(CompanionTimeUpdateListDto dto)
        {
            var result = await _companionTimeService.InsertUpdateListAsync(dto);
            return Ok(result);
        }
    }
}
