using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// رد رزروِ پرداخت‌شده‌ی پانسیون/مهد توسط مرکز: رزرو لغو و کل مبلغ به کیف پول کاربر برمی‌گردد.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveRejectController : ControllerBase
    {
        private readonly IPansionReserveApprovalService _approvalService;
        private readonly ICurrentUserHelper _currentUser;

        public PansionReserveRejectController(IPansionReserveApprovalService approvalService, ICurrentUserHelper currentUser)
        {
            _approvalService = approvalService;
            _currentUser = currentUser;
        }

        /// <summary>رد رزرو با دلیل (الزامی؛ برای کاربر ارسال می‌شود)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionReserveDecisionDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            return Ok(await _approvalService.RejectAsync(dto.Id, dto.Reason, _currentUser.CurrentUser.CompanionId.Value, HttpContext.RequestAborted));
        }
    }
}
