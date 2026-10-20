using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت  خدمات
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AssistanceController : ControllerBase
    {
        private readonly IAssistanceService _assistanceService;
        public AssistanceController(IAssistanceService assistanceService)
        {
            this._assistanceService = assistanceService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(AssistanceSearchDto), 200)]
        public IActionResult Get([FromQuery] AssistanceInputDto dto)
        {
            dto.IsGrooming ??= false;
            var search = _assistanceService.Search(dto);
            return Ok(search);
        }
    }
}
