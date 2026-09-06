using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// فهرست خدمات آنلاین نماینده
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageOnlineController : ControllerBase
    {
        private readonly ICompanionAssistancePackageOnlineService _companionAssistancePackageOnlineService;
        public CompanionAssistancePackageOnlineController(ICompanionAssistancePackageOnlineService companionAssistancePackageOnlineService)
        {
            this._companionAssistancePackageOnlineService = companionAssistancePackageOnlineService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistancePackageOnlineSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistancePackageOnlineInputDto dto)
        {
            var search = _companionAssistancePackageOnlineService.Search(dto);
            return Ok(search);
        }
    }
}
