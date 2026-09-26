using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// بستن بدهی پرداخت‌نشده توسط کلینیک
    /// </summary>
    /// <remarks>وقتی کاربر مبلغ را مستقیم به کلینیک داده، مالک کلینیک بدهی را «دریافت شد» می‌کند</remarks>
    [Area("Companion")]
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

        /// <summary>دریافت مستقیم مبلغ از کاربر تأیید شد</summary>
        [HttpPut("MarkPaid")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> MarkPaid([FromBody] ReserveDebtMarkPaidDto dto)
            => Ok(await _service.MarkPaidByClinicAsync(_currentUser.CurrentUser.UserId, dto?.ReserveId ?? 0));
    }
}
