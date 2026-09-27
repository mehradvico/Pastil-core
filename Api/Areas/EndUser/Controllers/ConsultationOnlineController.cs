using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
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

        /// <summary>آیا الان کسی از تیم این کلینیک برای مشاوره آنلاین است</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] long companionId)
            => Ok(new BaseResultDto<ConsultationOnlineStatusDto>(true, await _service.GetPublicAsync(companionId)));
    }
}
