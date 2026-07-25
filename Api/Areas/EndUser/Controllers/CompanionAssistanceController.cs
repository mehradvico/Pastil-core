using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت خدمات نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistanceController : ControllerBase
    {
        private readonly ICompanionAssistanceService _companionAssistanceService;
        public CompanionAssistanceController(ICompanionAssistanceService companionAssistanceService)
        {
            this._companionAssistanceService = companionAssistanceService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceInputDto dto)
        {
            dto.Available = true;
            var search = _companionAssistanceService.Search(dto);
            return Ok(search);
        }
    }
}
