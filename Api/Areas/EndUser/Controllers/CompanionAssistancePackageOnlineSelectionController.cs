using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// فهرست گزینه‌های خدمات آنلاین تأییدشده‌ی یک پکیج (برای انتخاب کاربر موقع رزرو)
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageOnlineSelectionController : ControllerBase
    {
        private readonly ICompanionAssistancePackageOnlineSelectionService _companionAssistancePackageOnlineSelectionService;
        public CompanionAssistancePackageOnlineSelectionController(ICompanionAssistancePackageOnlineSelectionService companionAssistancePackageOnlineSelectionService)
        {
            this._companionAssistancePackageOnlineSelectionService = companionAssistancePackageOnlineSelectionService;
        }

        /// <summary>
        ///  جستجو - فقط گزینه‌های تأییدشده (Active) این کنترلر برمی‌گردد
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistancePackageOnlineSelectionSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistancePackageOnlineSelectionInputDto dto)
        {
            dto.Available = true;
            var search = _companionAssistancePackageOnlineSelectionService.Search(dto);
            return Ok(search);
        }
    }
}
