using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت زمان های خدمات نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionAssistanceTimeController : ControllerBase
    {
        private readonly ICompanionAssistanceTimeService _companionAssistanceTimeService;
        public CompanionAssistanceTimeController(ICompanionAssistanceTimeService companionAssistanceTimeService)
        {
            this._companionAssistanceTimeService = companionAssistanceTimeService;
        }


        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceTimeSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceTimeInputDto dto)
        {
            var search = _companionAssistanceTimeService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه زمان خدمات نمایندگان</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceTimeDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistanceTimeService.FindAsyncVDto(id);
            return Ok(agency);
        }
    }
}
