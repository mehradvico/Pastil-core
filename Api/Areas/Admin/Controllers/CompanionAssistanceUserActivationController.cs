using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کاربران خدمات نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistanceUserActivationController : ControllerBase
    {
        private readonly ICompanionAssistanceUserService _companionAssistanceUserService;
        public CompanionAssistanceUserActivationController(ICompanionAssistanceUserService companionAssistanceUserService)
        {
            this._companionAssistanceUserService = companionAssistanceUserService;
        }

        /// <summary>
        ///  فعال سازی آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionAssistanceUserActivationDto dto)
        {
            var companion = _companionAssistanceUserService.ActivationDto(dto);
            return Ok(companion);
        }
    }
}
