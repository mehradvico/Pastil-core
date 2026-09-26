using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// هزینه‌های پرداخت‌نشده‌ی خدمات
    /// </summary>
    /// <remarks>بدهی‌هایی که اپراتور/پزشک «پرداخت‌نشده» ثبت کرده؛ بعد از ۷ روز رزرو جدید قفل می‌شود</remarks>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveDebtController : ControllerBase
    {
        private readonly ICompanionReserveDebtService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CompanionReserveDebtController(ICompanionReserveDebtService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>بدهی‌های باز من، وضعیت قفل و موجودی کیف پول</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ReserveDebtSummaryDto>), 200)]
        public async Task<IActionResult> Get()
            => Ok(await _service.GetMyDebtsAsync(_currentUser.CurrentUser.UserId));

        /// <summary>پرداخت یک بدهی از کیف پول</summary>
        [HttpPost("Pay")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Pay([FromBody] ReserveDebtPayDto dto)
            => Ok(await _service.PayFromWalletAsync(_currentUser.CurrentUser.UserId, dto?.ReserveId ?? 0));
    }
}
