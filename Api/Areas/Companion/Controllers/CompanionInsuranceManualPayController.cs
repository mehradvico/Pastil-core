using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    ///  پرداخت دستی بیمه ها
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsuranceManualPayController : ControllerBase
    {
        private ICompanionInsurancePackageSaleService _tripService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionInsuranceManualPayController(ICompanionInsurancePackageSaleService tripService, ICurrentUserHelper currentUserHelper)
        {
            _tripService = tripService;
            _currentUserHelper = currentUserHelper;
        }
        /// <summary>
        ///  پرداخت دستی بیمه ها
        /// </summary>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(CompanionInsurancePackageSaleManualPayDto), 200)]
        public async Task<IActionResult> Put(CompanionInsurancePackageSaleManualPayDto dto)
        {
            var result = await _tripService.CompanionInsurancePackageSaleManualPayAsync(dto, _currentUserHelper.CurrentUser.CompanionId);
            return Ok(result);
        }
    }
}
