
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  بیمه ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsuranceManualPayController : ControllerBase
    {
        private ICompanionInsurancePackageSaleService _insuranceService;
        public CompanionInsuranceManualPayController(ICompanionInsurancePackageSaleService insuranceService)
        {
            _insuranceService = insuranceService;
        }
        /// <summary>
        ///  پرداخت دستی آیتم
        /// </summary>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(CompanionInsurancePackageSaleManualPayDto), 200)]
        public async Task<IActionResult> Put(CompanionInsurancePackageSaleManualPayDto dto)
        {
            var result = await _insuranceService.CompanionInsurancePackageSaleManualPayAsync(dto);
            return Ok(result);
        }
    }
}
