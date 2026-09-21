using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// «مشاوره‌های من» برای نماینده: فهرست خریدهای مشاوره‌ی کلینیک، شروع و ورود دوباره در پنجره‌ی زمانی
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationSessionController : ControllerBase
    {
        private readonly IConsultationSessionService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public ConsultationSessionController(IConsultationSessionService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationAgentItemVDto>>), 200)]
        public async Task<IActionResult> Get()
            => Ok(await _service.GetForAgentAsync(_currentUserHelper.CurrentUser.UserId));

        /// <summary>شروع مشاوره: پنجره‌ی ۳۰/۶۰ دقیقه‌ای از همین لحظه حساب می‌شود (فقط یک‌بار)</summary>
        [HttpPost("{purchaseId}/Start")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationSessionInfoVDto>), 200)]
        public async Task<IActionResult> Start(long purchaseId)
            => Ok(await _service.StartAsync(_currentUserHelper.CurrentUser.UserId, purchaseId));

        /// <summary>ورود دوباره به مشاوره‌ی در جریان (تا پایان پنجره)</summary>
        [HttpPost("{purchaseId}/Enter")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationSessionInfoVDto>), 200)]
        public async Task<IActionResult> Enter(long purchaseId)
            => Ok(await _service.EnterAsync(_currentUserHelper.CurrentUser.UserId, purchaseId));
    }
}
