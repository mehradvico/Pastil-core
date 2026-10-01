using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ساعت‌های قابل رزرو برای پکیج‌های «مشاوره آنلاین قابل‌رزرو» (Bookable). فقط خواندنی؛ خرید با ConsultationPurchase و ScheduledStart.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationBookingController : ControllerBase
    {
        private readonly IConsultationBookingService _service;

        public ConsultationBookingController(IConsultationBookingService service)
        {
            _service = service;
        }

        /// <summary>روزهای ۱۴ روز آینده با تعداد ساعت خالی؛ برای انتخابگر تاریخ</summary>
        [HttpGet("Days")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationBookingDaysVDto>), 200)]
        public async Task<IActionResult> Days([FromQuery] long packageId)
            => Ok(await _service.GetDaysAsync(packageId));

        /// <summary>ساعت‌های شروع یک روز (date به‌صورت yyyy-MM-dd)؛ available=false یعنی ظرفیت پر است</summary>
        [HttpGet("Slots")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationSlotsVDto>), 200)]
        public async Task<IActionResult> Slots([FromQuery] long packageId, [FromQuery] DateTime date)
            => Ok(await _service.GetSlotsAsync(packageId, date));
    }
}
