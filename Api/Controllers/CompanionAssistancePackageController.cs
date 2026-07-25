using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت پکیج های خدمات نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionAssistancePackageController : ControllerBase
    {
        private readonly ICompanionAssistancePackageService _companionAssistancePackageService;
        public CompanionAssistancePackageController(ICompanionAssistancePackageService companionAssistancePackageService)
        {
            this._companionAssistancePackageService = companionAssistancePackageService;
        }


        /// <summary>
        /// جستجو پکیج های خدمات کاربران
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistancePackageSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistancePackageInputDto dto)
        {
            var search = _companionAssistancePackageService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه پکیج خدمات نمایندگان</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackageDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistancePackageService.FindAsyncVDto(id);
            return Ok(agency);
        }
    }
}
