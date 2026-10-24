using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Application.Services.ConsultationSrvs.ConsultationVenueSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    public class CoachConsultationOnlineSetDto
    {
        public long CoachId { get; set; }
        public bool Online { get; set; }
    }

    /// <summary>
    /// «من آنلاینم» مربی برای مشاوره‌ی آنلاین فوری (اعتبار حداکثر ۳ ساعت)؛ با شناسه‌ی مرکز
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachConsultationOnlineController : ControllerBase
    {
        private readonly IVenueConsultationService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachConsultationOnlineController(IVenueConsultationService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>وضعیت آنلاین من در این مرکز</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] long coachId)
            => Ok(await _service.GetOnlineAsync(VenueKind.Coach, coachId, _currentUser.CurrentUser.UserId));

        /// <summary>روشن/خاموش‌کردن وضعیت آنلاین من</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Put([FromBody] CoachConsultationOnlineSetDto dto)
            => Ok(await _service.SetOnlineAsync(VenueKind.Coach, dto?.CoachId ?? 0, _currentUser.CurrentUser.UserId, dto?.Online ?? false));
    }
}
