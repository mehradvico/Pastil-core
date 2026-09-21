using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// خرید پکیج «مشاوره آنلاین» توسط کاربر: خرید و پرداخت، فهرست/جزئیات و لغو قبل از شروع
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationPurchaseController : ControllerBase
    {
        private readonly IConsultationPurchaseService _service;
        private readonly IConsultationSessionService _sessionService;
        private readonly ICurrentUserHelper _currentUserHelper;

        public ConsultationPurchaseController(IConsultationPurchaseService service, IConsultationSessionService sessionService, ICurrentUserHelper currentUserHelper)
        {
            _sessionService = sessionService;
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>خرید پکیج و شروع پرداخت (قیمت از سرور؛ کلاینت فقط پکیج و روش پرداخت را می‌فرستد)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post([FromBody] ConsultationPurchaseCreateDto dto)
            => Ok(await _service.PurchaseAsync(_currentUserHelper.CurrentUser.UserId, dto));

        /// <summary>خریدهای من (جدیدترین اول)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPurchaseVDto>>), 200)]
        public async Task<IActionResult> Get()
            => Ok(await _service.GetMineAsync(_currentUserHelper.CurrentUser.UserId));

        /// <summary>مشاوره‌های در جریان من (شروع‌شده و منقضی‌نشده) برای نوار «بازگشت به مشاوره»</summary>
        [HttpGet("Active")]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationActiveWindowVDto>>), 200)]
        public async Task<IActionResult> Active()
            => Ok(await _sessionService.GetActiveWindowsAsync(_currentUserHelper.CurrentUser.UserId));

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPurchaseVDto>), 200)]
        public async Task<IActionResult> Get(long id)
            => Ok(await _service.GetAsync(_currentUserHelper.CurrentUser.UserId, id));

        /// <summary>لغو قبل از شروع نماینده + بازپرداخت کامل به کیف پول</summary>
        [HttpPut("{id}/Cancel")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(long id)
            => Ok(await _service.CancelAsync(_currentUserHelper.CurrentUser.UserId, id));
    }
}
