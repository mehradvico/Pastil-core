using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رد رزرو پانسیون/مهد به‌جای مرکز (ادمین): لغو + برگشت کل مبلغ به کیف پول کاربر
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveRejectController : ControllerBase
    {
        private readonly IPansionReserveApprovalService _approvalService;

        public PansionReserveRejectController(IPansionReserveApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionReserveDecisionDto dto)
            => Ok(await _approvalService.RejectAsync(dto.Id, dto.Reason, null, HttpContext.RequestAborted));
    }
}
