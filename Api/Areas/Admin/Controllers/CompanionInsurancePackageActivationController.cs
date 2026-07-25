using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// بیمه های نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsurancePackageActivationController : ControllerBase
    {
        private readonly ICompanionInsurancePackageService _companionService;
        public CompanionInsurancePackageActivationController(ICompanionInsurancePackageService companionService)
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
        public IActionResult Put(CompanionInsurancePackageActivationDto dto)
        {
            var companion = _companionService.ActivationDto(dto);
            return Ok(companion);
        }
    }
}
