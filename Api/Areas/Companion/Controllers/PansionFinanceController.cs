using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// گزارش مالی پانسیون‌های من. مثل بقیه‌ی Pansion* مالکیت از دیتابیس سنجیده می‌شود و به CompanionId توکن وابسته نیست.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionFinanceController : ControllerBase
    {
        private readonly IFinanceCompanionService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PansionFinanceController(IFinanceCompanionService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>pansionId خالی = همه‌ی پانسیون‌های من؛ permitted = فیلتر تأیید مالی</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinancePansionVDto>), 200)]
        public async System.Threading.Tasks.Task<IActionResult> Get([FromQuery] FinancePansionInputDto dto) =>
            Ok(await _service.SearchPansionAsync(dto, _currentUser.CurrentUser.UserId));
    }
}
