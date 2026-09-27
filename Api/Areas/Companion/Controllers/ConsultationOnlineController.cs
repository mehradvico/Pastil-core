using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    public class ConsultationOnlineSetDto
    {
        public bool Online { get; set; }
        // اختیاری: فقط وقتی کاربر عضو فعال چند کلینیک باشد لازم است
        public long? CompanionId { get; set; }
    }

    /// <summary>
    /// «من آنلاینم» برای مشاوره آنلاین
    /// </summary>
    /// <remarks>دکتر/اپراتور عضو کلینیک وضعیت خودش را روشن یا خاموش می‌کند؛ روشن‌کردن حداکثر ۳ ساعت اعتبار دارد</remarks>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationOnlineController : ControllerBase
    {
        private readonly IConsultationOnlineService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ConsultationOnlineController(IConsultationOnlineService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>وضعیت آنلاین من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] long? companionId)
            => Ok(await _service.GetMineAsync(_currentUser.CurrentUser.UserId, companionId));

        /// <summary>روشن/خاموش کردن وضعیت آنلاین من</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationOnlineStatusDto>), 200)]
        public async Task<IActionResult> Put([FromBody] ConsultationOnlineSetDto dto)
            => Ok(await _service.SetMineAsync(_currentUser.CurrentUser.UserId, dto?.CompanionId, dto?.Online ?? false));
    }
}
