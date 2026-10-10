using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveSrv;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// تغییر زمان رزرو (کلینیک / مربی / آرایشگاه) توسط نماینده. به کاربر (و راننده‌ی سفر پت‌رسانِ متصل) پیامک می‌رود.
    /// </summary>
    [Area("Companion")]
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

        /// <summary>بازه‌های ساعتیِ قابل انتخاب برای یک روز (با ظرفیت باقی‌مانده و دلیل غیرقابل‌انتخاب بودن)</summary>
        [HttpGet("slots")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveRescheduleSlotsVDto>), 200)]
        public async Task<IActionResult> Slots([FromQuery] long reserveId, [FromQuery] DateTime date)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            return Ok(await _rescheduleService.GetSlotsAsync(reserveId, date, _currentUser.CurrentUser.CompanionId.Value, HttpContext.RequestAborted));
        }

        /// <summary>تغییر زمان رزرو (روز + بازه‌ی ساعتی + دلیل الزامی)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveRescheduleResultVDto>), 200)]
        public async Task<IActionResult> Put(CompanionReserveRescheduleDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            return Ok(await _rescheduleService.RescheduleAsync(
                dto, _currentUser.CurrentUser.UserId, CompanionReserveRescheduleRules.ActorCompanion,
                _currentUser.CurrentUser.CompanionId.Value, HttpContext.RequestAborted));
        }

        /// <summary>تاریخچه‌ی تغییر زمان یک رزرو</summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(BaseResultDto<List<CompanionReserveRescheduleVDto>>), 200)]
        public async Task<IActionResult> History([FromQuery] long reserveId)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            return Ok(await _rescheduleService.GetHistoryAsync(reserveId, _currentUser.CurrentUser.CompanionId.Value, HttpContext.RequestAborted));
        }
    }
}
