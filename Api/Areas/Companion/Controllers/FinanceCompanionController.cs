using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت حسابداری کلینیک ها
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceCompanionController : ControllerBase
    {
        private readonly IFinanceCompanionService _financeCompanionService;
        private readonly ICurrentUserHelper _currentUser;

        public FinanceCompanionController(IFinanceCompanionService financeCompanionService, ICurrentUserHelper currentUser)
        {
            _financeCompanionService = financeCompanionService;
            _currentUser = currentUser;
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinanceCompanionVDto>), 200)]
        public async System.Threading.Tasks.Task<IActionResult> Get([FromQuery] FinanceCompanionInputDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
            {
                // پانسیونِ زیر ارائه‌دهنده نمایندگی ندارد؛ اپ‌های نصب‌شده با همین آدرس گزارش پانسیون را بگیرند.
                var pansionResult = await _financeCompanionService.SearchPansionAsync(new FinancePansionInputDto { Permitted = dto.Permitted }, _currentUser.CurrentUser.UserId);
                if (!pansionResult.IsSuccess && pansionResult.Messages.Any(m => m.Item1 == Resource.Notification.AccessDenied))
                    return Forbid();
                return Ok(pansionResult);
            }
            dto.CompanionId = _currentUser.CurrentUser.CompanionId.Value;
            var searchDto = await _financeCompanionService.SearchAsync(dto);
            return Ok(searchDto);
        }
    }
}
