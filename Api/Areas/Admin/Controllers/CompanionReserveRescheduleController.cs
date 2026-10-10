using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveSrv;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر زمان رزرو (کلینیک / مربی / آرایشگاه) توسط ادمین
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveRescheduleController : ControllerBase
    {
        private readonly ICompanionReserveRescheduleService _rescheduleService;
        private readonly ICurrentUserHelper _currentUser;

        public CompanionReserveRescheduleController(ICompanionReserveRescheduleService rescheduleService, ICurrentUserHelper currentUser)
        {
            _rescheduleService = rescheduleService;
            _currentUser = currentUser;
        }

        [HttpGet("slots")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveRescheduleSlotsVDto>), 200)]
        public async Task<IActionResult> Slots([FromQuery] long reserveId, [FromQuery] DateTime date)
            => Ok(await _rescheduleService.GetSlotsAsync(reserveId, date, null, HttpContext.RequestAborted));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveRescheduleResultVDto>), 200)]
        public async Task<IActionResult> Put(CompanionReserveRescheduleDto dto)
            => Ok(await _rescheduleService.RescheduleAsync(
                dto, _currentUser.CurrentUser.UserId, CompanionReserveRescheduleRules.ActorAdmin, null, HttpContext.RequestAborted));

        [HttpGet("history")]
        [ProducesResponseType(typeof(BaseResultDto<List<CompanionReserveRescheduleVDto>>), 200)]
        public async Task<IActionResult> History([FromQuery] long reserveId)
            => Ok(await _rescheduleService.GetHistoryAsync(reserveId, null, HttpContext.RequestAborted));
    }
}
