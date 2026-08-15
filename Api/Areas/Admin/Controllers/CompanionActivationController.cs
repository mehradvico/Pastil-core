using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال سازی نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionActivationController : ControllerBase
    {
        private readonly ICompanionService _companionService;
        public CompanionActivationController(ICompanionService companionService)
        {
            this._companionService = companionService;
        }

        /// <summary>
        ///  فعال سازی آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionActivationDto dto)
        {
            var companion = await _companionService.ActivationAsyncDto(dto);
            return Ok(companion);
        }
    }
}
