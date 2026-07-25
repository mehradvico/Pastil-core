using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت پکیج های بیمه نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionInsurancePackageController : ControllerBase
    {
        private readonly ICompanionInsurancePackageService _companionAssistanceUserService;
        public CompanionInsurancePackageController(ICompanionInsurancePackageService companionAssistanceUserService)
        {
            _companionAssistanceUserService = companionAssistanceUserService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionInsurancePackageSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionInsurancePackageInputDto dto)
        {
            var search = _companionAssistanceUserService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        /// اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه کاربر خدمات نمایندگان</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionInsurancePackageDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistanceUserService.FindAsyncVDto(id);
            return Ok(agency);
        }
    }
}
