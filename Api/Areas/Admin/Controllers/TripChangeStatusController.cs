using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر وضعیت سفر
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripChangeStatusController : ControllerBase
    {
        private readonly ITripService _assistanceService;
        public TripChangeStatusController(ITripService assistanceService)
        {
            this._assistanceService = assistanceService;
        }
        /// <summary>
        ///  تغییر وضعیت آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(TripChangeStatusDto dto)
        {
            var agency = await _assistanceService.TripChangeStatusAsync(dto);
            return Ok(agency);
        }

        /// <summary>
        /// لغو دستی سفر توسط ادمین (اعلان به کاربر و راننده؛ توضیح ادمین در دلیل لغو سفر ثبت می‌شود)
        /// </summary>
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Cancel(TripAdminActionDto dto)
        {
            return Ok(await _assistanceService.AdminCancelAsync(dto));
        }

        /// <summary>
        /// تکمیل دستی سفر پذیرفته‌شده توسط ادمین
        /// </summary>
        [HttpPost("complete")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Complete(TripAdminActionDto dto)
        {
            return Ok(await _assistanceService.AdminCompleteAsync(dto));
        }
    }
}
