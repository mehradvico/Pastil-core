using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// ساعت‌های کاری هفتگی کلینیک برای «مشاوره آنلاین قابل‌رزرو» توسط مالک کلینیک
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationAvailabilityController : ControllerBase
    {
        private readonly IConsultationBookingService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public ConsultationAvailabilityController(IConsultationBookingService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        private long? CompanionId
        {
            get
            {
                var id = _currentUserHelper.CurrentUser.CompanionId;
                return id.HasValue && id.Value > 0 ? id : null;
            }
        }

        /// <summary>برنامه‌ی هفتگی کلینیک من (WeekDayId ۱ شنبه … ۷ جمعه، ساعت «HH:mm»)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationAvailabilityWindowDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            if (CompanionId is not { } companionId)
                return Forbid();

            return Ok(await _service.GetAvailabilityAsync(companionId));
        }

        /// <summary>جایگزینی کل برنامه‌ی هفتگی (لیست خالی = رزرو بسته). رزروهای قبلی دست نمی‌خورند.</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationAvailabilityWindowDto>>), 200)]
        public async Task<IActionResult> Put([FromBody] List<ConsultationAvailabilityWindowDto> windows)
        {
            if (CompanionId is not { } companionId)
                return Forbid();

            return Ok(await _service.SaveAvailabilityAsync(companionId, windows));
        }
    }
}
