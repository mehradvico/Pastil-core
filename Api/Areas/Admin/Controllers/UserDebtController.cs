using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveDebtSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// بدهی‌های پرداخت‌نشده‌ی کاربر
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserDebtController : ControllerBase
    {
        private readonly ICompanionReserveDebtService _service;

        public UserDebtController(ICompanionReserveDebtService service)
        {
            _service = service;
        }

        /// <summary>بدهی‌های باز کاربر + موجودی کیف پول</summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(BaseResultDto<ReserveDebtSummaryDto>), 200)]
        public async Task<IActionResult> Get(long userId)
            => Ok(await _service.GetMyDebtsAsync(userId));

        /// <summary>صفر کردن بدهی یک رزرو</summary>
        [HttpPut("WriteOff/{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> WriteOff(long reserveId)
            => Ok(await _service.WriteOffAsync(reserveId));

        /// <summary>صفر کردن همه‌ی بدهی‌های باز کاربر</summary>
        [HttpPut("WriteOffAll/{userId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> WriteOffAll(long userId)
            => Ok(await _service.WriteOffAllForUserAsync(userId));
    }
}
