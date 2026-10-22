using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    public class CoachConsultationOnlineSetDto
    {
        public long CoachId { get; set; }
        public bool Online { get; set; }
    }

    /// <summary>
    /// «من آنلاینم» مربی برای مشاوره‌ی آنلاین فوری (همان سامانه‌ی ConsultationOnline کلینیک؛ اعتبار حداکثر ۳ ساعت)
    /// </summary>
    /// <remarks>با شناسه‌ی مربی کار می‌کند و به Companion قدیمیِ مربی (LegacyCompanionId) نگاشت می‌شود؛ مربی بدون آن خطای CoachConsultationUnavailable می‌گیرد.</remarks>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachConsultationOnlineController : ControllerBase
    {
        private readonly ICoachConsultationOnlineService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachConsultationOnlineController(ICoachConsultationOnlineService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>وضعیت آنلاین من در این مربی</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] long coachId)
            => Ok(await _service.GetMineAsync(_currentUser.CurrentUser.UserId, coachId));

        /// <summary>روشن/خاموش‌کردن وضعیت آنلاین من</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Put([FromBody] CoachConsultationOnlineSetDto dto)
            => Ok(await _service.SetMineAsync(_currentUser.CurrentUser.UserId, dto?.CoachId ?? 0, dto?.Online ?? false));
    }
}
