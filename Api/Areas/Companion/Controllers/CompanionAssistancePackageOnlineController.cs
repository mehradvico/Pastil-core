using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// فهرست خدمات آنلاین نماینده
    /// </summary>
    ///
    [Area("Companion")]
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

        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه پکیج خدمات آنلاین</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackageOnlineDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistancePackageOnlineService.FindAsyncDto(id);
            return Ok(agency);
        }
    }
}
