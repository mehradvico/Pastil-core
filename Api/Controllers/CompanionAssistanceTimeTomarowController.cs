using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت زمان های خدمات نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionAssistanceTimeTomarowController : ControllerBase
    {
        private readonly ICompanionAssistanceTimeService _companionAssistanceTimeService;
        public CompanionAssistanceTimeTomarowController(ICompanionAssistanceTimeService companionAssistanceTimeService)
        {
            _companionAssistanceTimeService = companionAssistanceTimeService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceTimeSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] CompanionAssistanceTimeInputDto dto)
        {
            var search = await _companionAssistanceTimeService.GetForTomarowAsync(dto);
            return Ok(search);
        }

    }
}
