using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// برنامه‌ی یک روز نماینده/کلینیک: رزروهای خدمت و مشاوره‌های ساعت‌دار به ترتیب ساعت، با مشتری و پت و ساعت کاری همان روز
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionDayScheduleController : ControllerBase
    {
        private readonly ICompanionDayScheduleService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public CompanionDayScheduleController(ICompanionDayScheduleService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        /// <param name="companionId">کلینیک؛ خالی = کلینیک خودم (مالک) یا اولین کلینیکی که عضو آن هستم. فهرست کلینیک‌ها در پاسخ (availableClinics) است</param>
        /// <param name="date">روز (yyyy-MM-dd)؛ خالی = امروز (به ساعت سرور)</param>
        /// <param name="includeCancelled">true = لغو/بازپرداخت‌شده‌ها هم بیایند (پیش‌فرض false)</param>
        /// <param name="mineOnly">true = فقط رزروهای تخصیص‌یافته به خودم (مالک به‌صورت پیش‌فرض همه‌ی کلینیک را می‌بیند)</param>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CompanionDayScheduleVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] long? companionId, [FromQuery] DateTime? date, [FromQuery] bool includeCancelled = false, [FromQuery] bool mineOnly = false)
        {
            var user = _currentUserHelper.CurrentUser;
            // مالک (CompanionId پر) یا عضو تیم (IsCompanionUser)؛ بقیه دسترسی ندارند
            if ((user.CompanionId ?? 0) <= 0 && !user.IsCompanionUser)
                return Forbid();

            return Ok(await _service.GetAsync(user.UserId, companionId, date ?? DateTime.Now, includeCancelled, mineOnly));
        }
    }
}
