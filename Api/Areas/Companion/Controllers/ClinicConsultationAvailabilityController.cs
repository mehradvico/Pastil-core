using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationVenueSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// برنامه‌ی هفتگی رزرو «مشاوره آنلاین» کلینیک (پکیج‌های «قابل رزرو») با شناسه‌ی خودِ مرکز
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicConsultationAvailabilityController : ControllerBase
    {
        private readonly IVenueConsultationService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicConsultationAvailabilityController(IVenueConsultationService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>برنامه‌ی هفتگی این مرکز (WeekDayId ۱ شنبه … ۷ جمعه، ساعت «HH:mm»)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationAvailabilityWindowDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] long clinicId)
            => Ok(await _service.GetAvailabilityAsync(VenueKind.Clinic, clinicId, _currentUser.CurrentUser.UserId));

        /// <summary>جایگزینی کل برنامه‌ی هفتگی (لیست خالی = رزرو بسته). رزروهای قبلی دست نمی‌خورند.</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationAvailabilityWindowDto>>), 200)]
        public async Task<IActionResult> Put([FromQuery] long clinicId, [FromBody] List<ConsultationAvailabilityWindowDto> windows)
            => Ok(await _service.SaveAvailabilityAsync(VenueKind.Clinic, clinicId, _currentUser.CurrentUser.UserId, windows));
    }
}
