using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// وضعیت آنلاین‌بودن کلینیک برای مشاوره آنلاین (عمومی)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationOnlineController : ControllerBase
    {
        private readonly IConsultationOnlineService _service;

        public ConsultationOnlineController(IConsultationOnlineService service)
        {
            _service = service;
        }

        /// <summary>آیا الان کسی از تیم این مرکز برای مشاوره آنلاین است (دقیقاً یکی از companionId / clinicId / coachId)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] long? companionId, [FromQuery] long? clinicId, [FromQuery] long? coachId)
        {
            var owner = ConsultationOwner.From(companionId, clinicId, coachId);
            if (!owner.HasValue)
                return Ok(new BaseResultDto<ConsultationOnlineStatusDto>(false, Resource.Notification.InvalidData, null));
            return Ok(new BaseResultDto<ConsultationOnlineStatusDto>(true, await _service.GetPublicAsync(owner.Value)));
        }
    }
}
