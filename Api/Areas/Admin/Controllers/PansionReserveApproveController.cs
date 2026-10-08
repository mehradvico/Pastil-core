using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تأیید رزرو پانسیون/مهد به‌جای مرکز (ادمین)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveApproveController : ControllerBase
    {
        private readonly IPansionReserveApprovalService _approvalService;

        public PansionReserveApproveController(IPansionReserveApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionReserveDecisionDto dto)
            => Ok(await _approvalService.ApproveAsync(dto.Id, null, HttpContext.RequestAborted));
    }
}
