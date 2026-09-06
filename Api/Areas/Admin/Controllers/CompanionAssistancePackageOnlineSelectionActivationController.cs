using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال سازی انتخاب خدمت آنلاین نماینده
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageOnlineSelectionActivationController : ControllerBase
    {
        private readonly ICompanionAssistancePackageOnlineSelectionService _companionAssistancePackageOnlineSelectionService;
        public CompanionAssistancePackageOnlineSelectionActivationController(ICompanionAssistancePackageOnlineSelectionService companionAssistancePackageOnlineSelectionService)
        {
            this._companionAssistancePackageOnlineSelectionService = companionAssistancePackageOnlineSelectionService;
        }

        /// <summary>
        ///  فعال سازی آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionAssistancePackageOnlineSelectionActivationDto dto)
        {
            var companion = _companionAssistancePackageOnlineSelectionService.ActivationDto(dto);
            return Ok(companion);
        }
    }
}
