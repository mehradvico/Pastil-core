using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// تأیید رزروِ پرداخت‌شده‌ی پانسیون/مهد توسط مرکز. بدون پاسخ تا مهلت = لغو خودکار و برگشت پول به کاربر.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveApproveController : ControllerBase
    {
        private readonly IPansionReserveApprovalService _approvalService;
        private readonly ICurrentUserHelper _currentUser;

        public PansionReserveApproveController(IPansionReserveApprovalService approvalService, ICurrentUserHelper currentUser)
        {
            _approvalService = approvalService;
            _currentUser = currentUser;
        }

        /// <summary>تأیید رزرو (فقط مرکزِ مالک)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionReserveDecisionDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            return Ok(await _approvalService.ApproveAsync(dto.Id, _currentUser.CurrentUser.CompanionId.Value, HttpContext.RequestAborted));
        }
    }
}
