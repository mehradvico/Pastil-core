using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پکیج های خدمات نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageActivationController : ControllerBase
    {
        private readonly ICompanionAssistancePackageService _companionAssistancePackageService;
        public CompanionAssistancePackageActivationController(ICompanionAssistancePackageService companionAssistancePackageService)
        {
            this._companionAssistancePackageService = companionAssistancePackageService;
        }

        /// <summary>
        ///  فعال سازی آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionAssistancePackageActivationDto dto)
        {
            var companion = _companionAssistancePackageService.ActivationDto(dto);
            return Ok(companion);
        }
    }
}
